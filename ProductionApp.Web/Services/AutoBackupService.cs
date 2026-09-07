using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;

namespace ProductionApp.Web.Services
{
    // Kế thừa BackgroundService để ứng dụng tự động chạy ngầm trên Server
    public class AutoBackupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AutoBackupService> _logger;

        // 💡 CẤU HÌNH RETENTION POLICY: Chỉ giữ lại 30 bản backup mới nhất (tương đương 1 tháng)
        private readonly int _maxBackupFiles = 30;

        public AutoBackupService(IServiceProvider serviceProvider, ILogger<AutoBackupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("⏳ Dịch vụ Auto Backup đã khởi động...");

            while (!stoppingToken.IsCancellationRequested)
            {
                // 1. Tính toán thời gian ngủ (Sleep) cho đến 8:30 Sáng hôm sau
                var now = DateTime.Now;
                var nextRunTime = new DateTime(now.Year, now.Month, now.Day, 8, 30, 0); // Đặt lịch lúc 8h30

                if (now > nextRunTime)
                {
                    nextRunTime = nextRunTime.AddDays(1); // Nếu đã qua 2h sáng thì chờ đến 2h sáng ngày mai
                }

                var delay = nextRunTime - now;
                _logger.LogInformation($"💤 Auto Backup sẽ chạy sau {delay.TotalHours:F2} giờ nữa (Lúc {nextRunTime:dd/MM/yyyy HH:mm}).");

                // Chờ đến đúng giờ
                await Task.Delay(delay, stoppingToken);

                if (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        _logger.LogInformation("🚀 Đang tiến hành Auto Backup...");
                        PerformBackup();
                        CleanupOldBackups();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "❌ Lỗi nghiêm trọng khi chạy Auto Backup.");
                    }
                }
            }
        }

        private void PerformBackup()
        {
            // Vì BackgroundService là Singleton, ta phải dùng CreateScope để gọi AppDbContext (Scoped)
            using (var scope = _serviceProvider.CreateScope())
            {
                // Tùy thuộc vào namespace DbContext của bạn, hãy using cho chuẩn
                var dbContext = scope.ServiceProvider.GetRequiredService<ProductionManager.Data.AppDbContext>();
                var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

                var provider = dbContext.Database.ProviderName;
                if (provider != null && (provider.Contains("MySql") || provider.Contains("Pomelo")))
                {
                    var connectionString = dbContext.Database.GetConnectionString();
                    // Đặt tên có chữ AUTO để dễ phân biệt với bản người dùng tự bấm
                    var fileName = $"AUTO_Backup_MySQL_{DateTime.Now:yyyyMMdd_HHmmss}.sql";
                    var backupFolder = Path.Combine(env.ContentRootPath, "App_Data", "backups", "mysql");

                    if (!Directory.Exists(backupFolder)) Directory.CreateDirectory(backupFolder);

                    var filePath = Path.Combine(backupFolder, fileName);

                    using (var conn = new MySqlConnection(connectionString))
                    {
                        using (var cmd = new MySqlCommand())
                        {
                            using (var mb = new MySqlBackup(cmd))
                            {
                                cmd.Connection = conn;
                                conn.Open();

                                // Cấu hình chuẩn mực: Backup CẢ CẤU TRÚC lẫn DỮ LIỆU
                                mb.ExportInfo.ExportTableStructure = true;
                                mb.ExportInfo.AddDropTable = true;

                                mb.ExportToFile(filePath);
                            }
                        }
                    }
                    _logger.LogInformation($"✅ Auto Backup thành công: {fileName}");
                }
            }
        }

        private void CleanupOldBackups()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
                var backupFolder = Path.Combine(env.ContentRootPath, "App_Data", "backups", "mysql");

                if (Directory.Exists(backupFolder))
                {
                    var directoryInfo = new DirectoryInfo(backupFolder);

                    // Lấy tất cả file .sql và sắp xếp từ Mới nhất -> Cũ nhất
                    var files = directoryInfo.GetFiles("*.sql")
                                             .OrderByDescending(f => f.CreationTime)
                                             .ToList();

                    // 💡 LỌC: Nếu số file vượt quá _maxBackupFiles thì xóa bớt
                    if (files.Count > _maxBackupFiles)
                    {
                        var filesToDelete = files.Skip(_maxBackupFiles).ToList();
                        foreach (var file in filesToDelete)
                        {
                            try
                            {
                                file.Delete();
                                _logger.LogInformation($"🗑️ Đã xóa bản sao lưu cũ: {file.Name} để giải phóng ổ cứng.");
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, $"⚠️ Không thể xóa file {file.Name}");
                            }
                        }
                    }
                }
            }
        }
    }
}