using System.ComponentModel.DataAnnotations;

namespace ProductionManager.Data
{
    public class CompanyConfig
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string CompanyName { get; set; } = "CÔNG TY TNHH SẢN XUẤT DEMO";

        public string? Address { get; set; } = "P. Thủ Dầu Một, TP. Hồ Chí Minh";

        public string? TaxCode { get; set; } = "0312345xxx";

        public string? Phone { get; set; } = "0909 xxx xxx";

        public string? Email { get; set; } = "contact@company.com";
    }
}