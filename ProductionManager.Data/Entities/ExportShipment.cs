using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductionManager.Data
{
    public enum ExportStep
    {
        Contract = 1,       // 1. Ký Hợp đồng
        Booking = 2,        // 2. Thuê tàu (Booking)
        Packing = 3,        // 3. Invoice & Packing List
        Container = 4,      // 4. Đóng hàng & VGM
        Customs = 5,        // 5. Khai báo Hải quan
        PortDropOff = 6,    // 6. Hạ bãi & Vào sổ tàu
        BillOfLading = 7,   // 7. Nhận Vận đơn (B/L)
        DocSend = 8,        // 8. Gửi chứng từ
        Payment = 9         // 9. Thanh toán
    }

    public enum StepStatus
    {
        Pending = 0,    // Chưa đến bước này
        Active = 1,     // Đang thực hiện
        Done = 2,       // Đã xong
        Problem = 3     // Có sự cố
    }
    public enum TransportType { Container, Truck }
    public enum PaymentStatuses { Unpaid, Paid }
    public enum ExportStatus { Planning, Loading, Shipped, Completed }


    public class ExportShipment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string ShipmentCode { get; set; } // Mã lô hàng (VD: EXP-2310-001)

        public int CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? ETD { get; set; } // Ngày tàu chạy dự kiến
        public DateTime? ETA { get; set; } // Ngày đến dự kiến

        // Thông tin Logistics (Điền dần qua các bước)
        public string? BookingNumber { get; set; } // Số Booking
        public string? VesselName { get; set; }    // Tên tàu
        public string? ContainerNumber { get; set; } // Số Cont
        public string? SealNumber { get; set; }      // Số Kẹp chì
        public string? BLNumber { get; set; }        // Số Vận đơn

        // Trạng thái tổng thể (Đang ở bước nào)
        public ExportStep CurrentStep { get; set; } = ExportStep.Contract;

        // Tiến độ tổng (0-100%)
        public int ProgressPercent { get; set; } = 0;

        // Danh sách các đơn hàng được gộp trong lô này
        public virtual ICollection<ExportShipmentOrder> ShipmentOrders { get; set; } = new List<ExportShipmentOrder>();

        // Danh sách tài liệu đính kèm
        public virtual ICollection<ExportDocument> Documents { get; set; } = new List<ExportDocument>();

        // Chi tiết trạng thái từng bước
        public virtual ICollection<ExportStepTracker> StepTrackers { get; set; } = new List<ExportStepTracker>();

        public TransportType TransportType { get; set; } = TransportType.Container;

        [StringLength(200)]
        public string? CarrierName { get; set; } // Tên nhà xe / Hãng tàu

        [StringLength(50)]
        public string? LicensePlate { get; set; } // Biển số xe

        [Column(TypeName = "decimal(18,2)")]
        public decimal ShippingCost { get; set; } = 0; // Chi phí

        //-----------------------------
        public PaymentStatuses PaymentStatus { get; set; } = PaymentStatuses.Unpaid;
        // Danh sách các Phiếu Xuất Kho thực tế đã được xếp lên chuyến xe này
        public ICollection<Shipment>? Shipments { get; set; }

        public ExportStatus Status { get; set; } = ExportStatus.Planning;

        [System.ComponentModel.DataAnnotations.Schema.Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; }
        public double VatRate { get; set; }

        [System.ComponentModel.DataAnnotations.Schema.Column(TypeName = "decimal(18,2)")]
        public decimal VatAmount { get; set; }

        // Cột dùng để lưu lịch sử sửa giá dưới dạng chuỗi JSON
        public string? PriceHistoryJson { get; set; }

        // Thêm khóa ngoại trỏ về Hóa đơn đầu vào
        public int? APInvoiceId { get; set; }
        public virtual APInvoice APInvoice { get; set; }

        [StringLength(200)]
        public string? Departure { get; set; } // Nơi xuất phát (Ví dụ: Kho Tổng Bình Dương)

        [StringLength(200)]
        public string? Destination { get; set; } // Nơi đến (Ví dụ: Cảng Cát Lái / Kho Khách Hàng)
    }

    // Bảng trung gian: 1 Lô xuất khẩu có thể chứa nhiều Đơn hàng
    public class ExportShipmentOrder
    {
        public int Id { get; set; }
        public int ExportShipmentId { get; set; }
        public int OrderId { get; set; } // Link tới bảng Order cũ
        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; }
    }

    // Bảng lưu file đính kèm
    public class ExportDocument
    {
        [Key]
        public int Id { get; set; }
        public int ExportShipmentId { get; set; }

        public ExportStep Step { get; set; } // File này thuộc bước nào?
        public string FileName { get; set; } // Tên hiển thị
        public string FilePath { get; set; } // Đường dẫn lưu trên server
        public DateTime UploadDate { get; set; } = DateTime.Now;
        public string? Note { get; set; }
    }

    // Bảng theo dõi trạng thái từng bước (Để biết bước nào xong ngày nào)
    public class ExportStepTracker
    {
        [Key]
        public int Id { get; set; }
        public int ExportShipmentId { get; set; }
        public ExportStep Step { get; set; }
        public StepStatus Status { get; set; } = StepStatus.Pending;
        public DateTime? CompletedDate { get; set; }
        public string? Note { get; set; } // Ghi chú (VD: Tàu delay, Hải quan kiểm hóa...)
    }

}