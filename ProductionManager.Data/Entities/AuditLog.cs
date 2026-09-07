using System;
using System.ComponentModel.DataAnnotations;

namespace ProductionManager.Data
{
    public class AuditLog
    {
        [Key]
        public int Id { get; set; }
        public string? UserName { get; set; } // Tên người dùng thao tác
        public string Action { get; set; } = ""; // Thêm, Sửa, Xóa
        public string TableName { get; set; } = ""; // Tên bảng (VD: Shipments, Orders)
        public string? RecordId { get; set; } // ID của dòng bị sửa
        public string? OldValues { get; set; } // Dữ liệu cũ (dạng JSON)
        public string? NewValues { get; set; } // Dữ liệu mới (dạng JSON)
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}