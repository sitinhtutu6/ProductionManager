using System;
using System.Collections.Generic; // Cần thêm để dùng ICollection
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq; // Cần thêm để dùng .Sum()

namespace ProductionManager.Data
{
    public enum TransactionType
    {
        Receipt = 1, // Thu (Tiền vào)
        Payment = 2  // Chi (Tiền ra)
    }

    public enum PaymentMethod
    {
        Cash = 1,    // Tiền mặt
        Transfer = 2 // Chuyển khoản
    }

    public enum EntryCategory
    {
        Business = 1, // HĐ Kinh doanh (Bán hàng/Mua vật tư) -> Tính vào Lãi lỗ
        Loan = 2,     // Vay & Cho vay (Tài chính) -> KHÔNG tính vào Lãi lỗ
        Other = 3     // Khác
    }

    public class CashEntry
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Số phiếu gốc")]
        public string? PaperVoucherNumber { get; set; }
        public DateTime TransactionDate { get; set; } = DateTime.Now;
        public DateTime ReportDate { get; set; } = DateTime.Now;

        [Required]
        public string VoucherCode { get; set; } // Mã phiếu (VD: PT-001, PC-001)

        public TransactionType Type { get; set; }
        public PaymentMethod Method { get; set; } = PaymentMethod.Cash;
        public EntryCategory Category { get; set; } = EntryCategory.Business;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public string? Description { get; set; } // Diễn giải/Ghi chú
        public string? TargetName { get; set; }  // Người nộp/Người nhận

        // --- LIÊN KẾT ĐƠN HÀNG (Dùng cho thu tiền bán hàng) ---
        public int? OrderId { get; set; }
        [ForeignKey("OrderId")]
        public Order? Order { get; set; }

        public int? ParentId { get; set; }

        [ForeignKey("ParentId")]
        public CashEntry? ParentEntry { get; set; }

        // Repayments: Danh sách các lần trả nợ (Chỉ dùng cho dòng Cha)
        public virtual ICollection<CashEntry>? Repayments { get; set; } = new List<CashEntry>();

        // --- THÔNG TIN HỆ THỐNG ---
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // --- 🔥 [MỚI] CÁC THUỘC TÍNH TÍNH TOÁN (Không lưu vào DB) ---
        // Giúp hiển thị nhanh trên View mà không cần tính lại trong Controller

        [NotMapped]
        public decimal PaidAmount => Repayments?.Sum(x => x.Amount) ?? 0; // Tổng đã trả/đã thu

        [NotMapped]
        public decimal RemainingAmount => Amount - PaidAmount; // Còn lại

        [NotMapped]
        public double ProgressPercent => Amount == 0 ? 0 : (double)(PaidAmount / Amount * 100); // Tiến độ %

        //LIÊN KẾT VỚI PHIẾU NHẬP
        public int? MaterialImportId { get; set; }

        [ForeignKey("MaterialImportId")]
        public MaterialImport? MaterialImport { get; set; }

        public int? CostCategoryId { get; set; }
        [ForeignKey("CostCategoryId")]
        public CostCategory? CostCategory { get; set; }

        public int? ForMonth { get; set; }
        public int? ForYear { get; set; }

        // Số tháng phân bổ chi phí (Mặc định là 1 tháng)
        public int? AllocatedMonths { get; set; } = 1;

        // Thêm khóa ngoại trỏ về Hóa đơn đầu vào
        public int? APInvoiceId { get; set; }
        public virtual APInvoice APInvoice { get; set; }

        public int? PurchaseOrderId { get; set; }
        public int? ExportShipmentId { get; set; }
    }
}