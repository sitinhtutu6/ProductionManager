using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using ProductionManager.Data;
using ProductionApp.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace ProductionApp.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        // Inject DbContext để "Giám đốc" (Home) có thể xem dữ liệu các phòng ban
        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // ==========================================
                // 1. SỐ LIỆU TỔNG QUAN (CARDS)
                // ==========================================
                ViewBag.PendingOrders = await _context.Orders.CountAsync(x => x.Status == OrderStatus.PendingApproval);
                ViewBag.TotalOrders = await _context.Orders.CountAsync();
                ViewBag.TotalProducts = await _context.ProductDetails.CountAsync();

                // Dùng try-catch cho bảng kho để tránh lỗi nếu chưa tạo bảng
                try
                {
                    ViewBag.LowStockItems = await _context.WarehouseItems.CountAsync(x => x.StockQuantity < 10);
                }
                catch { ViewBag.LowStockItems = 0; }


                // ==========================================
                // 2. BIỂU ĐỒ DOANH THU (FIX LỖI KHÔNG HIỆN DỮ LIỆU)
                // ==========================================

                // BƯỚC A: Lấy dữ liệu thô từ SQL về trước (Load về RAM)
                // Chỉ lấy cột Ngày và Tổng tiền để nhẹ gánh
                var rawOrders = await _context.Orders
                    .Where(o => o.OrderDate >= DateTime.Now.AddMonths(-6) && o.Status != OrderStatus.Canceled)
                    .Select(o => new { o.OrderDate, o.TotalAmount })
                    .ToListAsync(); // <--- Lệnh này sẽ chạy SQL ngay lập tức

                // BƯỚC B: Tính toán Group By bằng C# (Trên RAM)
                // Cách này an toàn tuyệt đối, không bao giờ bị lỗi dịch SQL
                var revenueData = rawOrders
                    .GroupBy(x => new { x.OrderDate.Year, x.OrderDate.Month })
                    .Select(g => new {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        Total = g.Sum(x => x.TotalAmount)
                    })
                    .OrderBy(x => x.Year).ThenBy(x => x.Month)
                    .ToList();

                // BƯỚC C: Chuẩn bị List để gửi sang View vẽ biểu đồ
                var chartLabels = new List<string>();
                var chartValues = new List<decimal>();

                if (revenueData.Count > 0)
                {
                    foreach (var item in revenueData)
                    {
                        chartLabels.Add($"T{item.Month}/{item.Year}");
                        chartValues.Add(item.Total);
                    }
                }
                else
                {
                    // Nếu không có đơn hàng nào, tạo dữ liệu giả để test biểu đồ hiện lên
                    chartLabels.Add("Chưa có dữ liệu");
                    chartValues.Add(0);
                }

                ViewBag.ChartLabels = chartLabels;
                ViewBag.ChartValues = chartValues;


                // ==========================================
                // 3. BIỂU ĐỒ TRẠNG THÁI (CŨNG TÍNH TRÊN RAM)
                // ==========================================
                var rawStatus = await _context.Orders
                    .Select(o => o.Status)
                    .ToListAsync();

                var statusData = rawStatus
                    .GroupBy(x => x)
                    .Select(g => new { Status = g.Key, Count = g.Count() })
                    .ToList();

                ViewBag.StatusLabels = statusData.Select(x => x.Status.ToString()).ToList();
                ViewBag.StatusValues = statusData.Select(x => x.Count).ToList();
            }
            catch (Exception ex)
            {
                // Ghi lỗi ra cửa sổ Output của Visual Studio
                Debug.WriteLine("LỖI DASHBOARD: " + ex.Message);

                // Khởi tạo list rỗng để web không bị chết (Error 500)
                ViewBag.ChartLabels = new List<string>();
                ViewBag.ChartValues = new List<decimal>();
                ViewBag.StatusLabels = new List<string>();
                ViewBag.StatusValues = new List<int>();
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}