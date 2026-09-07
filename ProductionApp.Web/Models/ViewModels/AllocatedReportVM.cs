using System;
using System.Collections.Generic;

namespace ProductionApp.Web.Models.ViewModels
{
    public class AllocatedReportVM
    {
        public int Month { get; set; }
        public int Year { get; set; }

        public decimal TotalRevenueAllocated { get; set; }
        public decimal TotalCostAllocated { get; set; }
        public decimal NetProfit => TotalRevenueAllocated - TotalCostAllocated;

        public List<AllocatedDetail> AllocatedList { get; set; } = new List<AllocatedDetail>();

        public List<UnallocatedItem> UnallocatedItems { get; set; } = new List<UnallocatedItem>();
    }

    public class AllocatedDetail
    {
        public int AllocationId { get; set; }
        public string VoucherCode { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Description { get; set; }
        public string Type { get; set; } // "Thu" hoặc "Chi"
        public decimal Amount { get; set; }
        public string Note { get; set; }
    }

    public class UnallocatedItem
    {
        public int EntryId { get; set; }
        public DateTime TransactionDate { get; set; }
        public string VoucherCode { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AllocatedSoFar { get; set; }
        public decimal RemainingAmount => TotalAmount - AllocatedSoFar;
    }
}