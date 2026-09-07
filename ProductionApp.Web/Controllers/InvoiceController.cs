using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProductionApp.Web.Constants;
using ProductionManager.Data;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ProductionApp.Web.Controllers
{
    [Authorize]
    public class InvoiceController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public InvoiceController(AppDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // ==========================================
        // 1. GIAO DIỆN TRA CỨU HÓA ĐƠN
        // ==========================================
        public async Task<IActionResult> Index(string invoiceNo, int? customerId, string productName, DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.Invoices
                .Include(i => i.InvoiceDetails)
                    .ThenInclude(d => d.Order)
                        .ThenInclude(o => o.Customer)
                .AsQueryable();

            // Lọc theo Số HĐ hoặc Mã Đơn (PO)
            if (!string.IsNullOrEmpty(invoiceNo))
            {
                query = query.Where(i => i.InvoiceNumber.Contains(invoiceNo) ||
                                         i.InvoiceDetails.Any(d => d.Order.OrderCode.Contains(invoiceNo)));
            }

            // Lọc theo Tên Sản Phẩm (Nâng cấp mới)
            if (!string.IsNullOrEmpty(productName))
            {
                query = query.Where(i => i.InvoiceDetails.Any(d => d.Order.OrderDetails.Any(od => od.ProductName.Contains(productName))));
            }

            // Lọc thời gian
            if (fromDate.HasValue) query = query.Where(i => i.InvoiceDate >= fromDate.Value);
            if (toDate.HasValue)
            {
                var tEnd = toDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(i => i.InvoiceDate <= tEnd);
            }

            // Lọc Khách hàng
            if (customerId.HasValue)
            {
                query = query.Where(i => i.InvoiceDetails.Any(d => d.Order.CustomerId == customerId.Value));
            }

            var invoices = await query.OrderByDescending(i => i.InvoiceDate).ToListAsync();

            ViewBag.CustomerList = await _context.Customers
                .Where(c => c.Type == PartnerType.Customer || c.Type == PartnerType.Both)
                .OrderBy(c => c.CompanyName)
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.CompanyName })
                .ToListAsync();

            ViewBag.InvoiceNo = invoiceNo;
            ViewBag.CustomerId = customerId;
            ViewBag.ProductName = productName;
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;

            return View(invoices);
        }

        // ==========================================
        // 2. XUẤT EXCEL CHUYÊN NGHIỆP
        // ==========================================
        [HttpGet]
        [Authorize(Policy = AppPermissions.Orders.View)] // Yêu cầu có quyền xem
        public async Task<IActionResult> ExportExcel(string invoiceNo, int? customerId, string productName, DateTime? fromDate, DateTime? toDate)
        {
            // 1. Dùng chung logic lọc dữ liệu với hàm Index
            var query = _context.Invoices
                .Include(i => i.InvoiceDetails)
                    .ThenInclude(d => d.Order)
                        .ThenInclude(o => o.Customer)
                .Include(i => i.InvoiceDetails)
                    .ThenInclude(d => d.Order)
                        .ThenInclude(o => o.OrderDetails)
                .AsQueryable();

            if (!string.IsNullOrEmpty(invoiceNo))
                query = query.Where(i => i.InvoiceNumber.Contains(invoiceNo) || i.InvoiceDetails.Any(d => d.Order.OrderCode.Contains(invoiceNo)));

            if (customerId.HasValue)
                query = query.Where(i => i.InvoiceDetails.Any(d => d.Order.CustomerId == customerId.Value));

            if (!string.IsNullOrEmpty(productName))
                query = query.Where(i => i.InvoiceDetails.Any(d => d.Order.OrderDetails.Any(od => od.ProductName.Contains(productName))));

            if (fromDate.HasValue) query = query.Where(i => i.InvoiceDate >= fromDate.Value);
            if (toDate.HasValue)
            {
                var tEnd = toDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(i => i.InvoiceDate <= tEnd);
            }

            var data = await query.OrderByDescending(i => i.InvoiceDate).ToListAsync();

            // 2. Vẽ bảng Excel với ClosedXML
            using (var wb = new XLWorkbook())
            {
                var ws = wb.Worksheets.Add("SoHoaDon");

                // Header Báo Cáo
                ws.Cell("A1").Value = "CÔNG TY SẢN XUẤT NỘI THẤT";
                ws.Cell("A1").Style.Font.SetBold().Font.SetFontSize(12);
                ws.Cell("A2").Value = "SỔ THEO DÕI HÓA ĐƠN (VAT) ĐẦU RA";
                ws.Range("A2:F2").Merge().Style.Font.SetBold().Font.SetFontSize(16).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                string dateRangeStr = "Tất cả thời gian";
                if (fromDate.HasValue && toDate.HasValue) dateRangeStr = $"Từ {fromDate.Value:dd/MM/yyyy} đến {toDate.Value:dd/MM/yyyy}";
                ws.Cell("A3").Value = $"Kỳ báo cáo: {dateRangeStr} - Ngày lập: {DateTime.Now:dd/MM/yyyy HH:mm}";
                ws.Range("A3:F3").Merge().Style.Font.SetItalic().Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                // Tiêu đề Cột
                string[] headers = { "STT", "Số Hóa Đơn", "Ngày Xuất", "Khách Hàng (Đại diện)", "Đơn Hàng Tham Chiếu (PO)", "Tổng Tiền Thuế (VNĐ)" };
                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = ws.Cell(5, i + 1);
                    cell.Value = headers[i];
                    cell.Style.Font.SetBold().Font.SetFontColor(XLColor.White).Fill.SetBackgroundColor(XLColor.Teal);
                    cell.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                }

                int row = 6; int stt = 1;
                decimal grandTotal = 0;

                foreach (var inv in data)
                {
                    // Lấy ra danh sách khách hàng và PO tương ứng
                    var customerNames = inv.InvoiceDetails.Select(d => d.Order.Customer?.CompanyName).Where(c => c != null).Distinct().ToList();
                    var orderCodes = inv.InvoiceDetails.Select(d => d.Order.OrderCode).Distinct().ToList();

                    ws.Cell(row, 1).Value = stt++;
                    ws.Cell(row, 2).Value = inv.InvoiceNumber;
                    ws.Cell(row, 3).Value = inv.InvoiceDate.ToString("dd/MM/yyyy");
                    ws.Cell(row, 4).Value = string.Join(", ", customerNames);
                    ws.Cell(row, 5).Value = string.Join(", ", orderCodes);
                    ws.Cell(row, 6).Value = inv.TotalAmount;

                    // Định dạng dòng
                    ws.Range(row, 1, row, 6).Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
                    ws.Cell(row, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                    ws.Cell(row, 2).Style.Font.SetBold();
                    ws.Cell(row, 3).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                    ws.Cell(row, 6).Style.NumberFormat.Format = "#,##0";
                    ws.Cell(row, 6).Style.Font.SetBold().Font.SetFontColor(XLColor.DarkGreen);

                    grandTotal += inv.TotalAmount;
                    row++;
                }

                // Dòng Tổng Cộng
                ws.Cell(row, 5).Value = "TỔNG CỘNG DOANH THU XUẤT HĐ:";
                ws.Cell(row, 5).Style.Font.SetBold().Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
                ws.Cell(row, 6).Value = grandTotal;
                ws.Cell(row, 6).Style.Font.SetBold().Font.SetFontColor(XLColor.Red).NumberFormat.Format = "#,##0";
                ws.Range(row, 1, row, 6).Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);

                // Chữ ký
                row += 3;
                ws.Cell(row, 2).Value = "NGƯỜI LẬP BIỂU";
                ws.Cell(row, 5).Value = "GIÁM ĐỐC";
                ws.Range(row, 1, row, 6).Style.Font.SetBold().Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell(row + 1, 2).Value = "(Ký, ghi rõ họ tên)";
                ws.Cell(row + 1, 5).Value = "(Ký, đóng dấu)";
                ws.Range(row + 1, 1, row + 1, 6).Style.Font.SetItalic().Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                ws.Columns().AdjustToContents();
                ws.Column(4).Width = 35; // Rộng cột Khách hàng
                ws.Column(5).Width = 25; // Rộng cột Đơn hàng

                using (var stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"SoHoaDonVAT_{DateTime.Now:ddMMyy}.xlsx");
                }
            }
        }

        // ==========================================
        // 3. SỬA THÔNG TIN HÓA ĐƠN
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken] // Đảm bảo bảo mật AJAX
        [Authorize(Policy = AppPermissions.Orders.Edit)]
        public async Task<IActionResult> Edit(int id, string invoiceNumber, DateTime invoiceDate, IFormFile? invoiceFile)
        {
            var invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.Id == id);
            if (invoice == null) return Json(new { success = false, message = "Không tìm thấy hóa đơn!" });

            try
            {
                invoice.InvoiceNumber = invoiceNumber;
                invoice.InvoiceDate = invoiceDate;

                if (invoiceFile != null && invoiceFile.Length > 0)
                {
                    // Xóa file cũ nếu có
                    if (!string.IsNullOrEmpty(invoice.InvoiceFile))
                    {
                        string oldPath = Path.Combine(_webHostEnvironment.WebRootPath, "invoices", invoice.InvoiceFile);
                        if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                    }

                    // Upload file mới
                    string yearFolder = invoiceDate.ToString("yyyy");
                    string monthFolder = invoiceDate.ToString("MM");
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "invoices", yearFolder, monthFolder);
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    string uniqueFileName = $"INV_{DateTime.Now.Ticks}{Path.GetExtension(invoiceFile.FileName)}";
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await invoiceFile.CopyToAsync(fileStream);
                    }
                    invoice.InvoiceFile = $"{yearFolder}/{monthFolder}/{uniqueFileName}";
                }

                _context.Update(invoice);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Cập nhật hóa đơn thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        // ==========================================
        // 4. XÓA HÓA ĐƠN (VÀ TỰ ĐỘNG LÙI TRẠNG THÁI ORDER)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken] // Đảm bảo bảo mật AJAX
        [Authorize(Policy = AppPermissions.Orders.Delete)]
        public async Task<IActionResult> Delete(int id)
        {
            var invoice = await _context.Invoices
                .Include(i => i.InvoiceDetails)
                    .ThenInclude(d => d.Order)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null) return Json(new { success = false, message = "Không tìm thấy hóa đơn!" });

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Phục hồi trạng thái cho TẤT CẢ các Đơn hàng nằm trong lô Hóa đơn này
                foreach (var detail in invoice.InvoiceDetails)
                {
                    var order = detail.Order;

                    // Tính tổng tiền Hóa đơn của Order này (Bỏ qua tờ Hóa đơn sắp bị xóa)
                    var otherBilledTotal = await _context.InvoiceDetails
                        .Where(d => d.OrderId == order.Id && d.InvoiceId != invoice.Id)
                        .SumAsync(d => d.BilledAmount);

                    // Nếu số tiền hóa đơn còn lại nhỏ hơn tổng trị giá đơn và đơn đang ở trạng thái Invoiced -> Lùi trạng thái
                    if (otherBilledTotal < order.TotalAmount && order.Status == OrderStatus.Invoiced)
                    {
                        // Kiểm tra kho thực tế để lùi trạng thái về Giao hàng hay Đang sản xuất
                        var orderDetails = await _context.OrderDetails.Where(od => od.OrderId == order.Id).ToListAsync();
                        double totalOrdered = orderDetails.Sum(od => od.Quantity);

                        var detailIds = orderDetails.Select(od => od.Id).ToList();
                        double totalShipped = await _context.ShipmentDetails
                            .Where(sd => detailIds.Contains(sd.OrderDetailId) && sd.Shipment.Type == ShipmentType.Standard)
                            .SumAsync(sd => (double?)sd.QuantityShipped) ?? 0;

                        if (totalShipped >= totalOrdered && totalOrdered > 0)
                            order.Status = OrderStatus.Delivered; // Đã giao đủ
                        else
                            order.Status = OrderStatus.InProduction; // Chưa giao đủ

                        _context.Update(order);
                    }
                }

                // Xóa File vật lý
                if (!string.IsNullOrEmpty(invoice.InvoiceFile))
                {
                    string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "invoices", invoice.InvoiceFile);
                    if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
                }

                // Xóa Data
                _context.InvoiceDetails.RemoveRange(invoice.InvoiceDetails);
                _context.Invoices.Remove(invoice);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { success = true, message = "Đã xóa hóa đơn và cập nhật trạng thái Đơn hàng!" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "Lỗi xử lý: " + ex.Message });
            }
        }
    }
}