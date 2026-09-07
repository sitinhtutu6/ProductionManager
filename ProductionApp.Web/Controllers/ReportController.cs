using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductionApp.Web.Constants;
using ProductionApp.Web.Models.ViewModels;
using ProductionManager.Data;
using System.Text.Json;
using ClosedXML.Excel; 

namespace ProductionApp.Web.Controllers
{
    [Authorize(Roles = "Admin, Manager, Accountant")]
    public class ReportController : Controller
    {
        private readonly AppDbContext _context;

        public ReportController(AppDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. INDEX: BÁO CÁO HIỆU QUẢ SXKD
        // ==========================================
        [Authorize(Policy = AppPermissions.Debts.View)]
        public async Task<IActionResult> Index(DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                // 1. THIẾT LẬP KỲ BÁO CÁO
                var f = fromDate ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var t = toDate ?? DateTime.Now;
                var tEnd = t.Date.AddDays(1).AddTicks(-1);

                var model = new BusinessDashboardVM
                {
                    FromDate = f,
                    ToDate = t,
                    Production = new ProductionReportVM(),
                    CashFlow = new CashFlowReportVM()
                };

                // ====================================================
                // A. SẢN XUẤT & DOANH THU (Dựa trên Đơn hàng - Orders)
                // ====================================================
                // Lưu ý: Doanh thu ở đây CHỈ tính từ đơn hàng, KHÔNG bao gồm tiền đi vay (Loan)

                var allOrders = await _context.Orders
                    .Select(o => new { o.Status, o.TotalAmount, o.OrderDate })
                    .ToListAsync();

                var periodOrders = allOrders
                    .Where(o => o.OrderDate >= f && o.OrderDate <= tEnd)
                    .ToList();

                var completedOrders = periodOrders
                    .Where(o => o.Status == OrderStatus.Completed || o.Status == OrderStatus.Delivered)
                    .ToList();

                model.Production.TotalOrdersReceived = periodOrders.Count;
                model.Production.PotentialRevenue = periodOrders.Sum(x => x.TotalAmount);
                model.Production.TotalOrdersCompleted = completedOrders.Count;
                model.Production.Revenue = completedOrders.Sum(x => x.TotalAmount);

                // ====================================================
                // B. VẬT TƯ & GIÁ VỐN (Dựa trên Kho - StockTransactions)
                // ====================================================

                var allStock = await _context.StockTransactions
                    .Include(x => x.WarehouseItem)
                    .Select(x => new
                    {
                        x.Type,
                        x.Quantity,
                        x.Price,
                        x.TransactionDate,
                        x.WarehouseItemId,
                        ItemName = x.WarehouseItem != null ? x.WarehouseItem.Name : "N/A",
                        ItemUnit = x.WarehouseItem != null ? x.WarehouseItem.Unit : ""
                    })
                    .ToListAsync();

                var periodExports = allStock
                    .Where(x => x.Type == "Export" && x.TransactionDate >= f && x.TransactionDate <= tEnd)
                    .ToList();

                var allImports = allStock.Where(x => x.Type == "Import").ToList();

                foreach (var group in periodExports.GroupBy(x => x.WarehouseItemId))
                {
                    var itemImports = allImports.Where(x => x.WarehouseItemId == group.Key).ToList();

                    // Tính giá bình quân gia quyền (Weighted Average Cost)
                    decimal totalImpVal = itemImports.Sum(x => x.Price * (decimal)x.Quantity);
                    double totalImpQty = itemImports.Sum(x => x.Quantity);
                    decimal avgPrice = totalImpQty > 0 ? totalImpVal / (decimal)totalImpQty : 0;

                    model.Production.MaterialStats.Add(new MaterialUsageStats
                    {
                        MaterialName = group.First().ItemName,
                        Unit = group.First().ItemUnit,
                        ImportQty = totalImpQty,
                        ExportQty = group.Sum(x => x.Quantity),
                        AvgImportPrice = avgPrice
                    });
                }
                model.Production.TotalMaterialCost = model.Production.MaterialStats.Sum(x => x.TotalCost);

                // ====================================================
                // C. DÒNG TIỀN (Dựa trên Sổ quỹ - CashEntries)
                // ====================================================

                // 🔥 QUAN TRỌNG: Phải lấy thêm cột Category để đề phòng cần lọc sau này
                var allCash = await _context.CashEntries
                    .Select(x => new { x.Type, x.Amount, x.ReportDate, x.Category })
                    .ToListAsync();

                var currentCash = allCash
                    .Where(x => x.ReportDate >= f && x.ReportDate <= tEnd)
                    .ToList();

                // Tính tổng dòng tiền thực tế (Bao gồm cả Tiền hàng + Tiền vay mượn)
                // Vì đây là Sổ quỹ nên cần phản ánh đúng số tiền thực có trong két
                model.CashFlow.TotalReceipts = currentCash
                    .Where(x => x.Type == TransactionType.Receipt)
                    .Sum(x => x.Amount);

                model.CashFlow.TotalPayments = currentCash
                    .Where(x => x.Type == TransactionType.Payment)
                    .Sum(x => x.Amount);

                // ====================================================
                // D. DỮ LIỆU BIỂU ĐỒ
                // ====================================================
                model.ChartRevenueData = JsonSerializer.Serialize(new[] { model.Production.Revenue });
                model.ChartCostData = JsonSerializer.Serialize(new[] { model.Production.TotalMaterialCost });
                model.ChartProfitData = JsonSerializer.Serialize(new[] { model.Production.GrossProfit });

                return View(model);
            }
            catch (Exception ex)
            {
                return Content($"LỖI INDEX: {ex.Message}");
            }
        }

