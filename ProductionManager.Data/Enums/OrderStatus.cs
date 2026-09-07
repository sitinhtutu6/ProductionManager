namespace ProductionManager.Data
{
    public enum OrderStatus
    {
        PendingApproval = 0, // Chờ duyệt
        Approved = 1,        // Đã duyệt (Chờ SX)
        InProduction = 2,    // Đang sản xuất
        Completed = 3,        // Thành phẩm (Chờ giao)
        Packing = 4,         // Đang đóng gói
        Delivered = 5,       // Đã giao hàng xong
        Invoiced = 6,       // Đã xuất hoá đơn
        Canceled = 99        // Hủy đơn
    }

    public enum WorkOrderStatus
    {
        Pending = 0,       // Chờ đưa vào máy / Chờ xếp lịch
        InProgress = 1,    // Đang sản xuất / Đang gia công
        Completed = 2,     // Đã hoàn thành (Sẵn sàng nhập kho/giao hàng)
        Paused = 3,        // Tạm dừng (Thiếu vật tư, hỏng máy...)
        Canceled = 99      // Đã hủy
    }

}