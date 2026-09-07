//using ClosedXML.Excel;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using ProductionApp.Web.Constants;
//using ProductionApp.Web.Models.ViewModels;
//using ProductionManager.Data;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Threading.Tasks;

//namespace ProductionApp.Web.Controllers
//{
//    [Authorize]
//    public class ReportControllerMonth : Controller
//    {
//        private readonly AppDbContext _context;

//        public ReportControllerMonth(AppDbContext context)
//        {
//            _context = context;
//        }

//        // ==============================================================
//        // 1. TÍNH TOÁN DỮ LIỆU P&L TỔNG HỢP (TÍNH NĂNG CŨ)
//        // ==============================================================

//        private async Task<MonthlyPnLViewModel> GetPnLDataAsync(int m, int y)
//        {
//            var model = new MonthlyPnLViewModel { Month = m, Year = y };

//            var totalOrderAmount = await _context.Orders
//                .Where(o => o.OrderDate.Month == m && o.OrderDate.Year == y && o.Status != OrderStatus.Canceled)
//                .SumAsync(o => o.TotalAmount);

//            model.NetRevenue = totalOrderAmount / 1.1m;
//            model.VatPayable = totalOrderAmount - model.NetRevenue;
//            model.TotalRevenue = model.NetRevenue;

//            model.OtherIncome = await _context.CashEntries
//                .Where(c => c.Type == TransactionType.Receipt && c.OrderId == null && c.Category != EntryCategory.Loan && c.TransactionDate.Month == m && c.TransactionDate.Year == y)
//                .SumAsync(c => c.Amount);

//            var productionExports = await _context.StockTransactions
//                .Where(x => x.TransactionDate.Month == m && x.TransactionDate.Year == y && x.Type == "Export" && x.Reason == ExportReason.ProductionOrSale)
//                .ToListAsync();

//            model.TotalCOGS = productionExports.Sum(x => (decimal)x.Quantity * x.Price);

//            var allocations = await _context.CashAllocations
//                .Include(a => a.CashEntry)
//                    .ThenInclude(c => c.CostCategory)
//                .Where(a => a.TargetMonth == m && a.TargetYear == y && a.CashEntry.Type == TransactionType.Payment)
//                .ToListAsync();

//            model.OpexGroups = allocations
//                .Where(a => a.CashEntry.CostCategory != null)
//                .GroupBy(a => a.CashEntry.CostCategory.Name)
//                .ToDictionary(g => g.Key, g => g.Sum(a => a.AllocatedAmount));

//            model.TotalOperatingExpense = model.OpexGroups.Values.Sum();

//            var damageExports = await _context.StockTransactions
//                .Where(x => x.TransactionDate.Month == m && x.TransactionDate.Year == y && x.Type == "Export" && x.Reason == ExportReason.DamageOrLoss)
//                .ToListAsync();

//            if (damageExports.Any())
//            {
//                decimal damageTotal = damageExports.Sum(x => (decimal)x.Quantity * x.Price);
//                model.TotalOperatingExpense += damageTotal;

//                if (model.OpexGroups.ContainsKey("Hao hụt / Hủy vật tư"))
//                    model.OpexGroups["Hao hụt / Hủy vật tư"] += damageTotal;
//                else
//                    model.OpexGroups.Add("Hao hụt / Hủy vật tư", damageTotal);
//            }

//            model.GrossProfit = model.TotalRevenue + model.OtherIncome - model.TotalCOGS;
//            model.ProfitBeforeTax = model.GrossProfit - model.TotalOperatingExpense;
//            model.CorporateTax = model.ProfitBeforeTax > 0 ? model.ProfitBeforeTax * 0.20m : 0;
//            model.NetProfit = model.ProfitBeforeTax - model.CorporateTax;

//            return model;
//        }

