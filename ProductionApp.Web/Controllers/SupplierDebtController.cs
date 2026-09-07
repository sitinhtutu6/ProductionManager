using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductionApp.Web.Constants;
using ProductionManager.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ProductionApp.Web.Controllers
{
    [Authorize]
    public class SupplierDebtController : Controller
    {
        private readonly AppDbContext _context;

        public SupplierDebtController(AppDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // 1. DASHBOARD & DANH SÁCH (INDEX)
        // ============================================================
        [Authorize(Policy = AppPermissions.Debts.View)]
        public async Task<IActionResult> Index(string keyword, string status)
        {
            var query = _context.MaterialImports.AsQueryable();

            // Filter theo từ khóa
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(x => x.SupplierName.Contains(keyword) || x.ImportCode.Contains(keyword));
            }

            // Filter theo trạng thái
            if (!string.IsNullOrEmpty(status))
            {
                if (status == "Debt") query = query.Where(x => x.PaymentStatus != ImportPaymentStatus.Paid);
                else if (status == "Paid") query = query.Where(x => x.PaymentStatus == ImportPaymentStatus.Paid);
            }

            var rawData = await query.OrderByDescending(x => x.ImportDate).ToListAsync();

            // --- TÍNH TOÁN CHO DASHBOARD ---
            ViewBag.TotalDebt = rawData.Sum(x => x.RemainingAmount); // Tổng còn nợ
            ViewBag.TotalPaid = rawData.Sum(x => x.PaidAmount);      // Tổng đã trả
            ViewBag.TotalAll = rawData.Sum(x => x.TotalAmount);      // Tổng mua vào (để tính %)

            // Group theo NCC
            var groupedData = rawData
                .GroupBy(x => x.SupplierName)
                .Select(g => new SupplierDebtVM
                {
                    SupplierName = g.Key,
                    TotalDebt = g.Sum(x => x.RemainingAmount),
                    TotalPurchase = g.Sum(x => x.TotalAmount), // Tổng mua của ông này
                    ImportCount = g.Count(),
                    Imports = g.ToList()
                })
                .OrderByDescending(x => x.TotalDebt) // Ai nợ nhiều nhất lên đầu
                .ToList();

            ViewBag.Keyword = keyword;
            ViewBag.Status = status;

            return View(groupedData);
        }

        // ============================================================
        // 2. XUẤT EXCEL BÁO CÁO CÔNG NỢ (ĐÃ SỬA LỖI)
        // ============================================================
        [HttpGet]
        [Authorize(Policy = AppPermissions.Debts.Create)]
        public async Task<IActionResult> ExportExcel()
        {
            var data = await _context.MaterialImports
                .OrderBy(x => x.SupplierName)
                .ThenByDescending(x => x.ImportDate)
                .ToListAsync();

            using (var wb = new XLWorkbook())
            {
                var ws = wb.Worksheets.Add("BaoCaoCongNo");

                // --- SỬA LỖI TẠI ĐÂY: Tách lệnh style ra ---
                var headerRange = ws.Range(1, 1, 1, 7).Merge();
                headerRange.Value = "BÁO CÁO CÔNG NỢ NHÀ CUNG CẤP";
                headerRange.Style.Font.SetBold();
                headerRange.Style.Font.FontSize = 16;
                headerRange.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                // -------------------------------------------

                ws.Cell(2, 1).Value = $"Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm}";

                // Header Bảng
                int row = 4;
                string[] headers = { "STT", "Nhà Cung Cấp", "Mã Phiếu", "Ngày Nhập", "Tổng Tiền", "Đã Trả", "Còn Nợ" };
                for (int i = 0; i < headers.Length; i++)
                {
                    ws.Cell(row, i + 1).Value = headers[i];
                    ws.Cell(row, i + 1).Style.Fill.BackgroundColor = XLColor.CornflowerBlue;
                    ws.Cell(row, i + 1).Style.Font.FontColor = XLColor.White;
                    ws.Cell(row, i + 1).Style.Font.SetBold();
                    ws.Cell(row, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }

                row++;
                int stt = 1;
                foreach (var item in data)
                {
                    ws.Cell(row, 1).Value = stt++;
                    ws.Cell(row, 2).Value = item.SupplierName;
                    ws.Cell(row, 3).Value = item.ImportCode;
                    ws.Cell(row, 4).Value = item.ImportDate;
                    ws.Cell(row, 5).Value = item.TotalAmount;
                    ws.Cell(row, 6).Value = item.PaidAmount;
                    ws.Cell(row, 7).Value = item.RemainingAmount;

                    // Tô đỏ nếu còn nợ
                    if (item.RemainingAmount > 0)
                    {
                        ws.Cell(row, 7).Style.Font.FontColor = XLColor.Red;
                        ws.Cell(row, 7).Style.Font.SetBold();
                    }

                    row++;
                }

                // Format số & ngày
                ws.Column(4).Style.DateFormat.Format = "dd/MM/yyyy";
                ws.Columns(5, 7).Style.NumberFormat.Format = "#,##0";
                ws.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"CongNo_{DateTime.Now:ddMMyy}.xlsx");
                }
            }
        }

        // ============================================================
        // 3. XỬ LÝ TRẢ NỢ (PAY DEBT)
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = AppPermissions.Debts.Create)]
        public async Task<IActionResult> PayDebt(int importId, decimal amount, DateTime date, string note)
        {
            var import = await _context.MaterialImports.FindAsync(importId);
            if (import == null) return NotFound();

            if (amount > import.RemainingAmount) amount = import.RemainingAmount;

            // A. Tạo phiếu chi
            var cashEntry = new CashEntry
            {
                TransactionDate = date,
                ReportDate = date,
                Type = TransactionType.Payment,
                Category = EntryCategory.Business,
                Amount = amount,
                TargetName = import.SupplierName,
                Description = $"Thanh toán công nợ phiếu {import.ImportCode} - {note}",
                MaterialImportId = importId,
                VoucherCode = $"PC-DEBT-{DateTime.Now:yyMMdd}-{new Random().Next(100, 999)}",
                CreatedBy = User.Identity?.Name ?? "System"
            };
            _context.Add(cashEntry);

            // B. Update trạng thái
            import.PaidAmount += amount;
            if (import.PaidAmount >= import.TotalAmount - 0.01m)
                import.PaymentStatus = ImportPaymentStatus.Paid;
            else
                import.PaymentStatus = ImportPaymentStatus.Partial;

            _context.Update(import);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Thanh toán thành công!";
            return RedirectToAction(nameof(Index));
        }
    }

    // ViewModel cho View Index
    public class SupplierDebtVM
    {
        public string SupplierName { get; set; }
        public decimal TotalDebt { get; set; }
        public decimal TotalPurchase { get; set; } // Tổng mua
        public int ImportCount { get; set; }
        public List<MaterialImport> Imports { get; set; }
    }
}