using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductionManager.Data; // Sửa lại namespace này cho khớp với Project của bạn

namespace ProductionManager.Controllers
{
    // Thường thì chỉ có Admin hoặc Quản lý cấp cao mới được xem Nhật ký hệ thống
    [Authorize(Roles = "Admin,Manager")]
    public class AuditLogController : Controller
    {
        private readonly AppDbContext _context;

        public AuditLogController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(DateTime? fromDate, DateTime? toDate, string? userName, string? tableName)
        {
            // Mặc định lấy 7 ngày gần nhất nếu không chọn ngày
            var startDate = fromDate ?? DateTime.Now.Date.AddDays(-7);
            var endDate = toDate ?? DateTime.Now.Date;
            var endDateTime = endDate.Date.AddDays(1).AddTicks(-1);

            var query = _context.AuditLogs.AsQueryable();

            // Lọc theo ngày
            query = query.Where(x => x.Timestamp >= startDate && x.Timestamp <= endDateTime);

            // Lọc theo User
            if (!string.IsNullOrEmpty(userName))
                query = query.Where(x => x.UserName != null && x.UserName.Contains(userName));

            // Lọc theo Bảng
            if (!string.IsNullOrEmpty(tableName))
                query = query.Where(x => x.TableName == tableName);

            // Lấy tối đa 500 dòng mới nhất để tránh đơ máy nếu dữ liệu quá lớn
            var logs = await query.OrderByDescending(x => x.Timestamp).Take(500).ToListAsync();

            ViewBag.FromDate = startDate;
            ViewBag.ToDate = endDate;
            ViewBag.UserName = userName;
            ViewBag.TableName = tableName;

            return View(logs);
        }
    }
}