using ProductionManager.Data;
using System.ComponentModel.DataAnnotations;

namespace ProductionApp.Web.Models.ViewModels
{
    public class MasterPlanVM
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        // 1. Danh sách tổng hợp vật tư toàn xưởng (Cộng dồn tất cả đơn)
        public List<PlanMaterialSummary> AggregatedMaterials { get; set; } = new List<PlanMaterialSummary>();

        // 2. Danh sách chi tiết từng đơn hàng
        public List<PlanOrder> Orders { get; set; } = new List<PlanOrder>();

        // 3. Danh sách cảnh báo (Đơn hàng chưa có BOM)
        public List<string> Warnings { get; set; } = new List<string>();
    }

    public class PlanMaterialSummary
    {
        public int MaterialId { get; set; }
        public string MaterialName { get; set; }
        public string Unit { get; set; }
        public double TotalNeeded { get; set; }  // Tổng cần cho tất cả đơn
        public double CurrentStock { get; set; } // Tồn kho hiện tại
        public double Missing => (TotalNeeded - CurrentStock) > 0 ? (TotalNeeded - CurrentStock) : 0; // Cần mua/sản xuất thêm
    }

    public class PlanOrder
    {
        public int OrderId { get; set; }
        public string OrderCode { get; set; }
        public string CustomerName { get; set; }
        public DateTime DeliveryDate { get; set; }
        public string Status { get; set; }
        public string ProgressColor { get; set; }

        // Vật tư dự kiến theo BOM
        public List<PlanOrderDetailItem> BomRequirements { get; set; } = new List<PlanOrderDetailItem>();

        // Vật tư đã xuất thực tế (Lịch sử)
        public List<PlanOrderExportedItem> ExportedHistory { get; set; } = new List<PlanOrderExportedItem>();
    }

    public class PlanOrderDetailItem
    {
        public int ProductId { get; set; } // 🔥 THÊM DÒNG NÀY
        public string ProductName { get; set; }
        public string MaterialName { get; set; }
        public string Unit { get; set; }
        public double QuantityNeeded { get; set; }
    }

    public class PlanOrderExportedItem
    {
        public string MaterialName { get; set; }
        public string Unit { get; set; }
        public double QuantityExported { get; set; }
    }

    // 1. View Model cho Index
    public class BomProductStatsVM
    {
        public int ProductId { get; set; }
        public string ProductCode { get; set; } // Lấy từ Product.ProductCode
        public string ProductName { get; set; } // Lấy từ Product.ProductName
        public string Unit { get; set; }        // Lấy từ Product.Unit
        public int MaterialCount { get; set; }
        public decimal EstimatedCost { get; set; }
        public string? ImagePath { get; set; }
    }

    // 2. View Model cho Create/Edit
    public class BomCreateVM
    {
        public int ProductId { get; set; }
        public List<BomItemInput> Items { get; set; } = new List<BomItemInput>();
    }

    // (Class BomItemInput giữ nguyên như cũ)
    public class BomItemInput
    {
        public int MaterialId { get; set; }
        public string ComponentName { get; set; }
        public string? Unit { get; set; }
        public double? Length { get; set; }
        public double? Width { get; set; }
        public double? Height { get; set; }

        public double CutQuantity { get; set; }
        public double Coefficient { get; set; } = 1;
        public string? Note { get; set; }
    }

    // 3. View Model cho Details
    public class BomDetailVM
    {
        public Product Product { get; set; } // 🔥 Sửa thành Product
        public List<BomDetailItem> Materials { get; set; } = new List<BomDetailItem>();
        public List<BomSummaryItem> Summary { get; set; } = new List<BomSummaryItem>();
        public decimal TotalCost { get; set; }
    }

    // (Class BomDetailItem và BomSummaryItem giữ nguyên)
    public class BomDetailItem
    {
        public string MaterialName { get; set; }
        public string ComponentName { get; set; }
        public string? Unit { get; set; }
        public double Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice;
        public string Note { get; set; }
        public double? Length { get; set; }
        public double? Width { get; set; }
        public double? Height { get; set; }

        public double CutQuantity { get; set; }
        public double Coefficient { get; set; }

    }

    public class BomSummaryItem
    {
        public string MaterialName { get; set; }
        public string Unit { get; set; }
        public double TotalQuantity { get; set; }
        public decimal TotalCost { get; set; }
    }

    // (MasterBomVM giữ nguyên)
    public class MasterBomVM
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public List<MaterialRequirement> Requirements { get; set; } = new List<MaterialRequirement>();
        public List<string> WarningOrders { get; set; } = new List<string>();
    }
    public class MaterialRequirement
    {
        public int MaterialId { get; set; }
        public string MaterialCode { get; set; }
        public string MaterialName { get; set; }
        public string Unit { get; set; }
        public double TotalQuantityNeeded { get; set; }
        public double CurrentStock { get; set; }
        public double Missing => TotalQuantityNeeded - CurrentStock > 0 ? TotalQuantityNeeded - CurrentStock : 0;
    }

}