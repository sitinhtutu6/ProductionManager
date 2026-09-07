using ProductionManager.Data;
using System.Text.Json;
using System.Text;

namespace ProductionApp.Web.Services
{
    public class DiscordService
    {
        private readonly HttpClient _httpClient;
        private readonly ServerConfig _config;

        public DiscordService(HttpClient httpClient, ServerConfig config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        public async Task SendMessageAsync(string content)
        {
            // Lấy Webhook động từ MainForm
            var webhookUrl = _config.DiscordWebhook;

            if (string.IsNullOrEmpty(webhookUrl)) return;

            var payload = new { content = content };
            var json = JsonSerializer.Serialize(payload);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            try { _ = _httpClient.PostAsync(webhookUrl, data); } catch { }
        }
    }
}