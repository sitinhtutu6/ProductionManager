using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductionManager.Data
{
    public enum TicketType
    {
        Import = 1,    // Nhập kho
        Export = 2,    // Xuất kho
        Inventory = 3  // Kiểm kê
    }

    public class WarehouseTicket
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string TicketCode { get; set; } // Mã phiếu (NK-..., XK-...)

        public TicketType Type { get; set; }

        public DateTime Date { get; set; } = DateTime.Now;

        [StringLength(200)]
        public string Subject { get; set; } // Người giao/nhận hoặc Tiêu đề kiểm kê

        public string? Note { get; set; }   // Ghi chú

        // Người tạo phiếu (User đăng nhập)
        public string? CreatorId { get; set; }

        // Danh sách chi tiết hàng hóa
        public List<WarehouseTicketDetail> TicketDetails { get; set; } = new List<WarehouseTicketDetail>();
    }

    public class WarehouseTicketDetail
    {
        [Key]
        public int Id { get; set; }

        public int WarehouseTicketId { get; set; }
        [ForeignKey("WarehouseTicketId")]
        public WarehouseTicket WarehouseTicket { get; set; }

        public int ProductDetailId { get; set; } // ID sản phẩm (Variant)
        [ForeignKey("ProductDetailId")]
        public ProductDetail ProductDetail { get; set; }

        public int Quantity { get; set; } // Số lượng thay đổi

        // Dùng cho kiểm kê: Số lượng tồn kho lúc kiểm
        public int? SystemStock { get; set; }
    }
}