using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
    public class ProductQuotationController : Controller
    {
        private readonly AppDbContext _context;

        public ProductQuotationController(AppDbContext context)
        {
            _context = context;
        }

        // 1. GIAO DIỆN CHÍNH
        [Authorize(Policy = AppPermissions.Debts.View)]
        public async Task<IActionResult> Index()
        {
            var companyConfig = await _context.CompanyConfigs.FirstOrDefaultAsync() ?? new CompanyConfig();
            ViewBag.CompanyInfo = companyConfig;

            var products = await _context.Products.OrderBy(p => p.ProductCode).ToListAsync();
            return View(products);
        }

        // 2. XUẤT EXCEL (Chuẩn form báo giá theo hình mẫu)
        [HttpPost]
        [Authorize(Policy = AppPermissions.Debts.Create)]
        public async Task<IActionResult> ExportExcel(string customerName)
        {
            // Chỉ lấy các sản phẩm đã có giá
            var products = await _context.Products.Where(p => p.DefaultPrice > 0).OrderBy(p => p.ProductCode).ToListAsync();

            using (var wb = new XLWorkbook())
            {
                var ws = wb.Worksheets.Add("BaoGia");

                // --- HEADER ---
                var title = ws.Range("A1:H1").Merge();
                title.Value = "BẢNG BÁO GIÁ";
                title.Style.Font.SetBold().Font.FontSize = 16;
                title.Style.Font.SetItalic();
                title.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                var dateTitle = ws.Range("A2:H2").Merge();
                dateTitle.Value = DateTime.Now.ToString("dd/MM/yyyy");
                dateTitle.Style.Font.SetBold().Font.SetItalic();
                dateTitle.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                ws.Cell("B4").Value = "KÍNH GỬI:";
                ws.Cell("B4").Style.Font.SetBold().Font.SetUnderline();
                ws.Cell("C4").Value = string.IsNullOrEmpty(customerName) ? "..................................................." : customerName.ToUpper();
                ws.Cell("C4").Style.Font.SetBold();

                // --- TIÊU ĐỀ CỘT ---
                int r = 6;
                ws.Cell(r, 1).Value = "STT";
                ws.Cell(r, 2).Value = "Ngày tháng";
                ws.Cell(r, 3).Value = "Số đơn hàng";
                ws.Cell(r, 4).Value = "Tên sản phẩm";
                ws.Cell(r, 5).Value = "ĐVT";
                ws.Cell(r, 6).Value = "Đơn giá (VNĐ)";
                ws.Cell(r, 7).Value = "Loại gỗ";
                ws.Cell(r, 8).Value = "Ghi chú";

                var headerRange = ws.Range($"A{r}:H{r}");
                headerRange.Style.Font.SetBold();
                headerRange.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                headerRange.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                headerRange.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
                headerRange.Style.Border.SetInsideBorder(XLBorderStyleValues.Thin);

                // --- ĐỔ DỮ LIỆU ---
                r++;
                int stt = 1;
                decimal totalAmount = 0;

                foreach (var item in products)
                {
                    ws.Cell(r, 1).Value = stt++;
                    ws.Cell(r, 2).Value = DateTime.Now.ToString("dd/MM/yyyy"); // Ngày tháng
                    ws.Cell(r, 3).Value = ""; // Số đơn hàng (Để trống theo mẫu)
                    ws.Cell(r, 4).Value = item.ProductName;
                    ws.Cell(r, 5).Value = item.Unit ?? "Cái";
                    ws.Cell(r, 6).Value = item.DefaultPrice;
                    ws.Cell(r, 7).Value = item.MaterialType;
                    ws.Cell(r, 8).Value = item.Note;

                    totalAmount += item.DefaultPrice;
                    r++;
                }

                // --- TỔNG CỘNG ---
                ws.Range($"A{r}:E{r}").Merge().Value = "CỘNG:";
                ws.Range($"A{r}:E{r}").Style.Font.SetBold().Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell(r, 6).Value = totalAmount;
                ws.Cell(r, 6).Style.Font.SetBold();

                // Đóng khung viền toàn bộ bảng
                ws.Range($"A6:H{r}").Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
                ws.Range($"A6:H{r}").Style.Border.SetInsideBorder(XLBorderStyleValues.Thin);

                // --- GHI CHÚ CHÂN TRANG ---
                r += 2;
                ws.Cell(r, 1).Value = "* Ghi chú:"; ws.Cell(r, 1).Style.Font.SetBold(); r++;
                ws.Cell(r, 2).Value = "- Giá chưa bao gồm thuế GTGT"; ws.Cell(r, 2).Style.Font.SetBold(); r++;
                ws.Cell(r, 2).Value = $"- {customerName ?? "Khách hàng"} cấp vật tư"; ws.Cell(r, 2).Style.Font.SetBold();

                // Định dạng cột
                ws.Column(1).Width = 5;
                ws.Column(2).Width = 12;
                ws.Column(3).Width = 15;
                ws.Column(4).Width = 40;
                ws.Column(5).Width = 8;
                ws.Column(6).Width = 15;
                ws.Column(6).Style.NumberFormat.Format = "#,##0"; // Định dạng tiền
                ws.Column(7).Width = 15;
                ws.Column(8).Width = 20;

                using (var stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"BaoGia_{DateTime.Now:yyyyMMdd}.xlsx");
                }
            }
        }

        // 3. IMPORT TỪ EXCEL (Cập nhật giá hàng loạt)
        [HttpPost]
        public async Task<IActionResult> ImportExcel(IFormFile file)
        {
            if (file == null || file.Length == 0) return RedirectToAction("Index");

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                using (var wb = new XLWorkbook(stream))
                {
                    var ws = wb.Worksheet(1);
                    var rows = ws.RangeUsed().RowsUsed().Skip(1); // Bỏ qua dòng tiêu đề

                    foreach (var row in rows)
                    {
                        string productName = row.Cell(4).GetValue<string>(); // Cột Tên SP
                        decimal price = row.Cell(6).GetValue<decimal>();     // Cột Đơn giá

                        if (!string.IsNullOrEmpty(productName) && price > 0)
                        {
                            // Tìm SP theo tên (Hoặc có thể map theo ProductCode nếu bạn thêm cột mã SP vào Excel)
                            var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductName.ToLower() == productName.ToLower());
                            if (product != null)
                            {
                                product.DefaultPrice = price;
                            }
                        }
                    }
                    await _context.SaveChangesAsync();
                }
            }
            TempData["Success"] = "Nhập báo giá thành công!";
            return RedirectToAction("Index");
        }
    }
}