        // ==========================================
        // 2. CASHFLOW: DÒNG TIỀN THỰC TẾ
        // ==========================================
        [Authorize(Policy = AppPermissions.Debts.View)]
        public async Task<IActionResult> CashFlow(int? year)
        {
            try
            {
                int y = year ?? DateTime.Now.Year;
                var model = new CashFlowYearlyVM { Year = y };
                var startOfYear = new DateTime(y, 1, 1);

                // Load dữ liệu thô về RAM
                var allCash = await _context.CashEntries.Select(x => new { x.Type, x.Amount, x.ReportDate }).ToListAsync();

                // Tồn đầu năm
                var prevTrans = allCash.Where(x => x.ReportDate < startOfYear).ToList();
                decimal runningBalance = prevTrans.Where(x => x.Type == TransactionType.Receipt).Sum(x => x.Amount)
                                       - prevTrans.Where(x => x.Type == TransactionType.Payment).Sum(x => x.Amount);
                model.YearOpeningBalance = runningBalance;

                // Chi tiết 12 tháng (Tính trong RAM)
                var yearTrans = allCash.Where(x => x.ReportDate.Year == y).ToList();
                for (int m = 1; m <= 12; m++)
                {
                    var monthData = yearTrans.Where(x => x.ReportDate.Month == m).ToList();
                    var stat = new MonthlyCashStats
                    {
                        Month = m,
                        Opening = runningBalance,
                        In = monthData.Where(x => x.Type == TransactionType.Receipt).Sum(x => x.Amount),
                        Out = monthData.Where(x => x.Type == TransactionType.Payment).Sum(x => x.Amount)
                    };
                    model.MonthlyStats.Add(stat);
                    runningBalance = stat.Closing;
                }
                model.YearClosingBalance = runningBalance;

                model.ChartLabels = JsonSerializer.Serialize(model.MonthlyStats.Select(x => "Thg " + x.Month).ToArray());
                model.ChartBalanceData = JsonSerializer.Serialize(model.MonthlyStats.Select(x => x.Closing).ToArray());

                return View(model);
            }
            catch (Exception ex) { return Content($"LỖI CASHFLOW: {ex.Message}"); }
        }

