using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductionManager.Data
{
    public class ImportPriceHistory
    {
        [Key]
        public int Id { get; set; }

        public int MaterialImportId { get; set; }
        [ForeignKey("MaterialImportId")]
        public MaterialImport MaterialImport { get; set; }

        public decimal OldTotalAmount { get; set; } // Giá cũ
        public decimal NewTotalAmount { get; set; } // Giá mới
        public string Reason { get; set; }          // Lý do đổi
        public DateTime ChangedAt { get; set; } = DateTime.Now;
        public string ChangedBy { get; set; }       // Người đổi
    }
}