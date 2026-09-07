using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductionManager.Data
{
    public class OrderDetail
    {
        [Key]
        public int Id { get; set; }

        public int OrderId { get; set; }
        [ForeignKey("OrderId")]
        public Order? Order { get; set; }

        [Required]
        public string ProductName { get; set; }
        public int? ProductId { get; set; }
        public string? Specifications { get; set; } // Quy cách/Kích thước

        public int Quantity { get; set; } // Số lượng ĐẶT
        public string? Notes { get; set; } // Quy cách/Kích thước
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }
        public double VatPercent { get; set; } = 0;
        public bool IsComponent { get; set; } = false; // Đánh dấu: false = SP Mẹ (Tính tiền), true = Chi tiết Con (Xuất kho)
        public string? BundleCode { get; set; } // Mã liên kết để gom nhóm Mẹ - Con (Ví dụ: BND_12345)
        public string? ExcludedMaterials { get; set; } // Chứa ID các vật tư bị Sale bỏ tick lúc lên đơn (VD: "15,22")
        public virtual ICollection<ShipmentDetail>? ShipmentDetails { get; set; } = new List<ShipmentDetail>();

    }
}