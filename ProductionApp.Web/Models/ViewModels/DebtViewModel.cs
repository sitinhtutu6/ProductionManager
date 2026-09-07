using System;
using System.Collections.Generic;

namespace ProductionApp.Web.Models.ViewModels
{
    // 1 dòng chi tiết (1 Đơn hàng hoặc 1 Phiếu nhập)
    public class DebtDetailItem
    {
        public int Id { get; set; } // OrderId hoặc MaterialImportId
        public string Code { get; set; } // Mã đơn / Mã phiếu
        public DateTime Date { get; set; }
        public decimal TotalAmount { get; set; } // Tổng tiền đơn
        public decimal PaidAmount { get; set; }  // Đã trả
        public decimal RemainingAmount => TotalAmount - PaidAmount; // Còn nợ
        public string Note { get; set; }

        public bool IsLocked { get; set; }
    }

    // 1 Nhóm (1 Khách hàng hoặc 1 NCC)
    public class PartnerDebtGroup
    {
        public int PartnerId { get; set; } // CustomerId hoặc null nếu là NCC vãng lai
        public string PartnerName { get; set; }
        public string PartnerCode { get; set; }
        public string Phone { get; set; }
        public decimal TotalDebt { get; set; } // Tổng nợ của ông này
        public List<DebtDetailItem> Details { get; set; } = new List<DebtDetailItem>();
    }

    // ViewModel cho toàn trang
    public class DebtDashboardVM
    {
        public DateTime? FromDate { get; set; } // Cho phép null để lấy "Tất cả"
        public DateTime? ToDate { get; set; }
        public string Keyword { get; set; }
        public int Type { get; set; } // 1: Thu, 2: Trả
        public string SortOrder { get; set; } = "debt_desc"; // Mặc định: Nợ giảm dần

        public decimal TotalReceivable { get; set; }
        public decimal TotalPayable { get; set; }

        public List<PartnerDebtGroup> DebtGroups { get; set; }
    }


}