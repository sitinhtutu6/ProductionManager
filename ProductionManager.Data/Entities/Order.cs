using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductionManager.Data
{
    public enum PaymentStatus
    {
        Unpaid = 0,     // Chưa thanh toán
        Partial = 1,    // Đã cọc / Thanh toán 1 phần
        Paid = 2        // Đã thanh toán đủ
    }

    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string OrderCode { get; set; } 

        [StringLength(100)]
        public string? PaperOrderCode { get; set; }

        // Liên kết Khách hàng
        public int CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public Customer Customer { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now;
        public DateTime? DeliveryDeadline { get; set; } // Hạn chót giao hàng

        // Tiền nong (Decimal cho chính xác)
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DepositAmount { get; set; } // Tiền cọc

        // Trạng thái (Sử dụng Enum ở trên)
        public OrderStatus Status { get; set; } = OrderStatus.PendingApproval;

        public bool IsLocked { get; set; } = false; // Khóa khi xong hoặc Admin khóa
        public string? Notes { get; set; }
        public string? DesignFile { get; set; }

        // Liên kết
        public ICollection<OrderDetail>? OrderDetails { get; set; } = new List<OrderDetail>();
        public ICollection<Shipment>? Shipments { get; set; }

        public int? ProductionIndex { get; set; } = 9999;
        public DateTime? ProductionStartDate { get; set; } // Ngày bắt đầu SX
        public DateTime? ProductionEndDate { get; set; }   // Ngày kết thúc SX
        public string? PlanColor { get; set; }

        /*
        public string? InvoiceFile { get; set; }   // File đính kèm
        [StringLength(50)]
        public string? InvoiceNumber { get; set; } // Số hóa đơn đỏ
        public DateTime? InvoiceDate { get; set; } // Ngày xuất hóa đơn
        */

        public ICollection<InvoiceDetail> InvoiceDetails { get; set; } = new List<InvoiceDetail>();

        [Column(TypeName = "decimal(18,2)")]
        public decimal PaidAmount { get; set; } = 0; // Tổng tiền khách đã trả (Cập nhật từ Sổ Quỹ)

        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;
        //public ICollection<StockTransaction> StockTransactions { get; set; }
        public virtual ICollection<StockTransaction>? StockTransactions { get; set; } = new List<StockTransaction>();

        // Thuộc tính tính toán (không lưu DB)
        [NotMapped]
        public decimal RemainingAmount => TotalAmount - PaidAmount;

        // Tổng giá vốn hàng bán của đơn hàng (Tiền vật tư tiêu hao)
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalCOGS { get; set; }

        // Tùy chọn: Lợi nhuận gộp của riêng đơn này
        [NotMapped]
        public decimal GrossProfit => TotalAmount - TotalCOGS;

        [Timestamp]
        public byte[]? RowVersion { get; set; }


    }
}