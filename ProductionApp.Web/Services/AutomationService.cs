using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ProductionApp.Web.Services
{
    public class AutomationService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AutomationService> _logger;

        // URL của n8n (mặc định port 5678)
        private const string N8N_BASE_URL = "http://localhost:5678/webhook/";

        public AutomationService(HttpClient httpClient, ILogger<AutomationService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            // 🔥 QUAN TRỌNG: Chỉ đợi tối đa 2 giây. 
            // Nếu n8n chưa bật hoặc bị treo -> Bỏ qua luôn để Web chính chạy tiếp.
            _httpClient.Timeout = TimeSpan.FromSeconds(2);
        }

        public async Task Trigger(string webhookSlug, object data)
        {
            try
            {
                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Gửi request POST
                var response = await _httpClient.PostAsync(N8N_BASE_URL + webhookSlug, content);

                // (Tùy chọn) Kiểm tra kết quả nếu muốn log
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"[Automation] n8n trả về lỗi: {response.StatusCode}");
                }
            }
            catch (HttpRequestException)
            {
                // Lỗi này xảy ra khi KHÔNG kết nối được (n8n chưa bật)
                // -> Nuốt lỗi, coi như không có chuyện gì xảy ra
                _logger.LogInformation("[Automation] Không kết nối được n8n. Bỏ qua.");
            }
            catch (TaskCanceledException)
            {
                // Lỗi này xảy ra khi Timeout (quá 2 giây)
                _logger.LogInformation("[Automation] Gửi sang n8n quá lâu -> Bỏ qua.");
            }
            catch (Exception ex)
            {
                // Các lỗi khác
                _logger.LogError($"[Automation] Lỗi lạ: {ex.Message}");
            }
        }
    }
}