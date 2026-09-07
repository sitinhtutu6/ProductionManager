using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductionApp.Web.Models.ViewModels;
using ProductionManager.Data;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace ProductionApp.Web.Controllers
{
    [Authorize]
    public class PartnerController : Controller
    {
        private readonly AppDbContext _context;

        public PartnerController(AppDbContext context)
        {
            _context = context;
        }
     

    public async Task<IActionResult> Index(string filterDate, string company, string department, string contactName, string jobDesc)
        {
            // Lưu lại giá trị bộ lọc để hiển thị trên Giao diện
            ViewBag.FilterDate = filterDate;
            ViewBag.Company = company;
            ViewBag.Department = department;
            ViewBag.ContactName = contactName;
            ViewBag.JobDesc = jobDesc;

            var query = _context.PartnerContacts.AsQueryable();

            // 1. LỌC TRỰC TIẾP TRÊN DATABASE (Chữ chứa từ khóa, không phân biệt hoa thường)
            if (!string.IsNullOrEmpty(company))
                query = query.Where(c => c.Company.Contains(company));

            if (!string.IsNullOrEmpty(department))
                query = query.Where(c => c.Department.Contains(department));

            if (!string.IsNullOrEmpty(contactName))
                query = query.Where(c => c.ContactName.Contains(contactName));

            if (!string.IsNullOrEmpty(jobDesc))
                query = query.Where(c => c.JobDescription.Contains(jobDesc));

            var rawContacts = await query.ToListAsync();

            // 2. LỌC THEO KHOẢNG NGÀY (Date Range từ Flatpickr: "dd/MM/yyyy to dd/MM/yyyy")
            if (!string.IsNullOrEmpty(filterDate))
            {
                var dates = filterDate.Split(" to ");
                if (DateTime.TryParseExact(dates[0], "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime startDate))
                {
                    DateTime endDate = startDate; // Mặc định nếu chỉ chọn 1 ngày
                    if (dates.Length > 1 && DateTime.TryParseExact(dates[1], "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedEnd))
                    {
                        endDate = parsedEnd;
                    }

                    // Logic lọc: Lấy những người có thời gian làm việc giao (cắt ngang) với khoảng thời gian lọc
                    rawContacts = rawContacts.Where(c =>
                        c.StartDate.Date <= endDate.Date &&
                        (!c.EndDate.HasValue || c.EndDate.Value.Date >= startDate.Date)
                    ).ToList();
                }
            }

            // 3. AUTO CẬP NHẬT TRẠNG THÁI (Nếu ai đó đến ngày EndDate)
            bool isModified = false;
            foreach (var contact in rawContacts)
            {
                var oldStatus = contact.Status;
                contact.UpdateStatusBasedOnDate();
                if (oldStatus != contact.Status) isModified = true;
            }
            if (isModified) await _context.SaveChangesAsync();

            // 4. GROUP VÀ SORT
            var groupedData = rawContacts
                .GroupBy(c => c.Company)
                .OrderBy(g => g.Key)
                .Select(g => new PartnerGroupVM
                {
                    CompanyName = g.Key,
                    ActiveCount = g.Count(x => x.Status != PartnerStatus.Ended),
                    Contacts = g.OrderBy(c => c.Status == PartnerStatus.Ended ? 1 : 0)
                                .ThenBy(c => c.Department)
                                .ThenBy(c => c.ContactName)
                                .ToList()
                })
                .ToList();

            return View(groupedData);
        }

    // GET: Partner/Create
    public IActionResult Create()
        {
            // Mặc định ngày bắt đầu là hôm nay
            return View(new PartnerContact { StartDate = DateTime.Now });
        }

        // POST: Partner/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PartnerContact model)
        {
            if (ModelState.IsValid)
            {
                model.UpdateStatusBasedOnDate(); // Check logic ngày tháng
                _context.PartnerContacts.Add(model);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã thêm liên hệ mới thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: Partner/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var contact = await _context.PartnerContacts.FindAsync(id);
            if (contact == null) return NotFound();

            return View(contact);
        }

        // POST: Partner/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PartnerContact model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    model.UpdateStatusBasedOnDate(); // Check logic ngày tháng
                    _context.Update(model);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Đã cập nhật thông tin thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.PartnerContacts.Any(e => e.Id == model.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // POST: Partner/Delete/5 (SOFT DELETE / KẾT THÚC)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var contact = await _context.PartnerContacts.FindAsync(id);
            if (contact != null)
            {
                // Chuyển trạng thái sang Kết thúc
                contact.Status = PartnerStatus.Ended;

                // Nếu chưa có ngày kết thúc thì tự động chốt là hôm nay
                if (!contact.EndDate.HasValue)
                {
                    contact.EndDate = DateTime.Now;
                }

                _context.Update(contact);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Đã kết thúc làm việc với đối tác {contact.ContactName}!";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> SearchLive(string filterDate, string company, string department, string contactName, string jobDesc)
        {
            var query = _context.PartnerContacts.AsQueryable();

            // 1. Lọc theo các từ khóa nhập vào
            if (!string.IsNullOrEmpty(company)) query = query.Where(c => c.Company.Contains(company));
            if (!string.IsNullOrEmpty(department)) query = query.Where(c => c.Department.Contains(department));
            if (!string.IsNullOrEmpty(contactName)) query = query.Where(c => c.ContactName.Contains(contactName));
            if (!string.IsNullOrEmpty(jobDesc)) query = query.Where(c => c.JobDescription.Contains(jobDesc));

            var rawContacts = await query.ToListAsync();

            // 2. Lọc theo ngày tháng (Flatpickr)
            if (!string.IsNullOrEmpty(filterDate))
            {
                var dates = filterDate.Split(" to ");
                if (DateTime.TryParseExact(dates[0], "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime startDate))
                {
                    DateTime endDate = startDate;
                    if (dates.Length > 1 && DateTime.TryParseExact(dates[1], "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime parsedEnd))
                    {
                        endDate = parsedEnd;
                    }

                    rawContacts = rawContacts.Where(c =>
                        c.StartDate.Date <= endDate.Date &&
                        (!c.EndDate.HasValue || c.EndDate.Value.Date >= startDate.Date)
                    ).ToList();
                }
            }

            // 3. Gom nhóm và Sắp xếp
            var groupedData = rawContacts
                .GroupBy(c => c.Company)
                .OrderBy(g => g.Key)
                .Select(g => new ProductionApp.Web.Models.ViewModels.PartnerGroupVM
                {
                    CompanyName = g.Key,
                    ActiveCount = g.Count(x => x.Status != ProductionManager.Data.PartnerStatus.Ended),
                    Contacts = g.OrderBy(c => c.Status == ProductionManager.Data.PartnerStatus.Ended ? 1 : 0)
                                .ThenBy(c => c.Department)
                                .ThenBy(c => c.ContactName)
                                .ToList()
                }).ToList();

            // 4. Trả về đúng file Partial View bạn đã tạo
            return PartialView("_ContactList", groupedData);
        }


    }
}