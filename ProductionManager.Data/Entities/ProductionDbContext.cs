using Microsoft.EntityFrameworkCore;
using ProductionManager.Data; // Để lấy ServerConfig

namespace ProductionManager.Data
{
    public class ProductionDbContext : DbContext
    {
        private readonly ServerConfig _config;

        public ProductionDbContext(DbContextOptions<ProductionDbContext> options, ServerConfig config)
            : base(options)
        {
            _config = config;
        }

        // Khai báo các bảng dữ liệu
        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; } // Module Sản phẩm mới

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Logic tự động chọn Database dựa trên cấu hình WinForms gửi sang
                if (_config.DbProvider == "MySQL")
                {
                    // Lưu ý: Cần cài gói Pomelo.EntityFrameworkCore.MySql
                    optionsBuilder.UseMySql(_config.ConnectionString, ServerVersion.AutoDetect(_config.ConnectionString));
                }
                else // Mặc định là SQL Server
                {
                    optionsBuilder.UseSqlServer(_config.ConnectionString);
                }
            }
        }
    }
}