//        // ==============================================================
//        // 2. GIAO DIỆN WEB: BÁO CÁO P&L 
//        // ==============================================================
//        [Authorize(Policy = AppPermissions.Debts.View)]
//        public async Task<IActionResult> FinancialStatement(int? month, int? year)
//        {
//            int m = month ?? (DateTime.Now.Day <= 10 ? DateTime.Now.AddMonths(-1).Month : DateTime.Now.Month);
//            int y = year ?? (DateTime.Now.Day <= 10 ? DateTime.Now.AddMonths(-1).Year : DateTime.Now.Year);

//            var companyConfig = await _context.CompanyConfigs.FirstOrDefaultAsync() ?? new CompanyConfig();
//            ViewBag.CompanyInfo = companyConfig;

//            var model = await GetPnLDataAsync(m, y);
//            return View(model);
//        }

//        // ==============================================================
//        // 3. XUẤT EXCEL: BÁO CÁO P&L TỔNG HỢP (ĐÃ ĐƯỢC BẢO TỒN)
//        // ==============================================================
//        [Authorize(Policy = AppPermissions.Debts.Edit)]
//        public async Task<IActionResult> ExportExcel(int month, int year)
//        {
//            var model = await GetPnLDataAsync(month, year);
//            var companyConfig = await _context.CompanyConfigs.FirstOrDefaultAsync() ?? new CompanyConfig();

//            using (var wb = new XLWorkbook())
//            {
//                var ws = wb.Worksheets.Add($"PnL_T{month}_{year}");
//                ws.Cell("A1").Value = companyConfig.CompanyName.ToUpper();
//                ws.Cell("A1").Style.Font.SetBold().Font.FontSize = 14;
//                ws.Cell("A2").Value = "Địa chỉ: " + companyConfig.Address;
//                ws.Cell("A3").Value = "MST: " + companyConfig.TaxCode + " | SĐT: " + companyConfig.Phone;

//                var titleRange = ws.Range("A5:C5").Merge();
//                titleRange.Value = "BÁO CÁO KẾT QUẢ HOẠT ĐỘNG KINH DOANH (P&L)";
//                titleRange.Style.Font.SetBold().Font.FontSize = 16;
//                titleRange.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

//                var subTitleRange = ws.Range("A6:C6").Merge();
//                subTitleRange.Value = $"Kỳ báo cáo: Tháng {month} năm {year}";
//                subTitleRange.Style.Font.SetItalic();
//                subTitleRange.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

//                int r = 8;
//                ws.Cell(r, 1).Value = "CHỈ TIÊU"; ws.Cell(r, 2).Value = "SỐ TIỀN (VNĐ)"; ws.Cell(r, 3).Value = "GHI CHÚ";
//                ws.Range($"A{r}:C{r}").Style.Font.SetBold().Fill.SetBackgroundColor(XLColor.LightGray); r++;

//                ws.Cell(r, 1).Value = "I. TỔNG DOANH THU & THU NHẬP"; ws.Cell(r, 2).Value = model.TotalRevenue + model.OtherIncome; ws.Range($"A{r}:B{r}").Style.Font.SetBold(); r++;
//                ws.Cell(r, 1).Value = "  1. Doanh thu thuần (Chưa VAT)"; ws.Cell(r, 2).Value = model.TotalRevenue; r++;
//                ws.Cell(r, 1).Value = "  2. Thu nhập khác"; ws.Cell(r, 2).Value = model.OtherIncome; r++;

//                ws.Cell(r, 1).Value = "II. GIÁ VỐN HÀNG BÁN (COGS)"; ws.Cell(r, 2).Value = -model.TotalCOGS; ws.Range($"A{r}:B{r}").Style.Font.SetBold(); r++;
//                ws.Cell(r, 1).Value = "LỢI NHUẬN GỘP (GROSS PROFIT)"; ws.Cell(r, 2).Value = model.GrossProfit; ws.Range($"A{r}:C{r}").Style.Font.SetBold().Fill.SetBackgroundColor(XLColor.Yellow); r++;

//                ws.Cell(r, 1).Value = "III. CHI PHÍ VẬN HÀNH (OPEX)"; ws.Cell(r, 2).Value = -model.TotalOperatingExpense; ws.Range($"A{r}:B{r}").Style.Font.SetBold(); r++;

