using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using ProductionApp.Web.Constants;
using ProductionApp.Web.Helpers;
using ProductionApp.Web.Services;
using ProductionManager.Data;
using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.HttpOverrides;

using System.IO;


// 🔥 Ép thư mục gốc luôn là thư mục chứa ứng dụng
Directory.SetCurrentDirectory(AppContext.BaseDirectory);

//var builder = WebApplication.CreateBuilder(args);

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    // Lệnh này ép hệ thống luôn tìm thư mục wwwroot ở ngay cạnh file chạy (.exe/.dll)
    ContentRootPath = AppContext.BaseDirectory
});

// ==============================================================================
// 1. CẤU HÌNH SERVICES
// ==============================================================================

builder.Services.AddControllersWithViews();

// --- DATABASE ---
builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) =>
{
    var serverConfig = serviceProvider.GetService<ServerConfig>();
    string? dbProvider = serverConfig?.DbProvider ?? builder.Configuration["DatabaseProvider"];
    string? connectionString = serverConfig?.ConnectionString;

    // Fallback nếu không có config
    if (string.IsNullOrEmpty(connectionString))
    {
        if (dbProvider == "Sqlite") connectionString = builder.Configuration.GetConnectionString("SqliteConnection");
        else if (dbProvider == "MySql") connectionString = builder.Configuration.GetConnectionString("MySqlConnection");
        else connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    }

    if (dbProvider == "Sqlite") options.UseSqlite(connectionString);
    else if (dbProvider == "MySql") options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
    else options.UseSqlServer(connectionString);
});

// --- IDENTITY (QUẢN LÝ TÀI KHOẢN) ---
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    // Cấu hình password đơn giản để dễ test (Production nên bật lại)
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;

    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false; // Tắt xác thực email để login được ngay
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.Configure<AuthenticationOptions>(options =>
{
    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
});

// --- COOKIE (FIX LỖI NGROK / LOGIN) ---
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;

    // 🔥 QUAN TRỌNG: Để Cookie chạy được trên Ngrok (Http over Https)
    options.Cookie.SecurePolicy = CookieSecurePolicy.None;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// --- EMAIL & BACKUP ---
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddSingleton<BackupService>();
builder.Services.AddHostedService(provider => provider.GetRequiredService<BackupService>());

// --- CẤU HÌNH HEADER (CHO NGROK) ---
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// ==============================================================================
// 🔥 2. CẤU HÌNH PHÂN QUYỀN (AUTO POLICY) 🔥
// ==============================================================================
builder.Services.AddAuthorization(options =>
{
    // Dùng hàm Reflection mới (Đảm bảo AppPermissions.cs đã cập nhật)
    var allPolicies = AppPermissions.GetAllPolicyNames();

    foreach (var policyName in allPolicies)
    {
        options.AddPolicy(policyName, policy =>
        {
            // Logic: Admin được phép hết, HOẶC user phải có Claim
            policy.RequireAssertion(context =>
                context.User.IsInRole("Admin") ||
                context.User.HasClaim(c => c.Type == AppPermissions.ClaimType && c.Value == policyName)
            );
        });
    }
});

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Trong file Program.cs, đoạn app.UseRequestLocalization
var supportedCultures = new[] { new System.Globalization.CultureInfo("vi-VN") };
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("vi-VN"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

// ==============================================================================
// 3. MIDDLEWARE PIPELINE
// ==============================================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("en-US")
});

app.UseForwardedHeaders(); // Phải đặt trước Auth

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ==============================================================================
// 4. KHỞI TẠO DỮ LIỆU (AUTO MIGRATION & SEEDING) 🔥
// ==============================================================================
using (var scope = app.Services.CreateScope())
{
    try
    {
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<AppDbContext>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<AppUser>>();

        // A. Tự động tạo Database nếu chưa có
        context.Database.EnsureCreated();

        // B. Tạo Role chuẩn
        string[] roles = { "Admin", "User", "Manager" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // C. Cấp Full Quyền cho Role Admin (Dùng Reflection để không bị sót)
        var adminRole = await roleManager.FindByNameAsync("Admin");
        var allPerms = AppPermissions.GetAllPolicyNames(); // 🔥 Dùng hàm mới
        var currentClaims = await roleManager.GetClaimsAsync(adminRole);

        foreach (var p in allPerms)
        {
            if (!currentClaims.Any(c => c.Type == AppPermissions.ClaimType && c.Value == p))
            {
                await roleManager.AddClaimAsync(adminRole, new Claim(AppPermissions.ClaimType, p));
            }
        }

        // D. Tạo User Admin mặc định (Nếu chưa có)
        // Email: admin@gmail.com / Pass: Admin@123
        // Đặt đoạn này trong scope của Program.cs
        // ------------------------------------------------------------

        // 1. Định nghĩa Email Admin
        string adminEmail = "admin@gmail.com"; // Email muốn cấp quyền

        // 2. Tìm User trong DB
        var adminUser = userManager.FindByEmailAsync(adminEmail).GetAwaiter().GetResult();

        // 3. Xử lý cấp quyền
        if (adminUser != null)
        {
            // Kiểm tra xem User này đã có quyền Admin chưa
            // Lưu ý: Dùng adminUser tìm được ở trên, KHÔNG query lại db
            bool isAdmin = userManager.IsInRoleAsync(adminUser, "Admin").GetAwaiter().GetResult();

            if (!isAdmin)
            {
                // Nếu chưa có quyền thì thêm vào
                userManager.AddToRoleAsync(adminUser, "Admin").GetAwaiter().GetResult();
                Console.WriteLine($"✅ Đã cấp quyền Admin cho: {adminEmail}");
            }
        }

        Console.WriteLine("✅ Database & Permissions Initialized Successfully!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️ Init Error: {ex.Message}");
    }
}



app.Run();