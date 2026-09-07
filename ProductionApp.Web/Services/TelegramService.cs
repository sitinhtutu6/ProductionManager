using ProductionManager.Data; // Để dùng ServerConfig

namespace ProductionApp.Web.Services
{
    public class TelegramService
    {
        private readonly HttpClient _httpClient;
        private readonly ServerConfig _config; // Inject Config

        public TelegramService(HttpClient httpClient, ServerConfig config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        public async Task SendMessageAsync(string message)
        {
            // Lấy Token động từ MainForm
            var token = _config.TelegramToken;
            var chatId = _config.TelegramChatId;

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(chatId)) return;

            string url = $"https://api.telegram.org/bot{token}/sendMessage?chat_id={chatId}&text={System.Net.WebUtility.UrlEncode(message)}&parse_mode=Markdown";

            try { _ = _httpClient.GetAsync(url); } catch { }
        }
    }
}