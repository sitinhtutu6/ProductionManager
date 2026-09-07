namespace ProductionApp.Web.Models.ViewModels
{
    public class ProductProfileViewModel
    {
        public ProductionManager.Data.Product Product { get; set; }
        public List<TimelineItem> Timeline { get; set; } = new List<TimelineItem>();
    }

    public class TimelineItem
    {
        public int Id { get; set; } // Id của Document hoặc PriceHistory
        public DateTime Date { get; set; }
        public string Type { get; set; } // "Price" hoặc "Document"
        public string Title { get; set; }
        public string Detail { get; set; }
        public string? FilePath { get; set; }
        public string? Icon { get; set; }
        public string? ColorClass { get; set; }
    }
}