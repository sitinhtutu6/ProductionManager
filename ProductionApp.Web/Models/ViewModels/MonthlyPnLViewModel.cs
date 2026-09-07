using System.Collections.Generic;

namespace ProductionApp.Web.Models.ViewModels
{
    public class MonthlyPnLViewModel
    {
        public int Month { get; set; }
        public int Year { get; set; }

        // ==========================================
        // I. DOANH THU & THU NHẬP
        // ==========================================
        public decimal TotalRevenue { get; set; }

        // 2 biến mới thêm cho Tách Thuế VAT
        public decimal NetRevenue { get; set; }  // Doanh thu thuần
        public decimal VatPayable { get; set; }  // VAT phải nộp

        public decimal OtherIncome { get; set; }
        public List<ExpenseDetail> OtherIncomeDetails { get; set; } = new List<ExpenseDetail>();

        // ==========================================
        // II. GIÁ VỐN HÀNG BÁN & LỢI NHUẬN GỘP
        // ==========================================
        public decimal TotalCOGS { get; set; }

        // Đã sửa thành { get; set; } để mở khóa (Fix lỗi CS0200)
        public decimal GrossProfit { get; set; }

        // Các biến tham khảo Dòng tiền vật tư
        public decimal MaterialImport_CurrentMonth { get; set; }
        public decimal MaterialDebt_Total { get; set; }
        public List<MaterialDebtDetail> UnpaidMaterialDetails { get; set; } = new List<MaterialDebtDetail>();

        // ==========================================
        // III. CHI PHÍ VẬN HÀNH (OPEX)
        // ==========================================
        /*
        public decimal SalaryAndInsurance { get; set; }
        public decimal UtilitiesExpense { get; set; }
        public decimal OtherExpenses { get; set; }
        */
        public List<ExpenseDetail> OtherExpenseDetails { get; set; } = new List<ExpenseDetail>();
        // Thêm biến này vào class MonthlyPnLViewModel
        public Dictionary<string, decimal> OpexGroups { get; set; } = new Dictionary<string, decimal>();

        // Đã sửa thành { get; set; } để mở khóa (Fix lỗi CS0200)
        public decimal TotalOperatingExpense { get; set; }

        // ==========================================
        // IV. LỢI NHUẬN RÒNG & THUẾ TNDN
        // ==========================================

        // 2 biến mới thêm cho Thuế TNDN
        public decimal ProfitBeforeTax { get; set; } // Lợi nhuận trước thuế (EBT)
        public decimal CorporateTax { get; set; }    // Thuế TNDN (CIT)

        // Đã sửa thành { get; set; } để mở khóa (Fix lỗi CS0200)
        public decimal NetProfit { get; set; }
    }

    // Các Class phụ trợ (Giữ nguyên)
    public class ExpenseDetail
    {
        public string Date { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
    }

    public class MaterialDebtDetail
    {
        public string SupplierName { get; set; }
        public string ImportCode { get; set; }
        public decimal DebtAmount { get; set; }
    }
}