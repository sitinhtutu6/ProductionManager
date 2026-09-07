namespace ProductionManager.Data
{
    public class InvoiceDetail
    {
        public int Id { get; set; }

        public int InvoiceId { get; set; }
        public Invoice Invoice { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; }

        public decimal BilledAmount { get; set; } // Số tiền trích từ đơn hàng này để đưa vào hóa đơn
    }
}