using System.ComponentModel.DataAnnotations;

namespace ProductionManager.Data
{
    public class SystemNotification
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Nội dung thông báo")]
        public string Message { get; set; } = "";

        [Display(Name = "Bắt đầu hiển thị")]
        public DateTime StartTime { get; set; } = DateTime.Now;

        [Display(Name = "Kết thúc hiển thị")]
        public DateTime EndTime { get; set; } = DateTime.Now.AddDays(7);

        [Display(Name = "Bật/Tắt")]
        public bool IsEnabled { get; set; } = true;
    }
}