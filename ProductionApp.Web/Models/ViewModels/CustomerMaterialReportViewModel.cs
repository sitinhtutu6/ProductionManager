namespace ProductionApp.Web.Models.ViewModels
{
    public class CustomerMaterialReportViewModel
    {
        public int OrderId { get; set; }
        public string OrderCode { get; set; }
        public string CustomerName { get; set; }

        public int WarehouseItemId { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string Unit { get; set; }

        public double TotalReceived { get; set; } // Tổng số lượng khách giao
        public double TotalUsed { get; set; }     // Tổng số lượng xưởng đã xuất dùng

        public double TotalRequired { get; set; } // Định mức BOM yêu cầu cho đơn hàng này

        // Tồn dư: Dương (Khách giao dư), Âm (Khách giao thiếu / Xưởng làm hao hụt quá mức)
        public double Balance => TotalReceived - TotalUsed;
    }
}