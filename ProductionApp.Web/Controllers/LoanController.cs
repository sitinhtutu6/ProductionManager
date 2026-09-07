using ClosedXML.Excel; // Nhớ thêm thư viện này
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductionApp.Web.Constants;
using ProductionApp.Web.Models.ViewModels;
using ProductionManager.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ProductionApp.Web.Controllers
{
    [Authorize]
    public class LoanController : Controller
    {
        private readonly AppDbContext _context;

        public LoanController(AppDbContext context)
        {
            _context = context;
        }

        // 1. INDEX: HIỂN THỊ DANH SÁCH
        [Authorize(Policy = AppPermissions.Debts.View)]
        public async Task<IActionResult> Index()
        {
            // Lấy toàn bộ khoản vay gốc
            var rawLoans = await _context.CashEntries
                .Include(x => x.Repayments)
                .Where(x => x.Category == EntryCategory.Loan && x.ParentId == null)
                .OrderByDescending(x => x.TransactionDate)
                .ToListAsync();

            var model = rawLoans.Select(item => new LoanVM
            {
                Id = item.Id,
                // Thu (Receipt) -> Mình đi vay (Borrowing)
                // Chi (Payment) -> Mình cho vay (Lending)
                Type = item.Type == TransactionType.Receipt ? LoanType.Borrowing : LoanType.Lending,
                PartnerName = item.TargetName ?? "Không tên",
                StartDate = item.TransactionDate,
                TotalAmount = item.Amount,
                ProcessedAmount = item.Repayments?.Sum(r => r.Amount) ?? 0,
                History = item.Repayments?
                    .OrderByDescending(h => h.TransactionDate)
                    .Select(h => new RepaymentHistory
                    {
                        Id = h.Id,
                        Date = h.TransactionDate,
                        Amount = h.Amount,
                        Note = h.Description
                    }).ToList() ?? new List<RepaymentHistory>()
            }).ToList();

            return View(model);
        }


        //2. XUẤT EXCEL BÁO CÁO
        [Authorize(Policy = AppPermissions.Debts.Create)]
        public async Task<IActionResult> ExportExcel(DateTime? fromDate, DateTime? toDate)
        {
            var f = fromDate ?? new DateTime(DateTime.Now.Year, 1, 1);
            var t = toDate ?? DateTime.Now;

            // Lấy các khoản vay phát sinh hoặc có giao dịch trong khoảng thời gian này
            var loans = await _context.CashEntries
                .Include(x => x.Repayments)
                .Where(x => x.Category == EntryCategory.Loan && x.ParentId == null)
                .ToListAsync();

            // Lọc trên RAM
            var reportData = loans.Where(x =>
                (x.TransactionDate >= f && x.TransactionDate <= t) ||
                (x.Repayments != null && x.Repayments.Any(r => r.TransactionDate >= f && r.TransactionDate <= t))
            ).ToList();

            using (var wb = new XLWorkbook())
            {
                var ws = wb.Worksheets.Add("BaoCaoVayNo");

                // Header
                ws.Cell("A1").Value = $"BÁO CÁO TÌNH HÌNH VAY & CHO VAY ({f:dd/MM/yyyy} - {t:dd/MM/yyyy})";
                ws.Range("A1:H1").Merge().Style.Font.Bold = true;

                // 🔥 [SỬA LỖI TẠI ĐÂY]: Thêm .Font vào trước .FontSize
                ws.Range("A1:H1").Style.Font.FontSize = 14;

                ws.Range("A1:H1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                int row = 3;
                ws.Cell(row, 1).Value = "STT";
                ws.Cell(row, 2).Value = "Ngày tạo";
                ws.Cell(row, 3).Value = "Đối tượng";
                ws.Cell(row, 4).Value = "Loại";
                ws.Cell(row, 5).Value = "Tổng gốc";
                ws.Cell(row, 6).Value = "Đã trả/thu";
                ws.Cell(row, 7).Value = "Còn lại";
                ws.Cell(row, 8).Value = "Trạng thái";

                // Style Header Table
                ws.Range(row, 1, row, 8).Style.Font.Bold = true;
                ws.Range(row, 1, row, 8).Style.Fill.BackgroundColor = XLColor.LightGray;

                row++;
                int stt = 1;

                foreach (var item in reportData)
                {
                    ws.Cell(row, 1).Value = stt++;
                    ws.Cell(row, 2).Value = item.TransactionDate;
                    ws.Cell(row, 3).Value = item.TargetName;

                    bool isBorrow = item.Type == TransactionType.Receipt;
                    ws.Cell(row, 4).Value = isBorrow ? "Đi vay" : "Cho vay";

                    // Fix màu chữ (đoạn này bạn đã làm đúng ở code trước, kiểm tra lại cho chắc)
                    if (isBorrow) ws.Cell(row, 4).Style.Font.FontColor = XLColor.Red;
                    else ws.Cell(row, 4).Style.Font.FontColor = XLColor.Green;

                    decimal total = item.Amount;
                    decimal processed = item.Repayments?.Sum(r => r.Amount) ?? 0;
                    decimal remain = total - processed;

                    ws.Cell(row, 5).Value = total;
                    ws.Cell(row, 6).Value = processed;
                    ws.Cell(row, 7).Value = remain;

                    if (remain <= 0)
                    {
                        ws.Cell(row, 8).Value = "Hoàn tất";
                        ws.Cell(row, 8).Style.Font.FontColor = XLColor.Gray;
                    }
                    else
                    {
                        ws.Cell(row, 8).Value = "Đang nợ";
                        ws.Cell(row, 8).Style.Font.Bold = true;
                    }

                    ws.Range(row, 1, row, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    row++;
                }

                ws.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"BaoCaoVayNo_{DateTime.Now:yyyyMMdd}.xlsx");
                }
            }
        }
    }
}