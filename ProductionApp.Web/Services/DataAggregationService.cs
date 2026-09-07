using Microsoft.EntityFrameworkCore;
using ProductionApp.Web.Constants;
using ProductionManager.Data;
using System.Text.Json;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace ProductionApp.Web.Services
{
    public class DataAggregationService
    {
        private readonly AppDbContext _context;

        public DataAggregationService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<string> GetFullBusinessContextAsync()
        {
            // Mốc thời gian báo cáo
            var now = DateTime.Now;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var today = now.Date;

            // =========================================================================
            // 1. KINH DOANH (Orders - Doanh thu & Đơn hàng)
            // =========================================================================
            var orders = await _context.Orders
                .Where(o => o.OrderDate >= startOfMonth)
                .Select(o => new { o.Status, o.TotalAmount })
                .ToListAsync();

            var revenueContext = new
            {
                DoanhThuThangNay = orders.Where(o => o.Status == OrderStatus.Completed).Sum(o => o.TotalAmount).ToString("N0") + " VNĐ",
                SoDonChoDuyet = orders.Count(o => o.Status == OrderStatus.PendingApproval),
                SoDonDangSanXuat = orders.Count(o => o.Status == OrderStatus.InProduction),
                TyLeHoanThanh = orders.Any()
                    ? Math.Round((double)orders.Count(o => o.Status == OrderStatus.Completed) / orders.Count * 100, 1) + "%"
                    : "0%"
            };

            // =========================================================================
            // 2. TÀI CHÍNH & SỔ QUỸ (CashBook - Dòng tiền thực tế)
            // =========================================================================

            // A. Lấy dữ liệu dòng tiền từ đầu tháng (Để liệt kê chi tiết)
            // Lưu ý: Cần chuyển Enum sang String (.ToString()) để AI đọc được
            var cashFlows = await _context.CashEntries
                .Where(x => x.TransactionDate >= startOfMonth)
                .Select(x => new {
                    Type = x.Type.ToString(), // Chuyển Enum thành string "Thu"/"Chi"
                    x.Amount,
                    x.Description
                })
                .ToListAsync();

            // B. Tính tồn quỹ hiện tại (Tổng Thu - Tổng Chi toàn lịch sử)
            // Fix lỗi: So sánh với Enum TransactionType.Thu / Chi thay vì string "Thu"/"Chi"
            var totalThu = await _context.CashEntries
                .Where(x => x.Type == TransactionType.Receipt)
                .SumAsync(x => x.Amount);

            var totalChi = await _context.CashEntries
                .Where(x => x.Type == TransactionType.Payment)
                .SumAsync(x => x.Amount);

            var currentBalance = totalThu - totalChi;

            // C. Lấy top 3 khoản chi lớn nhất tháng này để cảnh báo
            var topExpenses = cashFlows
                .Where(x => x.Type == "Chi") // Ở list cashFlows đã select ra string rồi nên so sánh string ok
                .OrderByDescending(x => x.Amount)
                .Take(3)
                .Select(x => $"{x.Description} ({x.Amount:N0})");

            // D. Đóng gói dữ liệu vào biến financeContext
            var financeContext = new
            {
                TonQuyHienTai = currentBalance.ToString("N0") + " VNĐ",
                DongTienThangNay = new
                {
                    ThuVao = cashFlows.Where(x => x.Type == "Thu").Sum(x => x.Amount).ToString("N0"),
                    ChiRa = cashFlows.Where(x => x.Type == "Chi").Sum(x => x.Amount).ToString("N0"),
                    KetQua = (cashFlows.Where(x => x.Type == "Thu").Sum(x => x.Amount) -
                              cashFlows.Where(x => x.Type == "Chi").Sum(x => x.Amount)).ToString("N0")
                },
                CanhBaoChiTieu = topExpenses.Any() ? topExpenses : null
            };

            // =========================================================================
            // 3. KHO VẬN (WarehouseItems - Tồn kho báo động)
            // =========================================================================
            // Chỉ lấy 5 mặt hàng sắp hết để báo cáo AI
            var lowStockItems = await _context.WarehouseItems
                .Where(x => x.StockQuantity <= 10) // Ngưỡng báo động
                .OrderBy(x => x.StockQuantity)
                .Take(5)
                .Select(x => $"{x.Name} (còn {x.StockQuantity} {x.Unit})")
                .ToListAsync();

            var inventoryContext = new
            {
                TongMaHang = await _context.WarehouseItems.CountAsync(),
                CanhBaoSapHet = lowStockItems.Any() ? lowStockItems : new List<string> { "Kho ổn định" }
            };

            // =========================================================================
            // 4. CÔNG NỢ (MaterialImports - Nợ nhà cung cấp)
            // =========================================================================
            var debts = await _context.MaterialImports
                .Where(x => x.PaymentStatus != ImportPaymentStatus.Paid)
                .Select(x => new { x.TotalAmount, x.PaidAmount })
                .ToListAsync();

            var debtContext = new
            {
                TongNoPhaiTra = debts.Sum(x => x.TotalAmount - x.PaidAmount).ToString("N0") + " VNĐ",
                SoPhieuChuaThanhToan = debts.Count
            };

            // =========================================================================
            // 5. NHÂN SỰ (Users - Tổng hợp theo vai trò, Ẩn danh tính)
            // =========================================================================
            // Yêu cầu: Đã thêm DbSet<IdentityUserRole<string>> UserRoles trong AppDbContext
            var userStats = await _context.Roles
                .Select(role => new
                {
                    ChucVu = role.Name,
                    SoLuong = _context.UserRoles.Count(ur => ur.RoleId == role.Id)
                })
                .ToListAsync();

            var hrContext = new
            {
                TongNhanSu = await _context.Users.CountAsync(u => u.IsActive),
                PhanBo = userStats
            };

            // =========================================================================
            // 6. ĐÓNG GÓI DỮ LIỆU (FINAL PAYLOAD)
            // =========================================================================
            var fullContext = new
            {
                BaoCaoLuc = now.ToString("dd/MM/yyyy HH:mm"),
                KinhDoanh = revenueContext,
                TaiChinh_SoQuy = financeContext,
                KhoBai = inventoryContext,
                CongNo = debtContext,
                NhanSu = hrContext,
                GhiChu = "Đơn vị tiền tệ: VNĐ. Dữ liệu được trích xuất tự động từ hệ thống ERP."
            };

            // Chuyển thành chuỗi JSON đẹp để AI dễ đọc
            return JsonSerializer.Serialize(fullContext, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = null // Giữ nguyên tên tiếng Việt/Anh
            });
        }
    }
}