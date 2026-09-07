using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductionManager.Data
{
    [Table("OrderHistory")]
    public class OrderHistory
    {
        [Key]
        public int Id { get; set; }
        public int? OrderId { get; set; } // Nếu thay đổi 1 đơn lẻ
        public string? Action { get; set; } // VD: "Sắp xếp lại", "Đổi trạng thái"
        public string? OldValue { get; set; } // Giá trị cũ (JSON hoặc chuỗi)
        public string? NewValue { get; set; } // Giá trị mới
        public DateTime ChangedAt { get; set; } = DateTime.Now;
        public string? ChangedBy { get; set; } // Người thực hiện
    }
}