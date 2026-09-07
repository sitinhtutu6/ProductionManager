using System.Collections.Generic;

namespace ProductionApp.Web.Models.ViewModels
{
    public class MonthlyRealizedReportVM
    {
        public int Month { get; set; }
        public int Year { get; set; }

        // ==========================================
        // 1. DOANH THU (GIAO HÀNG vs XUẤT HÓA ĐƠN)
        // ==========================================
        public decimal TotalDeliveredRevenue { get; set; } // Tổng tiền hàng đã giao (Hoàn thành)
        public decimal BilledRevenue { get; set; }         // Thực xuất Hóa đơn
        public decimal UnbilledRevenue { get; set; }       // Còn nợ Hóa đơn chưa xuất

        // ==========================================
        // 2. CHI PHÍ VẬT TƯ (MUA VÀO vs THỰC DÙNG)
        // ==========================================
        public decimal TotalMaterialPurchased { get; set; } // Tổng tiền mua vật tư trong tháng
        public decimal MaterialConsumed { get; set; }       // Thực dùng (Đã xuất kho SX)

        // Phần còn tồn đọng trong kho (Mua - Dùng)
        // *Lưu ý: Có thể âm nếu tháng này không mua mà lôi kho cũ ra xài
        public decimal MaterialRemaining => TotalMaterialPurchased - MaterialConsumed;

        // ==========================================
        // 3. CHI PHÍ VẬN HÀNH (OPEX - GOM NHÓM ĐỘNG)
        // ==========================================
        public Dictionary<string, decimal> OpexGroups { get; set; } = new Dictionary<string, decimal>();
        public decimal TotalOpex { get; set; }

        // ==========================================
        // 4. TỔNG HỢP SO SÁNH
        // ==========================================
        // Tổng tiền thực sự đã xuất ra khỏi túi (Chi mua vật tư + Chi OPEX)
        public decimal TotalSpent => TotalMaterialPurchased + TotalOpex;

        // Tổng chi phí cấu thành giá vốn & hạch toán (Dùng vật tư + Chi OPEX)
        public decimal TotalConsumed => MaterialConsumed + TotalOpex;
    }
}