using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductionApp.Web.Constants;
using ProductionManager.Data;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

namespace ProductionApp.Web.Controllers
{
    public class CustomerController : Controller
    {
        private readonly AppDbContext _context;

        public CustomerController(AppDbContext context) { _context = context; }

        // 1. Danh sách
        [Authorize(Policy = AppPermissions.Customers.View)]
        public async Task<IActionResult> Index(PartnerType? type, string keyword)
        {
            var query = _context.Customers.AsQueryable();

            if (type.HasValue)
            {
                // Nếu lọc Khách hàng (1) -> Lấy Customer hoặc Both
                if (type == PartnerType.Customer)
                    query = query.Where(x => x.Type == PartnerType.Customer || x.Type == PartnerType.Both);
                // Nếu lọc NCC (2) -> Lấy Supplier hoặc Both
                else if (type == PartnerType.Supplier)
                    query = query.Where(x => x.Type == PartnerType.Supplier || x.Type == PartnerType.Both);
            }

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(x => x.CompanyName.Contains(keyword) ||
                x.CustomerCode.Contains(keyword) ||
                                         x.PhoneNumber.Contains(keyword));
            }

            ViewBag.CurrentType = type;
            ViewBag.Keyword = keyword;

            return View(await query.OrderByDescending(x => x.Id).ToListAsync());
        }

        // 2. Tạo mới / Sửa (Dùng chung 1 View Modal cho gọn hoặc tách ra tùy bạn)
        // Ở đây tôi làm tách ra cho cơ bản
        [Authorize(Policy = AppPermissions.Customers.Create)]
        public IActionResult Create(PartnerType type = PartnerType.Customer)
        {
            // Gợi ý mã: KH-xxx hoặc NCC-xxx
            string prefix = type == PartnerType.Supplier ? "NCC" : "KH";
            string datePart = DateTime.Now.ToString("yyMM");
            string autoCode = $"{prefix}{datePart}-{new Random().Next(1000, 9999)}";

            var model = new Customer
            {
                Type = type,
                CustomerCode = autoCode
            };
            return View(model);
        }

        [HttpPost]
        [Authorize(Policy = AppPermissions.Customers.Create)]
        public async Task<IActionResult> Create(Customer customer)
        {
            // Kiểm tra trùng mã
            if (await _context.Customers.AnyAsync(x => x.CustomerCode == customer.CustomerCode))
            {
                ModelState.AddModelError("CustomerCode", "Mã đối tác đã tồn tại!");
                return View(customer);
            }

            if (ModelState.IsValid)
            {
                _context.Add(customer);
                await _context.SaveChangesAsync();

                // Quay lại đúng Tab vừa tạo
                return RedirectToAction(nameof(Index), new { type = customer.Type == PartnerType.Supplier ? 2 : 1 });
            }
            return View(customer);
        }

        [Authorize(Policy = AppPermissions.Customers.Edit)]
        public async Task<IActionResult> Edit(int id)
        {
            var c = await _context.Customers.FindAsync(id);
            return c == null ? NotFound() : View(c);
        }

        [HttpPost]
        [Authorize(Policy = AppPermissions.Customers.Edit)]
        public async Task<IActionResult> Edit(Customer customer)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(customer);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Customers.Any(e => e.Id == customer.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index), new { type = customer.Type == PartnerType.Supplier ? 2 : 1 });
            }
            return View(customer);
        }

        [Authorize(Policy = AppPermissions.Customers.Delete)]
        public async Task<IActionResult> Delete(int id)
        {
            var c = await _context.Customers.Include(x => x.Orders).FirstOrDefaultAsync(x => x.Id == id);
            if (c != null)
            {
                // Kiểm tra ràng buộc dữ liệu
                if (c.Orders != null && c.Orders.Any())
                {
                    TempData["Error"] = $"Không thể xóa đối tác {c.CustomerCode} vì đã phát sinh đơn hàng!";
                }
                else
                {
                    _context.Customers.Remove(c);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Đã xóa đối tác thành công!";
                }
            }
            return RedirectToAction(nameof(Index));
        }
    }
}