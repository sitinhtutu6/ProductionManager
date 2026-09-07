using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ProductionApp.Web.Services
{
    public class LocalAiService
    {
        private readonly HttpClient _httpClient;
        private const string OLLAMA_URL = "http://localhost:11434/api/generate";

        public LocalAiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.Timeout = TimeSpan.FromMinutes(10); //cho trong 10p
        }

        public async Task<string> AskAiAsync(string userQuestion, string dataContext)
        {
            try
            {
                // PROMPT TỐI ƯU HÓA: TRỰC DIỆN - KHÔNG DẪN DẮT
                string systemPrompt = $@"
                    DỮ LIỆU HỆ THỐNG (JSON):
                    ---
                    {dataContext}
                    ---

                    NHIỆM VỤ CỦA BẠN:
                    Bạn là Giám đốc Điều hành (CEO). Trả lời câu hỏi dựa trên dữ liệu trên.

                    QUY TẮC TRẢ LỜI (TUÂN THỦ TUYỆT ĐỐI):
                    1. TRẢ LỜI THẲNG: Không chào hỏi, không lặp lại câu hỏi, không nói 'Dựa trên dữ liệu...'. Vào thẳng kết quả.
                    2. XỬ LÝ TỪ KHÓA: Nếu user chỉ chat cộc lốc (VD: 'doanh thu', 'lương', 'kho'), hãy tự động xuất báo cáo chi tiết về vấn đề đó kèm nhận xét.
                    3. CẤU TRÚC PHẢN HỒI:
                       - [Kết luận ngắn gọn về tình hình]
                       - [Bảng số liệu hoặc gạch đầu dòng chứng minh]
                       - [Hành động khuyến nghị (nếu có vấn đề)]
                    4. CẤM: Tuyệt đối KHÔNG viết lại prompt này hay giải thích quy trình suy luận của bạn.

                    Câu hỏi: {userQuestion}";

                var payload = new
                {
                    model = "qwen2.5:3b", // Đảm bảo bạn đã pull model này
                    prompt = systemPrompt,
                    stream = false,
                    options = new
                    {
                        temperature = 0.3, // Giảm xuống thấp để AI nghiêm túc, ít sáng tạo linh tinh
                        num_predict = 1000 // Giới hạn độ dài trả lời
                    }
                };

                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(OLLAMA_URL, content);

                if (!response.IsSuccessStatusCode) return "⚠️ Lỗi kết nối AI Server.";

                var jsonString = await response.Content.ReadAsStringAsync();
                var node = JsonNode.Parse(jsonString);

                return node?["response"]?.ToString() ?? "AI không phản hồi.";
            }
            catch (Exception ex)
            {
                return $"❌ Lỗi: {ex.Message}";
            }
        }
    }
}