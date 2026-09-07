using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace ProductionApp.Web.Controllers // Chú ý: Nhớ giữ đúng namespace của bạn nhé
{
    public class LanguageController : Controller
    {
        [HttpGet]
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            var cookieOptions = new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true,
                Path = "/",
                Secure = false, // 🔥 Tạm tắt Secure để Server HTTP chạy được
                SameSite = SameSiteMode.Lax
            };

            // 🔥 PHÂN XỬ THÔNG MINH CHO NGROK (HTTPS) VÀ LOCAL (HTTP)
            if (Request.IsHttps)
            {
                cookieOptions.Secure = true;
                cookieOptions.SameSite = SameSiteMode.None;
            }
            else
            {
                cookieOptions.Secure = false;
                cookieOptions.SameSite = SameSiteMode.Lax;
            }

            // Dùng đúng tên Cookie mới đã khai báo ở Startup.cs
            Response.Cookies.Append(
                "ProManager.Lang",
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                cookieOptions
            );

            if (string.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))
            {
                returnUrl = "/";
            }

            return LocalRedirect(returnUrl);
        }
    }
}