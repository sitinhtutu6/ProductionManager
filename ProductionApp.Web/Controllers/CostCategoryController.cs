using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductionManager.Data;
using System.Threading.Tasks;

namespace ProductionApp.Web.Controllers
{
    // Chỉ cấp quyền cho cấp Quản lý hoặc Kế toán trưởng được phép tạo/sửa Danh mục P&L
    [Authorize(Roles = "Admin,Manager,Accountant")]
    public class CostCategoryController : Controller
    {
        private readonly AppDbContext _context;

        public CostCategoryController(AppDbContext context)
        {
            _context = context;
        }

        // 1. DANH SÁCH
        public async Task<IActionResult> Index()
        {
            var categories = await _context.CostCategories
                .OrderBy(c => c.Section)
                .ThenBy(c => c.Name)
                .ToListAsync();
            return View(categories);
        }

        // 2. TẠO MỚI
        public IActionResult Create()
        {
            return View(new CostCategory { IsActive = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CostCategory model)
        {
            if (ModelState.IsValid)
            {
                _context.Add(model);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã thêm loại chi phí mới!";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // 3. CHỈNH SỬA
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var category = await _context.CostCategories.FindAsync(id);
            if (category == null) return NotFound();
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CostCategory model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(model);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Cập nhật thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // 4. XÓA
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.CostCategories.FindAsync(id);
            if (category != null)
            {
                // Kiểm tra xem đã có Sổ quỹ nào dùng loại chi phí này chưa
                bool isUsed = await _context.CashEntries.AnyAsync(c => c.CostCategoryId == id);
                if (isUsed)
                {
                    TempData["Error"] = "Không thể xóa vì đã có Phiếu chi sử dụng danh mục này! Vui lòng chuyển sang trạng thái 'Đã ẩn'.";
                    return RedirectToAction(nameof(Index));
                }

                _context.CostCategories.Remove(category);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã xóa danh mục chi phí!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}