using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductionManager.Data
{
    public class ShipmentDetail
    {
        [Key]
        public int Id { get; set; }

        public int ShipmentId { get; set; }
        [ForeignKey("ShipmentId")]
        public Shipment Shipment { get; set; }

        // Giao cho dòng sản phẩm nào trong đơn hàng gốc?
        public int OrderDetailId { get; set; }
        [ForeignKey("OrderDetailId")]
        public OrderDetail OrderDetail { get; set; }

        public int QuantityShipped { get; set; } // Số lượng GIAO THỰC TẾ đợt này
    }
}