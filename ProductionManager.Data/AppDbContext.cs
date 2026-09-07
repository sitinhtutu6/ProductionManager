using Microsoft.AspNetCore.Identity; // Sửa lỗi CS0246 (IdentityRole)
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using ProductionManager.Data.Entities;

namespace ProductionManager.Data
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        private readonly IHttpContextAccessor? _httpContextAccessor;

        public AppDbContext(DbContextOptions<AppDbContext> options, IHttpContextAccessor? httpContextAccessor = null) : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        // --- CÁC BẢNG NGHIỆP VỤ ---
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Shipment> Shipments { get; set; }
        public DbSet<ShipmentDetail> ShipmentDetails { get; set; }

        public DbSet<Product> Products { get; set; }
        public DbSet<ProductDetail> ProductDetails { get; set; }
        public DbSet<ProductPriceHistory> ProductPriceHistories { get; set; }

        public DbSet<SystemNotification> SystemNotifications { get; set; }

        public DbSet<Material> Materials { get; set; }
        public DbSet<WarehouseItem> WarehouseItems { get; set; }
        public DbSet<StockTransaction> StockTransactions { get; set; }

        public DbSet<WarehouseTicket> WarehouseTickets { get; set; }
        public DbSet<WarehouseTicketDetail> WarehouseTicketDetails { get; set; }

        public DbSet<CashEntry> CashEntries { get; set; }

        public DbSet<CashAllocation> CashAllocations { get; set; }

        public DbSet<ProductBom> ProductBoms { get; set; }

        public DbSet<MaterialImport> MaterialImports { get; set; }

        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<PurchaseOrderDetail> PurchaseOrderDetails { get; set; }

        public DbSet<Document> Documents { get; set; }

        public DbSet<ExportShipment> ExportShipments { get; set; }
        public DbSet<ExportShipmentOrder> ExportShipmentOrders { get; set; }
        public DbSet<ExportDocument> ExportDocuments { get; set; }
        public DbSet<ExportStepTracker> StepTrackers { get; set; }

        public DbSet<AuditLog> AuditLogs { get; set; }

        public DbSet<CostCategory> CostCategories { get; set; }

        public DbSet<CompanyConfig> CompanyConfigs { get; set; }

        public DbSet<FinancialPeriod> FinancialPeriods { get; set; }

        public DbSet<ImportPriceHistory> ImportPriceHistories { get; set; }

        public DbSet<WarehouseCategory> WarehouseCategories { get; set; }

        public DbSet<ProductDocument> ProductDocuments { get; set; }

        public DbSet<PartnerContact> PartnerContacts { get; set; }

        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceDetail> InvoiceDetails { get; set; }

        public DbSet<APInvoice> APInvoices { get; set; }
        public DbSet<OrderHistory> OrderHistorys { get; set; }

        public DbSet<WorkOrder> WorkOrders { get; set; }
        public DbSet<OrderHistory> OrderHistories { get; set; }
        public DbSet<WorkOrderLog> WorkOrderLogs { get; set; }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var auditEntries = new List<AuditLog>();

            // Lấy tên User đang đăng nhập, nếu không có thì mặc định là "Hệ thống"
            var userName = _httpContextAccessor?.HttpContext?.User?.Identity?.Name ?? "Hệ thống";

            // Quét tất cả các dòng dữ liệu đang bị thay đổi trong Entity Framework
            foreach (var entry in ChangeTracker.Entries())
            {
                // Bỏ qua nếu là bảng AuditLog (không tự log chính nó) hoặc dữ liệu không thay đổi
                if (entry.Entity is AuditLog || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                    continue;

                var auditLog = new AuditLog
                {
                    TableName = entry.Entity.GetType().Name,
                    UserName = userName,
                    Action = entry.State.ToString() // Trả về: Added, Modified, Deleted
                };

                // Trích xuất dữ liệu cũ và mới
                var oldValues = new Dictionary<string, object>();
                var newValues = new Dictionary<string, object>();

                foreach (var property in entry.Properties)
                {
                    string propertyName = property.Metadata.Name;

                    // Lấy ID của dòng dữ liệu đang thao tác
                    if (property.Metadata.IsPrimaryKey())
                    {
                        auditLog.RecordId = property.CurrentValue?.ToString();
                    }

                    switch (entry.State)
                    {
                        case EntityState.Added:
                            if (property.CurrentValue != null)
                                newValues[propertyName] = property.CurrentValue;
                            break;

                        case EntityState.Deleted:
                            if (property.OriginalValue != null)
                                oldValues[propertyName] = property.OriginalValue;
                            break;

                        case EntityState.Modified:
                            if (property.IsModified)
                            {
                                if (property.OriginalValue != null) oldValues[propertyName] = property.OriginalValue;
                                if (property.CurrentValue != null) newValues[propertyName] = property.CurrentValue;
                            }
                            break;
                    }
                }

                if (oldValues.Count > 0) auditLog.OldValues = JsonSerializer.Serialize(oldValues);
                if (newValues.Count > 0) auditLog.NewValues = JsonSerializer.Serialize(newValues);

                auditEntries.Add(auditLog);
            }

            // Lưu danh sách log vào Database trước khi thực hiện lưu dữ liệu gốc
            if (auditEntries.Any())
            {
                AuditLogs.AddRange(auditEntries);
            }

            // Thực hiện lệnh lưu gốc của Entity Framework
            return await base.SaveChangesAsync(cancellationToken);
        }
        // ====================================================================

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Bắt buộc để tạo bảng Identity

            // ======================================================
            // CẤU HÌNH MỚI CHO: LỊNH SẢN XUẤT & LỊCH SỬ (WORKORDER)
            // ======================================================

            // Ép tên bảng chính xác để tránh lỗi EF Core tự thêm chữ "s" (Ví dụ: OrderHistorys)
            modelBuilder.Entity<OrderHistory>().ToTable("OrderHistory");
            modelBuilder.Entity<WorkOrder>().ToTable("WorkOrder");

            // Cấu hình quan hệ: Khi xóa 1 Đơn hàng (Order) -> Xóa sạch các Lệnh sản xuất (WorkOrder) đi kèm
            modelBuilder.Entity<WorkOrder>()
                .HasOne(w => w.Order)
                .WithMany()
                .HasForeignKey(w => w.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // ======================================================
            // CẤU HÌNH CŨ 
            // ======================================================

            // 1. Cấu hình Order
            modelBuilder.Entity<Order>()
                .HasIndex(o => o.OrderCode)
                .IsUnique();

            // 2. Cấu hình Product (ĐÃ SỬA LẠI CHO KHỚP VỚI FILE CỦA BẠN)
            // Đổi p.Code -> p.ProductCode
            modelBuilder.Entity<Product>()
                .Property(p => p.ProductCode).IsRequired().HasMaxLength(50);

            // Đổi p.Code -> p.ProductCode
            modelBuilder.Entity<Product>()
                .HasIndex(p => p.ProductCode).IsUnique();

            // 3. Đổi tên bảng Identity cho gọn (Tùy chọn)
            modelBuilder.Entity<AppUser>().ToTable("Users");
            modelBuilder.Entity<IdentityRole>().ToTable("Roles");
            modelBuilder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");

            // Kho vat tu
            modelBuilder.Entity<WarehouseItem>().HasIndex(w => w.Code).IsUnique();

            // Nếu SQLite, ép kiểu Decimal -> Double
            if (Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
            {
                foreach (var entityType in modelBuilder.Model.GetEntityTypes())
                {
                    var properties = entityType.ClrType.GetProperties()
                        .Where(p => (p.PropertyType == typeof(decimal) || p.PropertyType == typeof(decimal?))
                                    && p.CanWrite); // <--- CHỈNH SỬA DÒNG NÀY (Bỏ qua thuộc tính chỉ đọc)

                    foreach (var property in properties)
                    {
                        modelBuilder.Entity(entityType.Name)
                            .Property(property.Name)
                            .HasConversion<double>();
                    }
                }
            }

            // CostCategory
            modelBuilder.Entity<CostCategory>().HasData(
                new CostCategory { Id = 1, Section = PnlSection.Salary, IsActive = true, Name = "Lương cơ bản nhân viên", Description = "Chi trả lương cứng hàng tháng" },
                new CostCategory { Id = 2, Section = PnlSection.Salary, IsActive = true, Name = "Tiền tăng ca, phụ cấp, thưởng", Description = "Chi phí làm thêm giờ, phụ cấp" },
                new CostCategory { Id = 3, Section = PnlSection.Salary, IsActive = true, Name = "Bảo hiểm xã hội, BHYT", Description = "Phần BH công ty đóng" },

                new CostCategory { Id = 4, Section = PnlSection.Utilities, IsActive = true, Name = "Tiền điện, nước, rác", Description = "Chi phí điện nước tại xưởng/VP" },
                new CostCategory { Id = 5, Section = PnlSection.Utilities, IsActive = true, Name = "Tiền thuê mặt bằng, nhà xưởng", Description = "Chi phí thuê địa điểm" },
                new CostCategory { Id = 6, Section = PnlSection.Utilities, IsActive = true, Name = "Bảo trì, sửa chữa, khấu hao", Description = "Khấu hao TSCĐ, sửa chữa" },

                new CostCategory { Id = 7, Section = PnlSection.Logistics, IsActive = true, Name = "Tiền xăng xe, cầu đường", Description = "Chi phí xe công ty" },
                new CostCategory { Id = 8, Section = PnlSection.Logistics, IsActive = true, Name = "Phí thuê xe ngoài, bưu cục", Description = "ViettelPost, thuê tải ngoài" },
                new CostCategory { Id = 9, Section = PnlSection.Logistics, IsActive = true, Name = "Vật tư bao bì, đóng gói", Description = "Băng keo, thùng carton" },

                new CostCategory { Id = 10, Section = PnlSection.Office, IsActive = true, Name = "Văn phòng phẩm", Description = "Giấy, mực, bút..." },
                new CostCategory { Id = 11, Section = PnlSection.Office, IsActive = true, Name = "Cước Internet, Phần mềm", Description = "Cước mạng, phần mềm ERP" },
                new CostCategory { Id = 12, Section = PnlSection.Office, IsActive = true, Name = "Chi phí tiếp khách", Description = "Ngoại giao, ăn uống" },

                new CostCategory { Id = 13, Section = PnlSection.Marketing, IsActive = true, Name = "Quảng cáo, Truyền thông", Description = "Ads Facebook, Google" },
                new CostCategory { Id = 14, Section = PnlSection.Marketing, IsActive = true, Name = "Chiết khấu, Khuyến mãi", Description = "Quà tặng, hoa hồng" },

                new CostCategory { Id = 15, Section = PnlSection.OtherOpex, IsActive = true, Name = "Lệ phí ngân hàng, Lãi vay", Description = "Phí chuyển khoản, lãi vay" },
                new CostCategory { Id = 16, Section = PnlSection.OtherOpex, IsActive = true, Name = "Thuế, Lệ phí nhà nước", Description = "Thuế môn bài, phí môi trường" }
            );

            // Khởi tạo 5 nhóm hàng mặc định
            modelBuilder.Entity<WarehouseCategory>().HasData(
                new WarehouseCategory { Id = 1, Name = "Nguyên vật liệu" },
                new WarehouseCategory { Id = 2, Name = "Bán thành phẩm" },
                new WarehouseCategory { Id = 3, Name = "Thành phẩm" },
                new WarehouseCategory { Id = 4, Name = "Gỗ" },
                new WarehouseCategory { Id = 5, Name = "Màu" }
            );

            // Cấu hình Restrict khi xóa: Không cho xóa Nhóm nếu đang có Vật tư
            modelBuilder.Entity<WarehouseItem>()
                .HasOne(w => w.WarehouseCategory)
                .WithMany(c => c.WarehouseItems)
                .HasForeignKey(w => w.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}