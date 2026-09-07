using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductionManager.Data
{
    public class ProductBom
    {
        [Key]
        public int Id { get; set; }

        // 🔥 SỬA: Liên kết với bảng Product (Thành phẩm)
        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public Product Product { get; set; }

        // 🔥 GIỮ NGUYÊN: Liên kết với WarehouseItem (Nguyên vật liệu)
        public int MaterialId { get; set; }
        [ForeignKey("MaterialId")]
        public WarehouseItem Material { get; set; }

        [MaxLength(50)]
        public string? Unit { get; set; }

        [MaxLength(200)]
        public string? ComponentName { get; set; }

        // Các thông tin khác giữ nguyên
        public double? Length { get; set; }
        public double? Width { get; set; }
        public double? Height { get; set; }

        public double CutQuantity { get; set; } = 1;
        public double Coefficient { get; set; } = 1;
        public double Quantity { get; set; } = 0;
        public string? Note { get; set; }

    }
}