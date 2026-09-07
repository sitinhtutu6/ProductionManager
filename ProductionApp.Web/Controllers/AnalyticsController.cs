using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductionApp.Web.Constants;
using ProductionManager.Data;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using ClosedXML.Excel;
using System.IO;

namespace ProductionApp.Web.Controllers
{
    [Authorize]
    public class AnalyticsController : Controller
    {
        private readonly AppDbContext _context;

        public AnalyticsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(DateTime? fromDate, DateTime? toDate)
        {
            // 1. KHỞI TẠO BỘ LỌC THỜI GIAN
            var start = fromDate ?? new DateTime(DateTime.Now.Year, 1, 1);
            var end = toDate ?? DateTime.Now.Date;
            var endOfDay = end.Date.AddDays(1).AddTicks(-1);

            ViewBag.FromDate = start.ToString("yyyy-MM-dd");
            ViewBag.ToDate = end.ToString("yyyy-MM-dd");

            // 2. TRUY VẤN DỮ LIỆU GỐC THEO THỜI GIAN
            var orders = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails)
                .Where(o => o.OrderDate >= start && o.OrderDate <= endOfDay && o.Status != OrderStatus.Canceled)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            var invoices = await _context.Invoices
                .Include(i => i.InvoiceDetails).ThenInclude(d => d.Order)
                .Where(i => i.InvoiceDate >= start && i.InvoiceDate <= endOfDay)
                .OrderByDescending(i => i.InvoiceDate)
                .ToListAsync();

            var cashEntries = await _context.CashEntries
                .Include(c => c.Order)
                .Where(c => c.TransactionDate >= start && c.TransactionDate <= endOfDay && (int)c.Type == 1) // 1 = Thu
                .OrderByDescending(c => c.TransactionDate)
                .ToListAsync();

            // 3. TÍNH TOÁN & PHÂN LOẠI DANH SÁCH KPI
            var receivedOrders = orders.Where(o => o.Status == OrderStatus.Approved || o.Status == OrderStatus.InProduction).ToList();
            var deliveredOrders = orders.Where(o => o.Status == OrderStatus.Delivered || o.Status == OrderStatus.Completed).ToList();

            ViewBag.KpiReceived = CalculateAccurateAmount(receivedOrders);
            ViewBag.KpiDelivered = CalculateAccurateAmount(deliveredOrders);

            decimal invGrandTotal = invoices.Sum(i => i.TotalAmount);
            decimal invSubTotal = invGrandTotal / 1.1m;
            decimal invVat = invGrandTotal - invSubTotal;
            ViewBag.KpiInvoice = new { Sub = invSubTotal, Vat = invVat, Total = invGrandTotal };
            ViewBag.KpiCash = cashEntries.Sum(c => c.Amount);

            // Gửi danh sách chi tiết xuống View
            ViewBag.ListReceived = receivedOrders;
            ViewBag.ListDelivered = deliveredOrders;
            ViewBag.ListInvoices = invoices;
            ViewBag.ListCash = cashEntries;

            // 4. XỬ LÝ DỮ LIỆU CHO BIỂU ĐỒ (CHARTS)
            var statusDistribution = orders.GroupBy(o => o.Status)
                .Select(g => new { Status = GetStatusName(g.Key), Count = g.Count() }).ToList();
            ViewBag.ChartStatus = System.Text.Json.JsonSerializer.Serialize(statusDistribution);

            var monthlyData = new List<object>();
            int startMonth = start.Month;
            int endMonth = end.Month;
            if (start.Year < end.Year) { startMonth = 1; endMonth = 12; }

            for (int i = startMonth; i <= endMonth; i++)
            {
                var monthOrders = orders.Where(o => o.OrderDate.Month == i);
                decimal monthReceived = CalculateAccurateAmount(monthOrders.Where(o => o.Status == OrderStatus.Approved || o.Status == OrderStatus.InProduction).ToList()).Total;
                decimal monthDelivered = CalculateAccurateAmount(monthOrders.Where(o => o.Status == OrderStatus.Delivered || o.Status == OrderStatus.Completed).ToList()).Total;
                decimal monthInv = invoices.Where(inv => inv.InvoiceDate.Month == i).Sum(inv => inv.TotalAmount);

                monthlyData.Add(new { Month = "Tháng " + i, Received = monthReceived, Delivered = monthDelivered, Invoiced = monthInv });
            }
            ViewBag.ChartMonthly = System.Text.Json.JsonSerializer.Serialize(monthlyData);

