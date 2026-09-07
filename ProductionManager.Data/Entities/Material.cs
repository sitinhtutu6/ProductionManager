using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductionManager.Data
{
    // Bảng này giờ đóng vai trò là BOM (Định mức)
    public class Material
    {
        [Key]
        public int Id { get; set; }

        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public Product? Product { get; set; }

        // --- LIÊN KẾT VỚI KHO ---
        public int WarehouseItemId { get; set; } // <--- Cột Mới
        [ForeignKey("WarehouseItemId")]
        public WarehouseItem? WarehouseItem { get; set; }

        // Số lượng cần cho 1 sản phẩm (Định mức)
        public double Quantity { get; set; }

        public string? Note { get; set; }

        // Các trường cũ như MaterialName, Unit, CostPrice có thể bỏ hoặc để ReadOnly lấy từ WarehouseItem
        [NotMapped] // Không lưu vào DB nữa, chỉ để hiển thị
        public string MaterialName => WarehouseItem?.Name ?? "";

        [NotMapped]
        public string MaterialCode => WarehouseItem?.Code ?? "";

        [NotMapped]
        public string Unit => WarehouseItem?.Unit ?? "";

        [NotMapped]
        public decimal CostPrice => WarehouseItem?.CostPrice ?? 0;
    }
}