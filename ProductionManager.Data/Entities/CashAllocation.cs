using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductionManager.Data
{
    public class CashAllocation
    {
        [Key]
        public int Id { get; set; }

        public int CashEntryId { get; set; }
        [ForeignKey("CashEntryId")]
        public CashEntry CashEntry { get; set; }

        // Tháng/Năm được phân bổ doanh thu/chi phí
        public int TargetMonth { get; set; }
        public int TargetYear { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal AllocatedAmount { get; set; } // Số tiền phân bổ vào tháng này

        public string? Note { get; set; }
    }
}