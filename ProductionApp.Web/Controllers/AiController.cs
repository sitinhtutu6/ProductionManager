using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductionApp.Web.Services;

namespace ProductionApp.Web.Controllers
{
    [Authorize] // Chỉ cho phép người đăng nhập dùng
    public class AiController : Controller
    {
        private readonly LocalAiService _aiService;
        private readonly DataAggregationService _dataService;

        public AiController(LocalAiService aiService, DataAggregationService dataService)
        {
            _aiService = aiService;
            _dataService = dataService;
        }

        // 1. Hiển thị giao diện Chat
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // 2. API nhận câu hỏi (AJAX gọi vào đây)
        [HttpPost]
        public async Task<IActionResult> Ask([FromBody] UserQueryModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Question))
                return Json(new { answer = "Bạn chưa nhập câu hỏi." });

            // Bước 1: Lấy dữ liệu mới nhất từ DB
            string contextData = await _dataService.GetFullBusinessContextAsync();

            // Bước 2: Gửi sang AI phân tích
            string answer = await _aiService.AskAiAsync(model.Question, contextData);

            return Json(new { answer = answer });
        }

        // Class hứng dữ liệu JSON từ client
        public class UserQueryModel
        {
            public string Question { get; set; }
        }
    }
}