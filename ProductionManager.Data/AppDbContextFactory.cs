using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ProductionManager.Data
{
    // 🔥 Class này chỉ dùng để Dev chạy lệnh Add-Migration
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<AppDbContext>();

            // Dùng một chuỗi kết nối "mồi" (Dummy connection string) trên máy của bạn
            var connectionString = "Server=localhost;Database=productiondb;User=root;Password=123456;";

            builder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

            return new AppDbContext(builder.Options);
        }
    }
}