using System;
using System.ComponentModel.DataAnnotations;

namespace ProductionManager.Data
{
    // Định nghĩa 3 trạng thái
    public enum PartnerStatus
    {
        [Display(Name = "Đang làm")] Active = 0,
        [Display(Name = "Tạm dừng")] Paused = 1,
        [Display(Name = "Kết thúc")] Ended = 2
    }

    public class PartnerContact
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên công ty")]
        public string Company { get; set; } // Bắt buộc

        public string? Department { get; set; } // Thêm ? để cho phép bỏ trống

        [Required(ErrorMessage = "Vui lòng nhập tên người liên hệ")]
        public string ContactName { get; set; } // Bắt buộc

        public string? Phone { get; set; } // Cho phép bỏ trống
        public string? Email { get; set; } // Cho phép bỏ trống
        public string? Zalo { get; set; } // Cho phép bỏ trống
        public string? Telegram { get; set; } // Cho phép bỏ trống

        public DateTime StartDate { get; set; } = DateTime.Now; // Mặc định ngày khởi tạo

        public DateTime? EndDate { get; set; } // Có thể bỏ trống

        public PartnerStatus Status { get; set; } = PartnerStatus.Active;

        public string? JobDescription { get; set; } // Cho phép bỏ trống
        public string? Notes { get; set; } // Cho phép bỏ trống

        // Hàm tiện ích: Tự động cập nhật trạng thái nếu đã đến hạn EndDate
        public void UpdateStatusBasedOnDate()
        {
            if (EndDate.HasValue && EndDate.Value.Date <= DateTime.Now.Date)
            {
                Status = PartnerStatus.Ended;
            }
        }
    }
}