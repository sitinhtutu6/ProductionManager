using System;
using System.Collections.Generic;

namespace ProductionApp.Web.Models.ViewModels
{
    public enum LoanType
    {
        Borrowing, // Đi vay (Mình nợ người ta)
        Lending    // Cho vay (Người ta nợ mình)
    }

    public class LoanVM
    {
        public int Id { get; set; }
        public LoanType Type { get; set; }
        public string PartnerName { get; set; } // Chủ nợ hoặc Con nợ
        public DateTime StartDate { get; set; }
        public decimal TotalAmount { get; set; } // Gốc
        public decimal ProcessedAmount { get; set; } // Đã trả/Đã thu

        // Tính toán
        public decimal Remaining => TotalAmount - ProcessedAmount;
        public double Progress => TotalAmount == 0 ? 0 : (double)(ProcessedAmount / TotalAmount * 100);
        public bool IsFinished => Remaining <= 0;

        public List<RepaymentHistory> History { get; set; } = new List<RepaymentHistory>();
    }

    public class RepaymentHistory
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string Note { get; set; }
        public string VoucherCode { get; set; }
    }
}