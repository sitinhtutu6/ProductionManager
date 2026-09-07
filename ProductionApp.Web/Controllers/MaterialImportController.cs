using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductionApp.Web.Constants;
using ProductionManager.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ProductionApp.Web.Controllers
{
    [Authorize]
    public class MaterialImportController : Controller
    {
        private readonly AppDbContext _context;

        public MaterialImportController(AppDbContext context)
        {
            _context = context;
        }

        // =============================================
        // 1. DANH SÁCH PHIẾU NHẬP
        // =============================================
        [Authorize(Policy = AppPermissions.Warehouse.View)]
        public async Task<IActionResult> Index()
        {
            var items = await _context.MaterialImports
                .OrderByDescending(x => x.ImportDate)
                .ToListAsync();
            return View(items);
        }

        // =============================================
        // 2. TẠO PHIẾU NHẬP (GET - Hiển thị Form)
        // =============================================
        [Authorize(Policy = AppPermissions.Warehouse.Create)]
        public IActionResult Create()
        {
            return View(new MaterialImport());
        }

        // =============================================
        // 3. TẠO PHIẾU NHẬP (POST - Xử lý Lưu & Thanh toán)
        // =============================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = AppPermissions.Warehouse.Create)]
        public async Task<IActionResult> Create(MaterialImport model, bool isPayNow, int paymentMethod)
        {
            // 1. Sinh mã phiếu nhập tự động (VD: PN-260122-001)
            model.ImportCode = GenerateImportCode();

            // Đảm bảo ngày giờ chính xác
            if (model.ImportDate == default) model.ImportDate = DateTime.Now;

            // 2. XỬ LÝ TRẠNG THÁI THANH TOÁN (Logic lõi)
            if (isPayNow)
            {
                // Nếu chọn trả ngay -> Ghi nhận đã trả hết
                model.PaidAmount = model.TotalAmount;
                model.PaymentStatus = ImportPaymentStatus.Paid;
            }
            else
            {
                // Nếu chọn công nợ -> Ghi nhận chưa trả, treo nợ
                model.PaidAmount = 0;
                model.PaymentStatus = ImportPaymentStatus.Unpaid;
            }

            // 3. Lưu Phiếu Nhập vào DB trước (Để lấy ID)
            _context.Add(model);
            await _context.SaveChangesAsync();

            // 4. NẾU THANH TOÁN NGAY -> TỰ ĐỘNG TẠO PHIẾU CHI
            if (isPayNow)
            {
                var cashEntry = new CashEntry
                {
                    TransactionDate = model.ImportDate,
                    ReportDate = model.ImportDate,
                    VoucherCode = $"PC-{model.ImportCode}", // Mã phiếu chi theo mã nhập (VD: PC-PN-...)

                    Type = TransactionType.Payment,         // Loại: Chi tiền
                    Category = EntryCategory.Business,      // Hạng mục: Kinh doanh

                    Amount = model.TotalAmount,
                    TargetName = model.SupplierName,
                    Description = $"Thanh toán ngay phiếu nhập kho {model.ImportCode}",
                    Method = (PaymentMethod)paymentMethod,

                    MaterialImportId = model.Id,            // 🔥 LIÊN KẾT: Để truy xuất ngược sau này

                    CreatedBy = User.Identity?.Name ?? "System",
                    CreatedAt = DateTime.Now
                };

                _context.Add(cashEntry);
                await _context.SaveChangesAsync();

                model.PaidAmount = model.TotalAmount;
                model.PaymentStatus = ImportPaymentStatus.Paid;
                _context.Update(model);
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = $"Đã nhập kho phiếu {model.ImportCode} thành công!";
            return RedirectToAction(nameof(Index));
        }

        // =============================================
        // HELPER: SINH MÃ PHIẾU TỰ ĐỘNG
        // =============================================
        private string GenerateImportCode()
        {
            string prefix = "PN";
            string datePart = DateTime.Now.ToString("yyMMdd"); // VD: 260122

            // Tìm phiếu cuối cùng trong ngày hôm nay để tăng số thứ tự
            var lastImport = _context.MaterialImports
                .Where(x => x.ImportCode.StartsWith($"{prefix}-{datePart}"))
                .OrderByDescending(x => x.Id)
                .Select(x => x.ImportCode)
                .FirstOrDefault();

            int nextNum = 1;
            if (!string.IsNullOrEmpty(lastImport))
            {
                var parts = lastImport.Split('-');
                // Cấu trúc: PN-yyMMdd-XXX -> Lấy phần XXX (index 2)
                if (parts.Length >= 3 && int.TryParse(parts[2], out int currentNum))
                {
                    nextNum = currentNum + 1;
                }
            }

            // Kết quả: PN-260122-001
            return $"{prefix}-{datePart}-{nextNum:000}";
        }
    }
}