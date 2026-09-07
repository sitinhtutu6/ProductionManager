using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ProductionApp.Web.Models; // Namespace chứa ServerConfig của bạn
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ProductionManager.Data;

namespace ProductionApp.Web.Controllers
{
    [Authorize(Roles = "Admin")] // 🔥 QUAN TRỌNG: Chỉ Admin mới được gọi
    public class SystemController : Controller
    {
        private readonly ServerConfig _config;

        // Inject ServerConfig để lấy ConnectionString và Token Telegram
        public SystemController(ServerConfig config)
        {
            _config = config;
        }

        [HttpPost]
        public async Task<IActionResult> SendDebugInfo()
        {
            if (string.IsNullOrEmpty(_config.TelegramToken) || string.IsNullOrEmpty(_config.TelegramChatId))
            {
                return Json(new { success = false, message = "Server chưa cấu hình Telegram Bot!" });
            }

            try
            {
                // Nội dung tin nhắn gửi đi
                // Dùng Markdown để format đẹp
                var sb = new StringBuilder();
                sb.AppendLine("🔐 **ADMIN DEBUG INFO**");
                sb.AppendLine($"📅 Time: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
                sb.AppendLine("-------------------------");
                sb.AppendLine($"🗄 **DB Provider:** `{_config.DbProvider}`");
                sb.AppendLine($"🔗 **Connection String:**");
                sb.AppendLine($"`{_config.ConnectionString}`");

                // Gửi request lên Telegram API
                string url = $"https://api.telegram.org/bot{_config.TelegramToken}/sendMessage";
                var payload = new
                {
                    chat_id = _config.TelegramChatId,
                    text = sb.ToString(),
                    parse_mode = "Markdown"
                };

                using (var client = new HttpClient())
                {
                    var jsonPayload = JsonSerializer.Serialize(payload);
                    var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync(url, content);

                    if (response.IsSuccessStatusCode)
                        return Json(new { success = true, message = "Đã gửi thông tin bảo mật qua Telegram!" });
                    else
                        return Json(new { success = false, message = $"Lỗi Telegram: {response.ReasonPhrase}" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi Server: " + ex.Message });
            }
        }
    }
}