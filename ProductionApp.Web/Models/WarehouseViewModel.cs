using System;
using System.Collections.Generic;

namespace ProductionApp.Web.Models
{
    // 1. Model hứng dữ liệu JSON khi Nhập/Xuất nhiều dòng

    // File: ProductionApp.Web.Models.WarehouseViewModel.cs


    public class BulkTransactionViewModel
    {
        public string Type { get; set; }
        public string DocumentCode { get; set; }
        public string Receiver { get; set; }
        public int? OrderId { get; set; } // ID Đơn hàng (nếu có)
        public string Note { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public List<TransactionItemDetail> Items { get; set; } = new List<TransactionItemDetail>();

        public decimal TotalAmount { get; set; }

        // THÊM 3 TRƯỜNG NÀY VÀO CUỐI
        public int? PurchaseOrderId { get; set; } // Liên kết với Đơn Đặt Hàng NCC
        public int PaymentMethod { get; set; }    // 0: Công nợ, 1: Tiền mặt, 2: Chuyển khoản
        public decimal PaidAmount { get; set; }   // Số tiền đã trả (để tự động sinh phiếu chi)

        public bool IsFinishPO { get; set; }      // Cờ chốt đơn

        public int? Reason { get; set; }
    }


    // ==========================================
    // 2. KIỂM KÊ KHO (STOCK TAKE) - CẬP NHẬT
    // ==========================================
    public class StockTakeViewModel
    {
        public string DocumentCode { get; set; } // Mã phiếu (nếu cần)
        public string Note { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public List<StockTakeItem> Items { get; set; } = new List<StockTakeItem>();
    }

    public class StockTakeItem
    {
        public int WarehouseItemId { get; set; }

        // --- CÁC TRƯỜNG MỚI CHO UI KIỂM KÊ ---
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string Category { get; set; }

        public double SystemStock { get; set; } // Tồn phần mềm (để so sánh)                                            

        public double RealStock { get; set; }   // Số lượng thực tế (Đổi tên từ RealQuantity để khớp logic Controller mới)
    }

    public class BulkItem
    {
        public int WarehouseItemId { get; set; }
        public double Quantity { get; set; } // Số lượng OK (Vào kho)
        public double QuantityNG { get; set; } // Số lượng Hỏng (Chỉ ghi nhận) - MỚI
        public decimal Price { get; set; }
    }

    public class TransactionItemDetail
    {
        public int WarehouseItemId { get; set; }
        public double Quantity { get; set; }
        public double QuantityNG { get; set; } = 0;// Số lượng Hỏng
        public decimal Price { get; set; } // Giá nhập (nếu có)
        public string Category { get; set; }
    }


    // ==========================================
    // 3. BÁO CÁO (REPORT)
    // ==========================================
    public class MonthlyReportViewModel
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal TotalValue { get; set; } // Tổng giá trị kho hiện tại
        public List<StockItemReport> Items { get; set; }
    }

    public class StockItemReport
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string Unit { get; set; }

        public double OpeningStock { get; set; } // Tồn đầu kỳ
        public double ImportQty { get; set; }    // Nhập trong kỳ
        public double ExportQty { get; set; }    // Xuất trong kỳ
        public double ClosingStock { get; set; } // Tồn cuối kỳ

        public decimal CostPrice { get; set; }
        public decimal TotalValue => (decimal)ClosingStock * CostPrice; // Thành tiền
    }

    public class StockReportViewModel
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public decimal TotalValue { get; set; }
        public List<StockItemReport> Items { get; set; } = new List<StockItemReport>();
    }

}