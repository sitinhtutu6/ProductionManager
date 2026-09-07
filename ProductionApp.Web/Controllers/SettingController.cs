using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductionManager.Data;
using ProductionApp.Web.Constants;
using System;
using System.Linq;

namespace ProductionApp.Web.Controllers
{
    public class SettingsViewModel
    {
        public SystemNotification Notification { get; set; }
        public CompanyConfig Company { get; set; }
    }

    [Authorize]
    public class SettingController : Controller
    {
        private readonly AppDbContext _context;

        public SettingController(AppDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. MỞ TRANG CẤU HÌNH CHUNG
        // ==========================================
        [Authorize(Policy = AppPermissions.Orders.Create)] // Hoặc Roles="Admin"
        public IActionResult Index()
        {
            // Lấy cấu hình Thông báo (Ticker)
            var notification = _context.SystemNotifications.FirstOrDefault();
            if (notification == null)
            {
                notification = new SystemNotification
                {
                    Message = "Thông báo: lịch nghỉ Tết 2026 từ ngày .... đến ngày ....",
                    StartTime = DateTime.Now,
                    EndTime = DateTime.Now.AddDays(7),
                    IsEnabled = true
                };
                _context.SystemNotifications.Add(notification);
                _context.SaveChanges();
            }

            // Lấy cấu hình Công ty
            var company = _context.CompanyConfigs.FirstOrDefault();
            if (company == null)
            {
                company = new CompanyConfig();
                _context.CompanyConfigs.Add(company);
                _context.SaveChanges();
            }

            // Gộp vào ViewModel để đẩy ra View
            var model = new SettingsViewModel
            {
                Notification = notification,
                Company = company
            };

            return View(model);
        }

        // ==========================================
        // 2. LƯU CẤU HÌNH THÔNG BÁO (TICKER)
        // ==========================================
        [HttpPost]
        public IActionResult UpdateNotification([Bind(Prefix = "Notification")] SystemNotification Notification)
        {
            var setting = _context.SystemNotifications.FirstOrDefault();
            if (setting != null)
            {
                setting.Message = Notification.Message;
                setting.StartTime = Notification.StartTime;
                setting.EndTime = Notification.EndTime;
                setting.IsEnabled = Notification.IsEnabled;

                _context.SaveChanges();
                TempData["SuccessNotification"] = "✅ Đã cập nhật thông báo màn hình thành công!";
            }
            return RedirectToAction("Index");
        }

        // ==========================================
        // 3. LƯU CẤU HÌNH CÔNG TY (XUẤT PDF/EXCEL)
        // ==========================================
        [HttpPost]
        public IActionResult UpdateCompany([Bind(Prefix = "Company")] CompanyConfig Company)
        {
            var setting = _context.CompanyConfigs.FirstOrDefault();
            if (setting != null)
            {
                setting.CompanyName = Company.CompanyName;
                setting.Address = Company.Address;
                setting.TaxCode = Company.TaxCode;
                setting.Phone = Company.Phone;
                setting.Email = Company.Email;

                _context.SaveChanges();
                TempData["SuccessCompany"] = "✅ Đã cập nhật thông tin Doanh nghiệp thành công!";
            }
            return RedirectToAction("Index");
        }
    }
}