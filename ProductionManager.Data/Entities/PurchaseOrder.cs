using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductionManager.Data
{
    public enum POType
    {
        Purchase = 1,    // Mua hàng từ NCC
        Outsourcing = 2  // Gia công (Xuất phôi -> Nhập thành phẩm)
    }

    public enum POStatus
    {
        Open = 0,       // Đang thực hiện
        Partial = 1,    // Đã nhận 1 phần
        Completed = 2,  // Đã hoàn thành
        Cancelled = 3   // Đã hủy
    }

    public enum PaymentStatusPO { Unpaid = 0, Deposited = 1, Paid = 2 }

    // Bảng Đơn Đặt Hàng (Master)
    public class PurchaseOrder
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string POCode { get; set; } // Mã đơn (PO-xxx)

        public DateTime OrderDate { get; set; } = DateTime.Now;

        public int SupplierId { get; set; } // ID Nhà cung cấp (Nếu có bảng Supplier)
        public string SupplierName { get; set; } // Tên NCC (Lưu cứng để dễ hiển thị)

        public POType Type { get; set; }
        public POStatus Status { get; set; } = POStatus.Open;
        public string? Note { get; set; }

        // 🔥 CÁC TRƯỜNG TÀI CHÍNH TÁCH BIỆT (MỚI)
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; } = 0; // Tiền hàng (Chưa VAT)

        public double VATPercent { get; set; } = 0; // % Thuế VAT

        [Column(TypeName = "decimal(18,2)")]
        public decimal VATAmount { get; set; } = 0; // Tiền Thuế

        [Column(TypeName = "decimal(18,2)")]
        public decimal FinalTotal { get; set; } = 0; // Tổng thanh toán (Tiền hàng + Thuế)

        [Column(TypeName = "decimal(18,2)")]
        public decimal DepositAmount { get; set; } = 0; // Tiền cọc trước

        [Column(TypeName = "decimal(18,2)")]
        public decimal RemainingAmount { get; set; } = 0; // Còn lại phải trả

        public PaymentStatusPO PaymentStatusPO { get; set; } = PaymentStatusPO.Unpaid;

        public List<PurchaseOrderDetail> Details { get; set; }

        // Các trường dùng để liên kết với Sổ Công Nợ (DebtController)
        public string? InvoiceNumber { get; set; } // Số Hóa Đơn / Bảng kê gom nhóm
        public bool IsDebtFinalized { get; set; } = false; // Đã chốt công nợ chưa?
        public DateTime? DebtFinalizedDate { get; set; } // Ngày chốt công nợ

        [Timestamp]
        public byte[]? RowVersion { get; set; }

    }

    // Bảng Chi Tiết Đơn Hàng (Detail)
    public class PurchaseOrderDetail
    {
        [Key]
        public int Id { get; set; }

        public int PurchaseOrderId { get; set; }
        [ForeignKey("PurchaseOrderId")]
        public PurchaseOrder PurchaseOrder { get; set; }

        public int WarehouseItemId { get; set; } // Link với vật tư
        [ForeignKey("WarehouseItemId")]
        public WarehouseItem WarehouseItem { get; set; }

        // 🔥 THÊM MỚI: Liên kết với Đơn hàng / Lệnh Sản Xuất
        public int? LinkedOrderId { get; set; }
        public string? LinkedOrderCode { get; set; }

        public double QuantityOrdered { get; set; } // Số lượng đặt
        public double QuantityReceived { get; set; } = 0; // Số lượng ĐẠT (đã vào kho)
        public double QuantityBad { get; set; } = 0;      // Số lượng HỎNG (NG - trả về/huỷ)

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; } = 0; // Giá tạm tính lúc đặt

        public bool IsFinished { get; set; } = false; // Dòng này đã nhận đủ chưa
    }
}