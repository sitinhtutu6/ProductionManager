using System.Text;
using System.Text.Json;

namespace ProductionApp.Web.Helpers
{
    public static class NotificationHelper
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        // 1. Gửi Telegram
        public static async Task SendTelegram(string token, string chatId, string message)
        {
            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(chatId)) return;

            try
            {
                string url = $"https://api.telegram.org/bot{token}/sendMessage?chat_id={chatId}&text={System.Net.WebUtility.UrlEncode(message)}&parse_mode=Markdown";
                // Fire & Forget: Gửi đi mà không chờ phản hồi để Web chạy nhanh
                _ = _httpClient.GetAsync(url);
            }
            catch { /* Bỏ qua lỗi mạng */ }
        }

        // 2. Gửi Discord
        public static async Task SendDiscord(string webhookUrl, string message)
        {
            if (string.IsNullOrWhiteSpace(webhookUrl)) return;

            try
            {
                var payload = new { content = message };
                var json = JsonSerializer.Serialize(payload);
                var data = new StringContent(json, Encoding.UTF8, "application/json");

                _ = _httpClient.PostAsync(webhookUrl, data);
            }
            catch { /* Bỏ qua lỗi mạng */ }
        }
    }
}