//                int groupIndex = 1;
//                foreach (var group in model.OpexGroups)
//                {
//                    ws.Cell(r, 1).Value = $"  {groupIndex}. {group.Key}";
//                    ws.Cell(r, 2).Value = -group.Value;
//                    r++;
//                    groupIndex++;
//                }

//                ws.Cell(r, 1).Value = "IV. LỢI NHUẬN TRƯỚC THUẾ (EBT)"; ws.Cell(r, 2).Value = model.ProfitBeforeTax; ws.Range($"A{r}:C{r}").Style.Font.SetBold(); r++;
//                ws.Cell(r, 1).Value = "  - Thuế TNDN (20%)"; ws.Cell(r, 2).Value = -model.CorporateTax; r++;

//                ws.Cell(r, 1).Value = "V. LỢI NHUẬN RÒNG SAU THUẾ"; ws.Cell(r, 2).Value = model.NetProfit; ws.Range($"A{r}:C{r}").Style.Font.SetBold().Fill.SetBackgroundColor(XLColor.LightGreen);

//                ws.Column(2).Style.NumberFormat.Format = "#,##0";
//                ws.Column(1).Width = 45; ws.Column(2).Width = 20; ws.Column(3).Width = 35;

//                r += 3;
//                ws.Cell(r, 1).Value = "Người lập biểu"; ws.Cell(r, 3).Value = "Giám đốc";
//                ws.Range($"A{r}:C{r}").Style.Font.SetBold().Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

//                using (var stream = new MemoryStream())
//                {
//                    wb.SaveAs(stream);
//                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"BaoCao_PnL_Thang{month}_{year}.xlsx");
//                }
//            }
//        }

//        // ==============================================================
//        // 4. TÍNH TOÁN DỮ LIỆU BÁO CÁO THỰC CHỨNG 
//        // ==============================================================
//        private async Task<MonthlyRealizedReportVM> GetRealizedDataAsync(int m, int y)
//        {
//            var model = new MonthlyRealizedReportVM { Month = m, Year = y };

//            // 1. DOANH THU ĐÃ GIAO
//            var deliveredOrders = await _context.Orders
//                .Where(o => o.OrderDate.Month == m && o.OrderDate.Year == y && o.Status == OrderStatus.Completed)
//                .ToListAsync();

//            decimal rawTotalDelivered = deliveredOrders.Sum(o => o.TotalAmount);
//            model.TotalDeliveredRevenue = rawTotalDelivered / 1.1m; // Bóc VAT
//            var orderIds = deliveredOrders.Select(x => x.Id).ToList();

//            // THỰC XUẤT HÓA ĐƠN
//            var invoices = await _context.InvoiceDetails
//                .Where(i => orderIds.Contains(i.OrderId))
//                .ToListAsync();

//            decimal rawBilled = invoices.Sum(i => i.BilledAmount);
//            model.BilledRevenue = rawBilled / 1.1m;
//            model.UnbilledRevenue = model.TotalDeliveredRevenue - model.BilledRevenue;
//            if (model.UnbilledRevenue < 0) model.UnbilledRevenue = 0;

//            // 2. VẬT TƯ (TỔNG MUA VÀO TỪ CẢ PHIẾU CŨ VÀ ĐƠN PO MỚI)
//            var imports = await _context.MaterialImports
//                .Where(i => i.ImportDate.Month == m && i.ImportDate.Year == y)
//                .ToListAsync();

//            var pos = await _context.PurchaseOrders
//                .Where(p => p.OrderDate.Month == m && p.OrderDate.Year == y && p.Status != POStatus.Cancelled)
//                .ToListAsync();

//            // Tổng tiền mua = Phiếu cũ + PO Mới
//            model.TotalMaterialPurchased = imports.Sum(i => i.TotalAmount) + pos.Sum(p => p.FinalTotal);

