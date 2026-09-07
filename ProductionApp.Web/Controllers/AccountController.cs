using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using ProductionApp.Web.Models;
using ProductionApp.Web.Models.ViewModels;
using ProductionManager.Data;
using System.Text;
using System.Text.Encodings.Web;

namespace ProductionApp.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IEmailSender _emailSender;

        public AccountController(UserManager<AppUser> userManager,
                                 SignInManager<AppUser> signInManager,
                                 IEmailSender emailSender, AppDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _context = context;
        }


        // ==========================================
        // 1. ĐĂNG KÝ (REGISTER)
        // ==========================================
        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string fullName, string email, string password, string confirmPassword)
        {
            var companyConfig = await _context.CompanyConfigs.FirstOrDefaultAsync() ?? new CompanyConfig();
            var Title = companyConfig.CompanyName.ToUpper();

            // Validate dữ liệu đầu vào
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError(string.Empty, "Vui lòng nhập đầy đủ thông tin.");
                return View();
            }

            if (password != confirmPassword)
            {
                ModelState.AddModelError(string.Empty, "Mật khẩu xác nhận không khớp.");
                return View();
            }

            // Kiểm tra user đã tồn tại chưa
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                // Trường hợp A: Tài khoản đã tồn tại nhưng CHƯA kích hoạt (có thể do lỗi gửi mail lần trước)
                // -> Xóa user cũ để tạo lại
                if (!existingUser.EmailConfirmed)
                {
                    await _userManager.DeleteAsync(existingUser);
                }
                else
                {
                    // Trường hợp B: Tài khoản đã kích hoạt -> Báo lỗi
                    ModelState.AddModelError(string.Empty, "Email này đã được sử dụng. Vui lòng đăng nhập.");
                    return View();
                }
            }

            // Tạo User mới
            var user = new AppUser
            {
                UserName = email, // Username bắt buộc phải có, dùng luôn Email
                Email = email,
                FullName = fullName,
                EmailConfirmed = false // Bắt buộc xác thực email
            };

            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                // Mặc định gán quyền "User"
                await _userManager.AddToRoleAsync(user, "User");

                try
                {
                    // Tạo Token & Gửi Email
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                    var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Scheme);

                    // SỬA LẠI ĐOẠN KHAI BÁO CHUỖI NÀY
                    string emailBody = $@"
                        <!DOCTYPE html>
                        <html>
                        <head>
                        <meta charset='UTF-8'>
                        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                        <title>Xác thực tài khoản</title>
                        </head>

                        <body style='margin:0;padding:0;background-color:#f3f4f6;font-family:Arial,Helvetica,sans-serif;'>

                        <table width='100%' cellpadding='0' cellspacing='0' style='padding:40px 0'>
                        <tr>
                        <td align='center'>

                        <table width='600' cellpadding='0' cellspacing='0'
                        style='background:white;border-radius:10px;padding:40px;box-shadow:0 4px 12px rgba(0,0,0,0.05)'>

                        <tr>
                        <td align='center' style='padding-bottom:20px'>
                        <img src='YOUR_LOGO_URL' alt='Logo' width='120'/>
                        </td>
                        </tr>

                        <tr>
                        <td align='center'>
                        <h2 style='margin:0;color:#111827'>Xác thực tài khoản</h2>
                        </td>
                        </tr>

                        <tr>
                        <td style='padding-top:20px;color:#374151;font-size:16px;line-height:1.6'>

                        <p>Xin chào,</p>

                        <p>Bạn vừa đăng ký tài khoản trên hệ thống của chúng tôi.</p>

                        <p>Để hoàn tất quá trình đăng ký và kích hoạt tài khoản, vui lòng nhấn nút bên dưới:</p>

                        </td>
                        </tr>

                        <tr>
                        <td align='center' style='padding:30px 0'>
                        <a href='{System.Text.Encodings.Web.HtmlEncoder.Default.Encode(callbackUrl)}'
                        style='background-color:#2563eb;color:white;padding:14px 28px;
                        text-decoration:none;border-radius:8px;font-weight:bold;
                        display:inline-block;font-size:16px'>
                        Xác thực tài khoản
                        </a>
                        </td>
                        </tr>

                        <tr>
                        <td style='color:#6b7280;font-size:14px;line-height:1.6'>

                        <p>⚠️ Liên kết này sẽ hết hạn sau một khoảng thời gian vì lý do bảo mật.</p>

                        <p>Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email này.</p>

                        </td>
                        </tr>

                        <tr>
                        <td style='padding:20px 0'>
                        <hr style='border:none;border-top:1px solid #e5e7eb'>
                        </td>
                        </tr>

                        <tr>
                        <td style='color:#9ca3af;font-size:13px;text-align:center;line-height:1.6'>

                        <p>© {DateTime.Now.Year} Your Company. All rights reserved.</p>
                        <p>Email này được gửi tự động, vui lòng không trả lời.</p>

                        </td>
                        </tr>

                        </table>

                        </td>
                        </tr>
                        </table>

                        </body>
                        </html>";

                    await _emailSender.SendEmailAsync(user.Email, "Xác thực tài khoản - " + Title, emailBody);

                    // Chuyển hướng sang trang thông báo chờ kích hoạt
                    return View("RegisterConfirmation");
                }
                catch (Exception ex)
                {
                    // Nếu gửi mail lỗi thì báo cho user biết
                    ModelState.AddModelError(string.Empty, "Đăng ký thành công nhưng lỗi gửi email: " + ex.Message);
                    return View();
                }
            }

            // Nếu tạo user thất bại (ví dụ mật khẩu quá yếu)
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View();
        }

        // ==========================================
        // 2. XÁC THỰC EMAIL (CONFIRM EMAIL)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null) return RedirectToAction("Index", "Home");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound($"Không tìm thấy User ID '{userId}'.");

            try
            {
                code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
                var result = await _userManager.ConfirmEmailAsync(user, code);
                return View(result.Succeeded ? "ConfirmEmailSuccess" : "Error");
            }
            catch
            {
                return View("Error");
            }
        }

        // ==========================================
        // 3. ĐĂNG NHẬP (LOGIN)
        // ==========================================
        [HttpGet]
        public IActionResult Login()
        {
            // Hiển thị email đã ghi nhớ (nếu có)
            if (Request.Cookies.TryGetValue("RememberedEmail", out string savedEmail))
            {
                ViewBag.SavedEmail = savedEmail;
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password, bool rememberMe)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(email);

                // Kiểm tra User tồn tại & Đã kích hoạt Email chưa
                if (user != null)
                {
                    if (!await _userManager.IsEmailConfirmedAsync(user))
                    {
                        ModelState.AddModelError(string.Empty, "Tài khoản chưa kích hoạt. Vui lòng kiểm tra email.");
                        return View();
                    }
                }

                // Thực hiện đăng nhập
                //var result = await _signInManager.PasswordSignInAsync(email, password, rememberMe, lockoutOnFailure: true);
                // Ép isPersistent = false để Cookie đăng nhập chỉ sống theo phiên (Session). 
                // Tắt trình duyệt là hệ thống tự động đăng xuất.
                var result = await _signInManager.PasswordSignInAsync(email, password, isPersistent: false, lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    // Xử lý logic "Ghi nhớ Email" vào Cookie (không phải ghi nhớ đăng nhập của Identity)
                    if (rememberMe)
                    {
                        var option = new CookieOptions { Expires = DateTime.Now.AddDays(30), HttpOnly = true };
                        Response.Cookies.Append("RememberedEmail", email, option);
                    }
                    else
                    {
                        Response.Cookies.Delete("RememberedEmail");
                    }

                    return RedirectToAction("Index", "Home");
                }

                if (result.IsLockedOut) return View("Lockout");

                ModelState.AddModelError(string.Empty, "Sai email hoặc mật khẩu.");
            }
            return View();
        }

        // ==========================================
        // 4. QUÊN MẬT KHẨU & ĐĂNG XUẤT
        // ==========================================

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var companyConfig = await _context.CompanyConfigs.FirstOrDefaultAsync() ?? new CompanyConfig();
            var Title = companyConfig.CompanyName.ToUpper();

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                // Luôn hiện trang thành công để tránh dò email
                return View("ForgotPasswordConfirmation");
            }

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Action("ResetPassword", "Account", new { code }, protocol: Request.Scheme);

            // Tạo tiêu đề Email rõ ràng
            string emailSubject = "Yêu cầu cấp lại mật khẩu - Production App của " + Title;

            // Tạo nội dung Email chuẩn HTML đẹp mắt
            string emailBody = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 10px; background-color: #f9f9f9;'>
        
                    <div style='text-align: center; margin-bottom: 20px;'>
                        <h2 style='color: #0d6efd; margin: 0;'>Khôi phục mật khẩu</h2>
                    </div>

                    <div style='background-color: white; padding: 20px; border-radius: 5px;'>
                        <p>Chào bạn,</p>
                        <p>Chúng tôi vừa nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn trên hệ thống <strong>Production App</strong>.</p>
                        <p>Để tạo mật khẩu mới, vui lòng bấm vào nút bên dưới:</p>

                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='{HtmlEncoder.Default.Encode(callbackUrl)}' 
                               style='background-color: #0d6efd; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; font-weight: bold; font-size: 16px; display: inline-block;'>
                               Đặt lại mật khẩu ngay
                            </a>
                        </div>

                        <p style='color: #dc3545; font-size: 14px;'>
                            <strong>Lưu ý:</strong> Đường dẫn này chỉ có hiệu lực trong vòng 24 giờ.
                        </p>
                        <p>Nếu bạn không gửi yêu cầu này, vui lòng bỏ qua email này. Tài khoản của bạn vẫn an toàn.</p>
                    </div>

                    <hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;' />
                    <p style='font-size: 12px; color: #666; word-break: break-all;'>
                        Nếu nút bấm không hoạt động, hãy copy đường link sau vào trình duyệt:<br/>
                        <a href='{HtmlEncoder.Default.Encode(callbackUrl)}' style='color: #0d6efd;'>
                            {HtmlEncoder.Default.Encode(callbackUrl)}
                        </a>
                    </p>
                </div>";

            // Gửi email
            await _emailSender.SendEmailAsync(email, emailSubject, emailBody);

            return View("ForgotPasswordConfirmation");
        }

        // 1. Hàm GET: Nhận code từ URL và hiển thị Form
        [HttpGet]
        public IActionResult ResetPassword(string code = null)
        {
            if (code == null) return BadRequest("Mã token không hợp lệ.");

            // Truyền code vào ViewModel để View có thể render ra hidden input
            var model = new ResetPasswordViewModel
            {
                Code = code
            };
            return View(model);
        }

        // 2. Hàm POST: Xử lý đổi mật khẩu
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Không tìm thấy user thì cứ báo thành công để tránh lộ thông tin
                return RedirectToAction("ResetPasswordConfirmation");
            }

            try
            {
                // QUAN TRỌNG: Giải mã token đúng theo cách đã mã hóa lúc gửi mail
                // Code gửi đi: WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code))
                // Code nhận về: Phải Decode ngược lại y hệt
                var decodedBytes = WebEncoders.Base64UrlDecode(model.Code);
                var decodedToken = Encoding.UTF8.GetString(decodedBytes);

                var result = await _userManager.ResetPasswordAsync(user, decodedToken, model.Password);

                if (result.Succeeded)
                {
                    return RedirectToAction("ResetPasswordConfirmation");
                }

                foreach (var error in result.Errors)
                {
                    // Nếu lỗi là Invalid Token, thường do token hết hạn hoặc format sai
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Mã xác thực bị lỗi định dạng.");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        public IActionResult AccessDenied() => View();

        // ==========================================
        // 1. HỒ SƠ CÁ NHÂN (PROFILE)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            var model = new UserProfileVM
            {
                Username = user.UserName,
                Email = user.Email,
                FullName = user.FullName, // Giả sử AppUser có trường FullName
                PhoneNumber = user.PhoneNumber
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Profile(UserProfileVM model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            // Cập nhật thông tin
            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["Success"] = "Cập nhật hồ sơ thành công!";
                // Refresh lại cookie để cập nhật Claims (nếu có lưu tên trong Claim)
                await _signInManager.RefreshSignInAsync(user);
                return RedirectToAction(nameof(Profile));
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }

        // ==========================================
        // 2. ĐỔI MẬT KHẨU (CHANGE PASSWORD)
        // ==========================================
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordVM model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            // Thực hiện đổi mật khẩu
            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user); // Giữ đăng nhập
                TempData["Success"] = "Đổi mật khẩu thành công!";
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }


    }
}