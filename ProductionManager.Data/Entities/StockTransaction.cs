using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace ProductionManager.Data
{
    // ENUM LÝ DO XUẤT KHO ĐỂ BÁO CÁO P&L ĐIỀU HƯỚNG DÒNG TIỀN
    public enum ExportReason
    {
        [Display(Name = "Xuất để Sản xuất / Bán hàng")]
        ProductionOrSale = 1, // 👉 P&L tính vào Giá vốn (COGS)

        [Display(Name = "Xuất trả Nhà cung cấp")]
        ReturnToSupplier = 2, // 👉 P&L lờ đi (Chỉ giảm tồn kho)

        [Display(Name = "Xuất hủy / Hao hụt / Hư hỏng")]
        DamageOrLoss = 3      // 👉 P&L tính vào Chi phí khác (OPEX)
    }


    public class StockTransaction
    {
        [Key]
        public int Id { get; set; }

        public int WarehouseItemId { get; set; }
        [ForeignKey("WarehouseItemId")]
        public WarehouseItem? WarehouseItem { get; set; }

        public DateTime TransactionDate { get; set; } = DateTime.Now;

        // Loại giao dịch: "Import" (Nhập), "Export" (Xuất), "Adjustment" (Kiểm kê/Cân bằng), "Opening" (Tồn đầu)
        [Required]
        public string Type { get; set; } = "Import";

        public double Quantity { get; set; } // Số lượng thay đổi (+ hoặc -)

        public double CurrentStock { get; set; } // Tồn kho sau khi giao dịch (Snapshot)

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; } = 0; // Giá tại thời điểm giao dịch

        public string? Note { get; set; } // Diễn giải (VD: Nhập lô hàng X, Xuất cho SX đơn Y)

        [StringLength(50)]
        public string? DocumentCode { get; set; } // Mã phiếu (PN-001, PX-002...)

        [StringLength(100)]
        public string? Receiver { get; set; } // Người nhận / Người giao hàng

        [StringLength(100)]
        public string? Staff { get; set; } // Email nhân viên thực hiện (Lấy tự động)

        public int? OrderId { get; set; } // Dấu ? cho phép null (để nhập kho chung không cần đơn)
        [ForeignKey("OrderId")]
        public Order? Order { get; set; } // Navigation property để truy cập thông tin đơn hàng (OrderCode, Customer...)


        public ExportReason? Reason { get; set; }
    }
}