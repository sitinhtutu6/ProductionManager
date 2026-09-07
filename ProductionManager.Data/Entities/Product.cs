using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductionManager.Data
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string ProductCode { get; set; } // Mã SP

        [Required, StringLength(200)]
        public string ProductName { get; set; }

        public string? Color { get; set; }       // Màu sắc
        public string? MaterialType { get; set; } // Loại vật tư (Gỗ, Sắt, Nhựa...)
        public string? Unit { get; set; }        // ĐVT

        // Quy cách tổng (dùng cho SP đơn hoặc quy cách bao bì)
        public double Thick { get; set; }  // Dày
        public double Width { get; set; }  // Rộng
        public double Length { get; set; } // Dài

        public string? Note { get; set; }  // Ghi chú

        public bool HasVariants { get; set; } = false;

        [Column(TypeName = "decimal(18,2)")]
        public decimal DefaultPrice { get; set; }

        public string? ImagePath { get; set; }

        public ICollection<ProductDetail>? ProductDetails { get; set; } // 1. Chi tiết 
        public ICollection<ProductPriceHistory>? PriceHistories { get; set; } // 2. Lịch sử giá 
        public ICollection<Material>? Materials { get; set; } // 3. DANH SÁCH VẬT TƯ


        // Liên kết sang kho để lấy tồn kho
        public int? WarehouseItemId { get; set; }
        [ForeignKey("WarehouseItemId")]
        public WarehouseItem? WarehouseItem { get; set; }

        //-------- ho so san pham
        public ICollection<ProductDocument>? ProductDocuments { get; set; }
    }
}