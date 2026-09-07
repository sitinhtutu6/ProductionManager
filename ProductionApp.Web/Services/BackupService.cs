using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ProductionApp.Web.Services
{
    public class BackupService : BackgroundService
    {
        private readonly ILogger<BackupService> _logger;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;

        // Cấu hình thời gian chạy tự động (VD: 12 tiếng một lần)
        private readonly TimeSpan _period = TimeSpan.FromHours(12);

        public BackupService(ILogger<BackupService> logger, IConfiguration config, IWebHostEnvironment env)
        {
            _logger = logger;
            _config = config;
            _env = env;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("🔄 Đang chạy sao lưu tự động...");
                    PerformBackup("AUTO");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"❌ Lỗi sao lưu tự động: {ex.Message}");
                }

                // Chờ đến lần chạy tiếp theo
                await Task.Delay(_period, stoppingToken);
            }
        }

        public string PerformBackup(string type = "MANUAL")
        {
            // 1. Xác định đường dẫn file Database gốc (SQLite)
            // Lấy từ ConnectionString hoặc mặc định
            string dbFileName = "Production.db"; // Tên file DB hiện tại của bạn
            string sourcePath = Path.Combine(_env.ContentRootPath, dbFileName);

            if (!File.Exists(sourcePath)) throw new FileNotFoundException("Không tìm thấy file Database gốc!");

            // 2. Tạo thư mục chứa Backup
            string backupFolder = Path.Combine(_env.WebRootPath, "backups");
            if (!Directory.Exists(backupFolder)) Directory.CreateDirectory(backupFolder);

            // 3. Tạo tên file theo format yêu cầu: dbquanlydonhang_giờphútgiây_ngàythángnăm
            string timeStamp = DateTime.Now.ToString("HHmmss_ddMMyyyy");
            string backupFileName = $"dbquanlydonhang_{timeStamp}_{type}.db";
            string destPath = Path.Combine(backupFolder, backupFileName);

            // 4. Copy file (Dùng FileShare.ReadWrite để copy ngay cả khi đang chạy)
            File.Copy(sourcePath, destPath, true);

            _logger.LogInformation($"✅ Đã sao lưu thành công: {backupFileName}");
            return backupFileName;
        }
    }
}