using System;
using System.Collections.Generic;

namespace ProductionManager.Data
{
    public class APInvoice
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; } // Số Hóa đơn đỏ của đối tác
        public DateTime InvoiceDate { get; set; }
        public string PartnerName { get; set; }   // Tên Nhà xe / NCC

        public decimal SubTotal { get; set; }     // Tổng tiền cước / tiền hàng (Chưa thuế)
        public decimal VatRate { get; set; }      // Tỉ lệ VAT (%)
        public decimal VatAmount { get; set; }    // Tiền thuế VAT
        public decimal TotalAmount { get; set; }  // Tổng tiền phải trả (SubTotal + VatAmount)

        public decimal PaidAmount { get; set; }   // Tiền đã thanh toán
        public string Note { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // 1 Hóa đơn AP có thể gom nhiều chuyến xe
        public ICollection<ExportShipment> ExportShipments { get; set; } = new List<ExportShipment>();
    }
}