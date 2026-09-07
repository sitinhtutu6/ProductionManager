using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore; // <--- QUAN TRỌNG: Phải có dòng này mới dùng được ToListAsync()
using ProductionManager.Data;
using ProductionApp.Web.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;


namespace ProductionApp.Web.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _context;
        // Nếu muốn đọc config email admin để highlight
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;

        // Inject UserManager (để tạo user có pass) và AppDbContext (để list nhanh)
        public UserController(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, AppDbContext context, Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _configuration = configuration;
        }

        // ==========================================
        // 1. TRANG DANH SÁCH (INDEX)
        // ==========================================
        [HttpGet]

        public async Task<IActionResult> Index()
        {
            // 1. Lấy danh sách User
            var users = await _context.Users.ToListAsync();

            // 2. Tạo list ViewModel
            var userViewModels = new List<UserViewModel>();

            // 3. Lặp qua từng user để lấy Role
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user); // Lấy role từ DB

                userViewModels.Add(new UserViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    FullName = user.FullName,
                    Address = user.Address,
                    IsActive = user.IsActive,
                    Roles = roles.ToList() // Gán vào đây
                });
            }

            // Lấy email super admin để highlight (nếu cần)
            ViewBag.CurrentAdminEmail = _configuration["AdminEmail"]?.Trim().ToLower();

            return View(userViewModels); // Trả về ViewModel thay vì AppUser
        }

        // ==========================================
        // 2. TẠO MỚI (CREATE)
        // ==========================================
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // Ở đây ta nhận thêm string Password riêng vì AppUser không nên chứa password plain text
        [HttpPost]
        public async Task<IActionResult> Create(AppUser user, string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "Mật khẩu không được để trống");
                return View(user);
            }

            // Gán các giá trị mặc định
            user.UserName = user.Email; // Lấy Email làm tên đăng nhập luôn
            user.EmailConfirmed = true; // Tạm thời cho active luôn
            if (string.IsNullOrEmpty(user.FullName)) user.FullName = "New User";

            // ⚠️ QUAN TRỌNG: Dùng UserManager để tạo User + Mã hóa mật khẩu
            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                return RedirectToAction("Index");
            }

            // Nếu lỗi (ví dụ mật khẩu yếu, trùng email...)
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(user);
        }

        // ==========================================
        // 3. CHỈNH SỬA (EDIT)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(AppUser updatedUser)
        {
            var user = await _userManager.FindByIdAsync(updatedUser.Id);
            if (user != null)
            {
                // Cập nhật các thông tin cơ bản
                user.FullName = updatedUser.FullName;
                user.Email = updatedUser.Email;
                user.UserName = updatedUser.Email; // Đồng bộ username theo email
                user.IsActive = updatedUser.IsActive;
                user.Address = updatedUser.Address;

                // Lưu xuống DB
                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }

                foreach (var error in result.Errors) ModelState.AddModelError("", error.Description);
            }
            return View(updatedUser);
        }

        // ==========================================
        // 4. XÓA (DELETE)
        // ==========================================
        [HttpGet] // Để test nhanh, thực tế nên dùng Post
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
            }
            return RedirectToAction("Index");
        }

        // ==========================================
        // 5. QUẢN LÝ VAI TRÒ (MANAGE ROLES) - MỚI
        // ==========================================
        [HttpGet]
        [Authorize(Roles = "Admin")] // Chỉ Admin được phân quyền
        public async Task<IActionResult> ManageRoles(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var model = new ManageUserRolesViewModel
            {
                UserId = user.Id,
                Email = user.Email,
                FullName = user.FullName
            };

            // 1. Lấy tất cả Role trong hệ thống
            var allRoles = await _roleManager.Roles.ToListAsync();

            // 2. Lấy Role hiện tại của User này
            var userRoles = await _userManager.GetRolesAsync(user);

            // 3. Map dữ liệu để hiển thị checkbox
            foreach (var role in allRoles)
            {
                model.Roles.Add(new UserRoleSelection
                {
                    RoleName = role.Name,
                    IsSelected = userRoles.Contains(role.Name)
                });
            }

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageRoles(ManageUserRolesViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null) return NotFound();

            // 1. Lấy danh sách Role cũ của user
            var userRoles = await _userManager.GetRolesAsync(user);

            // 2. Lấy danh sách Role mới được chọn từ form
            var selectedRoles = model.Roles.Where(x => x.IsSelected).Select(x => x.RoleName).ToList();

            // 3. Xử lý logic thêm/xóa
            // - Xóa những role cũ không còn được chọn (trừ Admin nếu muốn bảo vệ)
            var rolesToRemove = userRoles.Except(selectedRoles).ToList();

            // Logic bảo vệ: Không cho tự hủy quyền Admin của chính mình (nếu đang đăng nhập)
            if (User.Identity.Name == user.UserName && rolesToRemove.Contains("Admin"))
            {
                TempData["Error"] = "Bạn không thể tự gỡ quyền Admin của chính mình!";
                return RedirectToAction("ManageRoles", new { userId = model.UserId });
            }

            if (rolesToRemove.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
            }

            // - Thêm những role mới chưa có
            var rolesToAdd = selectedRoles.Except(userRoles).ToList();
            if (rolesToAdd.Any())
            {
                await _userManager.AddToRolesAsync(user, rolesToAdd);
            }

            TempData["Success"] = $"Cập nhật vai trò cho {user.Email} thành công!";
            return RedirectToAction("Index");
        }
    }
}