using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using ProductionManager.Data;
using ProductionApp.Web.Services;
using Microsoft.AspNetCore.HttpOverrides;
using System;
using System.Linq;
using System.Collections.Generic;
using ProductionApp.Web.Constants;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ProductionApp.Web
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // 1. Cấu hình nhận diện Proxy (Ngrok/Pinggy)
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });

            // 2. MVC & Đa ngôn ngữ
            services.AddControllersWithViews()
                .AddViewLocalization(Microsoft.AspNetCore.Mvc.Razor.LanguageViewLocationExpanderFormat.Suffix)
                .AddDataAnnotationsLocalization();

            // 3. Database
            services.AddDbContext<AppDbContext>((serviceProvider, options) =>
            {
                var serverConfig = serviceProvider.GetService<ServerConfig>();
                string provider = serverConfig?.DbProvider ?? Configuration["DatabaseProvider"] ?? "SQLite";
                string connString = serverConfig?.ConnectionString ?? Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=ProductionDB.db";

                switch (provider)
                {
                    case "SQLite": options.UseSqlite(connString); break;
                    case "MySQL":
                        try { options.UseMySql(connString, ServerVersion.AutoDetect(connString)); }
                        catch { options.UseMySql(connString, new MySqlServerVersion(new Version(8, 0, 21))); }
                        break;
                    default: options.UseSqlServer(connString); break;
                }
            });

            // 4. Identity & Cookie (Đảm bảo hoạt động trên Iframe/Ngrok)
            services.AddIdentity<AppUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 6;
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = false;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            services.Configure<SecurityStampValidatorOptions>(options =>
            {
                // Mặc định Identity kiểm tra 30 phút/lần.
                // Set về TimeSpan.Zero để hệ thống kiểm tra sự thay đổi (mật khẩu, quyền) TRÊN MỖI LƯỢT CLICK.
                // Nếu phát hiện mật khẩu đã bị đổi, tài khoản sẽ bị văng ra trang Login ngay lập tức.
                options.ValidationInterval = TimeSpan.Zero;
            });

            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/Logout";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.ExpireTimeSpan = TimeSpan.FromHours(16); // Treo máy 16 tiếng tự đăng xuất
                options.Cookie.SecurePolicy = CookieSecurePolicy.None;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.SlidingExpiration = true;
            });

            // 5. Phân quyền
            services.AddAuthorization(options =>
            {
                var allPolicies = AppPermissions.GetAllPolicyNames();
                foreach (var policyName in allPolicies)
                {
                    options.AddPolicy(policyName, policy =>
                    {
                        policy.RequireAssertion(context =>
                            context.User.IsInRole("Admin") ||
                            context.User.HasClaim(c => c.Type == AppPermissions.ClaimType && c.Value == policyName)
                        );
                    });
                }
            });

            // 6. Custom Services
            services.AddScoped<DataAggregationService>();
            services.AddHttpClient<LocalAiService>();
            services.AddScoped<DiscordService>();
            services.AddTransient<IEmailSender, EmailSender>();
            services.AddHostedService<AutoBackupService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // BƯỚC QUAN TRỌNG NHẤT: Bật ForwardedHeaders ngay đầu Pipeline
            app.UseForwardedHeaders();

            // Khởi tạo DB an toàn qua Scope
            InitializeDatabase(app);

            if (!env.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            // Cấu hình Ngôn ngữ
            var supportedCultures = new[] { "vi-VN", "en-US", "zh-CN" }.Select(c => new CultureInfo(c)).ToList();
            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("vi-VN"),
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures,
                RequestCultureProviders = new List<IRequestCultureProvider>
                {
                    new QueryStringRequestCultureProvider(),
                    new CookieRequestCultureProvider { CookieName = "ProManager.Lang" }
                }
            });

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        private void InitializeDatabase(IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                try
                {
                    // Lệnh này tự động tạo DB nếu chưa có, và thêm bảng lịch sử Migration.
                    dbContext.Database.Migrate();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Lỗi DB Init: " + ex.Message);
                    return;
                }

                string[] roles = { "Admin", "Manager", "User" };
                foreach (var role in roles)
                {
                    if (!roleManager.RoleExistsAsync(role).GetAwaiter().GetResult())
                        roleManager.CreateAsync(new IdentityRole(role)).GetAwaiter().GetResult();
                }

                var adminRole = roleManager.FindByNameAsync("Admin").GetAwaiter().GetResult();
                if (adminRole != null)
                {
                    var allPermissions = AppPermissions.GetAllPolicyNames();
                    var currentClaims = roleManager.GetClaimsAsync(adminRole).GetAwaiter().GetResult().Select(c => c.Value).ToList();

                    foreach (var permission in allPermissions)
                    {
                        if (!currentClaims.Contains(permission))
                            roleManager.AddClaimAsync(adminRole, new Claim(AppPermissions.ClaimType, permission)).GetAwaiter().GetResult();
                    }
                }

                string adminEmail = "admin@gmail.com";
                try
                {
                    string configPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "server_settings.json");
                    if (System.IO.File.Exists(configPath))
                    {
                        var node = System.Text.Json.Nodes.JsonNode.Parse(System.IO.File.ReadAllText(configPath));
                        var emailFromConfig = node?["AdminEmail"]?.ToString()?.Trim().ToLowerInvariant();
                        if (!string.IsNullOrEmpty(emailFromConfig)) adminEmail = emailFromConfig;
                    }
                }
                catch { }

                var adminUser = userManager.FindByEmailAsync(adminEmail).GetAwaiter().GetResult();
                if (adminUser == null)
                {
                    adminUser = new AppUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        FullName = "System Administrator",
                        EmailConfirmed = true,
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    };
                    var createResult = userManager.CreateAsync(adminUser, "Admin@123").GetAwaiter().GetResult();
                    if (createResult.Succeeded) userManager.AddToRoleAsync(adminUser, "Admin").GetAwaiter().GetResult();
                }
                else if (!userManager.IsInRoleAsync(adminUser, "Admin").GetAwaiter().GetResult())
                {
                    userManager.AddToRoleAsync(adminUser, "Admin").GetAwaiter().GetResult();
                }
            }
        }
    }
}