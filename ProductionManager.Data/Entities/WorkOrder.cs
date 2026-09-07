using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductionManager.Data.Entities
{
    public class WorkOrder
    {
        [Key]
        public int Id { get; set; }

        // Liên kết ngược lại đơn hàng gốc
        public int OrderId { get; set; }
        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; }

        [Required]
        [StringLength(50)]
        public string WOCode { get; set; } // Mã lệnh sản xuất (VD: LSX-001)
        public string ProductName { get; set; }
        public int TargetQuantity { get; set; } // Số lượng cần làm
        public int FinishedQuantity { get; set; } = 0; // Số lượng thực tế đã xong

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime StartDate { get; set; } = DateTime.Now;
        public DateTime? EndDate { get; set; }

        public WorkOrderStatus Status { get; set; } = WorkOrderStatus.Pending; // Enum riêng cho xưởng
        public string? MachineId { get; set; } // Phân bổ máy móc

        // ==========================================
        // CÁC CỘT PHỤC VỤ CHO BẢNG KẾ HOẠCH KÉO THẢ
        // ==========================================
        public int ProductionIndex { get; set; } = 0; // Lưu thứ tự kéo thả
        public string PlanColor { get; set; } = "#ffffff"; // Lưu màu đánh dấu
        public string? Notes { get; set; } // Lấy ghi chú từ đơn gốc sang cho xưởng đọc

        public int ShortageQuantity { get; set; } = 0;
    }
}