//            var productionExports = await _context.StockTransactions
//                .Where(x => x.TransactionDate.Month == m && x.TransactionDate.Year == y && x.Type == "Export" && x.Reason == ExportReason.ProductionOrSale)
//                .ToListAsync();
//            model.MaterialConsumed = productionExports.Sum(x => (decimal)x.Quantity * x.Price);

//            // 3. CHI PHÍ VẬN HÀNH (Gom nhóm từ CashAllocation)
//            var allocations = await _context.CashAllocations
//                .Include(a => a.CashEntry).ThenInclude(c => c.CostCategory)
//                .Where(a => a.TargetMonth == m && a.TargetYear == y && a.CashEntry.Type == TransactionType.Payment)
//                .ToListAsync();

//            model.OpexGroups = allocations
//                .Where(a => a.CashEntry.CostCategory != null)
//                .GroupBy(a => a.CashEntry.CostCategory.Name)
//                .ToDictionary(g => g.Key, g => g.Sum(a => a.AllocatedAmount));
//            model.TotalOpex = model.OpexGroups.Values.Sum();

//            return model;
//        }

//        // ==============================================================
//        // 5. GIAO DIỆN WEB: BÁO CÁO THỰC CHỨNG 
//        // ==============================================================
//        [Authorize(Policy = AppPermissions.Debts.View)]
//        public async Task<IActionResult> RealizedReport(int? month, int? year)
//        {
//            int m = month ?? (DateTime.Now.Day <= 10 ? DateTime.Now.AddMonths(-1).Month : DateTime.Now.Month);
//            int y = year ?? (DateTime.Now.Day <= 10 ? DateTime.Now.AddMonths(-1).Year : DateTime.Now.Year);

//            var companyConfig = await _context.CompanyConfigs.FirstOrDefaultAsync() ?? new CompanyConfig();
//            ViewBag.CompanyInfo = companyConfig;

//            var model = await GetRealizedDataAsync(m, y);
//            return View(model);
//        }

//        // ==============================================================
//        // 6. XUẤT EXCEL: BÁO CÁO THỰC CHỨNG (TÍNH NĂNG MỚI)
//        // ==============================================================
//        [Authorize(Policy = AppPermissions.Debts.Edit)]
//        public async Task<IActionResult> ExportRealizedExcel(int month, int year)
//        {
//            var model = await GetRealizedDataAsync(month, year);
//            var companyConfig = await _context.CompanyConfigs.FirstOrDefaultAsync() ?? new CompanyConfig();

//            using (var wb = new XLWorkbook())
//            {
//                var ws = wb.Worksheets.Add($"ThucChung_T{month}_{year}");

//                // HEADER THÔNG TIN CÔNG TY
//                ws.Cell("A1").Value = companyConfig.CompanyName.ToUpper();
//                ws.Cell("A1").Style.Font.SetBold().Font.FontSize = 14;
//                ws.Cell("A2").Value = "Địa chỉ: " + companyConfig.Address;
//                ws.Cell("A3").Value = "MST: " + companyConfig.TaxCode + " | SĐT: " + companyConfig.Phone;

//                var titleRange = ws.Range("A5:C5").Merge();
//                titleRange.Value = "BÁO CÁO THỰC CHỨNG (DOANH THU & CHI PHÍ)";
//                titleRange.Style.Font.SetBold().Font.FontSize = 16;
//                titleRange.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

//                var subTitleRange = ws.Range("A6:C6").Merge();
//                subTitleRange.Value = $"Kỳ báo cáo: Tháng {month} năm {year}";
//                subTitleRange.Style.Font.SetItalic();
//                subTitleRange.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

//                int r = 8;
//                ws.Cell(r, 1).Value = "CHỈ TIÊU"; ws.Cell(r, 2).Value = "SỐ TIỀN (VNĐ)"; ws.Cell(r, 3).Value = "GHI CHÚ / TIẾN ĐỘ";
//                ws.Range($"A{r}:C{r}").Style.Font.SetBold().Fill.SetBackgroundColor(XLColor.LightGray); r++;