            return View();
        }

        // ==========================================================
        // XUẤT EXCEL CHI TIẾT 4 HẠNG MỤC
        // ==========================================================
        [HttpGet]
        public async Task<IActionResult> ExportExcel(DateTime? fromDate, DateTime? toDate)
        {
            var start = fromDate ?? new DateTime(DateTime.Now.Year, 1, 1);
            var endOfDay = (toDate ?? DateTime.Now.Date).AddDays(1).AddTicks(-1);

            var orders = await _context.Orders.Include(o => o.Customer).Include(o => o.OrderDetails).Where(o => o.OrderDate >= start && o.OrderDate <= endOfDay && o.Status != OrderStatus.Canceled).ToListAsync();
            var invoices = await _context.Invoices.Include(i => i.InvoiceDetails).ThenInclude(d => d.Order).Where(i => i.InvoiceDate >= start && i.InvoiceDate <= endOfDay).ToListAsync();
            var cashEntries = await _context.CashEntries.Include(c => c.Order).Where(c => c.TransactionDate >= start && c.TransactionDate <= endOfDay && (int)c.Type == 1).ToListAsync();

            using (var wb = new XLWorkbook())
            {
                var titleStyle = wb.Style; titleStyle.Font.Bold = true; titleStyle.Font.FontColor = XLColor.White; titleStyle.Fill.BackgroundColor = XLColor.Teal; titleStyle.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // 1. SHEET ĐƠN ĐÃ NHẬN
                var wsRcv = wb.Worksheets.Add("1. Đơn Đã Nhận");
                string[] hd1 = { "STT", "Mã Đơn", "Khách Hàng", "Ngày Nhận", "Tiền Hàng", "Thuế VAT", "Tổng Cộng", "Trạng Thái" };
                for (int i = 0; i < hd1.Length; i++) { wsRcv.Cell(1, i + 1).Value = hd1[i]; wsRcv.Cell(1, i + 1).Style = titleStyle; }

                int row = 2; int stt = 1;
                foreach (var o in orders.Where(o => o.Status == OrderStatus.Approved || o.Status == OrderStatus.InProduction))
                {
                    var kpi = CalculateAccurateAmount(new List<Order> { o });
                    wsRcv.Cell(row, 1).Value = stt++;
                    wsRcv.Cell(row, 2).Value = o.OrderCode;
                    wsRcv.Cell(row, 3).Value = o.Customer?.CompanyName;
                    wsRcv.Cell(row, 4).Value = o.OrderDate.ToString("dd/MM/yyyy");
                    wsRcv.Cell(row, 5).Value = kpi.Sub;
                    wsRcv.Cell(row, 6).Value = kpi.Vat;
                    wsRcv.Cell(row, 7).Value = kpi.Total;
                    wsRcv.Cell(row, 8).Value = GetStatusName(o.Status);
                    row++;
                }
                wsRcv.Columns().AdjustToContents();

                // 2. SHEET ĐƠN ĐÃ GIAO
                var wsDlv = wb.Worksheets.Add("2. Đơn Đã Giao");
                for (int i = 0; i < hd1.Length; i++) { wsDlv.Cell(1, i + 1).Value = hd1[i]; wsDlv.Cell(1, i + 1).Style = titleStyle; }

                row = 2; stt = 1;
                foreach (var o in orders.Where(o => o.Status == OrderStatus.Delivered || o.Status == OrderStatus.Completed))
                {
                    var kpi = CalculateAccurateAmount(new List<Order> { o });
                    wsDlv.Cell(row, 1).Value = stt++;
                    wsDlv.Cell(row, 2).Value = o.OrderCode;
                    wsDlv.Cell(row, 3).Value = o.Customer?.CompanyName;
                    wsDlv.Cell(row, 4).Value = o.OrderDate.ToString("dd/MM/yyyy");
                    wsDlv.Cell(row, 5).Value = kpi.Sub;
                    wsDlv.Cell(row, 6).Value = kpi.Vat;
                    wsDlv.Cell(row, 7).Value = kpi.Total;
                    wsDlv.Cell(row, 8).Value = GetStatusName(o.Status);
                    row++;
                }
                wsDlv.Columns().AdjustToContents();

                // 3. SHEET HÓA ĐƠN
                var wsInv = wb.Worksheets.Add("3. Hóa Đơn Đã Xuất");
                string[] hd3 = { "STT", "Số HĐ", "Ngày Xuất", "Đơn Tham Chiếu", "Tổng Tiền (Gồm VAT)" };
                for (int i = 0; i < hd3.Length; i++) { wsInv.Cell(1, i + 1).Value = hd3[i]; wsInv.Cell(1, i + 1).Style = titleStyle; }

                row = 2; stt = 1;
                foreach (var inv in invoices)
                {
                    string refOrders = string.Join(", ", inv.InvoiceDetails?.Select(d => d.Order?.OrderCode) ?? new List<string>());
                    wsInv.Cell(row, 1).Value = stt++;
                    wsInv.Cell(row, 2).Value = inv.InvoiceNumber;
                    wsInv.Cell(row, 3).Value = inv.InvoiceDate.ToString("dd/MM/yyyy");
                    wsInv.Cell(row, 4).Value = refOrders;
                    wsInv.Cell(row, 5).Value = inv.TotalAmount;
                    row++;
                }
                wsInv.Columns().AdjustToContents();

                // 4. SHEET THỰC THU
                var wsCash = wb.Worksheets.Add("4. Dòng Tiền Thu");
                string[] hd4 = { "STT", "Số Phiếu", "Ngày Thu", "Khách / Người Nộp", "Chứng Từ Chiếu", "Số Tiền" };
                for (int i = 0; i < hd4.Length; i++) { wsCash.Cell(1, i + 1).Value = hd4[i]; wsCash.Cell(1, i + 1).Style = titleStyle; }

                row = 2; stt = 1;
                foreach (var c in cashEntries)
                {
                    wsCash.Cell(row, 1).Value = stt++;
                    wsCash.Cell(row, 2).Value = c.VoucherCode;
                    wsCash.Cell(row, 3).Value = c.TransactionDate.ToString("dd/MM/yyyy");
                    wsCash.Cell(row, 4).Value = c.TargetName;
                    wsCash.Cell(row, 5).Value = c.Order?.OrderCode ?? c.PaperVoucherNumber;
                    wsCash.Cell(row, 6).Value = c.Amount;
                    row++;
                }
                wsCash.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"BaoCaoKinhDoanh_{start:MM_yyyy}.xlsx");
                }
            }
        }

        // ==========================================================
        // CÁC HÀM TIỆN ÍCH HỖ TRỢ
        // ==========================================================
        private dynamic CalculateAccurateAmount(List<Order> orderList)
        {
            decimal subTotal = 0; decimal vatAmount = 0;
            foreach (var order in orderList)
            {
                if (order.OrderDetails != null)
                {
                    var mainItems = order.OrderDetails.Where(d => !d.IsComponent).ToList();
                    foreach (var item in mainItems)
                    {
                        decimal itemSub = item.Quantity * item.UnitPrice;
                        decimal itemVat = itemSub * (decimal)(item.VatPercent / 100.0);
                        subTotal += itemSub; vatAmount += itemVat;
                    }
                }
            }
            return new { Sub = subTotal, Vat = vatAmount, Total = subTotal + vatAmount };
        }

        private string GetStatusName(OrderStatus status)
        {
            switch (status)
            {
                case OrderStatus.PendingApproval: return "Chờ duyệt";
                case OrderStatus.Approved: return "Đã duyệt";
                case OrderStatus.InProduction: return "Đang Sản Xuất";
                case OrderStatus.Packing: return "Đóng gói";
                case OrderStatus.Delivered: return "Đang Giao Hàng";
                case OrderStatus.Invoiced: return "Đã xuất HĐ";
                case OrderStatus.Completed: return "Hoàn Thành";
                case OrderStatus.Canceled: return "Đã Hủy";
                default: return status.ToString();
            }
        }
    }
}