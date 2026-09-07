using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductionManager.Data
{
    public enum ImportPaymentStatus
    {
        Unpaid = 0,   // Chưa thanh toán
        Partial = 1,  // Thanh toán một phần
        Paid = 2      // Đã thanh toán đủ
    }

    public class MaterialImport
    {
        [Key]
        public int Id { get; set; }
        public string ImportCode { get; set; } // Mã phiếu (VD: PN-230101)
        public DateTime ImportDate { get; set; } = DateTime.Now;

        public int SupplierId { get; set; }
        public string SupplierName { get; set; }

        public decimal TotalAmount { get; set; } // Tổng tiền hàng

        public decimal PaidAmount { get; set; } // Đã trả
        public ImportPaymentStatus PaymentStatus { get; set; } = ImportPaymentStatus.Unpaid;

        [NotMapped]
        public decimal RemainingAmount => TotalAmount - PaidAmount; // Còn nợ

        // TRƯỜNG NÀY ĐỂ ĐÁNH DẤU CHỐT SỔ
        public bool IsLocked { get; set; } = false;

        // Thêm vào class MaterialImport
        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; } // Tiền hàng (Trước thuế)

        public double VatRate { get; set; } // % Thuế VAT (0, 5, 8, 10)

        [Column(TypeName = "decimal(18,2)")]
        public decimal VatAmount { get; set; } // Tiền thuế VAT

        // Thêm khóa ngoại trỏ về Hóa đơn đầu vào
        public int? APInvoiceId { get; set; }
        public virtual APInvoice APInvoice { get; set; }
    }
}