//                // 1. DOANH THU & HÓA ĐƠN
//                ws.Cell(r, 1).Value = "I. DOANH THU ĐÃ GIAO HÀNG (ĐỦ SL)"; ws.Cell(r, 2).Value = model.TotalDeliveredRevenue; ws.Range($"A{r}:C{r}").Style.Font.SetBold().Fill.SetBackgroundColor(XLColor.PowderBlue); r++;
//                ws.Cell(r, 1).Value = "  - Thực xuất hóa đơn (Billed)"; ws.Cell(r, 2).Value = model.BilledRevenue;
//                ws.Cell(r, 3).Value = model.TotalDeliveredRevenue > 0 ? (model.BilledRevenue / model.TotalDeliveredRevenue).ToString("P1") : "0%"; r++;
//                ws.Cell(r, 1).Value = "  - Chưa xuất Hóa đơn (Unbilled)"; ws.Cell(r, 2).Value = model.UnbilledRevenue;
//                ws.Cell(r, 3).Value = model.TotalDeliveredRevenue > 0 ? (model.UnbilledRevenue / model.TotalDeliveredRevenue).ToString("P1") : "0%";
//                ws.Cell(r, 1).Style.Font.SetFontColor(XLColor.Red); ws.Cell(r, 2).Style.Font.SetFontColor(XLColor.Red); r++;

//                r++; // Dòng trống

//                // 2. VẬT TƯ
//                ws.Cell(r, 1).Value = "II. CHI PHÍ VẬT TƯ"; ws.Range($"A{r}:C{r}").Style.Font.SetBold().Fill.SetBackgroundColor(XLColor.MistyRose); r++;
//                ws.Cell(r, 1).Value = "  - Tổng tiền nhập mua trong tháng"; ws.Cell(r, 2).Value = model.TotalMaterialPurchased; r++;
//                ws.Cell(r, 1).Value = "  - Thực dùng sản xuất (Đã xuất kho)"; ws.Cell(r, 2).Value = model.MaterialConsumed;
//                ws.Cell(r, 3).Value = model.TotalMaterialPurchased > 0 ? (model.MaterialConsumed / model.TotalMaterialPurchased).ToString("P1") : ""; r++;
//                ws.Cell(r, 1).Value = "  - Tồn kho chênh lệch (Đọng vốn)"; ws.Cell(r, 2).Value = model.MaterialRemaining;
//                ws.Cell(r, 3).Value = model.TotalMaterialPurchased > 0 ? (model.MaterialRemaining / model.TotalMaterialPurchased).ToString("P1") : ""; r++;

//                r++;

//                // 3. OPEX
//                ws.Cell(r, 1).Value = "III. CHI PHÍ VẬN HÀNH (THỰC CHI)"; ws.Cell(r, 2).Value = model.TotalOpex; ws.Range($"A{r}:B{r}").Style.Font.SetBold().Fill.SetBackgroundColor(XLColor.LemonChiffon); r++;
//                int groupIndex = 1;
//                foreach (var group in model.OpexGroups)
//                {
//                    ws.Cell(r, 1).Value = $"  {groupIndex}. {group.Key}";
//                    ws.Cell(r, 2).Value = group.Value;
//                    r++;
//                    groupIndex++;
//                }

//                // ĐỊNH DẠNG EXCEL
//                ws.Column(2).Style.NumberFormat.Format = "#,##0";
//                ws.Column(1).Width = 50; ws.Column(2).Width = 20; ws.Column(3).Width = 25;

//                using (var stream = new MemoryStream())
//                {
//                    wb.SaveAs(stream);
//                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"BaoCao_ThucChung_Thang{month}_{year}.xlsx");
//                }
//            }
//        }
//    }
//}

