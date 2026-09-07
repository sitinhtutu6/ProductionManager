using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProductionApp.Web.Constants;
using ProductionApp.Web.Models;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ProductionApp.Web.Controllers
{
    [Authorize(Roles = "Admin")] // Chỉ Admin mới vào được khu vực này
    public class RoleController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public RoleController(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        // 1. DANH SÁCH ROLE
        public IActionResult Index()
        {
            var roles = _roleManager.Roles.OrderBy(r => r.Name).ToList();
            return View(roles);
        }

        // 2. TẠO ROLE MỚI
        [HttpPost]
        public async Task<IActionResult> Create(string roleName)
        {
            if (!string.IsNullOrEmpty(roleName))
            {
                var result = await _roleManager.CreateAsync(new IdentityRole(roleName.Trim()));
                if (result.Succeeded) TempData["Success"] = "Đã tạo nhóm quyền mới!";
                else TempData["Error"] = "Lỗi: " + string.Join(", ", result.Errors.Select(e => e.Description));
            }
            return RedirectToAction("Index");
        }

        // 3. XÓA ROLE
        public async Task<IActionResult> Delete(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role != null)
            {
                if (role.Name == "Admin")
                {
                    TempData["Error"] = "⛔ Không được xóa quyền Quản trị viên (Admin)!";
                    return RedirectToAction("Index");
                }
                await _roleManager.DeleteAsync(role);
                TempData["Success"] = "Đã xóa nhóm quyền.";
            }
            return RedirectToAction("Index");
        }

        // 4. HIỂN THỊ BẢNG PHÂN QUYỀN (GET)
        [HttpGet]
        public async Task<IActionResult> Permission(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null) return NotFound();

            var model = new PermissionViewModel
            {
                RoleId = roleId,
                RoleName = role.Name,
                RoleClaims = new List<RoleClaimsViewModel>()
            };

            // 🔥 DÙNG REFLECTION LẤY TẤT CẢ QUYỀN (Đảm bảo AppPermissions đã có hàm GetAllPolicyNames)
            var allPermissions = AppPermissions.GetAllPolicyNames();

            // Lấy quyền hiện tại của Role
            var currentClaims = await _roleManager.GetClaimsAsync(role);
            var currentClaimValues = currentClaims.Select(c => c.Value).ToHashSet(); // HashSet tra cứu nhanh hơn

            foreach (var permission in allPermissions)
            {
                model.RoleClaims.Add(new RoleClaimsViewModel
                {
                    Type = AppPermissions.ClaimType,
                    Value = permission,
                    Selected = currentClaimValues.Contains(permission)
                });
            }

            return View(model);
        }

        // 5. CẬP NHẬT QUYỀN (POST)
        [HttpPost]
        public async Task<IActionResult> UpdatePermission(PermissionViewModel model)
        {
            var role = await _roleManager.FindByIdAsync(model.RoleId);
            if (role == null) return NotFound();

            // --- BẢO VỆ ADMIN: Tránh trường hợp Admin tự tay bóp cổ mình (tắt quyền roles) ---
            if (role.Name == "Admin")
            {
                var adminCriticalParams = new[] { ".Roles.", ".Users." }; // Các module sống còn
                foreach (var item in model.RoleClaims)
                {
                    if (adminCriticalParams.Any(p => item.Value.Contains(p))) item.Selected = true;
                }
            }
            // ---------------------------------------------------------------------------------

            var existingClaims = await _roleManager.GetClaimsAsync(role);

            // 1. Xóa hết quyền cũ
            foreach (var claim in existingClaims.Where(c => c.Type == AppPermissions.ClaimType))
            {
                await _roleManager.RemoveClaimAsync(role, claim);
            }

            // 2. Thêm quyền mới (Chỉ thêm những cái Selected = true)
            var selectedClaims = model.RoleClaims.Where(a => a.Selected).Select(c => new Claim(AppPermissions.ClaimType, c.Value));
            foreach (var claim in selectedClaims)
            {
                await _roleManager.AddClaimAsync(role, claim);
            }

            TempData["Success"] = $"Đã cập nhật quyền cho nhóm '{role.Name}'!";
            return RedirectToAction("Permission", new { roleId = model.RoleId });
        }
    }
}