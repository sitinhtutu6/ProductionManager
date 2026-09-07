using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProductionManager.Data
{
    public class Invoice
    {
        public int Id { get; set; }
        public string? InvoiceNumber { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string? InvoiceFile { get; set; }

        public decimal TotalAmount { get; set; } // Tổng tiền của tờ hóa đơn này
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Quan hệ 1-N với bảng trung gian
        public ICollection<InvoiceDetail> InvoiceDetails { get; set; } = new List<InvoiceDetail>();

        [Timestamp]
        public byte[]? RowVersion { get; set; }
    }
}