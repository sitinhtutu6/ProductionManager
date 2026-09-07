using System;
using System.ComponentModel.DataAnnotations;

namespace ProductionManager.Data
{
    public class FinancialPeriod
    {
        [Key]
        public int Id { get; set; }

        public int Month { get; set; }
        public int Year { get; set; }

        public bool IsClosed { get; set; } = false; // Trạng thái Khóa sổ

        public DateTime? ClosedAt { get; set; }     // Thời gian khóa
        public string? ClosedBy { get; set; }       // Người thực hiện khóa
    }
}