using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductionManager.Data
{
    public enum ShipmentType
    {
        Standard = 0,   // Xuất hàng
        Warranty = 1,   // Xuất bù hàng hỏng / Bảo hành
        Return = 2      // Nhập trả hàng lỗi (Hệ thống tự sinh)
    }


    public class Shipment
    {
        public int Id { get; set; }
        public string ShipmentCode { get; set; }
        public DateTime ShipmentDate { get; set; }

        // --- THAY ĐỔI: Thêm Khách hàng, OrderId cho phép null (vì giao gộp) ---
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }

        public int? OrderId { get; set; } // Để null hoặc lưu ID đơn hàng chính (nếu cần)
        public Order? Order { get; set; }
        // ----------------------------------------------------------------------

        public string? VehicleNumber { get; set; }
        public string? Notes { get; set; }

        public List<ShipmentDetail> ShipmentDetails { get; set; }

        public int? ExportShipmentId { get; set; }

        [ForeignKey("ExportShipmentId")]
        public ExportShipment? ExportShipment { get; set; }

        public ShipmentType Type { get; set; } = ShipmentType.Standard;

        public int? ParentShipmentId { get; set; }

        [ForeignKey("ParentShipmentId")]
        public Shipment? ParentShipment { get; set; }
    }
}