        // ==========================================
        // 3. ALLOCATED: PHÂN BỔ LỢI NHUẬN (ĐÃ FIX LỖI SQLITE SUM) 🔥
        // ==========================================

        [HttpGet]
        [Authorize(Policy = AppPermissions.Debts.Create)]
        public async Task<IActionResult> AllocatedAnalysis(int? month, int? year)
        {
            try
            {
                int m = month ?? DateTime.Now.Month;
                int y = year ?? DateTime.Now.Year;
                var model = new AllocatedReportVM { Month = m, Year = y };

                // 1. LẤY DỮ LIỆU ĐÃ PHÂN BỔ (Load về RAM)
                var allocations = await _context.CashAllocations
                    .Include(x => x.CashEntry)
                    .Where(x => x.TargetMonth == m && x.TargetYear == y)
                    .ToListAsync();

                model.TotalRevenueAllocated = allocations.Where(x => x.CashEntry.Type == TransactionType.Receipt).Sum(x => x.AllocatedAmount);
                model.TotalCostAllocated = allocations.Where(x => x.CashEntry.Type == TransactionType.Payment).Sum(x => x.AllocatedAmount);

                // Map sang danh sách chi tiết
                model.AllocatedList = allocations.Select(x => new AllocatedDetail
                {
                    AllocationId = x.Id,
                    VoucherCode = x.CashEntry.VoucherCode,
                    TransactionDate = x.CashEntry.TransactionDate,
                    Description = x.CashEntry.Description,
                    Type = x.CashEntry.Type == TransactionType.Receipt ? "Thu" : "Chi",
                    Amount = x.AllocatedAmount,
                    Note = x.Note
                }).OrderBy(x => x.TransactionDate).ToList();

                // 2. DANH SÁCH CHỜ PHÂN BỔ (KHO)
                var lastDayOfMonth = new DateTime(y, m, 1).AddMonths(1).AddDays(-1);

                // 🔥 QUAN TRỌNG: CHỈ LẤY KHOẢN MỤC KINH DOANH (Business)
                // Loại bỏ hoàn toàn khoản Vay (Loan) khỏi danh sách này
                var allEntriesRaw = await _context.CashEntries
                    .Where(x => x.TransactionDate <= lastDayOfMonth && x.Category == EntryCategory.Business)
                    .Select(x => new { x.Id, x.TransactionDate, x.VoucherCode, x.Description, x.Type, x.Amount })
                    .ToListAsync();

                var allAllocationsRaw = await _context.CashAllocations
                    .Select(x => new { x.CashEntryId, x.AllocatedAmount })
                    .ToListAsync();

                model.UnallocatedItems = new List<UnallocatedItem>();
                foreach (var entry in allEntriesRaw)
                {
                    decimal allocatedSoFar = allAllocationsRaw.Where(a => a.CashEntryId == entry.Id).Sum(a => a.AllocatedAmount);
                    // Chỉ lấy những phiếu còn dư tiền (chưa phân bổ hết)
                    if (entry.Amount - allocatedSoFar > 0)
                    {
                        model.UnallocatedItems.Add(new UnallocatedItem
                        {
                            EntryId = entry.Id,
                            TransactionDate = entry.TransactionDate,
                            VoucherCode = entry.VoucherCode,
                            Description = entry.Description,
                            Type = entry.Type == TransactionType.Receipt ? "Thu" : "Chi",
                            TotalAmount = entry.Amount,
                            AllocatedSoFar = allocatedSoFar
                        });
                    }
                }
                model.UnallocatedItems = model.UnallocatedItems.OrderByDescending(x => x.TransactionDate).ToList();

                return View(model);
            }
            catch (Exception ex)
            {
                return Content($"LỖI PHÂN BỔ: {ex.Message} \n {ex.InnerException?.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveAllocation(int entryId, int month, int year, decimal amount)
        {
            try
            {
                var entry = await _context.CashEntries.FindAsync(entryId);
                if (entry == null) return Json(new { success = false, msg = "Không tìm thấy phiếu!" });

                // Fix lỗi SumAsync trong SaveAllocation luôn
                // Load danh sách phân bổ của phiếu này về RAM trước
                var allocationList = await _context.CashAllocations
                    .Where(x => x.CashEntryId == entryId)
                    .ToListAsync();

                var allocated = allocationList.Sum(x => x.AllocatedAmount); // Sum trong RAM

                decimal remaining = entry.Amount - allocated;

                if (amount > remaining) return Json(new { success = false, msg = $"Vượt quá số dư ({remaining:N0})!" });

                var newAlloc = new CashAllocation
                {
                    CashEntryId = entryId,
                    TargetMonth = month,
                    TargetYear = year,
                    AllocatedAmount = amount,
                    Note = "Phân bổ thủ công"
                };

                _context.Add(newAlloc);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex) { return Json(new { success = false, msg = ex.Message }); }
        }

        // ==========================================
        // 4. XUẤT EXCEL (4 LOẠI BÁO CÁO) 🔥
        // ==========================================
        public async Task<IActionResult> ExportExcel(int? month, int? year)
        {
            try
            {
                int m = month ?? DateTime.Now.Month;
                int y = year ?? DateTime.Now.Year;

                // Xác định ngày đầu tháng và cuối tháng
                var fromDate = new DateTime(y, m, 1);
                var toDate = fromDate.AddMonths(1).AddDays(-1).AddHours(23).AddMinutes(59).AddSeconds(59);

                using (var wb = new XLWorkbook())
                {
                    // --- SHEET 1: HIỆU QUẢ SXKD (Production) ---
                    var ws1 = wb.Worksheets.Add("HieuQua_SXKD");
                    await BuildProductionSheet(ws1, fromDate, toDate);

                    // --- SHEET 2: DÒNG TIỀN TỔNG HỢP (CashFlow Summary) ---
                    var ws2 = wb.Worksheets.Add("DongTien_TongHop");
                    await BuildCashFlowSummarySheet(ws2, y); // Báo cáo năm

                    // --- SHEET 3: CHI TIẾT THU CHI (CashFlow Details) ---
                    var ws3 = wb.Worksheets.Add("ChiTiet_ThuChi");
                    await BuildCashFlowDetailSheet(ws3, fromDate, toDate);

                    // --- SHEET 4: PHÂN BỔ LỢI NHUẬN (Allocation) ---
                    var ws4 = wb.Worksheets.Add("PhanBo_LoiNhuan");
                    await BuildAllocationSheet(ws4, m, y);

                    using (var stream = new MemoryStream())
                    {
                        wb.SaveAs(stream);
                        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"BaoCao_TongHop_T{m}_{y}.xlsx");
                    }
                }
            }
            catch (Exception ex)
            {
                return Content($"LỖI EXPORT: {ex.Message}");
            }
        }

        // --- CÁC HÀM CON BUILD SHEET ---

        private async Task BuildProductionSheet(IXLWorksheet ws, DateTime f, DateTime t)
        {
            ws.Cell("A1").Value = $"BÁO CÁO HIỆU QUẢ KINH DOANH ({f:dd/MM} - {t:dd/MM})";
            ws.Range("A1:E1").Merge().Style.Font.Bold = true;
            ws.Range("A1:E1").Style.Font.FontSize = 14;

            // Lấy dữ liệu
            var orders = await _context.Orders.Where(o => o.OrderDate >= f && o.OrderDate <= t && (o.Status == OrderStatus.Completed || o.Status == OrderStatus.Delivered)).ToListAsync();
            var revenue = orders.Sum(x => x.TotalAmount);

            // Tính giá vốn (Logic đơn giản hóa cho Excel)
            var exports = await _context.StockTransactions.Where(x => x.Type == "Export" && x.TransactionDate >= f && x.TransactionDate <= t).ToListAsync();
            var cost = exports.Sum(x => x.Price * (decimal)x.Quantity); // Lưu ý: Cần tính giá TB chính xác như Controller Index nếu muốn chuẩn 100%

            ws.Cell("A3").Value = "CHỈ TIÊU";
            ws.Cell("B3").Value = "GIÁ TRỊ (VNĐ)";
            ws.Range("A3:B3").Style.Font.Bold = true;
            ws.Range("A3:B3").Style.Fill.BackgroundColor = XLColor.LightGray;

            ws.Cell("A4").Value = "1. Doanh thu thực tế";
            ws.Cell("B4").Value = revenue;
            ws.Cell("A5").Value = "2. Giá vốn hàng bán";
            ws.Cell("B5").Value = cost;
            ws.Cell("A6").Value = "3. Lợi nhuận gộp";
            ws.Cell("B6").FormulaA1 = "B4-B5";
            ws.Cell("A7").Value = "4. Biên lợi nhuận (%)";
            ws.Cell("B7").FormulaA1 = "IF(B4>0, B6/B4, 0)";
            ws.Cell("B7").Style.NumberFormat.Format = "0.0%";

            ws.Column(1).Width = 30;
            ws.Column(2).Width = 20;
            ws.Column(2).Style.NumberFormat.Format = "#,##0";
        }

        private async Task BuildCashFlowSummarySheet(IXLWorksheet ws, int year)
        {
            ws.Cell("A1").Value = $"BÁO CÁO DÒNG TIỀN NĂM {year}";
            ws.Range("A1:F1").Merge().Style.Font.Bold = true;
            ws.Range("A1:F1").Style.Font.FontSize = 14;

            ws.Cell("A3").Value = "Tháng";
            ws.Cell("B3").Value = "Tồn đầu";
            ws.Cell("C3").Value = "Thu";
            ws.Cell("D3").Value = "Chi";
            ws.Cell("E3").Value = "Tồn cuối";
            ws.Cell("F3").Value = "Biến động";
            ws.Range("A3:F3").Style.Font.Bold = true;
            ws.Range("A3:F3").Style.Fill.BackgroundColor = XLColor.LightBlue;

            // Logic tính toán y hệt Action CashFlow
            var startOfYear = new DateTime(year, 1, 1);
            var allCash = await _context.CashEntries.Select(x => new { x.Type, x.Amount, x.ReportDate }).ToListAsync();

            var prevTrans = allCash.Where(x => x.ReportDate < startOfYear).ToList();
            decimal running = prevTrans.Where(x => x.Type == TransactionType.Receipt).Sum(x => x.Amount)
                            - prevTrans.Where(x => x.Type == TransactionType.Payment).Sum(x => x.Amount);

            var yearTrans = allCash.Where(x => x.ReportDate.Year == year).ToList();
            int row = 4;

            for (int m = 1; m <= 12; m++)
            {
                var monthData = yearTrans.Where(x => x.ReportDate.Month == m).ToList();
                decimal income = monthData.Where(x => x.Type == TransactionType.Receipt).Sum(x => x.Amount);
                decimal expense = monthData.Where(x => x.Type == TransactionType.Payment).Sum(x => x.Amount);
                decimal closing = running + income - expense;

                ws.Cell(row, 1).Value = $"Tháng {m}";
                ws.Cell(row, 2).Value = running;
                ws.Cell(row, 3).Value = income;
                ws.Cell(row, 4).Value = expense;
                ws.Cell(row, 5).Value = closing;
                ws.Cell(row, 6).Value = income - expense;

                running = closing;
                row++;
            }

            ws.Columns().AdjustToContents();
            ws.Columns("B:F").Style.NumberFormat.Format = "#,##0";
        }

        private async Task BuildCashFlowDetailSheet(IXLWorksheet ws, DateTime f, DateTime t)
        {
            ws.Cell("A1").Value = $"CHI TIẾT THU CHI THỰC TẾ ({f:MM/yyyy})";
            ws.Range("A1:G1").Merge().Style.Font.Bold = true;

            ws.Cell("A3").Value = "Ngày Report";
            ws.Cell("B3").Value = "Ngày GD";
            ws.Cell("C3").Value = "Số phiếu";
            ws.Cell("D3").Value = "Diễn giải";
            ws.Cell("E3").Value = "Loại";
            ws.Cell("F3").Value = "Số tiền";
            ws.Cell("G3").Value = "Đối tượng";
            ws.Range("A3:G3").Style.Font.Bold = true;
            ws.Range("A3:G3").Style.Fill.BackgroundColor = XLColor.LightGreen;

            var data = await _context.CashEntries
            .Where(x => x.ReportDate >= f && x.ReportDate <= t && x.Category == EntryCategory.Business)
            .OrderBy(x => x.ReportDate)
            .ToListAsync();

            int row = 4;
            foreach (var item in data)
            {
                ws.Cell(row, 1).Value = item.ReportDate;
                ws.Cell(row, 2).Value = item.TransactionDate;
                ws.Cell(row, 3).Value = item.VoucherCode;
                ws.Cell(row, 4).Value = item.Description;
                ws.Cell(row, 5).Value = item.Type == TransactionType.Receipt ? "Thu" : "Chi";

                var amount = item.Amount;
                if (item.Type == TransactionType.Payment) amount = -amount; // Chi thì số âm

                ws.Cell(row, 6).Value = amount;
                ws.Cell(row, 6).Style.Font.FontColor = amount >= 0 ? XLColor.Green : XLColor.Red;

                ws.Cell(row, 7).Value = item.TargetName;
                row++;
            }

            ws.Columns().AdjustToContents();
            ws.Column(6).Style.NumberFormat.Format = "#,##0";
        }

        private async Task BuildAllocationSheet(IXLWorksheet ws, int m, int y)
        {
            ws.Cell("A1").Value = $"BÁO CÁO PHÂN BỔ LỢI NHUẬN (Tháng {m}/{y})";
            ws.Range("A1:E1").Merge().Style.Font.Bold = true;

            // 1. Tổng hợp
            var allocations = await _context.CashAllocations
                .Include(x => x.CashEntry)
                .Where(x => x.TargetMonth == m && x.TargetYear == y)
                .ToListAsync();

            decimal rev = allocations.Where(x => x.CashEntry.Type == TransactionType.Receipt).Sum(x => x.AllocatedAmount);
            decimal cost = allocations.Where(x => x.CashEntry.Type == TransactionType.Payment).Sum(x => x.AllocatedAmount);

            ws.Cell("A3").Value = "Doanh thu ghi nhận:";
            ws.Cell("B3").Value = rev;
            ws.Cell("C3").Value = "Chi phí ghi nhận:";
            ws.Cell("D3").Value = cost;
            ws.Cell("E3").Value = "Lợi nhuận:";
            ws.Cell("F3").Value = rev - cost;
            ws.Range("A3:F3").Style.Font.Bold = true;

            // 2. Chi tiết
            ws.Cell("A5").Value = "Ngày GD";
            ws.Cell("B5").Value = "Số phiếu";
            ws.Cell("C5").Value = "Nội dung gốc";
            ws.Cell("D5").Value = "Loại";
            ws.Cell("E5").Value = "Số tiền phân bổ";
            ws.Range("A5:E5").Style.Font.Bold = true;
            ws.Range("A5:E5").Style.Fill.BackgroundColor = XLColor.Plum;

            int row = 6;
            foreach (var item in allocations)
            {
                ws.Cell(row, 1).Value = item.CashEntry.TransactionDate;
                ws.Cell(row, 2).Value = item.CashEntry.VoucherCode;
                ws.Cell(row, 3).Value = item.CashEntry.Description;
                ws.Cell(row, 4).Value = item.CashEntry.Type == TransactionType.Receipt ? "Thu" : "Chi";
                ws.Cell(row, 5).Value = item.AllocatedAmount;
                row++;
            }

            ws.Columns().AdjustToContents();
            ws.Columns("B:F").Style.NumberFormat.Format = "#,##0";
        }

    }
}