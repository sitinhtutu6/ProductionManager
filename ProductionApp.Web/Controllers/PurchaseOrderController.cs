using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductionApp.Web.Constants;
using ProductionManager.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ProductionApp.Web.Controllers
{
    [Authorize]
    public class PurchaseOrderController : Controller
    {
        private readonly AppDbContext _context;
        public PurchaseOrderController(AppDbContext context) { _context = context; }

        // ==========================================
        // 1. DANH SÁCH ĐƠN HÀNG (MASTER - DETAIL)
        // ==========================================
        [Authorize(Policy = AppPermissions.Orders.View)]
        public async Task<IActionResult> Index()
        {
            var list = await _context.PurchaseOrders
                .Include(x => x.Details)
                    .ThenInclude(d => d.WarehouseItem)
                .OrderByDescending(x => x.OrderDate)
                .ToListAsync();
            return View(list);
        }

        // ==========================================
        // 2. TẠO ĐƠN HÀNG MỚI (GET)
        // ==========================================
        [Authorize(Policy = AppPermissions.Orders.View)]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _context.WarehouseCategories.OrderBy(c => c.Name).ToListAsync();

            ViewBag.ItemsJson = System.Text.Json.JsonSerializer.Serialize(await _context.WarehouseItems.Select(x => new {
                x.Id,
                x.Code,
                x.Name,
                x.Unit,
                Price = x.CostPrice,
                CategoryId = x.CategoryId
            }).ToListAsync());

            var suppliers = await _context.Customers
                .Where(c => c.Type == PartnerType.Supplier || c.Type == PartnerType.Both)
                .Select(c => new { c.Id, Text = $"[{c.CustomerCode}] {c.CompanyName}" })
                .ToListAsync();
            ViewBag.SuppliersJson = System.Text.Json.JsonSerializer.Serialize(suppliers);

            // 🔥 LOGIC MỚI: Lấy danh sách Đơn hàng sản xuất (Chỉ lấy trạng thái đang sản xuất)
            var activeOrders = await _context.Orders
                .Where(o => o.Status == OrderStatus.InProduction)
                .Select(o => new {
                    Id = o.Id,
                    Code = o.OrderCode // Chỉnh lại 'OrderCode' nếu model của Sếp tên khác
                })
                .ToListAsync();
            ViewBag.OrdersJson = System.Text.Json.JsonSerializer.Serialize(activeOrders);

            ViewBag.AutoCode = $"PO-{DateTime.Now:yyMMdd}-{new Random().Next(100, 999)}";
            return View();
        }

        // ==========================================
        // 3. LƯU ĐƠN HÀNG (POST API)
        // ==========================================
        [HttpPost]
        [Authorize(Policy = AppPermissions.Orders.Create)]
        public async Task<IActionResult> Create([FromBody] PurchaseOrder model)
        {
            if (model.Details == null || !model.Details.Any()) return Json(new { success = false, message = "Chưa có vật tư" });

            model.Status = POStatus.Open;

            model.TotalAmount = model.Details.Sum(d => (decimal)d.QuantityOrdered * d.UnitPrice);
            model.VATAmount = model.TotalAmount * (decimal)(model.VATPercent / 100.0);
            model.FinalTotal = model.TotalAmount + model.VATAmount;
            model.RemainingAmount = model.FinalTotal - model.DepositAmount;

            if (model.RemainingAmount <= 0 && model.FinalTotal > 0) model.PaymentStatusPO = PaymentStatusPO.Paid;
            else if (model.DepositAmount > 0) model.PaymentStatusPO = PaymentStatusPO.Deposited;
            else model.PaymentStatusPO = PaymentStatusPO.Unpaid;

            // Xử lý làm sạch Details trước khi lưu
            foreach (var d in model.Details)
            {
                d.QuantityReceived = 0;
                d.QuantityBad = 0;
                d.IsFinished = false;
                // LinkedOrderId và LinkedOrderCode đã được JSON Body tự động map vào biến d
            }

            _context.Add(model);
            await _context.SaveChangesAsync();

            if (model.DepositAmount > 0)
            {
                var phieuChi = new CashEntry
                {
                    VoucherCode = $"PC-{DateTime.Now:yyMMdd}-{Guid.NewGuid().ToString("N").Substring(0, 4).ToUpper()}",
                    PaperVoucherNumber = model.POCode,
                    TransactionDate = DateTime.Now,
                    ReportDate = DateTime.Now,
                    Type = TransactionType.Payment,
                    Method = PaymentMethod.Transfer,
                    Category = EntryCategory.Business,
                    Amount = model.DepositAmount,
                    TargetName = model.SupplierName,
                    Description = $"Đặt cọc cho đơn đặt hàng {model.POCode}",
                    CreatedBy = User.Identity?.Name ?? "System",
                    CreatedAt = DateTime.Now
                };
                _context.CashEntries.Add(phieuChi);
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true });
        }

        // ==========================================
        // 4. HIỂN THỊ FORM EDIT (GET)
        // ==========================================
        [HttpGet]
        [Authorize(Policy = AppPermissions.Orders.Edit)]
        public async Task<IActionResult> Edit(int id)
        {
            var po = await _context.PurchaseOrders
                .Include(x => x.Details).ThenInclude(d => d.WarehouseItem)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (po == null) return NotFound();

            if (po.Status == POStatus.Completed || po.Status == POStatus.Cancelled)
            {
                TempData["Error"] = "Không thể sửa đơn hàng đã hoàn thành hoặc đã hủy.";
                return RedirectToAction("Details", new { id });
            }

            ViewBag.Categories = await _context.WarehouseCategories.OrderBy(c => c.Name).ToListAsync();

            ViewBag.ItemsJson = System.Text.Json.JsonSerializer.Serialize(await _context.WarehouseItems.Select(x => new {
                x.Id,
                x.Code,
                x.Name,
                x.Unit,
                Price = x.CostPrice,
                CategoryId = x.CategoryId
            }).ToListAsync());

            // 🔥 LOGIC MỚI: Truyền thêm LinkedOrderId và LinkedOrderCode cho giao diện Edit
            ViewBag.ExistingDetailsJson = System.Text.Json.JsonSerializer.Serialize(po.Details.Select(x => new {
                x.WarehouseItemId,
                x.QuantityOrdered,
                x.UnitPrice,
                LinkedOrderId = x.LinkedOrderId,
                LinkedOrderCode = x.LinkedOrderCode
            }));

            var suppliers = await _context.Customers
                .Where(c => c.Type == PartnerType.Supplier || c.Type == PartnerType.Both)
                .Select(c => new { c.Id, Text = $"[{c.CustomerCode}] {c.CompanyName}" })
                .ToListAsync();
            ViewBag.SuppliersJson = System.Text.Json.JsonSerializer.Serialize(suppliers);

            // 🔥 LOGIC MỚI: Lấy danh sách Đơn hàng sản xuất (Chỉ lấy trạng thái đang sản xuất)
            var activeOrders = await _context.Orders
                .Where(o => o.Status == OrderStatus.InProduction)
                .Select(o => new {
                    Id = o.Id,
                    Code = o.OrderCode
                })
                .ToListAsync();
            ViewBag.OrdersJson = System.Text.Json.JsonSerializer.Serialize(activeOrders);

            return View(po);
        }

        // ==========================================
        // 5. LƯU THAY ĐỔI (POST API)
        // ==========================================
        [HttpPost]
        [Authorize(Policy = AppPermissions.Orders.Edit)]
        public async Task<IActionResult> Edit(int id, [FromBody] PurchaseOrder model)
        {
            if (id != model.Id) return BadRequest("Mismatched ID");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var poInDb = await _context.PurchaseOrders.Include(x => x.Details).FirstOrDefaultAsync(x => x.Id == id);
                if (poInDb == null) return NotFound("Không tìm thấy đơn hàng.");

                if (poInDb.Status == POStatus.Completed || poInDb.Status == POStatus.Cancelled)
                    return Json(new { success = false, message = "Không thể sửa đơn hàng đã Hoàn thành hoặc đã Hủy." });

                poInDb.OrderDate = model.OrderDate;
                poInDb.SupplierId = model.SupplierId;
                poInDb.SupplierName = model.SupplierName;
                poInDb.Type = model.Type;
                poInDb.Note = model.Note;
                poInDb.VATPercent = model.VATPercent;
                poInDb.DepositAmount = model.DepositAmount;

                bool hasImported = poInDb.Details.Any(x => x.QuantityReceived > 0);
                string message = "Cập nhật thành công!";
                var activeDetails = poInDb.Details;

                if (hasImported)
                {
                    message = "Đã cập nhật thông tin chung. (Chi tiết vật tư không thay đổi do đơn hàng này đang nhập dang dở).";
                }
                else
                {
                    if (poInDb.Details != null && poInDb.Details.Any())
                        _context.PurchaseOrderDetails.RemoveRange(poInDb.Details);

                    if (model.Details != null && model.Details.Any())
                    {
                        foreach (var item in model.Details)
                        {
                            item.Id = 0;
                            item.PurchaseOrderId = poInDb.Id;
                            item.QuantityReceived = 0;
                            item.QuantityBad = 0;
                            item.IsFinished = false;

                            // LinkedOrderId và LinkedOrderCode tự động có từ payload
                            _context.PurchaseOrderDetails.Add(item);
                        }
                    }
                    activeDetails = model.Details;
                }

                poInDb.TotalAmount = activeDetails?.Sum(d => (decimal)d.QuantityOrdered * d.UnitPrice) ?? 0;
                poInDb.VATAmount = poInDb.TotalAmount * (decimal)(poInDb.VATPercent / 100.0);
                poInDb.FinalTotal = poInDb.TotalAmount + poInDb.VATAmount;
                poInDb.RemainingAmount = poInDb.FinalTotal - poInDb.DepositAmount;

                if (poInDb.RemainingAmount <= 0 && poInDb.FinalTotal > 0)
                    poInDb.PaymentStatusPO = PaymentStatusPO.Paid;
                else if (poInDb.DepositAmount > 0)
                    poInDb.PaymentStatusPO = PaymentStatusPO.Deposited;
                else
                    poInDb.PaymentStatusPO = PaymentStatusPO.Unpaid;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { success = true, message = message });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        // ==========================================
        // 6. XEM CHI TIẾT
        // ==========================================
        public async Task<IActionResult> Details(int id)
        {
            var po = await _context.PurchaseOrders
               .Include(x => x.Details).ThenInclude(d => d.WarehouseItem)
               .FirstOrDefaultAsync(x => x.Id == id);
            return View(po);
        }

        // ==========================================
        // 7. CHỐT ĐƠN (FORCE CLOSE)
        // ==========================================
        [HttpPost]
        [Authorize(Policy = AppPermissions.Orders.Edit)]
        public async Task<IActionResult> ForceClose(int id)
        {
            var po = await _context.PurchaseOrders
                .Include(x => x.Details)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (po == null) return Json(new { success = false, message = "Không tìm thấy đơn hàng." });

            if (po.Status == POStatus.Completed || po.Status == POStatus.Cancelled)
            {
                return Json(new { success = false, message = "Đơn hàng này đã đóng từ trước." });
            }

            po.Status = POStatus.Completed;

            foreach (var detail in po.Details)
            {
                detail.IsFinished = true;
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Đã chốt đơn hàng thành công!" });
        }

        // ==========================================
        // 8. CHỐT CÔNG NỢ (GOM NHIỀU PO THÀNH 1 HÓA ĐƠN)
        // ==========================================
        [HttpGet]
        [Authorize(Policy = AppPermissions.Orders.Edit)]
        public async Task<IActionResult> DebtFinalization()
        {
            var suppliers = await _context.Customers
                .Where(c => c.Type == PartnerType.Supplier || c.Type == PartnerType.Both)
                .Select(c => new { c.Id, Text = $"[{c.CustomerCode}] {c.CompanyName}" })
                .ToListAsync();

            ViewBag.SuppliersJson = System.Text.Json.JsonSerializer.Serialize(suppliers);
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetUnfinalizedPOs(int supplierId)
        {
            var posFromDb = await _context.PurchaseOrders
                .Where(p => p.SupplierId == supplierId && !p.IsDebtFinalized && p.Status != POStatus.Cancelled)
                .OrderByDescending(p => p.Id)
                .Select(p => new {
                    p.Id,
                    p.POCode,
                    p.OrderDate,
                    p.FinalTotal,
                    p.DepositAmount,
                    p.RemainingAmount
                })
                .ToListAsync();

            var pos = posFromDb.Select(p => new {
                p.Id,
                p.POCode,
                OrderDate = p.OrderDate.ToString("dd/MM/yyyy"),
                p.FinalTotal,
                p.DepositAmount,
                p.RemainingAmount
            }).ToList();

            return Json(pos);
        }

        public class DebtFinalizeRequest
        {
            public string InvoiceNumber { get; set; }
            public List<int> POIds { get; set; }
        }

        [HttpPost]
        [Authorize(Policy = AppPermissions.Orders.Edit)]
        public async Task<IActionResult> ProcessDebtFinalization([FromBody] DebtFinalizeRequest req)
        {
            if (string.IsNullOrEmpty(req.InvoiceNumber)) return Json(new { success = false, message = "Vui lòng nhập Số Hóa Đơn." });
            if (req.POIds == null || !req.POIds.Any()) return Json(new { success = false, message = "Vui lòng chọn ít nhất 1 đơn hàng." });

            foreach (var id in req.POIds)
            {
                var po = await _context.PurchaseOrders.FindAsync(id);
                if (po != null)
                {
                    po.InvoiceNumber = req.InvoiceNumber;
                    po.IsDebtFinalized = true;
                    po.DebtFinalizedDate = DateTime.Now;
                }
            }
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã chốt công nợ thành công!", redirectUrl = "/Debt/Index?type=2" });
        }

        [HttpPost]
        [Authorize(Policy = AppPermissions.Orders.Delete)]
        public async Task<IActionResult> Delete(int id)
        {
            var po = await _context.PurchaseOrders.Include(p => p.Details).FirstOrDefaultAsync(p => p.Id == id);
            if (po == null) return Json(new { success = false, message = "Không tìm thấy đơn hàng!" });

            if (po.IsDebtFinalized)
                return Json(new { success = false, message = "LỖI BẢO MẬT: Đơn hàng này ĐÃ ĐƯỢC CHỐT CÔNG NỢ. Vui lòng hủy chốt nợ trước khi xóa!" });

            if (po.DepositAmount > 0)
                return Json(new { success = false, message = "LỖI BẢO MẬT: Đơn hàng này ĐÃ CÓ PHIẾU CHI thanh toán. Vui lòng xóa phiếu chi trong sổ quỹ!" });

            _context.PurchaseOrderDetails.RemoveRange(po.Details);
            _context.PurchaseOrders.Remove(po);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Đã xóa đơn đặt hàng!" });
        }

        // ==========================================
        // 9. TỔNG QUAN CHI PHÍ (DASHBOARD) - FIX SỐ LƯỢNG & GIÁ 0Đ
        // ==========================================
        [HttpGet]
        [Authorize(Policy = AppPermissions.Orders.View)]
        public async Task<IActionResult> CostOverview(DateTime? fromDate, DateTime? toDate)
        {
            var start = fromDate ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var end = toDate ?? start.AddMonths(1).AddDays(-1);

            ViewBag.FromDate = start.ToString("yyyy-MM-dd");
            ViewBag.ToDate = end.ToString("yyyy-MM-dd");

            var pos = await _context.PurchaseOrders
                .Include(p => p.Details).ThenInclude(d => d.WarehouseItem)
                .Where(p => p.OrderDate >= start && p.OrderDate <= end && p.Status != POStatus.Cancelled)
                .ToListAsync();

            // 1. Thống kê theo NCC (Tính cả Tổng tiền & Tổng số lượng)
            var supplierCosts = pos
                .GroupBy(p => p.SupplierName)
                .Select(g => new {
                    Name = string.IsNullOrEmpty(g.Key) ? "Nhà cung cấp lẻ" : g.Key,
                    TotalAmount = g.Sum(p => p.TotalAmount),
                    TotalQuantity = g.SelectMany(p => p.Details).Sum(d => d.QuantityOrdered),
                    SubText = $"{g.Count()} đơn hàng"
                })
                .OrderByDescending(x => x.TotalAmount).ThenByDescending(x => x.TotalQuantity)
                .ToList();

            // 2. Thống kê theo Vật tư (Tính cả Tổng tiền & Tổng số lượng)
            var allDetails = pos.SelectMany(p => p.Details).Where(d => d.WarehouseItem != null).ToList();
            var productCosts = allDetails
                .GroupBy(d => new { d.WarehouseItemId, d.WarehouseItem.Name, d.WarehouseItem.Unit })
                .Select(g => new {
                    Name = g.Key.Name,
                    TotalAmount = g.Sum(d => (decimal)d.QuantityOrdered * d.UnitPrice),
                    TotalQuantity = g.Sum(d => d.QuantityOrdered),
                    Unit = string.IsNullOrEmpty(g.Key.Unit) ? "Cái" : g.Key.Unit,
                    SubText = $"ĐVT: {(string.IsNullOrEmpty(g.Key.Unit) ? "Cái" : g.Key.Unit)}"
                })
                .OrderByDescending(x => x.TotalAmount).ThenByDescending(x => x.TotalQuantity)
                .ToList();

            ViewBag.SupplierCostsJson = System.Text.Json.JsonSerializer.Serialize(supplierCosts);
            ViewBag.ProductCostsJson = System.Text.Json.JsonSerializer.Serialize(productCosts);
            ViewBag.TotalCost = pos.Sum(p => p.TotalAmount);
            ViewBag.TotalQty = allDetails.Sum(d => d.QuantityOrdered);

            return View();
        }


        // ==========================================
        // 10. IMPORT EXCEL: THEO SỔ NHẬT KÝ GIAO DỊCH (LEDGER)
        // ==========================================

        public class LedgerImportRow
        {
            public string TransCode { get; set; } // PX-0001 hoặc PN-0001
            public string TransDate { get; set; }
            public string ItemCode { get; set; }
            public string ItemName { get; set; }
            public string OrderCode { get; set; } // Số đơn hàng (VD: CTV-15.26)
            public string Unit { get; set; }
            public string Quantity { get; set; }
            public string SupplierName { get; set; } // Để tính công nợ
            public string UnitPrice { get; set; } // Để tính công nợ
            public string Note { get; set; }
        }

        public class LedgerImportResult : LedgerImportRow
        {
            public int RowIndex { get; set; }
            public bool IsValid { get; set; }
            public List<string> Errors { get; set; } = new List<string>();

            public int ItemId { get; set; }
            public int? SupplierId { get; set; }
            public int? LinkedOrderId { get; set; }
            public DateTime ParsedDate { get; set; }
            public double ParsedQty { get; set; }
            public decimal ParsedPrice { get; set; }
            public bool IsExport { get; set; } // True = PX (Đặt hàng), False = PN (Nhận hàng)
        }

        [HttpGet]
        [Authorize(Policy = AppPermissions.Orders.Create)]
        public IActionResult Import()
        {
            return View();
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        [Authorize(Policy = AppPermissions.Orders.Create)]
        public async Task<IActionResult> ValidateImport([FromBody] List<LedgerImportRow> rawRows)
        {
            var results = new List<LedgerImportResult>();
            int rowIndex = 2;

            var items = await _context.WarehouseItems.ToListAsync();
            var suppliers = await _context.Customers.Where(c => c.Type == PartnerType.Supplier || c.Type == PartnerType.Both).ToListAsync();
            var activeOrders = await _context.Orders.ToListAsync();

            foreach (var row in rawRows)
            {
                var res = new LedgerImportResult
                {
                    RowIndex = rowIndex++,
                    TransCode = row.TransCode,
                    TransDate = row.TransDate,
                    ItemCode = row.ItemCode,
                    ItemName = row.ItemName,
                    OrderCode = row.OrderCode,
                    Unit = row.Unit,
                    Quantity = row.Quantity,
                    SupplierName = row.SupplierName,
                    UnitPrice = row.UnitPrice,
                    Note = row.Note
                };

                // 1. Phân loại Giao dịch
                if (string.IsNullOrWhiteSpace(res.TransCode)) res.Errors.Add("Thiếu Mã phiếu.");
                else
                {
                    string prefix = res.TransCode.Trim().ToUpper();
                    if (prefix.StartsWith("PX")) res.IsExport = true;
                    else if (prefix.StartsWith("PN")) res.IsExport = false;
                    else res.Errors.Add("Mã phiếu phải bắt đầu bằng PX hoặc PN.");
                }

                if (string.IsNullOrWhiteSpace(res.OrderCode)) res.Errors.Add("Thiếu Số đơn hàng.");

                // 🔥 VÁ LỖI 3: Bắt buộc Nhà cung cấp
                if (string.IsNullOrWhiteSpace(res.SupplierName))
                {
                    res.Errors.Add("Thiếu Nhà Cung Cấp.");
                }
                else
                {
                    var supp = suppliers.FirstOrDefault(s => s.CustomerCode == res.SupplierName || s.CompanyName == res.SupplierName);
                    if (supp != null) res.SupplierId = supp.Id; else res.Errors.Add($"Không tìm thấy NCC: {res.SupplierName}");
                }

                // 2. Kiểm tra Vật tư & Số lượng
                var item = items.FirstOrDefault(i => i.Code == res.ItemCode);
                if (item != null) res.ItemId = item.Id; else res.Errors.Add($"Mã SP sai: {res.ItemCode}");

                if (double.TryParse(res.Quantity?.ToString(), out double q) && q > 0) res.ParsedQty = q;
                else res.Errors.Add("Số lượng phải > 0.");

                // 🔥 VÁ LỖI 2: Xử lý ngày tháng chuẩn VN (Tránh sập do server tiếng Anh)
                string[] formats = { "dd/MM/yyyy", "d/M/yyyy", "yyyy-MM-dd", "MM/dd/yyyy" };
                if (DateTime.TryParseExact(res.TransDate?.ToString(), formats, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime d))
                    res.ParsedDate = d;
                else
                    res.ParsedDate = DateTime.Now;

                decimal.TryParse(res.UnitPrice?.ToString(), out decimal p); res.ParsedPrice = p;

                var linkedOrder = activeOrders.FirstOrDefault(o => o.OrderCode == res.OrderCode);
                if (linkedOrder != null) res.LinkedOrderId = linkedOrder.Id;

                res.IsValid = !res.Errors.Any();
                results.Add(res);
            }
            return Json(results);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        [Authorize(Policy = AppPermissions.Orders.Create)]
        public async Task<IActionResult> ExecuteImport([FromBody] List<LedgerImportResult> validRows)
        {
            var groups = validRows.Where(r => r.IsValid).GroupBy(r => r.OrderCode);
            int successCount = 0;

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var group in groups)
                {
                    var firstRow = group.First();
                    string poCode = $"PO-{group.Key}";

                    // 🔥 VÁ LỖI 1: Thuật toán TÌM HOẶC TẠO MỚI (Tránh trùng mã đơn)
                    var po = await _context.PurchaseOrders
                        .Include(p => p.Details)
                        .FirstOrDefaultAsync(p => p.POCode == poCode);

                    bool isNewPo = false;
                    if (po == null)
                    {
                        po = new PurchaseOrder
                        {
                            POCode = poCode,
                            OrderDate = firstRow.ParsedDate,
                            SupplierId = firstRow.SupplierId ?? 0,
                            SupplierName = firstRow.SupplierName,
                            Type = POType.Outsourcing,
                            Details = new List<PurchaseOrderDetail>()
                        };
                        isNewPo = true;
                    }

                    var itemGroups = group.GroupBy(r => r.ItemId);
                    foreach (var itemGroup in itemGroups)
                    {
                        var firstItem = itemGroup.First();
                        double totalOrdered = itemGroup.Where(x => x.IsExport).Sum(x => x.ParsedQty);
                        double totalReceived = itemGroup.Where(x => !x.IsExport).Sum(x => x.ParsedQty);

                        // Tìm xem vật tư này đã có trong đơn cũ chưa
                        var detail = po.Details.FirstOrDefault(d => d.WarehouseItemId == firstItem.ItemId);

                        if (detail != null)
                        {
                            // Cộng dồn tiến độ nếu nhập lần 2, lần 3
                            detail.QuantityOrdered += totalOrdered;
                            detail.QuantityReceived += totalReceived;
                            if (firstItem.ParsedPrice > 0) detail.UnitPrice = firstItem.ParsedPrice; // Cập nhật lại giá
                            detail.IsFinished = detail.QuantityReceived >= detail.QuantityOrdered && detail.QuantityOrdered > 0;
                        }
                        else
                        {
                            // Thêm mới vật tư vào đơn
                            po.Details.Add(new PurchaseOrderDetail
                            {
                                WarehouseItemId = firstItem.ItemId,
                                QuantityOrdered = totalOrdered,
                                QuantityReceived = totalReceived,
                                UnitPrice = firstItem.ParsedPrice,
                                LinkedOrderId = firstItem.LinkedOrderId,
                                LinkedOrderCode = firstItem.OrderCode,
                                IsFinished = totalReceived >= totalOrdered && totalOrdered > 0
                            });
                        }

                        // TỰ ĐỘNG SYNC VÀO KHO HÀNG CHO CÁC DÒNG 'PN' (NHẬP HÀNG)
                        var pnTransactions = itemGroup.Where(x => !x.IsExport).ToList();
                        if (pnTransactions.Any())
                        {
                            var whItem = await _context.WarehouseItems.FindAsync(firstItem.ItemId);
                            if (whItem != null)
                            {
                                foreach (var pn in pnTransactions)
                                {
                                    whItem.StockQuantity += pn.ParsedQty;
                                    _context.StockTransactions.Add(new StockTransaction
                                    {
                                        WarehouseItemId = whItem.Id,
                                        Type = "Import",
                                        Quantity = pn.ParsedQty,
                                        CurrentStock = whItem.StockQuantity,
                                        Price = pn.ParsedPrice,
                                        TransactionDate = pn.ParsedDate,
                                        DocumentCode = pn.TransCode,
                                        Receiver = pn.SupplierName,
                                        Note = pn.Note ?? $"Nhập kho từ {pn.OrderCode}",
                                        Staff = User.Identity?.Name ?? "System"
                                    });
                                }
                            }
                        }
                    }

                    po.Status = po.Details.All(d => d.IsFinished) ? POStatus.Completed : (po.Details.Any(d => d.QuantityReceived > 0) ? POStatus.Partial : POStatus.Open);
                    po.TotalAmount = po.Details.Sum(d => (decimal)d.QuantityOrdered * d.UnitPrice);
                    po.FinalTotal = po.TotalAmount;
                    po.RemainingAmount = po.FinalTotal;

                    if (isNewPo) _context.PurchaseOrders.Add(po);

                    successCount++;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Json(new { success = true, count = successCount, message = $"Đã đồng bộ {successCount} Đơn hàng thành công!" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "Lỗi hệ thống khi lưu: " + ex.Message });
            }
        }

    }
}