using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductionManager.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ProductionApp.Web.Controllers
{
    // --- 1. VIEWMODEL CHUẨN QUẢN TRỊ TÀI CHÍNH ---
    public class MonthlyPnLViewModel
    {
        public int Month { get; set; }
        public int Year { get; set; }

        // I. DOANH THU
        public decimal TotalGrossRevenue { get; set; } // Tổng doanh thu
        public decimal VatPayable { get; set; }        // Thuế VAT phải nộp
        public decimal NetRevenue => TotalGrossRevenue - VatPayable; // Doanh thu thuần

        // II. GIÁ VỐN HÀNG BÁN (COGS)
        public decimal TotalMaterialPurchased { get; set; } // Tổng tiền mua vật tư
        public decimal MaterialRemaining { get; set; }      // Tồn kho cuối kỳ
        public decimal MaterialConsumed => TotalMaterialPurchased - MaterialRemaining; // Vật tư thực tế tiêu hao

        // LỢI NHUẬN GỘP
        public decimal GrossProfit => NetRevenue - MaterialConsumed;
        public decimal GrossMargin => NetRevenue > 0 ? (GrossProfit / NetRevenue) * 100 : 0;

        // III. CHI PHÍ VẬN HÀNH (OPEX)
        public Dictionary<string, decimal> OpexGroups { get; set; } = new Dictionary<string, decimal>();
        public decimal TotalOpex => OpexGroups.Values.Sum();

        // IV. LỢI NHUẬN RÒNG
        public decimal NetProfit => GrossProfit - TotalOpex;
        public decimal NetMargin => NetRevenue > 0 ? (NetProfit / NetRevenue) * 100 : 0;
    }

    [Authorize(Roles = "Admin,Manager,Accountant")]
    public class ReportControllerMonth : Controller
    {
        private readonly AppDbContext _context;

        public ReportControllerMonth(AppDbContext context)
        {
            _context = context;
        }

        // --- 2. HÀM HIỂN THỊ GIAO DIỆN CHÍNH ---
        [HttpGet]
        public async Task<IActionResult> PnLReport(int? month, int? year)
        {
            int m = month ?? DateTime.Now.Month;
            int y = year ?? DateTime.Now.Year;

            var model = await GetPnLDataAsync(m, y);
            return View(model);
        }

        // --- 3. HÀM XỬ LÝ SỐ LIỆU CORE ---
        private async Task<MonthlyPnLViewModel> GetPnLDataAsync(int m, int y)
        {
            var model = new MonthlyPnLViewModel { Month = m, Year = y };

            // Logic ví dụ (Kế toán sẽ map với các bảng thực tế trong DB của sếp):
            // 1. Doanh thu
            var orders = await _context.Orders
                .Where(o => o.OrderDate.Month == m && o.OrderDate.Year == y && o.Status != OrderStatus.Canceled)
                .ToListAsync();
            
            model.TotalGrossRevenue = orders.Sum(o => o.TotalAmount);
            model.VatPayable = model.TotalGrossRevenue - (model.TotalGrossRevenue / 1.1m); // Giả sử VAT 10%

            // 2. Chi phí nguyên vật liệu (Giá vốn)
            model.TotalMaterialPurchased = await _context.StockTransactions
            .Where(t => t.Type == "Import" && t.TransactionDate.Month == m && t.TransactionDate.Year == y) // Thay chữ "Nhập" bằng từ khóa chính xác trong DB của Sếp
            .SumAsync(t => (decimal)t.Quantity * t.Price);

            // Giả lập tồn kho chênh lệch (Cần map logic kiểm kê kho thực tế)
            model.MaterialRemaining = model.TotalMaterialPurchased * 0.15m; 

            // 3. OPEX - Nhóm chi phí hoạt động
            var expenses = await _context.CashEntries
                .Include(c => c.CostCategory)
                .Where(c => c.Type == TransactionType.Payment && c.TransactionDate.Month == m && c.TransactionDate.Year == y)
                .ToListAsync();

            model.OpexGroups = expenses
                .Where(c => c.CostCategory != null)
                .GroupBy(c => c.CostCategory.Name)
                .ToDictionary(g => g.Key, g => g.Sum(c => c.Amount));

            // Gom các khoản không định danh vào "Chi phí khác"
            var uncategorized = expenses.Where(c => c.CostCategory == null).Sum(c => c.Amount);
            if (uncategorized > 0) model.OpexGroups.Add("Chi phí khác", uncategorized);

            return model;
        }

        // --- 4. HÀM XUẤT EXCEL CHUYÊN NGHIỆP ---
        [HttpGet]
        public async Task<IActionResult> ExportExcel(int month, int year)
        {
            var model = await GetPnLDataAsync(month, year);

            using (var wb = new XLWorkbook())
            {
                var ws = wb.Worksheets.Add($"P&L T{month}-{year}");

                // Tiêu đề
                ws.Range("A1:C1").Merge().Value = $"BÁO CÁO KẾT QUẢ KINH DOANH - THÁNG {month}/{year}";
                ws.Range("A1:C1").Style.Font.SetBold().Font.FontSize = 14;
                ws.Range("A1:C1").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                ws.Cell("A3").Value = "CHỈ TIÊU"; ws.Cell("B3").Value = "SỐ TIỀN (VNĐ)"; ws.Cell("C3").Value = "TỶ TRỌNG";
                ws.Range("A3:C3").Style.Font.SetBold().Fill.SetBackgroundColor(XLColor.LightGray);

                int r = 4;
                
                // I. DOANH THU
                ws.Cell(r, 1).Value = "I. TỔNG DOANH THU"; ws.Cell(r, 2).Value = model.TotalGrossRevenue; ws.Range($"A{r}:C{r}").Style.Font.SetBold(); r++;
                ws.Cell(r, 1).Value = "  - Thuế VAT"; ws.Cell(r, 2).Value = -model.VatPayable; r++;
                ws.Cell(r, 1).Value = "DOANH THU THUẦN"; ws.Cell(r, 2).Value = model.NetRevenue; ws.Range($"A{r}:C{r}").Style.Font.SetBold().Font.SetFontColor(XLColor.Green); r++;

                // II. COGS
                r++;
                ws.Cell(r, 1).Value = "II. GIÁ VỐN HÀNG BÁN (VẬT TƯ)"; ws.Cell(r, 2).Value = -model.MaterialConsumed; ws.Range($"A{r}:C{r}").Style.Font.SetBold(); r++;
                ws.Cell(r, 1).Value = "  - Mua mới trong kỳ"; ws.Cell(r, 2).Value = -model.TotalMaterialPurchased; r++;
                ws.Cell(r, 1).Value = "  - Tồn kho lưu lại"; ws.Cell(r, 2).Value = model.MaterialRemaining; r++;

                // LỢI NHUẬN GỘP
                r++;
                ws.Cell(r, 1).Value = "LỢI NHUẬN GỘP"; ws.Cell(r, 2).Value = model.GrossProfit; 
                ws.Cell(r, 3).Value = $"{model.GrossMargin:0.0}%"; 
                ws.Range($"A{r}:C{r}").Style.Font.SetBold().Fill.SetBackgroundColor(XLColor.LightCyan); r++;

                // III. OPEX
                r++;
                ws.Cell(r, 1).Value = "III. CHI PHÍ VẬN HÀNH (OPEX)"; ws.Cell(r, 2).Value = -model.TotalOpex; ws.Range($"A{r}:C{r}").Style.Font.SetBold(); r++;
                foreach (var group in model.OpexGroups)
                {
                    ws.Cell(r, 1).Value = $"  - {group.Key}";
                    ws.Cell(r, 2).Value = -group.Value;
                    r++;
                }

                // IV. LỢI NHUẬN RÒNG
                r++;
                ws.Cell(r, 1).Value = "LỢI NHUẬN RÒNG (NET PROFIT)"; ws.Cell(r, 2).Value = model.NetProfit;
                ws.Cell(r, 3).Value = $"{model.NetMargin:0.0}%";
                ws.Range($"A{r}:C{r}").Style.Font.SetBold().Font.SetFontColor(XLColor.White).Fill.SetBackgroundColor(XLColor.DarkBlue);

                // Format cột tiền
                ws.Column(2).Style.NumberFormat.Format = "#,##0";
                ws.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"PnL_T{month}_{year}.xlsx");
                }
            }
        }
    }
}