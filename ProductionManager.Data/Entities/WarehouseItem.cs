using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductionManager.Data
{
    public class WarehouseItem
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string Code { get; set; } // Mã vật tư (Duy nhất)

        [Required, StringLength(200)]
        public string Name { get; set; }

        //[StringLength(100)]
        //public string Category { get; set; } = "Chưa phân loại";

        public int? CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public WarehouseCategory WarehouseCategory { get; set; }

        public string Unit { get; set; } = "Cái"; // ĐVT

        public double StockQuantity { get; set; } = 0; // Tồn kho hiện tại (Tự động tính)

        [Column(TypeName = "decimal(18,2)")]
        public decimal CostPrice { get; set; } = 0; // Giá vốn trung bình

        public string? Note { get; set; }

        public string? Image { get; set; }

        // Trường phân loại: "Material" (Vật tư), "Product" (Thành phẩm), "SemiProduct" (Bán thành phẩm)
        [StringLength(50)]
        public string ItemType { get; set; } = "Material";

        // Liên kết ngược (Optional - để dễ truy vấn)
        public int? LinkedProductId { get; set; }

        [StringLength(50)]
        public string? ImportUnit { get; set; } // Đơn vị nhập (VD: Kg, Thùng)

        public double ConversionRate { get; set; } = 1; // 1 Đơn vị nhập = X Đơn vị cơ bản
    }
}