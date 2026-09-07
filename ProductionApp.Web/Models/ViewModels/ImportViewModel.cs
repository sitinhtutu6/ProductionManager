using Microsoft.AspNetCore.Http;
using ProductionManager.Data;
using System.ComponentModel.DataAnnotations;

namespace ProductionApp.Web.Models.ViewModels
{
    public class ImportViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn file Excel")]
        [Display(Name = "File Excel (.xlsx)")]
        public IFormFile File { get; set; }

        [Required]
        [Display(Name = "Chế độ nhập liệu")]
        public ImportMode Mode { get; set; } = ImportMode.Append;

        public PaymentMethod TargetMethod { get; set; } = PaymentMethod.Cash;
    }

    public enum ImportMode
    {
        Append = 0,     // Nối tiếp
        Overwrite = 1   // Ghi đè
    }
    public class CashBookStat
    {
        public int Month { get; set; }
        public decimal In { get; set; }
        public decimal Out { get; set; }
    }
}