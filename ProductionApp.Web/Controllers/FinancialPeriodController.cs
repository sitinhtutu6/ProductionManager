using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductionManager.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ProductionApp.Web.Controllers
{
    // Chỉ Admin hoặc Manager mới được quyền Khóa sổ
    [Authorize(Roles = "Admin,Manager")]
    public class FinancialPeriodController : Controller
    {
        private readonly AppDbContext _context;

        public FinancialPeriodController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Tự động sinh ra danh sách 12 tháng gần nhất nếu chưa có trong DB
            var currentDate = DateTime.Now;
            for (int i = 0; i <= 12; i++)
            {
                var d = currentDate.AddMonths(-i);
                bool exists = await _context.FinancialPeriods.AnyAsync(p => p.Month == d.Month && p.Year == d.Year);
                if (!exists)
                {
                    _context.FinancialPeriods.Add(new FinancialPeriod { Month = d.Month, Year = d.Year, IsClosed = false });
                }
            }
            await _context.SaveChangesAsync();

            var periods = await _context.FinancialPeriods
                .OrderByDescending(p => p.Year).ThenByDescending(p => p.Month)
                .ToListAsync();

            return View(periods);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleLock(int id)
        {
            var period = await _context.FinancialPeriods.FindAsync(id);
            if (period != null)
            {
                period.IsClosed = !period.IsClosed; // Đảo trạng thái
                period.ClosedAt = period.IsClosed ? DateTime.Now : null;
                period.ClosedBy = period.IsClosed ? User.Identity.Name : null;

                await _context.SaveChangesAsync();
                TempData["Success"] = period.IsClosed ? $"Đã KHÓA SỔ Tháng {period.Month}/{period.Year}" : $"Đã MỞ KHÓA Tháng {period.Month}/{period.Year}";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}