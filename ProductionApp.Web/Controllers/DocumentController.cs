using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProductionApp.Web.Constants; // Để gọi AppPermissions
using ProductionApp.Web.Models;
using ProductionManager.Data;
using System.Globalization;

namespace ProductionApp.Web.Controllers
{
    [Authorize] // Bắt buộc đăng nhập
    public class DocumentController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public DocumentController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // 1. DANH SÁCH & FILTER
        [Authorize(Policy = AppPermissions.Documents.View)] // Check quyền Xem
        public async Task<IActionResult> Index(string? fromDate, string? toDate)
        {
            var query = _context.Documents.AsQueryable();

            DateTime? fDate = null;
            DateTime? tDate = null;

            if (!string.IsNullOrEmpty(fromDate))
            {
                if (DateTime.TryParseExact(fromDate, "d/M/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var temp))
                    fDate = temp;
            }

            if (!string.IsNullOrEmpty(toDate))
            {
                if (DateTime.TryParseExact(toDate, "d/M/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var temp))
                    tDate = temp;
            }

            if (fDate.HasValue)
                query = query.Where(x => x.CreatedAt.Date >= fDate.Value.Date);

            if (tDate.HasValue)
                query = query.Where(x => x.CreatedAt.Date <= tDate.Value.Date);

            var list = await query.OrderByDescending(x => x.CreatedAt).ToListAsync();
            return View(list);
        }

        // 2. UPLOAD FILE (TỰ ĐỘNG PHÂN LOẠI)
        [HttpPost]
        [Authorize(Policy = AppPermissions.Documents.Upload)] // Check quyền Upload
        public async Task<IActionResult> Upload(List<IFormFile> files)
        {
            if (files != null && files.Count > 0)
            {
                string uploadPath = Path.Combine(_env.WebRootPath, "uploads/documents");
                if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

                foreach (var file in files)
                {
                    if (file.Length == 0) continue;

                    // A. Lưu file vật lý
                    string ext = Path.GetExtension(file.FileName);
                    string sysFileName = $"{Guid.NewGuid()}{ext}";
                    string filePath = Path.Combine(uploadPath, sysFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    // B. Logic Auto-Grouping (Phân loại tự động)
                    string name = file.FileName.ToUpper();
                    string group = "Tài liệu chung";

                    if (name.Contains("HOPDONG") || name.Contains("HD-") || name.Contains("HỢP ĐỒNG")) group = "Hợp đồng";
                    else if (name.Contains("BANVE") || name.Contains("BV-") || name.Contains("BẢN VẼ")) group = "Bản vẽ";
                    else if (name.Contains("HOADON") || name.Contains("BILL") || name.Contains("HÓA ĐƠN")) group = "Hóa đơn - Chứng từ";
                    else if (name.Contains("BAOGIA") || name.Contains("BG-")) group = "Báo giá";
                    else if (ext.ToLower() == ".pdf") group = "Văn bản PDF";
                    else if (ext.ToLower().Contains("xls")) group = "Excel - Số liệu";
                    else if (ext.ToLower().Contains("doc")) group = "Word - Văn bản";

                    // C. Lưu vào DB
                    var doc = new Document
                    {
                        FileName = file.FileName,
                        SystemFileName = sysFileName,
                        FilePath = "/uploads/documents/" + sysFileName,
                        Group = group,
                        FileSize = file.Length / 1024, // KB
                        UploadedBy = User.Identity?.Name ?? "Unknown",
                        CreatedAt = DateTime.Now
                    };
                    _context.Documents.Add(doc);
                }
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Đã tải lên {files.Count} tài liệu.";
            }
            return RedirectToAction("Index");
        }

        // 3. XÓA FILE
        [Authorize(Policy = AppPermissions.Documents.Delete)] // Check quyền Xóa
        public async Task<IActionResult> Delete(int id)
        {
            var doc = await _context.Documents.FindAsync(id);
            if (doc != null)
            {
                // Xóa file trên ổ cứng
                string fullPath = Path.Combine(_env.WebRootPath, "uploads/documents", doc.SystemFileName);
                if (System.IO.File.Exists(fullPath)) System.IO.File.Delete(fullPath);

                _context.Documents.Remove(doc);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã xóa tài liệu.";
            }
            return RedirectToAction("Index");
        }
    }
}