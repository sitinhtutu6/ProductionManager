using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductionManager.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace ProductionApp.Web.Controllers
{
    [Authorize]
    public class APInvoiceController : Controller
    {
        private readonly AppDbContext _context;

        public APInvoiceController(AppDbContext context)
        {
            _context = context;
        }

        // ====================================================================
        // PHẦN 1: GOM HÓA ĐƠN ĐA CHỨNG TỪ (TỪ SỔ CÔNG NỢ)
        // ====================================================================

        [HttpGet]
        public async Task<IActionResult> CreateBatch(string partnerName, string docCodes)
        {
            if (string.IsNullOrEmpty(partnerName) || string.IsNullOrEmpty(docCodes))
                return RedirectToAction("Index", "Debt", new { type = 2 });

            var codes = docCodes.Split(',').Select(c => c.Trim()).ToList();

            decimal totalAmount = 0;
            var docList = new List<dynamic>();

            // 1. Quét Đơn Đặt Hàng (PO)
            var pos = await _context.PurchaseOrders
                .Where(x => codes.Contains(x.POCode) && string.IsNullOrEmpty(x.InvoiceNumber))
                .ToListAsync();
            foreach (var po in pos)
            {
                totalAmount += po.FinalTotal;
                docList.Add(new { Code = po.POCode, Type = "Đơn Đặt Hàng (PO)", Amount = po.FinalTotal });
            }

            // 2. Quét Chuyến Xe Vận Tải (ExportShipment)
            var exports = await _context.ExportShipments
                .Where(x => codes.Contains(x.ShipmentCode) && x.APInvoiceId == null)
                .ToListAsync();
            foreach (var exp in exports)
            {
                totalAmount += exp.ShippingCost;
                docList.Add(new { Code = exp.ShipmentCode, Type = "Cước Vận Tải", Amount = exp.ShippingCost });
            }

            // 3. Quét Phiếu Nhập Kho (MaterialImport) -> ĐÃ MỞ LẠI
            var imports = await _context.MaterialImports
                .Where(x => codes.Contains(x.ImportCode) && x.APInvoiceId == null)
                .ToListAsync();
            foreach (var imp in imports)
            {
                totalAmount += imp.TotalAmount;
                docList.Add(new { Code = imp.ImportCode, Type = "Phiếu Nhập Kho", Amount = imp.TotalAmount });
            }

            ViewBag.PartnerName = partnerName;
            ViewBag.DocCodes = docCodes;
            ViewBag.TotalAmount = totalAmount;
            ViewBag.DocList = docList;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SubmitBatch(string partnerName, string docCodes, string invoiceNumber, DateTime invoiceDate, decimal totalAmount, string note)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var apInvoice = new APInvoice
                {
                    InvoiceNumber = invoiceNumber,
                    InvoiceDate = invoiceDate,
                    PartnerName = partnerName,
                    TotalAmount = totalAmount,
                    PaidAmount = 0,
                    Note = note,
                    CreatedAt = DateTime.Now
                };

                _context.APInvoices.Add(apInvoice);
                await _context.SaveChangesAsync();

                var codes = docCodes.Split(',').Select(c => c.Trim()).ToList();

                // Dùng InvoiceNumber để neo Hóa Đơn vào PO
                var pos = await _context.PurchaseOrders.Where(x => codes.Contains(x.POCode)).ToListAsync();
                foreach (var po in pos) { po.InvoiceNumber = apInvoice.InvoiceNumber; _context.Update(po); }

                // Dùng APInvoiceId để neo vào ExportShipment
                var exports = await _context.ExportShipments.Where(x => codes.Contains(x.ShipmentCode)).ToListAsync();
                foreach (var exp in exports) { exp.APInvoiceId = apInvoice.Id; _context.Update(exp); }

                // Dùng APInvoiceId để neo vào MaterialImport -> ĐÃ MỞ LẠI
                var imports = await _context.MaterialImports.Where(x => codes.Contains(x.ImportCode)).ToListAsync();
                foreach (var imp in imports) { imp.APInvoiceId = apInvoice.Id; _context.Update(imp); }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { success = true, message = "Đã gom Hóa Đơn VAT thành công!" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "Lỗi xử lý: " + (ex.InnerException?.Message ?? ex.Message) });
            }
        }

        // ====================================================================
        // PHẦN 2: GOM HÓA ĐƠN VẬN TẢI (LUỒNG CŨ CỦA SẾP GIỮ NGUYÊN)
        // ====================================================================

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var partners = await _context.ExportShipments
                .Where(x => x.APInvoiceId == null && !string.IsNullOrWhiteSpace(x.CarrierName) && x.ShippingCost > 0)
                .Select(x => x.CarrierName)
                .Distinct()
                .ToListAsync();

            ViewBag.Partners = partners;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetUnbilledItems(string partnerName)
        {
            try
            {
                var items = await _context.ExportShipments
                    .Where(x => x.CarrierName == partnerName && x.APInvoiceId == null && x.ShippingCost > 0)
                    .OrderBy(x => x.CreatedDate)
                    .Select(x => new
                    {
                        id = x.Id,
                        code = x.ShipmentCode,
                        date = (x.ETD ?? x.CreatedDate).ToString("dd/MM/yyyy"),
                        amount = x.ShippingCost
                    })
                    .ToListAsync();

                return Json(items);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GenerateInvoice(string invoiceNo, DateTime date, string partnerName, string itemIds, decimal vatRate, string note)
        {
            if (string.IsNullOrWhiteSpace(itemIds))
                return Json(new { success = false, message = "Chưa chọn chứng từ!" });

            var ids = JsonSerializer.Deserialize<List<int>>(itemIds);
            if (ids == null || !ids.Any())
                return Json(new { success = false, message = "Chưa chọn chứng từ!" });

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var shipments = await _context.ExportShipments
                    .Where(x => ids.Contains(x.Id) && x.APInvoiceId == null)
                    .ToListAsync();

                if (!shipments.Any())
                    return Json(new { success = false, message = "Các chứng từ này đã được lập hóa đơn hoặc không tồn tại!" });

                decimal subTotal = shipments.Sum(x => x.ShippingCost);
                decimal vatAmount = subTotal * (vatRate / 100m);
                decimal totalAmount = subTotal + vatAmount;

                var apInvoice = new APInvoice
                {
                    InvoiceNumber = invoiceNo ?? $"HD-{DateTime.Now.Ticks}",
                    InvoiceDate = date,
                    PartnerName = partnerName,
                    SubTotal = subTotal,
                    VatRate = (decimal)vatRate,
                    VatAmount = vatAmount,
                    TotalAmount = totalAmount,
                    PaidAmount = 0,
                    Note = note,
                    CreatedAt = DateTime.Now
                };

                _context.APInvoices.Add(apInvoice);
                await _context.SaveChangesAsync();

                foreach (var s in shipments)
                {
                    s.APInvoiceId = apInvoice.Id;
                }

                _context.UpdateRange(shipments);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { success = true, message = "Lập Hóa đơn đầu vào thành công! Tổng nợ đã được ghi nhận." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "Lỗi xử lý: " + (ex.InnerException?.Message ?? ex.Message) });
            }
        }
    }
}