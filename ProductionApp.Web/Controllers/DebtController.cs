using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
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
    public class SimplePriceHistory
    {
        public string time { get; set; }
        public string user { get; set; }
        public decimal oldPrice { get; set; }
        public decimal newPrice { get; set; }
        public string note { get; set; }
    }

    [Authorize]
    public class DebtController : Controller
    {
        private readonly AppDbContext _context;

        public DebtController(AppDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. LẤY DỮ LIỆU CÔNG NỢ (MASTER)
        // ==========================================
        private async Task<DebtDashboardVM> GetDebtData(DateTime? fromDate, DateTime? toDate, string keyword, int type, string sortOrder)
        {
            var model = new DebtDashboardVM
            {
                FromDate = fromDate,
                ToDate = toDate,
                Keyword = keyword,
                Type = type,
                SortOrder = sortOrder,
                DebtGroups = new List<PartnerDebtGroup>()
            };

            var ordersQuery = _context.Orders.Include(o => o.Customer).Where(o => o.Status != OrderStatus.Canceled);
            var importsQuery = _context.MaterialImports.Where(x => x.APInvoiceId == null).AsQueryable();
            var exportsQuery = _context.ExportShipments.Where(x => x.APInvoiceId == null).AsQueryable();
            var posQuery = _context.PurchaseOrders.Where(p => p.IsDebtFinalized && p.Status != POStatus.Cancelled && string.IsNullOrEmpty(p.InvoiceNumber)).AsQueryable();
            var apInvoicesQuery = _context.APInvoices.AsQueryable();

            if (fromDate.HasValue)
            {
                ordersQuery = ordersQuery.Where(x => x.OrderDate.Date >= fromDate.Value.Date);
                importsQuery = importsQuery.Where(x => x.ImportDate.Date >= fromDate.Value.Date);
                exportsQuery = exportsQuery.Where(x => x.ETD != null && x.ETD.Value.Date >= fromDate.Value.Date);
                posQuery = posQuery.Where(x => x.DebtFinalizedDate != null && x.DebtFinalizedDate.Value.Date >= fromDate.Value.Date);
                apInvoicesQuery = apInvoicesQuery.Where(x => x.InvoiceDate.Date >= fromDate.Value.Date);
            }
            if (toDate.HasValue)
            {
                ordersQuery = ordersQuery.Where(x => x.OrderDate.Date <= toDate.Value.Date);
                importsQuery = importsQuery.Where(x => x.ImportDate.Date <= toDate.Value.Date);
                exportsQuery = exportsQuery.Where(x => x.ETD != null && x.ETD.Value.Date <= toDate.Value.Date);
                posQuery = posQuery.Where(x => x.DebtFinalizedDate != null && x.DebtFinalizedDate.Value.Date <= toDate.Value.Date);
                apInvoicesQuery = apInvoicesQuery.Where(x => x.InvoiceDate.Date <= toDate.Value.Date);
            }

            var orders = await ordersQuery.ToListAsync();
            var orderIds = orders.Select(x => x.Id).ToList();
            var receipts = await _context.CashEntries.Where(c => c.Type == TransactionType.Receipt && c.OrderId != null && orderIds.Contains(c.OrderId.Value)).ToListAsync();
            var invoiceDetails = await _context.InvoiceDetails.Where(i => orderIds.Contains(i.OrderId)).ToListAsync();

            var customerDebts = new List<PartnerDebtGroup>();
            decimal grandTotalReceivable = 0;

            foreach (var o in orders)
            {
                decimal paid = receipts.Where(r => r.OrderId == o.Id).Sum(r => r.Amount);
                decimal billed = invoiceDetails.Where(i => i.OrderId == o.Id).Sum(i => i.BilledAmount);
                decimal remain = billed - paid;

                if (remain > 100 && billed > 0)
                {
                    grandTotalReceivable += remain;
                    if (type == 1)
                    {
                        var group = customerDebts.FirstOrDefault(g => g.PartnerName == (o.Customer?.CompanyName ?? "Khách lẻ"));
                        if (group == null)
                        {
                            group = new PartnerDebtGroup { PartnerId = o.CustomerId, PartnerName = o.Customer?.CompanyName ?? "Khách lẻ", PartnerCode = o.Customer?.CustomerCode ?? "KL", Phone = o.Customer?.PhoneNumber, TotalDebt = 0 };
                            customerDebts.Add(group);
                        }

                        group.Details.Add(new DebtDetailItem { Id = o.Id, Code = o.OrderCode, Date = o.OrderDate, TotalAmount = billed, PaidAmount = paid, Note = string.IsNullOrWhiteSpace(o.Notes) ? "Công nợ theo HĐ Đầu Ra" : o.Notes, IsLocked = false });
                        group.TotalDebt += remain;
                    }
                }
            }
            model.TotalReceivable = grandTotalReceivable;

            var supplierDebts = new List<PartnerDebtGroup>();
            decimal grandTotalPayable = 0;

            var pos = await posQuery.ToListAsync();
            foreach (var po in pos)
            {
                decimal remain = po.RemainingAmount;
                if (Math.Abs(remain) > 10)
                {
                    grandTotalPayable += remain;
                    if (type == 2)
                    {
                        var group = supplierDebts.FirstOrDefault(g => g.PartnerName == po.SupplierName);
                        if (group == null) { group = new PartnerDebtGroup { PartnerName = po.SupplierName, PartnerCode = "NCC", TotalDebt = 0 }; supplierDebts.Add(group); }
                        group.Details.Add(new DebtDetailItem { Id = po.Id, Code = po.POCode, Date = po.DebtFinalizedDate ?? po.OrderDate, TotalAmount = po.FinalTotal, PaidAmount = po.DepositAmount, Note = "Công nợ theo Đơn hàng (PO)", IsLocked = false });
                        group.TotalDebt += remain;
                    }
                }
            }

            var apInvoices = await apInvoicesQuery.ToListAsync();
            foreach (var inv in apInvoices)
            {
                decimal remain = inv.TotalAmount - inv.PaidAmount;
                if (remain > 10)
                {
                    grandTotalPayable += remain;
                    if (type == 2)
                    {
                        string partner = string.IsNullOrWhiteSpace(inv.PartnerName) ? "Đối tác" : inv.PartnerName;
                        var group = supplierDebts.FirstOrDefault(g => g.PartnerName == partner);
                        if (group == null) { group = new PartnerDebtGroup { PartnerName = partner, PartnerCode = "NCC", TotalDebt = 0 }; supplierDebts.Add(group); }
                        group.Details.Add(new DebtDetailItem { Id = inv.Id, Code = inv.InvoiceNumber, Date = inv.InvoiceDate, TotalAmount = inv.TotalAmount, PaidAmount = inv.PaidAmount, Note = string.IsNullOrWhiteSpace(inv.Note) ? "Hóa đơn VAT Đầu vào" : inv.Note, IsLocked = false });
                        group.TotalDebt += remain;
                    }
                }
            }

            var imports = await importsQuery.ToListAsync();
            foreach (var imp in imports)
            {
                decimal remain = imp.TotalAmount - imp.PaidAmount;
                if (remain > 100)
                {
                    grandTotalPayable += remain;
                    if (type == 2)
                    {
                        var group = supplierDebts.FirstOrDefault(g => g.PartnerName == imp.SupplierName);
                        if (group == null) { group = new PartnerDebtGroup { PartnerName = imp.SupplierName, PartnerCode = "NCC", TotalDebt = 0 }; supplierDebts.Add(group); }
                        group.Details.Add(new DebtDetailItem { Id = imp.Id, Code = imp.ImportCode, Date = imp.ImportDate, TotalAmount = imp.TotalAmount, PaidAmount = imp.PaidAmount, Note = "Phiếu Nhập Kho", IsLocked = imp.IsLocked });
                        group.TotalDebt += remain;
                    }
                }
            }

            var exports = await exportsQuery.ToListAsync();
            foreach (var exp in exports)
            {
                if (exp.ShippingCost > 0 && exp.PaymentStatus == PaymentStatuses.Unpaid)
                {
                    decimal remain = exp.ShippingCost;
                    grandTotalPayable += remain;
                    if (type == 2)
                    {
                        string carrier = string.IsNullOrWhiteSpace(exp.CarrierName) ? "Nhà xe vãng lai" : exp.CarrierName;
                        var group = supplierDebts.FirstOrDefault(g => g.PartnerName == carrier);
                        if (group == null) { group = new PartnerDebtGroup { PartnerName = carrier, PartnerCode = "VẬN TẢI", TotalDebt = 0 }; supplierDebts.Add(group); }
                        group.Details.Add(new DebtDetailItem { Id = exp.Id, Code = exp.ShipmentCode, Date = exp.ETD ?? DateTime.Now, TotalAmount = remain, PaidAmount = 0, Note = "Cước Vận Tải", IsLocked = false });
                        group.TotalDebt += remain;
                    }
                }
            }

            model.TotalPayable = grandTotalPayable;
            var resultList = type == 1 ? customerDebts : supplierDebts;

            if (!string.IsNullOrEmpty(keyword))
            {
                string k = keyword.ToLower();
                resultList = resultList.Where(g => g.PartnerName.ToLower().Contains(k) || g.PartnerCode.ToLower().Contains(k)).ToList();
            }

            switch (sortOrder)
            {
                case "name_asc": resultList = resultList.OrderBy(x => x.PartnerName).ToList(); break;
                case "name_desc": resultList = resultList.OrderByDescending(x => x.PartnerName).ToList(); break;
                case "debt_asc": resultList = resultList.OrderBy(x => x.TotalDebt).ToList(); break;
                default: resultList = resultList.OrderByDescending(x => x.TotalDebt).ToList(); break;
            }

            model.DebtGroups = resultList;
            return model;
        }

        [Authorize(Policy = AppPermissions.Debts.View)]
        public async Task<IActionResult> Index(DateTime? fromDate, DateTime? toDate, string keyword, int type = 1, string sortOrder = "debt_desc")
        {
            var model = await GetDebtData(fromDate, toDate, keyword, type, sortOrder);
            ViewBag.PartnerList = await _context.Customers
                .Where(c => c.Type == (type == 1 ? PartnerType.Customer : PartnerType.Supplier))
                .Select(c => new SelectListItem { Value = c.CompanyName, Text = c.CompanyName })
                .ToListAsync();
            return View(model);
        }

        // ==========================================
        // 2. API LẤY CHI TIẾT CHỨNG TỪ (ĐÃ HỖ TRỢ ĐỌC XUYÊN HÓA ĐƠN)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> GetDocumentItems(int id, string code)
        {
            try
            {
                var items = new List<dynamic>();

                // 1. KIỂM TRA NẾU LÀ HÓA ĐƠN VAT (AP INVOICE) ĐÃ GOM
                var apInvoice = await _context.APInvoices.FirstOrDefaultAsync(x => x.Id == id && x.InvoiceNumber == code);
                if (apInvoice != null)
                {
                    // Quét các Đơn Đặt Hàng (PO) nằm trong Hóa đơn này
                    var pos = await _context.PurchaseOrders
                        .Include(x => x.Details).ThenInclude(x => x.WarehouseItem)
                        .Where(x => x.InvoiceNumber == code).ToListAsync();

                    foreach (var po in pos)
                    {
                        if (po.Details != null)
                        {
                            foreach (var d in po.Details)
                            {
                                items.Add(new
                                {
                                    name = $"[{po.POCode}] {d.WarehouseItem?.Name ?? "Vật tư"}",
                                    qty = d.QuantityOrdered,
                                    price = d.UnitPrice,
                                    total = (decimal)d.QuantityOrdered * d.UnitPrice
                                });
                            }
                        }
                    }

                    // Quét các Phiếu Nhập Kho (Material Import) nằm trong Hóa đơn này
                    var imports = await _context.MaterialImports.Where(x => x.APInvoiceId == id).Select(x => x.ImportCode).ToListAsync();
                    if (imports.Any())
                    {
                        var stockTrans = await _context.StockTransactions.Include(x => x.WarehouseItem).Where(x => imports.Contains(x.DocumentCode)).ToListAsync();
                        foreach (var t in stockTrans)
                        {
                            items.Add(new
                            {
                                name = $"[{t.DocumentCode}] {t.WarehouseItem?.Name ?? "Vật tư"}",
                                qty = t.Quantity,
                                price = t.Price,
                                total = (decimal)t.Quantity * t.Price
                            });
                        }
                    }

                    // Quét các Chuyến Xe Vận Tải (Export Shipment)
                    var exports = await _context.ExportShipments.Where(x => x.APInvoiceId == id).ToListAsync();
                    foreach (var exp in exports)
                    {
                        items.Add(new
                        {
                            name = $"[VẬN TẢI] Cước xe {exp.ShipmentCode} ({exp.CarrierName})",
                            qty = 1,
                            price = exp.ShippingCost,
                            total = exp.ShippingCost
                        });
                    }

                    if (!items.Any()) return Json(new { success = false, message = "Hóa đơn này không có chi tiết chứng từ gốc." });
                    return Json(new { success = true, data = items });
                }

                // 2. KIỂM TRA NẾU LÀ ĐƠN ĐẶT HÀNG (PO) ĐỘC LẬP CHƯA GOM HÓA ĐƠN
                if (code.StartsWith("PO"))
                {
                    var po = await _context.PurchaseOrders
                        .Include(x => x.Details).ThenInclude(x => x.WarehouseItem)
                        .FirstOrDefaultAsync(x => x.Id == id);

                    if (po != null && po.Details != null && po.Details.Any())
                    {
                        foreach (var d in po.Details)
                        {
                            items.Add(new
                            {
                                name = d.WarehouseItem?.Name ?? "Vật tư không xác định",
                                qty = d.QuantityOrdered,
                                price = d.UnitPrice,
                                total = (decimal)d.QuantityOrdered * d.UnitPrice
                            });
                        }
                        return Json(new { success = true, data = items });
                    }
                    return Json(new { success = false, message = "Đơn đặt hàng không có chi tiết vật tư." });
                }

                // 3. KIỂM TRA NẾU LÀ ĐƠN HÀNG BÁN RA (DH / SO)
                if (code.StartsWith("DH") || code.StartsWith("SO"))
                {
                    var order = await _context.Orders.Include(o => o.OrderDetails).FirstOrDefaultAsync(o => o.Id == id);
                    if (order != null && order.OrderDetails != null && order.OrderDetails.Any())
                    {
                        foreach (var d in order.OrderDetails)
                        {
                            items.Add(new
                            {
                                name = d.ProductName,
                                qty = d.Quantity,
                                price = d.UnitPrice,
                                total = (decimal)d.Quantity * d.UnitPrice
                            });
                        }
                        return Json(new { success = true, data = items });
                    }
                    return Json(new { success = false, message = "Đơn hàng bán ra không có chi tiết sản phẩm." });
                }

                // 4. FALLBACK: NẾU LÀ PHIẾU NHẬP / XUẤT KHO ĐỘC LẬP CHƯA GOM HÓA ĐƠN
                var singleStockTrans = await _context.StockTransactions
                    .Include(x => x.WarehouseItem)
                    .Where(x => x.DocumentCode == code)
                    .ToListAsync();

                if (singleStockTrans != null && singleStockTrans.Any())
                {
                    foreach (var t in singleStockTrans)
                    {
                        items.Add(new
                        {
                            name = t.WarehouseItem?.Name ?? "Vật tư không xác định",
                            qty = t.Quantity,
                            price = t.Price,
                            total = (decimal)t.Quantity * t.Price
                        });
                    }
                    return Json(new { success = true, data = items });
                }

                return Json(new { success = false, message = "Không tìm thấy chi tiết của chứng từ này." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi tải chi tiết: " + ex.Message });
            }
        }

        // ==========================================
        // 3. XỬ LÝ THANH TOÁN ĐƠN LẺ & THEO LÔ
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> ProcessPayment(int docId, int type, decimal amount, PaymentMethod method, DateTime date, string note)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var cashEntry = new CashEntry
                {
                    TransactionDate = date,
                    ReportDate = date,
                    Amount = amount,
                    Method = method,
                    Description = note,
                    CreatedBy = User.Identity?.Name ?? "Kế toán",
                    CreatedAt = DateTime.Now,
                    Category = EntryCategory.Business
                };

                if (type == 1)
                {
                    var order = await _context.Orders.Include(o => o.Customer).FirstOrDefaultAsync(o => o.Id == docId);
                    if (order == null) return Json(new { success = false, message = "Không tìm thấy đơn hàng!" });

                    cashEntry.Type = TransactionType.Receipt;
                    cashEntry.TargetName = order.Customer?.CompanyName ?? "Khách lẻ";
                    cashEntry.OrderId = order.Id;
                    cashEntry.VoucherCode = $"PT-{date:yyMM}-{new Random().Next(1000, 9999)}";

                    if (string.IsNullOrWhiteSpace(cashEntry.Description)) cashEntry.Description = $"Thu tiền công nợ đơn hàng {order.OrderCode}";

                    _context.CashEntries.Add(cashEntry);
                    await _context.SaveChangesAsync();

                    decimal totalPaid = await _context.CashEntries.Where(c => c.OrderId == order.Id && c.Type == TransactionType.Receipt).SumAsync(c => c.Amount);
                    if (totalPaid >= order.TotalAmount)
                    {
                        order.PaymentStatus = PaymentStatus.Paid;
                        if (order.Status != OrderStatus.Canceled) order.Status = OrderStatus.Completed;
                    }
                    else if (totalPaid > 0) order.PaymentStatus = PaymentStatus.Partial;
                    else order.PaymentStatus = PaymentStatus.Unpaid;

                    _context.Update(order);
                }
                else
                {
                    cashEntry.Type = TransactionType.Payment;
                    cashEntry.VoucherCode = $"PC-{date:yyMM}-{new Random().Next(1000, 9999)}";

                    var po = await _context.PurchaseOrders.FindAsync(docId);
                    if (po != null)
                    {
                        cashEntry.TargetName = po.SupplierName;
                        cashEntry.PurchaseOrderId = po.Id;
                        cashEntry.PaperVoucherNumber = po.InvoiceNumber;
                        cashEntry.Description = string.IsNullOrWhiteSpace(note) ? $"Thanh toán PO {po.POCode}" : note;
                        _context.CashEntries.Add(cashEntry);

                        po.DepositAmount += amount;
                        po.RemainingAmount = po.FinalTotal - po.DepositAmount;
                        po.PaymentStatusPO = po.RemainingAmount <= 0 ? PaymentStatusPO.Paid : PaymentStatusPO.Deposited;
                        _context.Update(po);
                    }
                    else
                    {
                        var ap = await _context.APInvoices.FindAsync(docId);
                        if (ap != null)
                        {
                            cashEntry.TargetName = ap.PartnerName;
                            cashEntry.APInvoiceId = ap.Id;
                            cashEntry.Description = string.IsNullOrWhiteSpace(note) ? $"Thanh toán hóa đơn {ap.InvoiceNumber}" : note;
                            _context.CashEntries.Add(cashEntry);
                            ap.PaidAmount += amount;
                            _context.Update(ap);
                        }
                        else
                        {
                            var import = await _context.MaterialImports.FindAsync(docId);
                            if (import != null)
                            {
                                cashEntry.TargetName = import.SupplierName;
                                cashEntry.MaterialImportId = import.Id;
                                cashEntry.Description = string.IsNullOrWhiteSpace(note) ? $"Thanh toán phiếu nhập {import.ImportCode}" : note;
                                _context.CashEntries.Add(cashEntry);
                                import.PaidAmount += amount;
                                import.PaymentStatus = import.PaidAmount >= import.TotalAmount - 0.01m ? ImportPaymentStatus.Paid : (import.PaidAmount > 0 ? ImportPaymentStatus.Partial : ImportPaymentStatus.Unpaid);
                                _context.Update(import);
                            }
                            else
                            {
                                var export = await _context.ExportShipments.FindAsync(docId);
                                if (export != null)
                                {
                                    cashEntry.TargetName = export.CarrierName ?? "Nhà xe vãng lai";
                                    cashEntry.ExportShipmentId = export.Id;
                                    cashEntry.Description = string.IsNullOrWhiteSpace(note) ? $"Thanh toán cước xe {export.ShipmentCode}" : note;
                                    _context.CashEntries.Add(cashEntry);
                                    export.PaymentStatus = PaymentStatuses.Paid;
                                    _context.Update(export);
                                }
                                else return Json(new { success = false, message = "Không tìm thấy chứng từ cần thanh toán!" });
                            }
                        }
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Json(new { success = true, message = "Thanh toán thành công và đã ghi vào Sổ Quỹ!" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "Lỗi xử lý: " + (ex.InnerException?.Message ?? ex.Message) });
            }
        }

        public class BatchPaymentItem { public int Id { get; set; } public string Code { get; set; } public decimal Remain { get; set; } }

        [HttpGet]
        public async Task<IActionResult> BatchPayment(int type = 1)
        {
            ViewBag.Type = type;
            var data = await GetDebtData(null, null, "", type, "");
            var debtors = data.DebtGroups.Where(g => g.TotalDebt > 0).ToList();
            return View(debtors);
        }

        [HttpGet]
        public async Task<IActionResult> GetPartnerDebts(string partnerCode, int type)
        {
            var data = await GetDebtData(null, null, "", type, "");
            var partner = data.DebtGroups.FirstOrDefault(g => g.PartnerCode == partnerCode);

            if (partner == null) return Json(new { items = new List<object>() });

            var unpaidItems = partner.Details.Where(d => d.RemainingAmount > 0).Select(d => new {
                id = d.Id,
                code = d.Code,
                date = d.Date.ToString("dd/MM/yyyy"),
                note = d.Note,
                total = d.TotalAmount,
                remain = d.RemainingAmount
            }).ToList();

            return Json(new { partnerName = partner.PartnerName, partnerCode = partner.PartnerCode, totalDebt = partner.TotalDebt, items = unpaidItems });
        }

        [HttpPost]
        public async Task<IActionResult> ProcessBatchPayment(string batchDataJson, int type, decimal amount, PaymentMethod method, DateTime date, string paperVoucherNumber, string note, string partnerName)
        {
            if (string.IsNullOrWhiteSpace(batchDataJson)) return Json(new { success = false, message = "Không có dữ liệu chứng từ!" });
            var items = JsonSerializer.Deserialize<List<BatchPaymentItem>>(batchDataJson);
            if (items == null || !items.Any()) return Json(new { success = false, message = "Danh sách chứng từ trống!" });

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                decimal remainToDistribute = amount;
                string commonVoucherCode = (type == 1 ? "PT" : "PC") + $"-{date:yyMM}-{new Random().Next(1000, 9999)}";

                foreach (var item in items)
                {
                    if (remainToDistribute <= 0) break;

                    decimal payForThis = Math.Min(item.Remain, remainToDistribute);
                    remainToDistribute -= payForThis;
                    bool isFullyPaid = payForThis >= (item.Remain - 0.01m);

                    var cashEntry = new CashEntry
                    {
                        TransactionDate = date,
                        ReportDate = date,
                        Amount = payForThis,
                        Method = method,
                        Description = string.IsNullOrWhiteSpace(note) ? $"Thanh toán lô ({paperVoucherNumber})" : note,
                        TargetName = partnerName,
                        VoucherCode = commonVoucherCode,
                        PaperVoucherNumber = paperVoucherNumber,
                        CreatedBy = User.Identity?.Name ?? "Kế toán",
                        CreatedAt = DateTime.Now,
                        Category = EntryCategory.Business,
                        Type = type == 1 ? TransactionType.Receipt : TransactionType.Payment
                    };

                    if (type == 1)
                    {
                        var order = await _context.Orders.FirstOrDefaultAsync(x => x.Id == item.Id && x.OrderCode == item.Code);
                        if (order != null)
                        {
                            cashEntry.OrderId = order.Id;
                            _context.CashEntries.Add(cashEntry);

                            decimal totalBilledForOrder = await _context.InvoiceDetails.Where(i => i.OrderId == order.Id).SumAsync(i => i.BilledAmount);
                            decimal totalPaidForOrder = await _context.CashEntries.Where(c => c.OrderId == order.Id && c.Type == TransactionType.Receipt).SumAsync(c => c.Amount) + payForThis;

                            bool isOrderFullyPaid = totalPaidForOrder >= (totalBilledForOrder - 0.01m);
                            order.PaymentStatus = isOrderFullyPaid ? PaymentStatus.Paid : PaymentStatus.Partial;
                            if (isOrderFullyPaid && order.Status != OrderStatus.Canceled) order.Status = OrderStatus.Completed;
                            _context.Update(order);
                        }
                    }
                    else
                    {
                        var po = await _context.PurchaseOrders.FirstOrDefaultAsync(x => x.Id == item.Id && x.POCode == item.Code);
                        if (po != null)
                        {
                            cashEntry.PurchaseOrderId = po.Id;
                            _context.CashEntries.Add(cashEntry);
                            po.DepositAmount += payForThis;
                            po.RemainingAmount = po.FinalTotal - po.DepositAmount;
                            po.PaymentStatusPO = po.RemainingAmount <= 0 ? PaymentStatusPO.Paid : PaymentStatusPO.Deposited;
                            _context.Update(po);
                        }
                        else
                        {
                            var apInvoice = await _context.APInvoices.FirstOrDefaultAsync(x => x.Id == item.Id && x.InvoiceNumber == item.Code);
                            if (apInvoice != null)
                            {
                                cashEntry.APInvoiceId = apInvoice.Id;
                                _context.CashEntries.Add(cashEntry);
                                apInvoice.PaidAmount += payForThis;
                                _context.Update(apInvoice);
                            }
                            else
                            {
                                var import = await _context.MaterialImports.FirstOrDefaultAsync(x => x.Id == item.Id && x.ImportCode == item.Code);
                                if (import != null)
                                {
                                    cashEntry.MaterialImportId = import.Id;
                                    _context.CashEntries.Add(cashEntry);
                                    import.PaidAmount += payForThis;
                                    import.PaymentStatus = isFullyPaid ? ImportPaymentStatus.Paid : ImportPaymentStatus.Partial;
                                    _context.Update(import);
                                }
                                else
                                {
                                    var export = await _context.ExportShipments.FirstOrDefaultAsync(x => x.Id == item.Id && x.ShipmentCode == item.Code);
                                    if (export != null)
                                    {
                                        cashEntry.ExportShipmentId = export.Id;
                                        _context.CashEntries.Add(cashEntry);
                                        if (isFullyPaid) export.PaymentStatus = PaymentStatuses.Paid;
                                        _context.Update(export);
                                    }
                                }
                            }
                        }
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Json(new { success = true, message = $"Đã thanh toán thành công. Mã Phiếu: {commonVoucherCode}. Dư: {remainToDistribute:N0} đ (nếu có)." });
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "🚨 XUNG ĐỘT DỮ LIỆU: Vui lòng F5 và thao tác lại!" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "Lỗi xử lý: " + (ex.InnerException?.Message ?? ex.Message) });
            }
        }

        // ==========================================
        // 4. API BỔ TRỢ (CẬP NHẬT GIÁ VÀ XUẤT EXCEL)
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> UpdateImportPrice(int importId, string subTotal, string vatRate, string newTotalAmount, string reason, string docType = "Import")
        {
            if (string.IsNullOrWhiteSpace(reason)) return Json(new { success = false, message = "Vui lòng nhập lý do / số hóa đơn." });

            try
            {
                subTotal = subTotal?.Replace(",", "").Replace(".", "");
                newTotalAmount = newTotalAmount?.Replace(",", "").Replace(".", "");
                vatRate = vatRate?.Replace(",", ".").Trim();

                decimal.TryParse(subTotal, out decimal parsedSubTotal);
                decimal.TryParse(newTotalAmount, out decimal parsedTotalAmount);
                decimal.TryParse(vatRate, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal parsedVatRate);

                if (docType == "Import")
                {
                    var import = await _context.MaterialImports.FindAsync(importId);
                    if (import == null) return Json(new { success = false, message = "Không tìm thấy phiếu." });
                    if (import.IsLocked) return Json(new { success = false, message = "Phiếu này đã khóa sổ!" });

                    var history = new ImportPriceHistory
                    {
                        MaterialImportId = import.Id,
                        OldTotalAmount = import.TotalAmount,
                        NewTotalAmount = parsedTotalAmount,
                        Reason = $"[HĐ VAT {parsedVatRate}%] " + reason,
                        ChangedAt = DateTime.Now,
                        ChangedBy = User.Identity?.Name ?? "Kế toán"
                    };
                    _context.ImportPriceHistories.Add(history);

                    import.SubTotal = parsedSubTotal;
                    import.VatRate = (double)parsedVatRate;
                    import.VatAmount = parsedTotalAmount - parsedSubTotal;
                    import.TotalAmount = parsedTotalAmount;
                    import.PaymentStatus = import.PaidAmount >= import.TotalAmount ? ImportPaymentStatus.Paid : (import.PaidAmount > 0 ? ImportPaymentStatus.Partial : ImportPaymentStatus.Unpaid);
                }
                else if (docType == "Transport")
                {
                    var export = await _context.ExportShipments.FindAsync(importId);
                    if (export == null) return Json(new { success = false, message = "Không tìm thấy chuyến xe." });

                    List<SimplePriceHistory> currentHistory = new List<SimplePriceHistory>();
                    if (!string.IsNullOrEmpty(export.PriceHistoryJson)) currentHistory = JsonSerializer.Deserialize<List<SimplePriceHistory>>(export.PriceHistoryJson);

                    currentHistory.Add(new SimplePriceHistory
                    {
                        time = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                        user = User.Identity?.Name ?? "Kế toán",
                        oldPrice = export.ShippingCost,
                        newPrice = parsedTotalAmount,
                        note = $"[HĐ VAT {parsedVatRate}%] " + reason
                    });

                    export.PriceHistoryJson = JsonSerializer.Serialize(currentHistory);
                    export.SubTotal = parsedSubTotal;
                    export.VatRate = (double)parsedVatRate;
                    export.VatAmount = parsedTotalAmount - parsedSubTotal;
                    export.ShippingCost = parsedTotalAmount;
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Cập nhật thành công!" });
            }
            catch (Exception ex) { return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message }); }
        }

        [HttpGet]
        [Authorize(Policy = AppPermissions.Debts.Edit)]
        public async Task<IActionResult> GetDocumentVATDetails(int id, string docType)
        {
            try
            {
                decimal subTotal = 0; double vatRate = 0; var historyList = new List<object>();

                if (docType == "Import")
                {
                    var import = await _context.MaterialImports.FindAsync(id);
                    if (import != null)
                    {
                        subTotal = import.SubTotal > 0 ? import.SubTotal : import.TotalAmount;
                        vatRate = import.VatRate;

                        var dbHistories = await _context.ImportPriceHistories.Where(h => h.MaterialImportId == id).OrderByDescending(h => h.ChangedAt).ToListAsync();
                        historyList.AddRange(dbHistories.Select(h => new { time = h.ChangedAt.ToString("dd/MM/yyyy HH:mm"), user = h.ChangedBy, oldPrice = h.OldTotalAmount, newPrice = h.NewTotalAmount, note = h.Reason }));
                    }
                }
                else if (docType == "Transport")
                {
                    var export = await _context.ExportShipments.FindAsync(id);
                    if (export != null)
                    {
                        subTotal = export.SubTotal > 0 ? export.SubTotal : export.ShippingCost;
                        vatRate = (double)export.VatRate;
                        if (!string.IsNullOrEmpty(export.PriceHistoryJson))
                        {
                            var parsedHistory = JsonSerializer.Deserialize<List<SimplePriceHistory>>(export.PriceHistoryJson);
                            parsedHistory.Reverse();
                            historyList.AddRange(parsedHistory);
                        }
                    }
                }

                return Json(new { success = true, subTotal = subTotal, vatRate = vatRate, history = historyList });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        [HttpPost]
        public async Task<IActionResult> LockImportTicket(int id)
        {
            var import = await _context.MaterialImports.FindAsync(id);
            if (import == null) return Json(new { success = false, message = "Không tìm thấy phiếu." });

            import.IsLocked = true;
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Đã KHÓA SỔ phiếu nhập này." });
        }

        public async Task<IActionResult> ExportExcel(DateTime? fromDate, DateTime? toDate, string keyword, int type = 1, string sortOrder = "debt_desc")
        {
            var model = await GetDebtData(fromDate, toDate, keyword, type, sortOrder);

            using (var wb = new XLWorkbook())
            {
                string sheetName = type == 1 ? "CongNo_PhaiThu" : "CongNo_PhaiTra";
                var ws = wb.Worksheets.Add(sheetName);

                ws.Cell(1, 1).Value = type == 1 ? "BÁO CÁO CÔNG NỢ PHẢI THU (KHÁCH HÀNG)" : "BÁO CÁO CÔNG NỢ PHẢI TRẢ (NHÀ CUNG CẤP & VẬN TẢI)";
                ws.Range("A1:F1").Merge().Style.Font.SetBold().Font.SetFontSize(14).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell(2, 1).Value = $"Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm}";
                ws.Range("A2:F2").Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                int currentRow = 4;
                ws.Cell(currentRow, 1).Value = "Ngày"; ws.Cell(currentRow, 2).Value = "Mã phiếu"; ws.Cell(currentRow, 3).Value = "Nội dung";
                ws.Cell(currentRow, 4).Value = "Tổng tiền"; ws.Cell(currentRow, 5).Value = "Đã thanh toán"; ws.Cell(currentRow, 6).Value = "Còn nợ";

                var rngHead = ws.Range(currentRow, 1, currentRow, 6);
                rngHead.Style.Font.Bold = true; rngHead.Style.Fill.BackgroundColor = XLColor.LightGray; rngHead.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                currentRow++;

                foreach (var group in model.DebtGroups)
                {
                    ws.Cell(currentRow, 1).Value = $"{group.PartnerName} ({group.PartnerCode})";
                    ws.Range(currentRow, 1, currentRow, 5).Merge().Style.Font.Bold = true;
                    ws.Cell(currentRow, 1).Style.Fill.BackgroundColor = XLColor.LightCyan;

                    ws.Cell(currentRow, 6).Value = group.TotalDebt;
                    ws.Cell(currentRow, 6).Style.Font.Bold = true;
                    ws.Cell(currentRow, 6).Style.Fill.BackgroundColor = XLColor.LightCyan;
                    ws.Cell(currentRow, 6).Style.NumberFormat.Format = "#,##0";
                    currentRow++;

                    foreach (var item in group.Details)
                    {
                        ws.Cell(currentRow, 1).Value = item.Date; ws.Cell(currentRow, 2).Value = item.Code; ws.Cell(currentRow, 3).Value = item.Note;
                        ws.Cell(currentRow, 4).Value = item.TotalAmount; ws.Cell(currentRow, 5).Value = item.PaidAmount; ws.Cell(currentRow, 6).Value = item.RemainingAmount;

                        ws.Cell(currentRow, 4).Style.NumberFormat.Format = "#,##0"; ws.Cell(currentRow, 5).Style.NumberFormat.Format = "#,##0";
                        ws.Cell(currentRow, 6).Style.NumberFormat.Format = "#,##0"; ws.Cell(currentRow, 6).Style.Font.FontColor = XLColor.Red;
                        currentRow++;
                    }
                }

                currentRow++;
                ws.Cell(currentRow, 1).Value = "TỔNG CỘNG TOÀN BỘ:";
                ws.Range(currentRow, 1, currentRow, 5).Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right).Font.Bold = true;

                decimal total = type == 1 ? model.TotalReceivable : model.TotalPayable;
                ws.Cell(currentRow, 6).Value = total;
                ws.Cell(currentRow, 6).Style.Font.Bold = true;
                ws.Cell(currentRow, 6).Style.NumberFormat.Format = "#,##0";
                ws.Cell(currentRow, 6).Style.Font.FontColor = XLColor.Red;
                ws.Cell(currentRow, 6).Style.Font.FontSize = 12;

                ws.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"BaoCaoCongNo_{DateTime.Now:ddMMyyyy}.xlsx");
                }
            }
        }
    }
}