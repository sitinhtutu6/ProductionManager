using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductionManager.Data.Entities
{
    public class WorkOrderLog
    {
        public int Id { get; set; }
        public int WorkOrderId { get; set; } // Khóa ngoại liên kết tới WorkOrder
        public string UserName { get; set; } // Người thay đổi
        public string ChangeDetail { get; set; } // Ví dụ: "-10 ➔ -15"
        public string Note { get; set; } // Lý do thay đổi
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
