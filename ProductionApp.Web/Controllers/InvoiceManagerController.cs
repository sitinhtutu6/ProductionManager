using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductionManager.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ProductionApp.Web.Controllers
{
    [Authorize]
    public class InvoiceManagerController : Controller
    {
        private readonly AppDbContext _context;

        public InvoiceManagerController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        // ==========================================
        // 1. API LẤY DỮ LIỆU HÓA ĐƠN
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> GetInvoices(int type, DateTime fromDate, DateTime toDate)
        {
            toDate = toDate.Date.AddDays(1).AddTicks(-1);

            if (type == 1) // 1. HÓA ĐƠN ĐẦU RA (AR INVOICES)
            {
                var invoices = await _context.Invoices
                    .Include(i => i.InvoiceDetails)
                        .ThenInclude(d => d.Order)
                            .ThenInclude(o => o.Customer)
                    .Where(i => i.InvoiceDate >= fromDate && i.InvoiceDate <= toDate)
                    .OrderByDescending(i => i.InvoiceDate)
                    .ToListAsync();

                var orderIds = invoices.SelectMany(i => i.InvoiceDetails.Select(d => d.OrderId)).Distinct().ToList();
                var cashEntries = await _context.CashEntries
                    .Where(c => c.Type == TransactionType.Receipt && c.OrderId != null && orderIds.Contains(c.OrderId.Value))
                    .ToListAsync();

                var result = invoices.Select(i => new
                {
                    id = i.Id,
                    invoiceNumber = i.InvoiceNumber,
                    invoiceDate = i.InvoiceDate.ToString("yyyy-MM-dd"),
                    partnerName = i.InvoiceDetails.FirstOrDefault()?.Order?.Customer?.CompanyName ?? "Khách lẻ đa đơn",
                    totalAmount = i.TotalAmount,
                    lastPaymentDate = cashEntries.Where(c => i.InvoiceDetails.Select(d => d.OrderId).Contains(c.OrderId.Value))
                                                 .OrderByDescending(c => c.TransactionDate).FirstOrDefault()?.TransactionDate.ToString("dd/MM/yyyy") ?? "",
                    note = "Hóa đơn VAT bán ra",
                    file = i.InvoiceFile,
                    linkedDocs = i.InvoiceDetails.Select(d => new
                    {
                        docCode = d.Order?.OrderCode ?? "N/A",
                        docType = "Đơn Hàng (Sale Order)",
                        amount = d.BilledAmount
                    }).ToList()
                });

                return Json(new { success = true, data = result });
            }
            else // 2. HÓA ĐƠN ĐẦU VÀO (AP INVOICES)
            {
                var apInvoices = await _context.APInvoices
                    .Where(i => i.InvoiceDate >= fromDate && i.InvoiceDate <= toDate)
                    .OrderByDescending(i => i.InvoiceDate)
                    .ToListAsync();

                var apIds = apInvoices.Select(i => i.Id).ToList();
                var apCodes = apInvoices.Select(i => i.InvoiceNumber).ToList();

                // ĐÃ BỔ SUNG QUÉT PHIẾU NHẬP KHO
                var linkedPOs = await _context.PurchaseOrders.Where(p => apCodes.Contains(p.InvoiceNumber)).ToListAsync();
                var linkedExports = await _context.ExportShipments.Where(e => e.APInvoiceId != null && apIds.Contains(e.APInvoiceId.Value)).ToListAsync();
                var linkedImports = await _context.MaterialImports.Where(m => m.APInvoiceId != null && apIds.Contains(m.APInvoiceId.Value)).ToListAsync();

                var cashEntries = await _context.CashEntries
                    .Where(c => c.Type == TransactionType.Payment && c.APInvoiceId != null && apIds.Contains(c.APInvoiceId.Value))
                    .ToListAsync();

                var result = apInvoices.Select(i =>
                {
                    var links = new List<dynamic>();

                    var pos = linkedPOs.Where(p => p.InvoiceNumber == i.InvoiceNumber).ToList();
                    foreach (var po in pos) links.Add(new { docCode = po.POCode, docType = "Đơn đặt hàng (PO)", amount = po.FinalTotal });

                    var exps = linkedExports.Where(e => e.APInvoiceId == i.Id).ToList();
                    foreach (var exp in exps) links.Add(new { docCode = exp.ShipmentCode, docType = "Cước Vận Tải", amount = exp.ShippingCost });

                    var imps = linkedImports.Where(m => m.APInvoiceId == i.Id).ToList();
                    foreach (var imp in imps) links.Add(new { docCode = imp.ImportCode, docType = "Phiếu Nhập Kho", amount = imp.TotalAmount });

                    return new
                    {
                        id = i.Id,
                        invoiceNumber = i.InvoiceNumber,
                        invoiceDate = i.InvoiceDate.ToString("yyyy-MM-dd"),
                        partnerName = i.PartnerName,
                        totalAmount = i.TotalAmount,
                        paidAmount = i.PaidAmount,
                        lastPaymentDate = cashEntries.Where(c => c.APInvoiceId == i.Id).OrderByDescending(c => c.TransactionDate).FirstOrDefault()?.TransactionDate.ToString("dd/MM/yyyy") ?? "",
                        note = i.Note,
                        linkedDocs = links
                    };
                });

                return Json(new { success = true, data = result });
            }
        }

        // ==========================================
        // 2. XUẤT EXCEL BÁO CÁO HÓA ĐƠN (HỖ TRỢ 2 CHẾ ĐỘ XEM)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> ExportExcel(int type, DateTime fromDate, DateTime toDate, string mode)
        {
            toDate = toDate.Date.AddDays(1).AddTicks(-1);
            string sheetTitle = type == 1 ? "BẢNG KÊ HÓA ĐƠN BÁN RA (ĐẦU RA)" : "BẢNG KÊ HÓA ĐƠN MUA VÀO (ĐẦU VÀO)";

            using (var wb = new XLWorkbook())
            {
                var ws = wb.Worksheets.Add("HoaDon");

                // Header Báo cáo
                ws.Cell(1, 1).Value = sheetTitle;
                ws.Range("A1:I1").Merge().Style.Font.SetBold().Font.SetFontSize(16).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell(2, 1).Value = $"Từ ngày {fromDate:dd/MM/yyyy} đến ngày {toDate:dd/MM/yyyy}";
                ws.Range("A2:I2").Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center).Font.SetItalic();

                int row = 4;

                // TỰ ĐỘNG CHIA CỘT THEO VIEW MODE
                string[] headers = mode == "linked"
                    ? new[] { "Ngày HĐ", "Số Hóa Đơn", "Công ty / Đối tác", "Giá trị HĐ", "Trạng thái Chi/Thu", "Ghi chú", "Mã Chứng Từ Gốc", "Loại Chứng Từ", "Giá trị phân bổ" }
                    : new[] { "Ngày HĐ", "Số Hóa Đơn", "Công ty / Đối tác", "Giá trị HĐ", "Trạng thái Chi/Thu", "Ngày thanh toán", "Ghi chú" };

                for (int i = 0; i < headers.Length; i++)
                {
                    ws.Cell(row, i + 1).Value = headers[i];
                    ws.Cell(row, i + 1).Style.Font.SetBold().Fill.SetBackgroundColor(XLColor.LightGray).Border.SetOutsideBorder(XLBorderStyleValues.Thin);
                }
                row++;

                if (type == 1) // ---------------- ĐẦU RA ----------------
                {
                    var invoices = await _context.Invoices.Include(i => i.InvoiceDetails).ThenInclude(d => d.Order).ThenInclude(o => o.Customer).Where(i => i.InvoiceDate >= fromDate && i.InvoiceDate <= toDate).OrderByDescending(i => i.InvoiceDate).ToListAsync();
                    var orderIds = invoices.SelectMany(i => i.InvoiceDetails.Select(d => d.OrderId)).Distinct().ToList();
                    var cashEntries = await _context.CashEntries.Where(c => c.Type == TransactionType.Receipt && c.OrderId != null && orderIds.Contains(c.OrderId.Value)).ToListAsync();

                    foreach (var inv in invoices)
                    {
                        string pName = inv.InvoiceDetails.FirstOrDefault()?.Order?.Customer?.CompanyName ?? "Khách lẻ đa đơn";
                        var lastPay = cashEntries.Where(c => inv.InvoiceDetails.Select(d => d.OrderId).Contains(c.OrderId.Value)).OrderByDescending(c => c.TransactionDate).FirstOrDefault();

                        if (mode == "linked") // Nếu chọn dạng Phân bổ (Linked)
                        {
                            foreach (var detail in inv.InvoiceDetails)
                            {
                                ws.Cell(row, 1).Value = inv.InvoiceDate.ToString("dd/MM/yyyy");
                                ws.Cell(row, 2).Value = inv.InvoiceNumber;
                                ws.Cell(row, 3).Value = pName;
                                ws.Cell(row, 4).Value = inv.TotalAmount;
                                ws.Cell(row, 5).Value = lastPay != null ? "Đã thu tiền" : "Chưa thu";
                                ws.Cell(row, 6).Value = "Hóa đơn xuất bán";
                                ws.Cell(row, 7).Value = detail.Order?.OrderCode;
                                ws.Cell(row, 8).Value = "Đơn Hàng";
                                ws.Cell(row, 9).Value = detail.BilledAmount;
                                row++;
                            }
                        }
                        else // Nếu chọn dạng Lưới (Grid)
                        {
                            ws.Cell(row, 1).Value = inv.InvoiceDate.ToString("dd/MM/yyyy");
                            ws.Cell(row, 2).Value = inv.InvoiceNumber;
                            ws.Cell(row, 3).Value = pName;
                            ws.Cell(row, 4).Value = inv.TotalAmount;
                            ws.Cell(row, 5).Value = lastPay != null ? "Đã thu tiền" : "Chưa thu";
                            ws.Cell(row, 6).Value = lastPay?.TransactionDate.ToString("dd/MM/yyyy") ?? "";
                            ws.Cell(row, 7).Value = "Hóa đơn xuất bán";
                            row++;
                        }
                    }
                }
                else // ---------------- ĐẦU VÀO ----------------
                {
                    var apInvoices = await _context.APInvoices.Where(i => i.InvoiceDate >= fromDate && i.InvoiceDate <= toDate).OrderByDescending(i => i.InvoiceDate).ToListAsync();
                    var apIds = apInvoices.Select(i => i.Id).ToList();
                    var apCodes = apInvoices.Select(i => i.InvoiceNumber).ToList();

                    var linkedPOs = await _context.PurchaseOrders.Where(p => apCodes.Contains(p.InvoiceNumber)).ToListAsync();
                    var linkedExports = await _context.ExportShipments.Where(e => e.APInvoiceId != null && apIds.Contains(e.APInvoiceId.Value)).ToListAsync();
                    var linkedImports = await _context.MaterialImports.Where(m => m.APInvoiceId != null && apIds.Contains(m.APInvoiceId.Value)).ToListAsync();

                    var cashEntries = await _context.CashEntries.Where(c => c.Type == TransactionType.Payment && c.APInvoiceId != null && apIds.Contains(c.APInvoiceId.Value)).ToListAsync();

                    foreach (var inv in apInvoices)
                    {
                        var lastPay = cashEntries.Where(c => c.APInvoiceId == inv.Id).OrderByDescending(c => c.TransactionDate).FirstOrDefault();

                        if (mode == "linked") // Dạng Phân Bổ (Linked)
                        {
                            var pos = linkedPOs.Where(p => p.InvoiceNumber == inv.InvoiceNumber).ToList();
                            var exps = linkedExports.Where(e => e.APInvoiceId == inv.Id).ToList();
                            var imps = linkedImports.Where(m => m.APInvoiceId == inv.Id).ToList();

                            if (!pos.Any() && !exps.Any() && !imps.Any())
                            {
                                ws.Cell(row, 1).Value = inv.InvoiceDate.ToString("dd/MM/yyyy");
                                ws.Cell(row, 2).Value = inv.InvoiceNumber; ws.Cell(row, 3).Value = inv.PartnerName; ws.Cell(row, 4).Value = inv.TotalAmount;
                                ws.Cell(row, 5).Value = inv.PaidAmount >= inv.TotalAmount ? "Đã chi xong" : (inv.PaidAmount > 0 ? "Chi 1 phần" : "Chưa chi");
                                ws.Cell(row, 6).Value = inv.Note; ws.Cell(row, 7).Value = "-"; ws.Cell(row, 8).Value = "Chi phí chung"; ws.Cell(row, 9).Value = inv.TotalAmount;
                                row++;
                            }

                            foreach (var po in pos)
                            {
                                ws.Cell(row, 1).Value = inv.InvoiceDate.ToString("dd/MM/yyyy"); ws.Cell(row, 2).Value = inv.InvoiceNumber; ws.Cell(row, 3).Value = inv.PartnerName; ws.Cell(row, 4).Value = inv.TotalAmount; ws.Cell(row, 5).Value = inv.PaidAmount >= inv.TotalAmount ? "Đã chi xong" : (inv.PaidAmount > 0 ? "Chi 1 phần" : "Chưa chi"); ws.Cell(row, 6).Value = inv.Note;
                                ws.Cell(row, 7).Value = po.POCode; ws.Cell(row, 8).Value = "Đơn đặt hàng PO"; ws.Cell(row, 9).Value = po.FinalTotal; row++;
                            }
                            foreach (var exp in exps)
                            {
                                ws.Cell(row, 1).Value = inv.InvoiceDate.ToString("dd/MM/yyyy"); ws.Cell(row, 2).Value = inv.InvoiceNumber; ws.Cell(row, 3).Value = inv.PartnerName; ws.Cell(row, 4).Value = inv.TotalAmount; ws.Cell(row, 5).Value = inv.PaidAmount >= inv.TotalAmount ? "Đã chi xong" : (inv.PaidAmount > 0 ? "Chi 1 phần" : "Chưa chi"); ws.Cell(row, 6).Value = inv.Note;
                                ws.Cell(row, 7).Value = exp.ShipmentCode; ws.Cell(row, 8).Value = "Cước vận tải"; ws.Cell(row, 9).Value = exp.ShippingCost; row++;
                            }
                            foreach (var imp in imps)
                            {
                                ws.Cell(row, 1).Value = inv.InvoiceDate.ToString("dd/MM/yyyy"); ws.Cell(row, 2).Value = inv.InvoiceNumber; ws.Cell(row, 3).Value = inv.PartnerName; ws.Cell(row, 4).Value = inv.TotalAmount; ws.Cell(row, 5).Value = inv.PaidAmount >= inv.TotalAmount ? "Đã chi xong" : (inv.PaidAmount > 0 ? "Chi 1 phần" : "Chưa chi"); ws.Cell(row, 6).Value = inv.Note;
                                ws.Cell(row, 7).Value = imp.ImportCode; ws.Cell(row, 8).Value = "Phiếu Nhập Kho"; ws.Cell(row, 9).Value = imp.TotalAmount; row++;
                            }
                        }
                        else // Dạng Lưới (Grid)
                        {
                            ws.Cell(row, 1).Value = inv.InvoiceDate.ToString("dd/MM/yyyy");
                            ws.Cell(row, 2).Value = inv.InvoiceNumber;
                            ws.Cell(row, 3).Value = inv.PartnerName;
                            ws.Cell(row, 4).Value = inv.TotalAmount;
                            ws.Cell(row, 5).Value = inv.PaidAmount >= inv.TotalAmount ? "Đã chi xong" : (inv.PaidAmount > 0 ? "Chi 1 phần" : "Chưa chi");
                            ws.Cell(row, 6).Value = lastPay?.TransactionDate.ToString("dd/MM/yyyy") ?? "";
                            ws.Cell(row, 7).Value = inv.Note;
                            row++;
                        }
                    }
                }

                // Format cột tiền tệ
                ws.Columns().AdjustToContents();
                ws.Column(4).Style.NumberFormat.Format = "#,##0";
                if (mode == "linked") ws.Column(9).Style.NumberFormat.Format = "#,##0";

                using (var stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    string fileName = type == 1 ? $"HoaDonDauRa_{DateTime.Now:ddMM}.xlsx" : $"HoaDonDauVao_{DateTime.Now:ddMM}.xlsx";
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
        }
    }
}