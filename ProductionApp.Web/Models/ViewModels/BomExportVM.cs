namespace ProductionApp.Web.Models.ViewModels
{
    public class BomExportVM
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }

        // Các trường cấu hình (Config)
        public string CompanyName { get; set; } = "CÔNG TY TNHH SẢN XUẤT ABC";
        public string CompanyAddress { get; set; } = "123 Đường KCN, Bình Dương, Việt Nam";
        public string CreatorName { get; set; } // Người lập phiếu
        public string ApproverName { get; set; } = "Ban Giám Đốc"; // Người duyệt
        public string WatermarkText { get; set; } = "LƯU HÀNH NỘI BỘ";
        public bool ShowLogo { get; set; } = true;
        public IFormFile? LogoFile { get; set; }
    }
}