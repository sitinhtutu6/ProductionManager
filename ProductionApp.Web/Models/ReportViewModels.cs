namespace ProductionApp.Web.Models.ViewModels
{
    public class BusinessDashboardVM
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public ProductionReportVM Production { get; set; } = new ProductionReportVM();
        public CashFlowReportVM CashFlow { get; set; } = new CashFlowReportVM();

        // Chart JSON
        public string ChartRevenueData { get; set; } = "[]";
        public string ChartCostData { get; set; } = "[]";
        public string ChartProfitData { get; set; } = "[]";
    }

    public class ProductionReportVM
    {
        public int TotalOrdersReceived { get; set; }
        public decimal PotentialRevenue { get; set; }
        public int TotalOrdersCompleted { get; set; }
        public decimal Revenue { get; set; }
        public decimal TotalMaterialCost { get; set; }

        public decimal GrossProfit => Revenue - TotalMaterialCost;
        public double ProfitMargin => Revenue > 0 ? (double)(GrossProfit / Revenue) * 100 : 0;

        public List<MaterialUsageStats> MaterialStats { get; set; } = new List<MaterialUsageStats>();
    }

    public class MaterialUsageStats
    {
        public string MaterialName { get; set; } = "";
        public string Unit { get; set; } = "";
        public double ImportQty { get; set; }
        public double ExportQty { get; set; }
        public decimal AvgImportPrice { get; set; }
        public decimal TotalCost => (decimal)ExportQty * AvgImportPrice;
    }

    public class CashFlowReportVM
    {
        public decimal OpeningBalance { get; set; }
        public decimal TotalReceipts { get; set; }
        public decimal TotalPayments { get; set; }
        public decimal ClosingBalance => OpeningBalance + TotalReceipts - TotalPayments;
        public decimal NetCashFlow => TotalReceipts - TotalPayments;
    }

    // ViewModel cho trang CashFlow riêng (nếu dùng)
    public class CashFlowYearlyVM
    {
        public int Year { get; set; }
        public decimal YearOpeningBalance { get; set; }
        public decimal YearClosingBalance { get; set; }
        public List<MonthlyCashStats> MonthlyStats { get; set; } = new List<MonthlyCashStats>();
        public string ChartLabels { get; set; } = "[]";
        public string ChartBalanceData { get; set; } = "[]";
    }

    public class MonthlyCashStats
    {
        public int Month { get; set; }
        public decimal Opening { get; set; }
        public decimal In { get; set; }
        public decimal Out { get; set; }
        public decimal Closing => Opening + In - Out;
        public decimal NetChange => In - Out;
    }
}