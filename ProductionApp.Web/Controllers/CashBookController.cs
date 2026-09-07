using ClosedXML.Excel;
using DocumentFormat.OpenXml.Vml.Office;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProductionApp.Web.Constants;
using ProductionApp.Web.Models.ViewModels;
using ProductionManager.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace ProductionApp.Web.Controllers
{
    public class CashBookStat
    {
        public int Month { get; set; }
        public decimal In { get; set; }
        public decimal Out { get; set; }
    }

    [Authorize]
    public class CashBookController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public CashBookController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ==========================================
        // 0. HELPER LẤY DATA
        // ==========================================
        private async Task<string> GetOrdersJsonAsync()
        {
            var result = await _context.Orders
                .Where(o => o.Status == OrderStatus.Completed ||
                    o.Status == OrderStatus.Approved || // add logic
                    o.Status == OrderStatus.Delivered ||
                    o.Status == OrderStatus.Invoiced)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new
                {
                    o.Id,
                    o.OrderCode,
                    o.TotalAmount,
                    CustomerName = o.Customer != null ? o.Customer.CompanyName : "Khách lẻ",
                    Paid = _context.CashEntries
                            .Where(r => r.OrderId == o.Id && r.Type == TransactionType.Receipt)
                            .Sum(r => r.Amount)
                })
                .ToListAsync();

            return JsonSerializer.Serialize(result);
        }

        private async Task<List<SelectListItem>> GetActiveLoansAsync(TransactionType currentType)
        {
            var targetParentType = currentType == TransactionType.Payment ? TransactionType.Receipt : TransactionType.Payment;
            var loans = await _context.CashEntries
                .Include(x => x.Repayments)
                .Where(x => x.Category == EntryCategory.Loan && x.ParentId == null && x.Type == targetParentType)
                .ToListAsync();

            var activeLoans = loans
                .Where(x => (x.Amount - (x.Repayments?.Sum(r => r.Amount) ?? 0)) > 0)
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = $"{x.TargetName} - Còn: {(x.Amount - (x.Repayments?.Sum(r => r.Amount) ?? 0)):N0} ({x.Description})"
                }).ToList();

            return activeLoans;
        }

        private async Task<List<SelectListItem>> GetActiveSupplierDebtsAsync()
        {
            var debts = await _context.MaterialImports
                .Where(x => x.PaymentStatus != ImportPaymentStatus.Paid)
                .OrderByDescending(x => x.ImportDate)
                .Select(x => new
                {
                    x.Id,
                    Text = $"{x.ImportCode} - {x.SupplierName} (Còn nợ: {(x.TotalAmount - x.PaidAmount):N0})"
                }).ToListAsync();

            return debts.Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Text }).ToList();
        }

        // Old function
        /*
        private async Task UpdateOrderPaymentStatus(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return;

            // Tính tổng tiền đã thu của đơn hàng này
            var totalPaid = await _context.CashEntries
                .Where(x => x.OrderId == orderId && x.Type == TransactionType.Receipt)
                .SumAsync(x => x.Amount);

            // Kiểm tra xem đã thu đủ tiền chưa
            if (totalPaid >= order.TotalAmount)
            {
                order.PaymentStatus = PaymentStatus.Paid;

                // (Chỉ đổi nếu đơn hàng chưa bị Hủy)
                if (order.Status != OrderStatus.Canceled)
                {
                    order.Status = OrderStatus.Completed;
                }
            }
            else if (totalPaid > 0)
            {
                order.PaymentStatus = PaymentStatus.Partial;
            }
            else
            {
                order.PaymentStatus = PaymentStatus.Unpaid;
            }

            _context.Update(order);
            await _context.SaveChangesAsync();
        } */

        // 1. DÀNH CHO ĐƠN BÁN HÀNG (ORDERS)
        private async Task UpdateOrderPaymentStatus(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return;

            var totalPaid = await _context.CashEntries.Where(x => x.OrderId == orderId && x.Type == TransactionType.Receipt).SumAsync(x => x.Amount);
            var totalBilled = await _context.InvoiceDetails.Where(i => i.OrderId == orderId).SumAsync(i => i.BilledAmount);

            if (totalPaid >= order.TotalAmount - 0.01m)
            {
                order.PaymentStatus = PaymentStatus.Paid;
                if (order.Status != OrderStatus.Canceled && (order.Status == OrderStatus.Delivered || order.Status == OrderStatus.Invoiced))
                    order.Status = OrderStatus.Completed;
            }
            else
            {
                order.PaymentStatus = totalPaid > 0 ? PaymentStatus.Partial : PaymentStatus.Unpaid;
                if (order.Status == OrderStatus.Completed)
                    order.Status = totalBilled >= order.TotalAmount ? OrderStatus.Invoiced : OrderStatus.Delivered;
            }
            _context.Update(order);
            await _context.SaveChangesAsync();
        }

        private async Task UpdatePurchaseOrderPaymentStatus(int poId)
        {
            var po = await _context.PurchaseOrders.FindAsync(poId);
            if (po == null) return;

            var totalPaid = await _context.CashEntries.Where(c => c.PurchaseOrderId == poId && c.Type == TransactionType.Payment).SumAsync(c => c.Amount);

            po.DepositAmount = totalPaid;
            po.RemainingAmount = po.FinalTotal - po.DepositAmount;

            if (po.RemainingAmount <= 0 && po.FinalTotal > 0) po.PaymentStatusPO = PaymentStatusPO.Paid;
            else if (po.DepositAmount > 0) po.PaymentStatusPO = PaymentStatusPO.Deposited;
            else po.PaymentStatusPO = PaymentStatusPO.Unpaid;

            _context.Update(po);
            await _context.SaveChangesAsync();
        }

        private async Task UpdateAPInvoicePaymentStatus(int apId)
        {
            var ap = await _context.APInvoices.FindAsync(apId);
            if (ap == null) return;

            var totalPaid = await _context.CashEntries.Where(c => c.APInvoiceId == apId && c.Type == TransactionType.Payment).SumAsync(c => c.Amount);
            ap.PaidAmount = totalPaid;
            _context.Update(ap);
            await _context.SaveChangesAsync();
        }

        private async Task UpdateExportShipmentPaymentStatus(int exportId)
        {
            var export = await _context.ExportShipments.FindAsync(exportId);
            if (export == null) return;

            var totalPaid = await _context.CashEntries.Where(c => c.ExportShipmentId == exportId && c.Type == TransactionType.Payment).SumAsync(c => c.Amount);
            export.PaymentStatus = totalPaid >= export.ShippingCost - 0.01m ? PaymentStatuses.Paid : PaymentStatuses.Unpaid;
            _context.Update(export);
            await _context.SaveChangesAsync();
        }

        private async Task UpdateImportPaymentStatus(int importId)
        {
            var import = await _context.MaterialImports.FindAsync(importId);
            if (import == null) return;

            var totalPaid = await _context.CashEntries
                .Where(c => c.MaterialImportId == importId && c.Type == TransactionType.Payment)
                .SumAsync(c => c.Amount);

            import.PaidAmount = totalPaid;

            if (import.PaidAmount >= import.TotalAmount - 0.01m) import.PaymentStatus = ImportPaymentStatus.Paid;
            else if (import.PaidAmount > 0) import.PaymentStatus = ImportPaymentStatus.Partial;
            else import.PaymentStatus = ImportPaymentStatus.Unpaid;

            _context.Update(import);
            await _context.SaveChangesAsync();
        }

        private string GenerateCode(TransactionType type)
        {
            string prefix = type == TransactionType.Receipt ? "PT" : "PC";
            string datePart = DateTime.Now.ToString("yyMM");
            var lastCode = _context.CashEntries
                .Where(x => x.VoucherCode.StartsWith($"{prefix}{datePart}"))
                .OrderByDescending(x => x.Id)
                .Select(x => x.VoucherCode)
                .FirstOrDefault();

            int nextNum = 1;
            if (!string.IsNullOrEmpty(lastCode) && lastCode.Length > 8)
            {
                var parts = lastCode.Split('-');
                if (parts.Length > 1 && int.TryParse(parts[1], out int currentNum)) nextNum = currentNum + 1;
            }
            return $"{prefix}{datePart}-{nextNum:000}";
        }

        // =========================================================================
        // 🔥 HÀM TỰ ĐỘNG NẠP DROPDOWN (Chống lỗi sập giao diện khi ModelState False)
        // =========================================================================
        private async Task PopulateDropdownsAsync(CashEntry model)
        {
            ViewBag.OrdersJson = await GetOrdersJsonAsync();

            var ordersQuery = _context.Orders.Include(x => x.Customer).AsQueryable();

            // Lọc đơn hàng hợp lệ (Hoàn thành, đã giao, xuất HĐ)
            if (model.Type == TransactionType.Receipt)
            {
                ordersQuery = ordersQuery.Where(o => o.Id == model.OrderId ||
                                                     o.Status == OrderStatus.Completed ||
                                                     o.Status == OrderStatus.Delivered ||
                                                     o.Status == OrderStatus.Invoiced);
            }

            var ordersList = await ordersQuery
                .OrderByDescending(x => x.OrderDate)
                .Select(x => new {
                    x.Id,
                    Text = $"{x.OrderCode} - {(x.Customer != null ? x.Customer.CompanyName : "Khách lẻ")} (Nợ: {(x.TotalAmount - _context.CashEntries.Where(c => c.OrderId == x.Id && c.Type == TransactionType.Receipt && c.Id != model.Id).Sum(c => c.Amount)):N0})"
                })
                .ToListAsync();

            ViewBag.OrderId = new SelectList(ordersList, "Id", "Text", model.OrderId);

            ViewBag.ActiveLoans = await GetActiveLoansAsync(model.Type);

            if (model.Type == TransactionType.Payment)
            {
                ViewBag.SupplierDebts = await GetActiveSupplierDebtsAsync();
            }

            ViewBag.CostCategories = await _context.CostCategories
                .Where(c => c.IsActive || c.Id == model.CostCategoryId)
                .OrderBy(c => c.Section).ThenBy(c => c.Name)
                .ToListAsync();
        }

        // ==========================================
        // 1. TRANG INDEX 
        // ==========================================
        [Authorize(Policy = AppPermissions.Debts.View)]
        public async Task<IActionResult> Index(DateTime? fromDate, DateTime? toDate, string keyword, PaymentMethod? method)
        {
            var f = fromDate ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var t = toDate ?? f.AddMonths(1).AddDays(-1);
            var tEnd = t.Date.AddDays(1).AddTicks(-1);

            var baseQuery = _context.CashEntries.AsQueryable();

            if (method.HasValue) baseQuery = baseQuery.Where(x => x.Method == method.Value);

            var prevTrans = await baseQuery
                .Where(x => x.TransactionDate < f)
                .Select(x => new { x.Type, x.Amount })
                .ToListAsync();

            decimal openBalance = prevTrans.Where(x => x.Type == TransactionType.Receipt).Sum(x => x.Amount)
                                - prevTrans.Where(x => x.Type == TransactionType.Payment).Sum(x => x.Amount);

            var query = baseQuery
                .Include(x => x.Order)
                .Include(x => x.CostCategory)
                .Where(x => x.TransactionDate >= f && x.TransactionDate <= tEnd);

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(x => x.VoucherCode.Contains(keyword)
                                      || x.Description.Contains(keyword)
                                      || x.TargetName.Contains(keyword));
            }

            var transactions = await query.OrderBy(x => x.TransactionDate).ToListAsync();

            var allTimeTrans = await baseQuery.Select(x => new { x.Type, x.Amount }).ToListAsync();
            decimal realBalance = allTimeTrans.Where(x => x.Type == TransactionType.Receipt).Sum(x => x.Amount)
                                - allTimeTrans.Where(x => x.Type == TransactionType.Payment).Sum(x => x.Amount);

            var yearStart = new DateTime(DateTime.Now.Year, 1, 1);
            var yearEnd = new DateTime(DateTime.Now.Year, 12, 31).AddDays(1).AddTicks(-1);

            var monthlyStats = await baseQuery
                .Where(x => x.TransactionDate >= yearStart && x.TransactionDate <= yearEnd)
                .GroupBy(x => x.TransactionDate.Month)
                .Select(g => new CashBookStat
                {
                    Month = g.Key,
                    In = g.Where(x => x.Type == TransactionType.Receipt).Sum(x => x.Amount),
                    Out = g.Where(x => x.Type == TransactionType.Payment).Sum(x => x.Amount)
                })
                .OrderBy(x => x.Month)
                .ToListAsync();

            ViewBag.MonthlyStats = monthlyStats;
            ViewBag.FromDate = f;
            ViewBag.ToDate = t;
            ViewBag.OpenBalance = openBalance;
            ViewBag.RealCurrentBalance = realBalance;
            ViewBag.Keyword = keyword;
            ViewBag.CurrentMethod = method;

            return View(transactions);
        }

        // ==========================================
        // 2. CREATE 
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Create(TransactionType type = TransactionType.Receipt, int? orderId = null)
        {
            var model = new CashEntry { Type = type, TransactionDate = DateTime.Today, OrderId = orderId };

            // Gọi hàm Helper để nạp data
            await PopulateDropdownsAsync(model);

            ViewData["Breadcrumbs"] = new List<(string Title, string Url)>
            {
                ("Sổ Quỹ", Url.Action("Index", "CashBook")),
                (type == TransactionType.Receipt ? "Lập Phiếu Thu" : "Lập Phiếu Chi", "")
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = AppPermissions.Debts.Create)]
        public async Task<IActionResult> Create(CashEntry model)
        {
            // 1. Kiểm tra khóa sổ
            if (await IsPeriodClosedAsync(model.TransactionDate))
            {
                TempData["Error"] = $"ℹ️: Kỳ kế toán Tháng {model.TransactionDate.Month}/{model.TransactionDate.Year} đã BỊ KHÓA.\nKhông thể thêm phiếu chi/thu vào tháng này!";
                return RedirectToAction(nameof(Index));
            }

            // 🔥 2. FIX LỖI "SILENT ERROR": Gỡ bỏ Validation cho TẤT CẢ các trường liên kết
            ModelState.Remove("VoucherCode");
            ModelState.Remove("CreatedBy");
            ModelState.Remove("Order");
            ModelState.Remove("CostCategory");
            ModelState.Remove("MaterialImport");
            ModelState.Remove("Parent");
            ModelState.Remove("Repayments");
            // Bổ sung các bảng mới
            ModelState.Remove("APInvoice");
            ModelState.Remove("CashAllocations");

            if (ModelState.IsValid)
            {
                model.CreatedBy = User.Identity?.Name ?? "System";
                model.CreatedAt = DateTime.Now;

                if (string.IsNullOrEmpty(model.VoucherCode)) model.VoucherCode = GenerateCode(model.Type);
                if (model.AllocatedMonths <= 0) model.AllocatedMonths = 1;

                _context.Add(model);
                await _context.SaveChangesAsync();

                // Tự động phân bổ chi phí P&L
                await GenerateAllocationsAsync(model);
                await _context.SaveChangesAsync();

                if (model.MaterialImportId.HasValue) await UpdateImportPaymentStatus(model.MaterialImportId.Value);
                if (model.OrderId.HasValue && model.Type == TransactionType.Receipt) await UpdateOrderPaymentStatus(model.OrderId.Value);

                TempData["Success"] = "Lập phiếu thành công!";
                return RedirectToAction(nameof(Index));
            }

            // 🔥 3. Bắt lỗi hiển thị nếu vẫn còn trường nào đó không hợp lệ (Tránh lỗi ngầm)
            var errors = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            TempData["Error"] = "Lỗi dữ liệu: " + errors;

            // Nếu điền thiếu data, gọi lại Dropdown để giao diện không bị sập
            await PopulateDropdownsAsync(model);
            return View(model);
        }


        // ==========================================
        // 3. EDIT 
        // ==========================================
        [HttpGet]
        [Authorize(Policy = AppPermissions.Debts.Edit)]
        public async Task<IActionResult> Edit(int id)
        {
            var entry = await _context.CashEntries.FindAsync(id);
            if (entry == null) return NotFound();

            // Nạp toàn bộ danh sách dropdown cho trang Edit
            await PopulateDropdownsAsync(entry);

            ViewData["Breadcrumbs"] = new List<(string Title, string Url)>
            {
                ("Sổ Quỹ", Url.Action("Index", "CashBook")),
                (entry.Type == TransactionType.Receipt ? "Sửa Phiếu Thu" : "Sửa Phiếu Chi", "")
            };

            return View(entry);
        }

        /// <summary>
        /// Old Edit
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /*
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = AppPermissions.Debts.Edit)]
        public async Task<IActionResult> Edit(int id, CashEntry entry)
        {
            if (id != entry.Id) return NotFound();

            // 🔥 FIX LỖI "SILENT ERROR" TƯƠNG TỰ NHƯ CREATE
            ModelState.Remove("VoucherCode");
            ModelState.Remove("CreatedBy");
            ModelState.Remove("Order");
            ModelState.Remove("CostCategory");
            ModelState.Remove("MaterialImport");
            ModelState.Remove("Parent");
            ModelState.Remove("Repayments");
            // Bổ sung các bảng mới
            ModelState.Remove("APInvoice");
            ModelState.Remove("CashAllocations");

            if (ModelState.IsValid)
            {
                try
                {
                    if (entry.AllocatedMonths <= 0) entry.AllocatedMonths = 1;

                    var oldEntry = await _context.CashEntries.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
                    if (oldEntry == null) return NotFound();

                    entry.CreatedBy = oldEntry.CreatedBy;
                    entry.CreatedAt = oldEntry.CreatedAt;
                    entry.VoucherCode = oldEntry.VoucherCode;

                    if (await IsPeriodClosedAsync(oldEntry.TransactionDate) || await IsPeriodClosedAsync(entry.TransactionDate))
                    {
                        TempData["Error"] = "ℹ️: Dữ liệu thuộc Kỳ kế toán đã BỊ KHÓA.\nBạn không thể sửa hay thay đổi ngày tháng của phiếu này!";
                        return RedirectToAction(nameof(Index));
                    }

                    _context.Update(entry);
                    await _context.SaveChangesAsync();

                    // Cập nhật lại dữ liệu phân bổ P&L
                    await GenerateAllocationsAsync(entry);
                    await _context.SaveChangesAsync();

                    // Cập nhật lại thanh toán nếu có thay đổi Order/Import
                    if (entry.OrderId.HasValue) await UpdateOrderPaymentStatus(entry.OrderId.Value);
                    if (entry.MaterialImportId.HasValue) await UpdateImportPaymentStatus(entry.MaterialImportId.Value);

                    // Phục hồi lại trạng thái nợ của đơn hàng cũ nếu bị tháo ra
                    if (oldEntry.OrderId.HasValue && oldEntry.OrderId != entry.OrderId) await UpdateOrderPaymentStatus(oldEntry.OrderId.Value);
                    if (oldEntry.MaterialImportId.HasValue && oldEntry.MaterialImportId != entry.MaterialImportId) await UpdateImportPaymentStatus(oldEntry.MaterialImportId.Value);

                    TempData["Success"] = "Cập nhật thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.CashEntries.Any(e => e.Id == id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            // Bắt và in lỗi nếu có
            var errors = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            TempData["Error"] = "Lỗi dữ liệu: " + errors;

            await PopulateDropdownsAsync(entry);
            return View(entry);
        } */

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = AppPermissions.Debts.Edit)]
        public async Task<IActionResult> Edit(int id, CashEntry entry)
        {
            if (id != entry.Id) return NotFound();

            // 1. GỠ BỎ VALIDATION CÁC TRƯỜNG LIÊN KẾT ĐỂ TRÁNH LỖI NGẦM
            ModelState.Remove("VoucherCode"); ModelState.Remove("CreatedBy"); ModelState.Remove("Order");
            ModelState.Remove("CostCategory"); ModelState.Remove("MaterialImport"); ModelState.Remove("Parent");
            ModelState.Remove("Repayments"); ModelState.Remove("APInvoice"); ModelState.Remove("CashAllocations");

            if (ModelState.IsValid)
            {
                // 🔥 GIAO DỊCH AN TOÀN (TRANSACTION)
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    if (entry.AllocatedMonths <= 0) entry.AllocatedMonths = 1;

                    var oldEntry = await _context.CashEntries.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
                    if (oldEntry == null) return NotFound();

                    entry.CreatedBy = oldEntry.CreatedBy;
                    entry.CreatedAt = oldEntry.CreatedAt;
                    entry.VoucherCode = oldEntry.VoucherCode;

                    if (await IsPeriodClosedAsync(oldEntry.TransactionDate) || await IsPeriodClosedAsync(entry.TransactionDate))
                    {
                        TempData["Error"] = "ℹ️: Dữ liệu thuộc Kỳ kế toán đã BỊ KHÓA.";
                        return RedirectToAction(nameof(Index));
                    }

                    // 2. CẬP NHẬT PHIẾU THU/CHI
                    _context.Update(entry);
                    await _context.SaveChangesAsync();

                    // 3. TÍNH LẠI PHÂN BỔ CHI PHÍ P&L
                    await GenerateAllocationsAsync(entry);
                    await _context.SaveChangesAsync();

                    // 🔥 4. ĐỒNG BỘ TRẠNG THÁI CÔNG NỢ (CHỨNG TỪ MỚI)
                    if (entry.OrderId.HasValue) await UpdateOrderPaymentStatus(entry.OrderId.Value);
                    if (entry.MaterialImportId.HasValue) await UpdateImportPaymentStatus(entry.MaterialImportId.Value);
                    if (entry.PurchaseOrderId.HasValue) await UpdatePurchaseOrderPaymentStatus(entry.PurchaseOrderId.Value);
                    if (entry.APInvoiceId.HasValue) await UpdateAPInvoicePaymentStatus(entry.APInvoiceId.Value);
                    if (entry.ExportShipmentId.HasValue) await UpdateExportShipmentPaymentStatus(entry.ExportShipmentId.Value);

                    // 🔥 5. PHỤC HỒI TRẠNG THÁI CHO CHỨNG TỪ CŨ (Nếu kế toán đổi liên kết)
                    if (oldEntry.OrderId.HasValue && oldEntry.OrderId != entry.OrderId) await UpdateOrderPaymentStatus(oldEntry.OrderId.Value);
                    if (oldEntry.MaterialImportId.HasValue && oldEntry.MaterialImportId != entry.MaterialImportId) await UpdateImportPaymentStatus(oldEntry.MaterialImportId.Value);
                    if (oldEntry.PurchaseOrderId.HasValue && oldEntry.PurchaseOrderId != entry.PurchaseOrderId) await UpdatePurchaseOrderPaymentStatus(oldEntry.PurchaseOrderId.Value);
                    if (oldEntry.APInvoiceId.HasValue && oldEntry.APInvoiceId != entry.APInvoiceId) await UpdateAPInvoicePaymentStatus(oldEntry.APInvoiceId.Value);
                    if (oldEntry.ExportShipmentId.HasValue && oldEntry.ExportShipmentId != entry.ExportShipmentId) await UpdateExportShipmentPaymentStatus(oldEntry.ExportShipmentId.Value);

                    await transaction.CommitAsync();
                    TempData["Success"] = "Đã cập nhật Phiếu và đồng bộ toàn bộ Công Nợ thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    await transaction.RollbackAsync();
                    TempData["Error"] = "🚨 XUNG ĐỘT: Phiếu này vừa được ai đó sửa. Vui lòng F5 làm lại!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    TempData["Error"] = "Lỗi đồng bộ: " + ex.Message;
                    return RedirectToAction(nameof(Index));
                }
            }

            var errors = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            TempData["Error"] = "Lỗi nhập liệu: " + errors;
            await PopulateDropdownsAsync(entry);
            return View(entry);
        }

        

        [HttpPost]
        [Authorize(Policy = AppPermissions.Debts.Delete)]
        public async Task<IActionResult> Delete(int id)
        {
            var entry = await _context.CashEntries.FindAsync(id);
            if (entry == null) return Json(new { success = false, message = "Không tìm thấy phiếu!" });

            if (await IsPeriodClosedAsync(entry.TransactionDate))
            {
                TempData["Error"] = $"ℹ️: Kỳ kế toán Tháng {entry.TransactionDate.Month}/{entry.TransactionDate.Year} đã BỊ KHÓA. Không thể xóa phiếu!";
                return RedirectToAction(nameof(Index));
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Lưu lại ID trước khi xóa
                int? rOrderId = entry.OrderId;
                int? rImportId = entry.MaterialImportId;
                int? rPurchaseId = entry.PurchaseOrderId;
                int? rAPId = entry.APInvoiceId;
                int? rExportId = entry.ExportShipmentId;

                // Xóa phân bổ chi phí & xóa phiếu
                var allocations = await _context.CashAllocations.Where(a => a.CashEntryId == entry.Id).ToListAsync();
                if (allocations.Any()) _context.CashAllocations.RemoveRange(allocations);

                _context.CashEntries.Remove(entry);
                await _context.SaveChangesAsync();

                // 🔥 PHỤC HỒI CÔNG NỢ SAU KHI XÓA
                if (rOrderId.HasValue) await UpdateOrderPaymentStatus(rOrderId.Value);
                if (rImportId.HasValue) await UpdateImportPaymentStatus(rImportId.Value);
                if (rPurchaseId.HasValue) await UpdatePurchaseOrderPaymentStatus(rPurchaseId.Value);
                if (rAPId.HasValue) await UpdateAPInvoicePaymentStatus(rAPId.Value);
                if (rExportId.HasValue) await UpdateExportShipmentPaymentStatus(rExportId.Value);

                await transaction.CommitAsync();
                TempData["Success"] = "Đã xóa phiếu và phục hồi công nợ cho chứng từ gốc!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = "Lỗi xử lý: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        /*
        // ==========================================
        // CÁC HÀM DELETE, PRINT, EXCEL GIỮ NGUYÊN
        // ==========================================
        [HttpPost]
        [Authorize(Policy = AppPermissions.Debts.Delete)]

        public async Task<IActionResult> Delete(int id)
        {
            var entry = await _context.CashEntries.FindAsync(id);
            if (entry != null)
            {
                if (await IsPeriodClosedAsync(entry.TransactionDate))
                {
                    TempData["Error"] = $"ℹ️: Kỳ kế toán Tháng {entry.TransactionDate.Month}/{entry.TransactionDate.Year} đã BỊ KHÓA.\n Không thể xóa phiếu!";
                    return RedirectToAction(nameof(Index));
                }

                int? relatedOrderId = entry.OrderId;
                int? importId = entry.MaterialImportId;

                _context.CashEntries.Remove(entry);
                await _context.SaveChangesAsync();

                if (relatedOrderId.HasValue) await UpdateOrderPaymentStatus(relatedOrderId.Value);
                if (importId.HasValue) await UpdateImportPaymentStatus(importId.Value);

                TempData["Success"] = "Đã xóa phiếu!";
            }
            return RedirectToAction(nameof(Index));
        }
        */


        [Authorize(Policy = AppPermissions.Debts.Create)]
        public async Task<IActionResult> Print(int id)
        {
            var item = await _context.CashEntries.FindAsync(id);
            if (item == null) return NotFound();

            var user = await _userManager.FindByNameAsync(item.CreatedBy);
            ViewBag.CreatorFullName = user?.FullName ?? item.CreatedBy;

            return View(item);
        }

        // ==========================================
        // 6. EXPORT EXCEL
        // ==========================================
        [Authorize(Policy = AppPermissions.Debts.Create)]
        public async Task<IActionResult> ExportExcel(DateTime? fromDate, DateTime? toDate, PaymentMethod? method)
        {
            var f = fromDate ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var t = toDate ?? DateTime.Now;
            var tEnd = t.Date.AddDays(1).AddTicks(-1);

            var baseQuery = _context.CashEntries.Include(x => x.CostCategory).AsQueryable();

            if (method.HasValue) baseQuery = baseQuery.Where(x => x.Method == method.Value);

            var allData = await baseQuery.OrderBy(x => x.TransactionDate).ToListAsync();

            var prevData = allData.Where(x => x.TransactionDate < f).ToList();
            decimal runningBalance = prevData.Where(x => x.Type == TransactionType.Receipt).Sum(x => x.Amount)
                                   - prevData.Where(x => x.Type == TransactionType.Payment).Sum(x => x.Amount);

            var reportData = allData.Where(x => x.TransactionDate >= f && x.TransactionDate <= tEnd).ToList();

            string title = method == PaymentMethod.Cash ? "SỔ QUỸ TIỀN MẶT" : (method == PaymentMethod.Transfer ? "SỔ QUỸ NGÂN HÀNG" : "SỔ QUỸ TỔNG HỢP");
            string fileName = method == PaymentMethod.Cash ? "SoQuy_TienMat" : (method == PaymentMethod.Transfer ? "SoQuy_NganHang" : "SoQuy_TongHop");

            using (var wb = new XLWorkbook())
            {
                var ws = wb.Worksheets.Add("SoQuy");

                ws.Cell("A1").Value = $"{title} ({f:dd/MM/yyyy} - {t:dd/MM/yyyy})";
                ws.Range("A1:M1").Merge().Style.Font.Bold = true;
                ws.Range("A1:M1").Style.Font.FontSize = 14;
                ws.Range("A1:M1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Range("A1:M1").Style.Fill.BackgroundColor = XLColor.FromHtml("#e2e8f0");

                int hRow = 3;
                ws.Cell(hRow, 1).Value = "Ngày";
                ws.Cell(hRow, 2).Value = "Số CT (PM)";
                ws.Cell(hRow, 3).Value = "Số phiếu (Gốc)";
                ws.Cell(hRow, 4).Value = "Phân loại";
                ws.Cell(hRow, 5).Value = "Chi phí P&L";
                ws.Cell(hRow, 6).Value = "Kỳ (MM/yyyy)";
                ws.Cell(hRow, 7).Value = "Số tháng PB";
                ws.Cell(hRow, 8).Value = "Hình thức";
                ws.Cell(hRow, 9).Value = "Diễn giải";
                ws.Cell(hRow, 10).Value = "Đối tượng";
                ws.Cell(hRow, 11).Value = "Thu";
                ws.Cell(hRow, 12).Value = "Chi";
                ws.Cell(hRow, 13).Value = "Tồn";

                var headerRange = ws.Range(hRow, 1, hRow, 13);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.Yellow;
                headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                int row = 4;
                ws.Cell(row, 9).Value = "Số dư đầu kỳ";
                ws.Cell(row, 9).Style.Font.Italic = true;
                ws.Cell(row, 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws.Cell(row, 13).Value = runningBalance;
                ws.Cell(row, 13).Style.Font.Bold = true;
                ws.Range(row, 1, row, 13).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                row++;

                foreach (var item in reportData)
                {
                    ws.Cell(row, 1).Value = item.TransactionDate.Date;
                    ws.Cell(row, 1).Style.DateFormat.Format = "dd/MM/yyyy";
                    ws.Cell(row, 2).Value = item.VoucherCode;
                    ws.Cell(row, 3).Value = item.PaperVoucherNumber;

                    string catName = item.Category == EntryCategory.Loan ? "Vay/Nợ" : "Kinh doanh";
                    ws.Cell(row, 4).Value = catName;
                    if (item.Category == EntryCategory.Loan) ws.Cell(row, 4).Style.Font.FontColor = XLColor.Red;

                    ws.Cell(row, 5).Value = item.CostCategory?.Name ?? "";
                    ws.Cell(row, 6).Value = item.ForMonth.HasValue ? $"{item.ForMonth:00}/{item.ForYear}" : "";
                    ws.Cell(row, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    ws.Cell(row, 7).Value = item.AllocatedMonths > 0 ? item.AllocatedMonths : 1;
                    ws.Cell(row, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    string methodStr = item.Method == PaymentMethod.Transfer ? "Chuyển khoản" : "Tiền mặt";
                    ws.Cell(row, 8).Value = methodStr;
                    if (item.Method == PaymentMethod.Transfer) ws.Cell(row, 8).Style.Font.FontColor = XLColor.Blue;

                    ws.Cell(row, 9).Value = item.Description;
                    ws.Cell(row, 10).Value = item.TargetName;

                    if (item.Type == TransactionType.Receipt)
                    {
                        ws.Cell(row, 11).Value = item.Amount;
                        runningBalance += item.Amount;
                    }
                    else
                    {
                        ws.Cell(row, 12).Value = item.Amount;
                        runningBalance -= item.Amount;
                    }
                    ws.Cell(row, 13).Value = runningBalance;

                    ws.Range(row, 1, row, 13).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    row++;
                }

                ws.Cell(row, 9).Value = "Số dư cuối kỳ";
                ws.Cell(row, 9).Style.Font.Bold = true;
                ws.Cell(row, 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws.Cell(row, 13).Value = runningBalance;
                ws.Cell(row, 13).Style.Font.Bold = true;
                ws.Cell(row, 13).Style.Fill.BackgroundColor = XLColor.Yellow;

                ws.Column(1).Style.DateFormat.Format = "dd/MM/yyyy";
                ws.Columns("K:M").Style.NumberFormat.Format = "#,##0";
                ws.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{fileName}_{f:ddMM}_{t:ddMM}.xlsx");
                }
            }
        }

        // ==========================================
        // 7. IMPORT EXCEL
        // ==========================================
        [HttpGet]
        public IActionResult Import(PaymentMethod? method)
        {
            var model = new ImportViewModel { TargetMethod = method ?? PaymentMethod.Cash };
            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = AppPermissions.Debts.Delete)]
        public async Task<IActionResult> Import(ImportViewModel model, [FromServices] Microsoft.AspNetCore.Hosting.IWebHostEnvironment env)
        {
            if (!ModelState.IsValid || model.File == null || model.File.Length == 0)
            {
                ModelState.AddModelError("", "Vui lòng chọn file hợp lệ.");
                return View(model);
            }

            // Bắt đầu Transaction để kiểm soát việc Rollback
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (model.Mode == ImportMode.Overwrite)
                {
                    if (!User.IsInRole("Admin"))
                    {
                        ModelState.AddModelError("", "❌ Lỗi bảo mật: Chỉ tài khoản Admin mới có quyền Xóa & Ghi đè dữ liệu.");
                        return View(model);
                    }

                    var oldData = await _context.CashEntries
                        .Where(x => x.Method == model.TargetMethod)
                        .OrderBy(x => x.TransactionDate).ToListAsync();

                    if (oldData.Any())
                    {
                        // Backup dữ liệu cũ (Giữ nguyên logic cũ)
                        string backupFolder = Path.Combine(env.WebRootPath, "backups", "cashbook");
                        if (!Directory.Exists(backupFolder)) Directory.CreateDirectory(backupFolder);

                        string backupFileName = $"Backup_{model.TargetMethod}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                        string backupPath = Path.Combine(backupFolder, backupFileName);

                        using (var wb = new XLWorkbook())
                        {
                            var ws = wb.Worksheets.Add("Backup_Data");
                            ws.Cell(1, 1).Value = "ID"; ws.Cell(1, 2).Value = "Ngày"; ws.Cell(1, 3).Value = "Mã PM";
                            ws.Cell(1, 4).Value = "Số tiền"; ws.Cell(1, 5).Value = "Nội dung";
                            int r = 2;
                            foreach (var item in oldData)
                            {
                                ws.Cell(r, 1).Value = item.Id; ws.Cell(r, 2).Value = item.TransactionDate;
                                ws.Cell(r, 3).Value = item.VoucherCode; ws.Cell(r, 4).Value = item.Amount;
                                ws.Cell(r, 5).Value = item.Description;
                                r++;
                            }
                            wb.SaveAs(backupPath);
                        }

                        var oldEntryIds = oldData.Select(x => x.Id).ToList();
                        await _context.CashAllocations.Where(a => oldEntryIds.Contains(a.CashEntryId)).ExecuteDeleteAsync();
                        await _context.CashEntries.Where(x => x.Method == model.TargetMethod).ExecuteDeleteAsync();
                    }
                }

                var activeCategories = await _context.CostCategories.Where(c => c.IsActive).ToListAsync();
                var validEntries = new List<CashEntry>();
                var errors = new List<string>();

                var existingCodes = new HashSet<string>(await _context.CashEntries.Select(x => x.VoucherCode).ToListAsync());

                using (var stream = new MemoryStream())
                {
                    await model.File.CopyToAsync(stream);
                    using (var wb = new XLWorkbook(stream))
                    {
                        var worksheet = wb.Worksheet(1);
                        var rows = worksheet.RowsUsed().Skip(1);
                        int rowIndex = 2;

                        foreach (var row in rows)
                        {
                            try
                            {
                                // Bỏ qua dòng rác/trống hoàn toàn không tính là lỗi
                                if (row.Cell(1).IsEmpty() && row.Cell(10).IsEmpty() && row.Cell(11).IsEmpty())
                                {
                                    rowIndex++;
                                    continue;
                                }

                                var entry = ParseCashEntry_Specific(row, model.TargetMethod, activeCategories);

                                // KIỂM TRA LỖI 1: Trùng lặp Mã PM
                                if (!string.IsNullOrEmpty(entry.VoucherCode) && model.Mode == ImportMode.Append && existingCodes.Contains(entry.VoucherCode))
                                {
                                    errors.Add($"Dòng {rowIndex}: Mã PM '{entry.VoucherCode}' đã tồn tại trong hệ thống.");
                                }
                                // KIỂM TRA LỖI 2: Dữ liệu bắt buộc bị thiếu
                                else if (entry.Amount <= 0)
                                {
                                    errors.Add($"Dòng {rowIndex}: Số tiền không hợp lệ hoặc bằng 0.");
                                }
                                else
                                {
                                    if (!string.IsNullOrEmpty(entry.VoucherCode))
                                    {
                                        existingCodes.Add(entry.VoucherCode);
                                    }

                                    entry.CreatedBy = User.Identity?.Name ?? "ImportSystem";
                                    entry.CreatedAt = DateTime.Now;
                                    validEntries.Add(entry);
                                }
                            }
                            catch (Exception ex)
                            {
                                // KIỂM TRA LỖI 3: Sai định dạng (chữ thay vì số, sai ngày tháng...)
                                errors.Add($"Dòng {rowIndex}: Lỗi định dạng dữ liệu ({ex.Message})");
                            }
                            rowIndex++;
                        }
                    }
                }

                // 🔥 LOGIC KIỂM SOÁT ALL-OR-NOTHING
                if (errors.Any())
                {
                    // Nếu CÓ BẤT KỲ LỖI NÀO -> Hủy toàn bộ tiến trình
                    await transaction.RollbackAsync();

                    // Lấy tối đa 10 lỗi đầu tiên để hiển thị cho người dùng đỡ bị tràn màn hình
                    var errorMsg = string.Join("<br/>", errors.Take(10));
                    if (errors.Count > 10) errorMsg += $"<br/>... và {errors.Count - 10} lỗi khác.";

                    ModelState.AddModelError("", $"❌ Dữ liệu có lỗi. Đã hủy toàn bộ quá trình Import để bảo vệ an toàn hệ thống. Vui lòng sửa file Excel và thử lại:<br/>{errorMsg}");
                    return View(model);
                }
                else if (validEntries.Any())
                {
                    // CHỈ LƯU KHI HOÀN TOÀN KHÔNG CÓ LỖI NÀO
                    await _context.CashEntries.AddRangeAsync(validEntries);
                    await _context.SaveChangesAsync();

                    // Thực hiện phân bổ P&L
                    foreach (var entry in validEntries)
                    {
                        await GenerateAllocationsAsync(entry);
                    }
                    await _context.SaveChangesAsync();

                    // Xác nhận hoàn tất Transaction
                    await transaction.CommitAsync();

                    string typeName = model.TargetMethod == PaymentMethod.Cash ? "TIỀN MẶT" : "NGÂN HÀNG";
                    string msg = $"✅ Đã Import thành công toàn bộ {validEntries.Count} dòng vào sổ {typeName}.";
                    if (model.Mode == ImportMode.Overwrite) msg += " (Đã backup & thay thế dữ liệu cũ).";

                    TempData["Success"] = msg;
                    return RedirectToAction(nameof(Index), new { method = model.TargetMethod });
                }
                else
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("", "⚠️ File Excel không có dữ liệu hợp lệ nào để Import.");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("", $"❌ Lỗi hệ thống khi Import: {ex.Message}");
                return View(model);
            }
        }

        private CashEntry ParseCashEntry_Specific(ClosedXML.Excel.IXLRow row, PaymentMethod targetMethod, List<CostCategory> activeCategories)
        {
            var entry = new CashEntry();
            entry.Method = targetMethod;

            entry.TransactionDate = GetSafeDate(row.Cell(1));
            entry.ReportDate = entry.TransactionDate;

            entry.VoucherCode = row.Cell(2).GetValue<string>()?.Trim();
            entry.PaperVoucherNumber = row.Cell(3).GetValue<string>()?.Trim();

            string catStr = row.Cell(4).GetValue<string>();
            entry.Category = IsMatch(catStr, "vay", "nợ") ? EntryCategory.Loan : EntryCategory.Business;

            string periodStr = row.Cell(6).GetValue<string>()?.Trim();
            if (!string.IsNullOrEmpty(periodStr) && periodStr.Contains("/"))
            {
                var parts = periodStr.Split('/');
                if (parts.Length >= 2 && int.TryParse(parts[0], out int m) && int.TryParse(parts[1], out int y))
                {
                    entry.ForMonth = m; entry.ForYear = y;
                }
            }

            int allocated = (int)GetSafeDecimal(row.Cell(7));
            entry.AllocatedMonths = allocated > 0 ? allocated : 1;

            entry.Description = row.Cell(8).GetValue<string>()?.Trim();
            entry.TargetName = row.Cell(9).GetValue<string>()?.Trim();

            // Xác định số tiền Thu / Chi trước
            decimal thu = GetSafeDecimal(row.Cell(10));
            decimal chi = GetSafeDecimal(row.Cell(11));

            if (thu > 0) { entry.Type = TransactionType.Receipt; entry.Amount = thu; }
            else { entry.Type = TransactionType.Payment; entry.Amount = chi; }

            // 🔥 TÍNH NĂNG MỚI: NHẬN DIỆN DANH MỤC THÔNG MINH (Chống mất báo cáo P&L)
            string pnlName = row.Cell(5).GetValue<string>()?.Trim();
            if (!string.IsNullOrEmpty(pnlName))
            {
                // Khớp tên tuyệt đối (Bỏ qua hoa thường và khoảng trắng thừa)
                var matchedCat = activeCategories.FirstOrDefault(c => c.Name.Trim().Equals(pnlName, StringComparison.OrdinalIgnoreCase));
                if (matchedCat != null)
                {
                    entry.CostCategoryId = matchedCat.Id;
                }
                else
                {
                    // Nếu Excel gõ sai tên, tự động tống vào "Chi phí khác" (hoặc lấy dòng đầu tiên)
                    var fallbackCat = activeCategories.FirstOrDefault(c => c.Name.ToLower().Contains("khác")) ?? activeCategories.FirstOrDefault();
                    entry.CostCategoryId = fallbackCat?.Id;
                }
            }
            // Chỉ áp dụng tự động đưa vào danh mục mặc định nếu nó là PHIẾU CHI Nội bộ (Không phải trả nợ)
            else if (entry.Type == TransactionType.Payment && entry.Category == EntryCategory.Business)
            {
                var fallbackCat = activeCategories.FirstOrDefault(c => c.Name.ToLower().Contains("khác")) ?? activeCategories.FirstOrDefault();
                entry.CostCategoryId = fallbackCat?.Id;
            }

            // 🔥 FIX: Sinh mã an toàn tuyệt đối, chống trùng lặp trong vòng lặp tốc độ cao
            if (string.IsNullOrEmpty(entry.VoucherCode))
            {
                string prefix = entry.Type == TransactionType.Receipt ? "PT" : "PC";

                // Sử dụng Guid để lấy ngẫu nhiên 4 ký tự (chữ + số) đảm bảo không bao giờ trùng nhau
                string uniqueSuffix = Guid.NewGuid().ToString("N").Substring(0, 4).ToUpper();

                entry.VoucherCode = $"{prefix}-AUTO-{DateTime.Now:MMddHHmmss}-{uniqueSuffix}";
            }

            return entry;
        }

        [Authorize(Policy = AppPermissions.Debts.Create)]
        public async Task<IActionResult> DownloadTemplate(PaymentMethod method)
        {
            var costCategoryNames = await _context.CostCategories.Where(c => c.IsActive).Select(c => c.Name).ToListAsync();

            using (var wb = new XLWorkbook())
            {
                string sheetName = method == PaymentMethod.Cash ? "Mau_TienMat" : "Mau_NganHang";
                var ws = wb.Worksheets.Add(sheetName);

                var hiddenSheet = wb.Worksheets.Add("Data_An_System");
                hiddenSheet.Hide();
                for (int i = 0; i < costCategoryNames.Count; i++)
                {
                    hiddenSheet.Cell(i + 1, 1).Value = costCategoryNames[i];
                }

                ws.Cell(1, 1).Value = "Ngày chứng từ *";
                ws.Cell(1, 2).Value = "Mã PM (Bỏ trống tự sinh)";
                ws.Cell(1, 3).Value = "Số phiếu (Gốc)";
                ws.Cell(1, 4).Value = "Phân loại (Vay/KD)";
                ws.Cell(1, 5).Value = "Tên Loại Chi Phí (Click chọn) ▼";
                ws.Cell(1, 6).Value = "Kỳ (MM/yyyy)";
                ws.Cell(1, 7).Value = "Số tháng PB";
                ws.Cell(1, 8).Value = "Diễn giải";
                ws.Cell(1, 9).Value = "Đối tượng";
                ws.Cell(1, 10).Value = "Thu *";
                ws.Cell(1, 11).Value = "Chi *";

                var header = ws.Range("A1:K1");
                header.Style.Font.Bold = true;
                header.Style.Fill.BackgroundColor = method == PaymentMethod.Cash ? XLColor.LightGreen : XLColor.LightBlue;
                header.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                if (costCategoryNames.Any())
                {
                    var validation = ws.Range("E2:E1000").CreateDataValidation();
                    validation.List(hiddenSheet.Range(1, 1, costCategoryNames.Count, 1));
                    validation.ErrorStyle = XLErrorStyle.Stop;
                    validation.ErrorTitle = "Sai danh mục P&L";
                    validation.ErrorMessage = "Vui lòng chọn tên chi phí từ danh sách thả xuống để hệ thống nhận diện đúng!";
                }

                ws.Cell(2, 1).Value = DateTime.Today;
                ws.Cell(2, 1).Style.DateFormat.Format = "dd/MM/yyyy";
                ws.Cell(2, 4).Value = "KD";
                ws.Cell(2, 5).Value = costCategoryNames.FirstOrDefault() ?? "Chi phí mặt bằng";
                ws.Cell(2, 6).Value = $"{DateTime.Now.Month:00}/{DateTime.Now.Year}";
                ws.Cell(2, 7).Value = 6;
                ws.Cell(2, 8).Value = "Thanh toán tiền thuê phân bổ";
                ws.Cell(2, 9).Value = "Nguyễn Văn A";
                ws.Cell(2, 11).Value = 6000000;

                ws.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Mau_Import_{sheetName}.xlsx");
                }
            }
        }

        private decimal GetSafeDecimal(IXLCell cell)
        {
            if (cell.IsEmpty()) return 0;
            if (cell.DataType == XLDataType.Number) return Convert.ToDecimal(cell.GetDouble());
            string val = cell.GetValue<string>().Replace(",", "").Replace(".", "");
            if (decimal.TryParse(val, out decimal result)) return result;
            return 0;
        }

        private DateTime GetSafeDate(IXLCell cell)
        {
            if (cell.DataType == XLDataType.DateTime) return cell.GetDateTime();
            string dateStr = cell.GetValue<string>();
            string[] formats = { "dd/MM/yyyy", "d/M/yyyy", "yyyy-MM-dd", "dd-MM-yyyy" };
            if (DateTime.TryParseExact(dateStr, formats, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var date))
                return date;
            return DateTime.Now;
        }

        private bool IsMatch(string input, params string[] keywords)
        {
            if (string.IsNullOrEmpty(input)) return false;
            return keywords.Any(k => input.Contains(k, StringComparison.OrdinalIgnoreCase));
        }

        private async Task<bool> IsPeriodClosedAsync(DateTime date)
        {
            return await _context.FinancialPeriods
                .AnyAsync(p => p.Month == date.Month && p.Year == date.Year && p.IsClosed);
        }

        [HttpGet]
        [Authorize(Policy = AppPermissions.Debts.View)]
        public async Task<IActionResult> SearchLive(DateTime? fromDate, DateTime? toDate, string keyword, PaymentMethod? method)
        {
            var f = fromDate ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var t = toDate ?? f.AddMonths(1).AddDays(-1);
            var tEnd = t.Date.AddDays(1).AddTicks(-1);

            var baseQuery = _context.CashEntries.AsQueryable();

            if (method.HasValue) baseQuery = baseQuery.Where(x => x.Method == method.Value);

            var prevTrans = await baseQuery
                .Where(x => x.TransactionDate < f)
                .Select(x => new { x.Type, x.Amount })
                .ToListAsync();

            decimal openBalance = prevTrans.Where(x => x.Type == TransactionType.Receipt).Sum(x => x.Amount)
                                - prevTrans.Where(x => x.Type == TransactionType.Payment).Sum(x => x.Amount);

            var query = baseQuery
                .Include(x => x.Order)
                .Include(x => x.CostCategory)
                .Where(x => x.TransactionDate >= f && x.TransactionDate <= tEnd);

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(x => x.VoucherCode.Contains(keyword)
                                      || x.Description.Contains(keyword)
                                      || x.TargetName.Contains(keyword));
            }

            var transactions = await query.OrderBy(x => x.TransactionDate).ToListAsync();

            ViewBag.OpenBalance = openBalance;
            ViewBag.CurrentMethod = method;

            return PartialView("_CashEntryList", transactions);
        }

        // ==========================================
        // TỰ ĐỘNG PHÂN BỔ VÀO BẢNG CASH ALLOCATION
        // ==========================================
        private async Task GenerateAllocationsAsync(CashEntry entry)
        {
            // 1. Xóa các phân bổ cũ nếu đang Edit
            var oldAllocations = await _context.CashAllocations.Where(a => a.CashEntryId == entry.Id).ToListAsync();
            if (oldAllocations.Any()) _context.CashAllocations.RemoveRange(oldAllocations);

            // 2. Chỉ phân bổ nếu là Phiếu Chi và có chọn Danh mục P&L
            if (!entry.CostCategoryId.HasValue || entry.Type != TransactionType.Payment) return;

            int months = (entry.AllocatedMonths ?? 1) > 0 ? (entry.AllocatedMonths ?? 1) : 1;
            decimal amountPerMonth = entry.Amount / months;

            // Lấy tháng bắt đầu (Nếu Kế toán quên chọn thì lấy theo ngày lập phiếu)
            int startMonth = entry.ForMonth ?? entry.TransactionDate.Month;
            int startYear = entry.ForYear ?? entry.TransactionDate.Year;

            // 3. Chạy vòng lặp tạo dữ liệu cho từng tháng
            for (int i = 0; i < months; i++)
            {
                int targetMonth = startMonth + i;
                int targetYear = startYear;

                // Xử lý nhảy năm nếu phân bổ vắt qua tháng 12
                while (targetMonth > 12)
                {
                    targetMonth -= 12;
                    targetYear++;
                }

                _context.CashAllocations.Add(new CashAllocation
                {
                    CashEntryId = entry.Id,
                    TargetMonth = targetMonth,
                    TargetYear = targetYear,
                    AllocatedAmount = amountPerMonth,
                    Note = months > 1 ? $"Phân bổ tháng {i + 1}/{months}" : "Trực tiếp 1 lần"
                });
            }
        }


    }
}