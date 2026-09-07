using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ProductionManager.Data
{
    public class ProductDetail
    {
        [Key]
        public int Id { get; set; }

        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Product Product { get; set; }

        [Required, StringLength(50)]
        public string DetailCode { get; set; } // Mã chi tiết

        [Required]
        public string VariantName { get; set; } // Tên chi tiết

        public string? Color { get; set; }
        public string? MaterialType { get; set; }

        // Quy cách chi tiết
        public double Thick { get; set; }
        public double Width { get; set; }
        public double Length { get; set; }
        public int Quantity { get; set; } // Số lượng chi tiết trong 1 SP cha

        // Tính số khối (m3) = (Dày * Rộng * Dài * SL) / 10^9
        // Giả sử đầu vào là mm. Thuộc tính này chỉ để hiển thị (không lưu DB cũng được, hoặc lưu tùy ý)
        [NotMapped] // Không tạo cột trong DB, chỉ tính toán để hiện
        public double Volume => (Thick * Width * Length * Quantity) / 1000000000.0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public string? Note { get; set; }

        // Link sang kho
        public int? WarehouseItemId { get; set; }
    }
}