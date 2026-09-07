using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductionManager.Data
{
    public class ProductPriceHistory
    {
        [Key]
        public int Id { get; set; }

        public int ProductId { get; set; }

        // Nullable: Nếu null thì là đổi giá của SP cha, có giá trị là đổi giá của biến thể
        public int? ProductDetailId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal OldPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal NewPrice { get; set; }

        public DateTime ChangedDate { get; set; } = DateTime.Now;
        public string? Note { get; set; } // Ghi chú lý do đổi giá
    }
}