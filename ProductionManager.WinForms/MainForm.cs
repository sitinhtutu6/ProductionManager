using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Win32;
using ProductionManager.Data;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;
using System.Text.Json.Nodes;

namespace ProductionManager.WinForms
{
    public partial class MainForm : Form
    {
        private IHost? _webHost;
        private CancellationTokenSource? _cts;
        private const string Url = "http://*:7078";
        private const string AppName = "ProductionManagerServer";

        // --- NGROK & OLLAMA & N8N ---
        private Process? _ngrokProcess;
        private Process? _ollamaProcess;
        private Process? _n8nProcess;
        private const string N8N_PORT = "5678";
        private const string NgrokApiUrl = "http://127.0.0.1:4040/api/tunnels";

        // --- LAYOUT CONTROLS ---
        private FlowLayoutPanel flowPanel;
        private Panel pnlBottomLog;

        // UI Config
        private GroupBox grpConfig;
        private ComboBox cboProvider;
        private Panel pnlMySQL, pnlSQL;
        private ComboBox cboMySQLHost, cboSQLServer;
        private TextBox txtMySQLPort, txtMySQLUser, txtMySQLPass, txtMySQLDb;
        private TextBox txtSQLDb, txtSQLUser, txtSQLPass;
        private CheckBox chkSQLWindowsAuth;
        private TextBox txtConnectionString;
        private CheckBox chkShowConnString;
        private Button btnSaveConfig;

        // UI Email
        private TextBox txtAdminEmail, txtSenderEmail, txtSenderPassword;
        private static readonly string KeyString = "ProductionApp_SecretKey_2026_@#$";

        // UI Notification
        private GroupBox grpNotify;
        private TextBox txtTeleToken, txtTeleChatId, txtDiscordWebhook;

        // UI Control Server
        private GroupBox grpControl;
        private Button btnStart, btnStop, btnCheckUpdate;
        private Label lblStatus;
        private LinkLabel lnkUrl;
        private CheckBox chkStartWithWin, chkAutoStartWeb;
        private CheckBox chkAutoStartAi;
        private CheckBox chkAutoStartN8n;

        // UI Ngrok
        private GroupBox grpNgrok;
        private TextBox txtNgrokToken;
        private Button btnStartNgrok;
        private Label lblNgrokStatus;
        private LinkLabel lnkNgrokUrl;

        // Logs & Tray
        private RichTextBox rtbLog;
        private NotifyIcon trayIcon;
        private ContextMenuStrip trayMenu;
        private bool _isRealExit = false;

        // Discovery Constants
        private const int DISCOVERY_PORT = 9999;
        private const int WEB_PORT = 7078;
        private const string REQUEST_MSG = "WHERE_ARE_YOU";
        private const string RESPONSE_MSG = "I_AM_SERVER";

        private WebApplication _webApp;

        public MainForm()
        {
            BuildUI();
            InitializeSystemTray();
            LoadConfig();
            this.FormClosing += MainForm_FormClosing;
            this.Resize += MainForm_Resize;
            this.Load += MainForm_Load;
        }

        private async void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                // 1. Khởi tạo Builder
                var builder = WebApplication.CreateBuilder();
                builder.WebHost.UseUrls("http://localhost:5000", "http://*:5000");

                // Cấu hình Database Context (Phải khai báo ở đây thì Web mới hiểu)
                // Lưu ý: Đổi lại chuỗi kết nối MySQL cho đúng với máy khách hàng
                string connectionString = "Server="+ cboMySQLHost.Text.ToString().Trim() + 
                    "; Database="+ txtMySQLDb.Text.ToString().Trim()+
                    ";User="+ txtMySQLUser.Text.ToString().Trim()+
                    ";Password="+ txtMySQLPass.Text.ToString().Trim()+ ";";
                builder.Services.AddDbContext<AppDbContext>(options =>
                    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

                builder.Services.AddControllersWithViews();

                // 2. Build Web Server
                _webApp = builder.Build();

                // ===============================================================
                // 🔥 BƯỚC QUAN TRỌNG: TỰ ĐỘNG UPDATE DATABASE TRƯỚC KHI CHẠY WEB
                // ===============================================================
                using (var scope = _webApp.Services.CreateScope())
                {
                    var services = scope.ServiceProvider;
                    try
                    {
                        // Lấy ra DbContext
                        var context = services.GetRequiredService<AppDbContext>();

                        await context.Database.MigrateAsync();
                    }
                    catch (Exception dbEx)
                    {
                        MessageBox.Show("Lỗi khi nâng cấp Database: " + dbEx.Message, "Cảnh báo nghiêm trọng", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return; 
                    }
                }
                // ===============================================================

                // 3. Cấu hình các route (API hoặc View)
                _webApp.MapGet("/", () => "Xin chào! Web Server đang được chạy từ WinForms!");

                // 4. Khởi chạy Server ngầm
                await _webApp.StartAsync();

                // Cập nhật giao diện:
                // lblStatus.Text = "Hệ thống đang hoạt động ổn định ở cổng 5000.";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khởi động phần mềm: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // =========================================================
        // 1. GIAO DIỆN
        // =========================================================
        private void BuildUI()
        {
            this.Text = "Production Manager Server";
            this.Size = new Size(960, 850);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            this.Icon = GetAppIcon();

            flowPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(10) };

            grpConfig = new GroupBox { Text = "1. Cấu hình Database & Email", Size = new Size(900, 380) };
            SetupGroupConfigUI();
            MakeGroupCollapsible(grpConfig);

            grpNotify = new GroupBox { Text = "2. API Thông báo (Telegram / Discord)", Size = new Size(900, 130) };
            SetupGroupNotifyUI();
            MakeGroupCollapsible(grpNotify);

            grpControl = new GroupBox { Text = "3. Điều khiển Server & Hệ thống", Size = new Size(900, 180) };
            SetupGroupControlUI();
            MakeGroupCollapsible(grpControl);

            grpNgrok = new GroupBox { Text = "4. Truy cập Internet (Ngrok)", Size = new Size(900, 80) };
            SetupGroupNgrokUI();
            MakeGroupCollapsible(grpNgrok);

            flowPanel.Controls.AddRange(new Control[] { grpControl, grpConfig, grpNotify, grpNgrok });

            pnlBottomLog = new Panel { Dock = DockStyle.Bottom, Height = 200, Padding = new Padding(5) };
            GroupBox grpLog = new GroupBox { Text = "System Logs", Dock = DockStyle.Fill };
            rtbLog = new RichTextBox { Dock = DockStyle.Fill, BackColor = Color.FromArgb(30, 30, 30), ForeColor = Color.LightGreen, ReadOnly = true, Font = new Font("Consolas", 9.5F), BorderStyle = BorderStyle.None };
            grpLog.Controls.Add(rtbLog);
            pnlBottomLog.Controls.Add(grpLog);

            this.Controls.Add(flowPanel);
            this.Controls.Add(pnlBottomLog);
        }

        private void SetupGroupControlUI()
        {
            btnStart = new Button { Text = "▶ START LAN", BackColor = Color.ForestGreen, ForeColor = Color.White, Location = new Point(15, 30), Size = new Size(160, 50), Font = new Font("Segoe UI", 11F, FontStyle.Bold) };
            btnStart.Click += BtnStart_Click;

            btnStop = new Button { Text = "⏹ STOP", BackColor = Color.Firebrick, ForeColor = Color.White, Location = new Point(190, 30), Size = new Size(100, 50), Font = new Font("Segoe UI", 11F, FontStyle.Bold), Enabled = false };
            btnStop.Click += BtnStop_Click;

            btnCheckUpdate = new Button { Text = "🔄 UPDATE", Location = new Point(300, 30), Size = new Size(100, 50), BackColor = Color.White, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            btnCheckUpdate.Click += BtnCheckUpdate_Click;

            lblStatus = new Label { Text = "STOPPED", ForeColor = Color.Red, Font = new Font("Segoe UI", 16F, FontStyle.Bold), Location = new Point(420, 35), AutoSize = true };
            lnkUrl = new LinkLabel { Text = "http://localhost:7078", Location = new Point(422, 70), AutoSize = true, Enabled = false };
            lnkUrl.LinkClicked += (s, e) => OpenBrowser(lnkUrl.Text);

            Button btnFindWeb = new Button { Text = "🔍 TÌM WEB LAN", Location = new Point(15, 85), Size = new Size(160, 25), BackColor = Color.WhiteSmoke, Font = new Font("Segoe UI", 8F, FontStyle.Regular) };
            btnFindWeb.Click += async (s, e) => await FindAndConnectServer();

            chkStartWithWin = new CheckBox { Text = "Khởi động cùng Win", Location = new Point(650, 30), AutoSize = true };
            chkStartWithWin.CheckedChanged += ChkStartWithWin_CheckedChanged;

            chkAutoStartWeb = new CheckBox { Text = "Auto Start Server", Location = new Point(650, 60), AutoSize = true };
            chkAutoStartWeb.CheckedChanged += (s, e) => BtnSaveConfig_Click(null, null);

            chkAutoStartAi = new CheckBox { Text = "Auto Start AI (Ollama)", Location = new Point(650, 90), AutoSize = true, ForeColor = Color.DarkBlue, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            chkAutoStartAi.CheckedChanged += (s, e) => BtnSaveConfig_Click(null, null);

            chkAutoStartN8n = new CheckBox { Text = "Auto Start n8n (Automation)", Location = new Point(650, 120), AutoSize = true, ForeColor = Color.OrangeRed, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            chkAutoStartN8n.CheckedChanged += async (s, e) =>
            {
                if (chkAutoStartN8n.Checked)
                {
                    bool isOk = await CheckN8nPrerequisites();
                    if (!isOk) chkAutoStartN8n.Checked = false;
                    else BtnSaveConfig_Click(null, null);
                }
                else BtnSaveConfig_Click(null, null);
            };

            Button btnOpenN8n = new Button { Text = "🔗 Mở Workflow", Location = new Point(15, 120), Size = new Size(160, 30), BackColor = Color.White };
            btnOpenN8n.Click += (s, e) => OpenBrowser($"http://localhost:{N8N_PORT}");

            grpControl.Controls.AddRange(new Control[] { btnStart, btnStop, btnCheckUpdate, lblStatus, lnkUrl, btnFindWeb, chkStartWithWin, chkAutoStartWeb, chkAutoStartAi, chkAutoStartN8n, btnOpenN8n });
        }

        private void SetupGroupConfigUI()
        {
            var lblProvider = new Label { Text = "Loại Database:", Location = new Point(15, 30), AutoSize = true };
            cboProvider = new ComboBox { Location = new Point(130, 27), Size = new Size(200, 30), DropDownStyle = ComboBoxStyle.DropDownList };
            cboProvider.Items.AddRange(new object[] { "SQLServer", "MySQL", "SQLite" });
            cboProvider.SelectedIndex = 0;
            cboProvider.SelectedIndexChanged += CboProvider_SelectedIndexChanged;

            BuildMySQLPanel(); BuildSQLServerPanel();

            var lblConn = new Label { Text = "Chuỗi kết nối:", Location = new Point(15, 220), AutoSize = true, Font = new Font(this.Font, FontStyle.Bold) };
            txtConnectionString = new TextBox { Location = new Point(130, 217), Size = new Size(600, 30), UseSystemPasswordChar = true };
            chkShowConnString = new CheckBox { Location = new Point(740, 220), Text = "Hiện", AutoSize = true };
            chkShowConnString.CheckedChanged += (s, e) => txtConnectionString.UseSystemPasswordChar = !chkShowConnString.Checked;

            Label lblAdminEmail = new Label { Text = "Email Admin:", Location = new Point(15, 265), AutoSize = true };
            txtAdminEmail = new TextBox { Location = new Point(130, 262), Size = new Size(400, 30), PlaceholderText = "Email người quản trị" };

            Label lblSenderEmail = new Label { Text = "Gmail Gửi:", Location = new Point(15, 310), AutoSize = true, ForeColor = Color.Blue };
            txtSenderEmail = new TextBox { Location = new Point(130, 307), Size = new Size(300, 30), PlaceholderText = "VD: no-reply@gmail.com" };

            Label lblSenderPass = new Label { Text = "App Password:", Location = new Point(450, 310), AutoSize = true, ForeColor = Color.Blue };
            txtSenderPassword = new TextBox { Location = new Point(560, 307), Size = new Size(250, 30), PasswordChar = '•', PlaceholderText = "Mật khẩu ứng dụng 16 ký tự" };
            var chkShowPass = new CheckBox { Location = new Point(820, 310), Text = "Show", AutoSize = true };
            chkShowPass.CheckedChanged += (s, e) => txtSenderPassword.PasswordChar = chkShowPass.Checked ? '\0' : '•';

            grpConfig.Controls.AddRange(new Control[] { lblProvider, cboProvider, pnlMySQL, pnlSQL, lblConn, txtConnectionString, chkShowConnString, lblAdminEmail, txtAdminEmail, lblSenderEmail, txtSenderEmail, lblSenderPass, txtSenderPassword, chkShowPass });
        }

        private void SetupGroupNotifyUI()
        {
            Label lblTeleToken = new Label { Text = "Tele Bot Token:", Location = new Point(15, 30), AutoSize = true };
            txtTeleToken = new TextBox { Location = new Point(130, 27), Size = new Size(300, 30), PasswordChar = '•' };
            Label lblTeleChatId = new Label { Text = "Tele Chat ID:", Location = new Point(450, 30), AutoSize = true };
            txtTeleChatId = new TextBox { Location = new Point(550, 27), Size = new Size(150, 30) };
            Label lblDiscord = new Label { Text = "Discord Hook:", Location = new Point(15, 75), AutoSize = true };
            txtDiscordWebhook = new TextBox { Location = new Point(130, 72), Size = new Size(570, 30), PasswordChar = '•' };

            btnSaveConfig = new Button { Text = "💾 LƯU CẤU HÌNH", Location = new Point(720, 25), Size = new Size(160, 80), BackColor = Color.LightBlue, Font = new Font("Segoe UI", 11F, FontStyle.Bold) };
            btnSaveConfig.Click += BtnSaveConfig_Click;

            grpNotify.Controls.AddRange(new Control[] { lblTeleToken, txtTeleToken, lblTeleChatId, txtTeleChatId, lblDiscord, txtDiscordWebhook, btnSaveConfig });
        }

        private void SetupGroupNgrokUI()
        {
            Label lblToken = new Label { Text = "Token:", Location = new Point(15, 30), AutoSize = true };
            txtNgrokToken = new TextBox { Location = new Point(70, 27), Size = new Size(300, 30), PasswordChar = '•' };
            Button btnHelpNgrok = new Button { Text = "?", Location = new Point(380, 26), Size = new Size(30, 30), BackColor = Color.LightYellow };
            btnHelpNgrok.Click += BtnHelpNgrok_Click;
            btnStartNgrok = new Button { Text = "🌐 PUBLIC WEB", Location = new Point(430, 22), Size = new Size(150, 40), BackColor = Color.Purple, ForeColor = Color.White, Font = new Font(this.Font, FontStyle.Bold), Enabled = false };
            btnStartNgrok.Click += BtnStartNgrok_Click;
            lblNgrokStatus = new Label { Text = "OFFLINE", ForeColor = Color.Gray, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Location = new Point(600, 32), AutoSize = true };
            lnkNgrokUrl = new LinkLabel { Text = "---", Location = new Point(680, 32), AutoSize = true, Enabled = false };
            lnkNgrokUrl.LinkClicked += (s, e) => OpenBrowser(lnkNgrokUrl.Text);
            grpNgrok.Controls.AddRange(new Control[] { lblToken, txtNgrokToken, btnHelpNgrok, btnStartNgrok, lblNgrokStatus, lnkNgrokUrl });
        }

        private void MakeGroupCollapsible(GroupBox grp)
        {
            Button btnToggle = new Button { Text = "➖", Size = new Size(30, 25), Location = new Point(grp.Width - 40, 0), BackColor = Color.WhiteSmoke, FlatStyle = FlatStyle.Flat };
            btnToggle.FlatAppearance.BorderSize = 0;
            int fullHeight = grp.Height; bool isCollapsed = false;
            btnToggle.Click += (s, e) => { isCollapsed = !isCollapsed; if (isCollapsed) { grp.Height = 30; btnToggle.Text = "➕"; } else { grp.Height = fullHeight; btnToggle.Text = "➖"; } };
            grp.Controls.Add(btnToggle);
        }

        // =========================================================
        // 2. LOGIC OLLAMA & N8N
        // =========================================================
        private async void StartOllama()
        {
            if (!chkAutoStartAi.Checked) return;
            await Task.Run(() =>
            {
                try
                {
                    if (Process.GetProcessesByName("ollama").Length > 0)
                    {
                        WriteLog("🧠 AI Server (Ollama) đang chạy sẵn.", Color.Cyan);
                        return;
                    }
                    WriteLog("🧠 Đang khởi động AI Server (Ollama)...", Color.Cyan);
                    var psi = new ProcessStartInfo { FileName = "ollama", Arguments = "serve", CreateNoWindow = true, UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true };
                    _ollamaProcess = new Process { StartInfo = psi };
                    _ollamaProcess.Start();
                    WriteLog("✅ AI Server đã khởi động ngầm.", Color.Green);
                }
                catch (Exception ex) { WriteLog($"⚠️ Lỗi khởi động AI: {ex.Message}", Color.Orange); }
            });
        }

        private void StopOllama()
        {
            if (_ollamaProcess == null) return;
            try
            {
                // Giết sạch tiến trình mang tên ollama
                foreach (var p in Process.GetProcessesByName("ollama")) { try { p.Kill(); } catch { } }
            }
            catch { }
            finally { _ollamaProcess = null; WriteLog("🧠 Đã dừng AI Server.", Color.Orange); }
        }

        private async void StartN8n()
        {
            if (!chkAutoStartN8n.Checked) return;
            await Task.Run(() =>
            {
                try
                {
                    WriteLog("⚡ Đang khởi động n8n Workflow Engine...", Color.Cyan);
                    var psi = new ProcessStartInfo
                    {
                        FileName = "cmd",
                        Arguments = "/c n8n start",
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        StandardOutputEncoding = Encoding.UTF8,
                        StandardErrorEncoding = Encoding.UTF8
                    };

                    psi.EnvironmentVariables["N8N_PORT"] = N8N_PORT;
                    psi.EnvironmentVariables["N8N_PROTOCOL"] = "http";
                    psi.EnvironmentVariables["WEBHOOK_URL"] = $"http://localhost:{N8N_PORT}/";
                    psi.EnvironmentVariables["N8N_LISTEN_ADDRESS"] = "0.0.0.0";

                    _n8nProcess = new Process { StartInfo = psi };
                    _n8nProcess.OutputDataReceived += (s, e) => { if (!string.IsNullOrEmpty(e.Data) && e.Data.Contains("Editor is now accessible")) WriteLog($"✅ n8n đã chạy tại: http://localhost:{N8N_PORT}", Color.LimeGreen); };
                    _n8nProcess.ErrorDataReceived += (s, e) => { if (!string.IsNullOrEmpty(e.Data) && (e.Data.Contains("Error") || e.Data.Contains("address already in use"))) WriteLog($"[n8n] {e.Data}", Color.Orange); };
                    _n8nProcess.Start();
                    _n8nProcess.BeginOutputReadLine();
                    _n8nProcess.BeginErrorReadLine();
                }
                catch (Exception ex) { WriteLog($"⚠️ Lỗi khởi động n8n: {ex.Message}", Color.Orange); }
            });
        }

        private void StopN8n()
        {
            if (_n8nProcess == null) return;
            try
            {
                // Vì n8n chạy trên node.exe nên phải cực kỳ cẩn thận, chỉ kill tiến trình cmd mở n8n
                if (!_n8nProcess.HasExited) _n8nProcess.Kill(true); // 'true' để kill cả cây tiến trình con (.NET Core 3.0+)
            }
            catch { }
            finally { _n8nProcess = null; WriteLog("⚡ Đã dừng n8n Engine.", Color.Orange); }
        }


        // =========================================================
        // 3. START SERVER
        // =========================================================
        private async void BtnStart_Click(object sender, EventArgs e)
        {
            // 1. Khóa giao diện để ngăn người dùng thao tác khi đang xử lý
            btnStart.Enabled = false;
            grpConfig.Enabled = false;
            grpNotify.Enabled = false;
            lblStatus.Text = "STARTING...";
            lblStatus.ForeColor = Color.Orange;

            await Task.Run(async () =>
            {
                try
                {
                    // 2. Lấy dữ liệu từ UI một cách an toàn
                    string connectionString = "", provider = "", adminEmail = "", senderEmail = "", senderPass = "", teleToken = "", teleChatId = "", discordWebhook = "";
                    bool autoStartN8n = false;

                    this.Invoke(() =>
                    {
                        connectionString = txtConnectionString.Text.Trim();
                        provider = cboProvider.SelectedItem?.ToString() ?? "SQLServer";
                        adminEmail = txtAdminEmail.Text.Trim();
                        senderEmail = txtSenderEmail.Text.Trim();
                        senderPass = txtSenderPassword.Text.Trim();
                        teleToken = txtTeleToken.Text.Trim();
                        teleChatId = txtTeleChatId.Text.Trim();
                        discordWebhook = txtDiscordWebhook.Text.Trim();
                        autoStartN8n = chkAutoStartN8n.Checked;
                    });

                    // 4. Khởi tạo WebHost
                    var builder = Host.CreateDefaultBuilder()
                        .ConfigureWebHostDefaults(webBuilder =>
                        {
                            webBuilder.UseUrls(Url);
                            webBuilder.UseStartup<ProductionApp.Web.Startup>();
                        })
                        .ConfigureServices(services =>
                        {
                            services.AddSingleton(new ServerConfig
                            {
                                DbProvider = provider,
                                ConnectionString = connectionString,
                                AdminEmail = adminEmail,
                                SenderEmail = senderEmail,
                                SenderPassword = senderPass,
                                TelegramToken = teleToken,
                                TelegramChatId = teleChatId,
                                DiscordWebhook = discordWebhook
                            });
                            services.AddHostedService<ServerDiscoveryService>();
                            services.AddHttpClient<ProductionApp.Web.Services.AutomationService>();
                            services.AddScoped<ProductionApp.Web.Helpers.FileHelper>();
                            services.AddLocalization(options => options.ResourcesPath = "");
                        });

                    _webHost = builder.Build();
                    _cts = new CancellationTokenSource();

                    // ===============================================================
                    // 🔥 TÍNH NĂNG MỚI: TỰ ĐỘNG CHẠY MIGRATION TỪ DI CONTAINER
                    // ===============================================================
                    this.Invoke(() => WriteLog("⏳ Đang kiểm tra và đồng bộ Database...", Color.LightBlue));
                    using (var scope = _webHost.Services.CreateScope())
                    {
                        var services = scope.ServiceProvider;
                        try
                        {
                            // Lấy AppDbContext đã được cấu hình sẵn trong Startup.cs
                            var context = services.GetRequiredService<ProductionManager.Data.AppDbContext>();

                            // Thực thi lệnh Update-Database ngầm
                            await context.Database.MigrateAsync();

                            this.Invoke(() => WriteLog("✅ Đồng bộ Database thành công!", Color.LimeGreen));
                        }
                        catch (Exception dbEx)
                        {
                            this.Invoke(() => WriteLog($"⚠️ Lỗi đồng bộ DB (Server vẫn sẽ thử chạy): {dbEx.Message}", Color.Orange));
                            // Nếu muốn server DỪNG HẲN khi lỗi DB, bạn có thể uncomment dòng dưới:
                            // throw; 
                        }
                    }
                    // ===============================================================

                    // Bắt đầu chạy WebHost
                    await _webHost.StartAsync(_cts.Token);

                    // 5. Cấu hình Firewall (Giữ nguyên code cũ)
                    OpenFirewallPort(7078, "TCP");
                    OpenFirewallPort(9999, "UDP");

                    // 6. Xử lý UI và các dịch vụ bên thứ 3 sau khi Server chạy thành công
                    this.Invoke(() =>
                    {
                        StartOllama();

                        if (autoStartN8n)
                        {
                            Task.Run(async () =>
                            {
                                if (await CheckN8nPrerequisites())
                                {
                                    this.Invoke(() => StartN8n());
                                }
                                else
                                {
                                    this.Invoke(() => { chkAutoStartN8n.Checked = false; WriteLog("⚠️ Không thể chạy n8n.", Color.Orange); });
                                }
                            });
                        }

                        lblStatus.Text = "RUNNING";
                        lblStatus.ForeColor = Color.Green;
                        btnStop.Enabled = true;
                        lnkUrl.Enabled = true;
                        btnStartNgrok.Enabled = true;
                        WriteLog($"✅ Server LAN chạy tại: {Url ?? "http://localhost:7078"}", Color.LimeGreen);
                    });
                }
                catch (Exception ex)
                {
                    this.Invoke(() =>
                    {
                        WriteLog($"❌ Lỗi Start: {ex.Message}", Color.Red);
                        ResetUI(); // Hàm này giả định bạn đã có để mở lại form
                    });
                }
            });
        }

        private async void BtnStop_Click(object sender, EventArgs e)
        {
            StopNgrok();
            StopOllama();
            StopN8n();
            if (_webHost != null)
            {
                await _webHost.StopAsync(); _webHost.Dispose(); _webHost = null;
                ResetUI(); WriteLog("🛑 Server đã dừng.", Color.Red);
            }
        }

        // =========================================================
        // 4. LOAD & SAVE CONFIG
        // =========================================================
        private void LoadConfig()
        {
            try
            {
                RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                if (rk.GetValue(AppName) != null) chkStartWithWin.Checked = true;

                if (File.Exists("server_settings.json"))
                {
                    var node = JsonNode.Parse(File.ReadAllText("server_settings.json"));
                    txtAdminEmail.Text = node?["AdminEmail"]?.ToString() ?? ""; txtSenderEmail.Text = node?["SenderEmail"]?.ToString() ?? ""; txtSenderPassword.Text = Decrypt(node?["SenderPassword"]?.ToString()) ?? "";
                    txtTeleToken.Text = node?["TeleToken"]?.ToString() ?? ""; txtTeleChatId.Text = node?["TeleChatId"]?.ToString() ?? ""; txtDiscordWebhook.Text = node?["DiscordWebhook"]?.ToString() ?? "";
                    string? savedProvider = node?["DbProvider"]?.ToString();
                    if (savedProvider != null) cboProvider.SelectedItem = savedProvider;
                    if (savedProvider == "MySQL") { cboMySQLHost.Text = node?["MySQL_Host"]?.ToString(); txtMySQLUser.Text = Decrypt(node?["MySQL_User"]?.ToString()); txtMySQLPass.Text = Decrypt(node?["MySQL_Pass"]?.ToString()); txtMySQLDb.Text = node?["MySQL_Db"]?.ToString(); }
                    if (savedProvider == "SQLServer") { cboSQLServer.Text = node?["SQL_Server"]?.ToString(); txtSQLDb.Text = node?["SQL_Db"]?.ToString(); if (bool.TryParse(node?["SQL_Auth"]?.ToString(), out bool wAuth)) chkSQLWindowsAuth.Checked = wAuth; if (!chkSQLWindowsAuth.Checked) { txtSQLUser.Text = Decrypt(node?["SQL_User"]?.ToString()); txtSQLPass.Text = Decrypt(node?["SQL_Pass"]?.ToString()); } }
                    GenerateConnectionString();
                    txtNgrokToken.Text = node?["NgrokToken"]?.ToString() ?? "";
                    if (bool.TryParse(node?["AutoStartAi"]?.ToString(), out bool autoAi)) chkAutoStartAi.Checked = autoAi;
                    if (bool.TryParse(node?["AutoStartN8n"]?.ToString(), out bool autoN8n)) chkAutoStartN8n.Checked = autoN8n;
                    if (bool.TryParse(node?["AutoStartWeb"]?.ToString(), out bool autoStart) && autoStart) { chkAutoStartWeb.Checked = true; Task.Delay(1000).ContinueWith(t => this.Invoke(() => { if (btnStart.Enabled) BtnStart_Click(null, null); })); }
                }
            }
            catch { }
        }

        private void BtnSaveConfig_Click(object? sender, EventArgs e)
        {
            var json = new JsonObject();
            json["AdminEmail"] = txtAdminEmail.Text; json["SenderEmail"] = txtSenderEmail.Text; json["SenderPassword"] = Encrypt(txtSenderPassword.Text);
            json["TeleToken"] = txtTeleToken.Text; json["TeleChatId"] = txtTeleChatId.Text; json["DiscordWebhook"] = txtDiscordWebhook.Text;
            json["DbProvider"] = cboProvider.Text; json["ConnectionString"] = txtConnectionString.Text;
            if (cboProvider.Text == "MySQL") { json["MySQL_Host"] = cboMySQLHost.Text; json["MySQL_Db"] = txtMySQLDb.Text; json["MySQL_User"] = Encrypt(txtMySQLUser.Text); json["MySQL_Pass"] = Encrypt(txtMySQLPass.Text); }
            if (cboProvider.Text == "SQLServer") { json["SQL_Server"] = cboSQLServer.Text; json["SQL_Db"] = txtSQLDb.Text; json["SQL_Auth"] = chkSQLWindowsAuth.Checked; if (!chkSQLWindowsAuth.Checked) { json["SQL_User"] = Encrypt(txtSQLUser.Text); json["SQL_Pass"] = Encrypt(txtSQLPass.Text); } }
            json["AutoStartWeb"] = chkAutoStartWeb.Checked; json["AutoStartAi"] = chkAutoStartAi.Checked; json["AutoStartN8n"] = chkAutoStartN8n.Checked; json["NgrokToken"] = txtNgrokToken.Text;
            File.WriteAllText("server_settings.json", json.ToString());
            if (sender != null) MessageBox.Show("Đã lưu cấu hình thành công!");
        }

        // =========================================================
        // 5. NGROK LOGIC
        // =========================================================
        private async void BtnStartNgrok_Click(object sender, EventArgs e)
        {
            if (_ngrokProcess == null)
            {
                if (string.IsNullOrWhiteSpace(txtNgrokToken.Text)) { MessageBox.Show("Nhập Ngrok Token!"); return; }
                if (!File.Exists("ngrok.exe")) { MessageBox.Show("Thiếu file ngrok.exe!"); return; }
                try
                {
                    btnStartNgrok.Enabled = false; WriteLog("🌍 Khởi động Ngrok...", Color.Magenta);
                    var tokenProc = new Process { StartInfo = new ProcessStartInfo("ngrok.exe", $"config add-authtoken {txtNgrokToken.Text}") { CreateNoWindow = true, UseShellExecute = false } };
                    tokenProc.Start(); await tokenProc.WaitForExitAsync();

                    _ngrokProcess = new Process { StartInfo = new ProcessStartInfo("ngrok.exe", "http 7078") { CreateNoWindow = true, UseShellExecute = false } };
                    _ngrokProcess.Start(); await Task.Delay(2000);

                    string publicUrl = await GetNgrokUrl();
                    if (!string.IsNullOrEmpty(publicUrl))
                    {
                        lblNgrokStatus.Text = "ONLINE"; lblNgrokStatus.ForeColor = Color.Green;
                        lnkNgrokUrl.Text = publicUrl; lnkNgrokUrl.Enabled = true;
                        btnStartNgrok.Text = "HỦY PUBLIC"; btnStartNgrok.BackColor = Color.Gray;
                        WriteLog($"🌍 Public OK: {publicUrl}", Color.Magenta);
                    }
                    else throw new Exception("Không lấy được URL.");
                }
                catch (Exception ex) { WriteLog($"❌ Lỗi Ngrok: {ex.Message}", Color.Red); StopNgrok(); }
                finally { btnStartNgrok.Enabled = true; }
            }
            else StopNgrok();
        }

        private void StopNgrok()
        {
            try { if (_ngrokProcess != null) { if (!_ngrokProcess.HasExited) _ngrokProcess.Kill(); _ngrokProcess.Dispose(); } }
            catch { }
            finally
            {
                _ngrokProcess = null; foreach (var p in Process.GetProcessesByName("ngrok")) { try { p.Kill(); } catch { } }
                if (lblNgrokStatus.InvokeRequired) lblNgrokStatus.Invoke(new Action(() => UpdateNgrokUIStop())); else UpdateNgrokUIStop();
            }
        }

        private void UpdateNgrokUIStop() { lblNgrokStatus.Text = "OFFLINE"; lblNgrokStatus.ForeColor = Color.Gray; lnkNgrokUrl.Text = "---"; lnkNgrokUrl.Enabled = false; btnStartNgrok.Text = "🌐 PUBLIC WEB"; btnStartNgrok.BackColor = Color.Purple; WriteLog("🌍 Đã đóng Ngrok.", Color.Orange); }
        private async Task<string> GetNgrokUrl() { try { using (var client = new HttpClient()) { var json = await client.GetStringAsync(NgrokApiUrl); var node = JsonNode.Parse(json); return node?["tunnels"]?.AsArray()?[0]?["public_url"]?.ToString() ?? ""; } } catch { return ""; } }
        private void BtnHelpNgrok_Click(object? sender, EventArgs e) { var result = MessageBox.Show("Hướng dẫn lấy Ngrok Token (Free):\n\n1. Vào dashboard.ngrok.com\n2. Đăng nhập\n3. Copy Authtoken\n\nBạn có muốn mở web ngay?", "Ngrok Help", MessageBoxButtons.YesNo, MessageBoxIcon.Information); if (result == DialogResult.Yes) OpenBrowser("https://dashboard.ngrok.com/get-started/your-authtoken"); }

        // =========================================================
        // 6. HELPER & DB
        // =========================================================
        public static string Encrypt(string plainText) { if (string.IsNullOrEmpty(plainText)) return plainText; using (Aes aes = Aes.Create()) { byte[] key = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(KeyString)); aes.Key = key; aes.GenerateIV(); byte[] iv = aes.IV; ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV); using (MemoryStream ms = new MemoryStream()) { ms.Write(iv, 0, iv.Length); using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write)) { using (StreamWriter sw = new StreamWriter(cs)) { sw.Write(plainText); } } return Convert.ToBase64String(ms.ToArray()); } } }
        public static string Decrypt(string cipherText) { if (string.IsNullOrEmpty(cipherText)) return cipherText; try { byte[] fullCipher = Convert.FromBase64String(cipherText); using (Aes aes = Aes.Create()) { byte[] key = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(KeyString)); aes.Key = key; byte[] iv = new byte[16]; Array.Copy(fullCipher, 0, iv, 0, iv.Length); aes.IV = iv; ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV); using (MemoryStream ms = new MemoryStream(fullCipher, iv.Length, fullCipher.Length - iv.Length)) { using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read)) { using (StreamReader sr = new StreamReader(cs)) { return sr.ReadToEnd(); } } } } } catch { return string.Empty; } }
        private async void BtnCheckUpdate_Click(object sender, EventArgs e) { btnCheckUpdate.Enabled = false; btnCheckUpdate.Text = "Checking..."; WriteLog("🔄 Đang kiểm tra cập nhật...", Color.Blue); try { var updater = new UpdateService(); var info = await updater.CheckForUpdatesAsync(); if (info.IsUpdateAvailable) { WriteLog($"🚀 Phát hiện bản mới: {info.LatestVersion}", Color.Magenta); var result = MessageBox.Show($"Có phiên bản mới: {info.LatestVersion}\n\n{info.Changelog}\n\nBạn có muốn cập nhật ngay?", "Update Available", MessageBoxButtons.YesNo, MessageBoxIcon.Information); if (result == DialogResult.Yes) { updater.OpenDownloadLink(info.DownloadUrl); } } else { WriteLog($"✅ Bạn đang dùng bản mới nhất ({info.CurrentVersion})", Color.Green); MessageBox.Show("Phần mềm đang là phiên bản mới nhất!", "Thông báo"); } } catch (Exception ex) { WriteLog($"❌ Lỗi Update: {ex.Message}", Color.Red); MessageBox.Show("Lỗi kiểm tra cập nhật: " + ex.Message); } finally { btnCheckUpdate.Enabled = true; btnCheckUpdate.Text = "🔄 UPDATE"; } }
        private void BuildMySQLPanel() { pnlMySQL = new Panel { Location = new Point(15, 65), Size = new Size(830, 140), BackColor = Color.AliceBlue, BorderStyle = BorderStyle.FixedSingle, Visible = false }; Label lblHost = new Label { Text = "Server Host:", Location = new Point(10, 35), AutoSize = true }; cboMySQLHost = new ComboBox { Location = new Point(115, 32), Size = new Size(200, 28) }; cboMySQLHost.Items.Add("localhost"); cboMySQLHost.TextChanged += (s, e) => GenerateConnectionString(); Label lblPort = new Label { Text = "Port:", Location = new Point(330, 35), AutoSize = true }; txtMySQLPort = new TextBox { Text = "3306", Location = new Point(370, 32), Size = new Size(60, 28) }; txtMySQLPort.TextChanged += (s, e) => GenerateConnectionString(); Button btnScan = new Button { Text = "🔄 Quét Local", Location = new Point(440, 31), Size = new Size(100, 30), BackColor = Color.White }; btnScan.Click += (s, e) => ScanServices("mysql"); Label lblDb = new Label { Text = "Database:", Location = new Point(10, 75), AutoSize = true }; txtMySQLDb = new TextBox { Text = "ProductionDB", Location = new Point(115, 72), Size = new Size(150, 28) }; txtMySQLDb.TextChanged += (s, e) => GenerateConnectionString(); Label lblUser = new Label { Text = "User:", Location = new Point(280, 75), AutoSize = true }; txtMySQLUser = new TextBox { Text = "root", Location = new Point(325, 72), Size = new Size(100, 28) }; txtMySQLUser.TextChanged += (s, e) => GenerateConnectionString(); Label lblPass = new Label { Text = "Pass:", Location = new Point(440, 75), AutoSize = true }; txtMySQLPass = new TextBox { Location = new Point(480, 72), Size = new Size(150, 28), UseSystemPasswordChar = true }; txtMySQLPass.TextChanged += (s, e) => GenerateConnectionString(); pnlMySQL.Controls.AddRange(new Control[] { lblHost, cboMySQLHost, lblPort, txtMySQLPort, btnScan, lblDb, txtMySQLDb, lblUser, txtMySQLUser, lblPass, txtMySQLPass }); }
        private void BuildSQLServerPanel() { pnlSQL = new Panel { Location = new Point(15, 65), Size = new Size(830, 140), BackColor = Color.LavenderBlush, BorderStyle = BorderStyle.FixedSingle, Visible = false }; Label lblServer = new Label { Text = "Server Name:", Location = new Point(10, 35), AutoSize = true }; cboSQLServer = new ComboBox { Location = new Point(115, 32), Size = new Size(300, 28) }; cboSQLServer.Items.Add(".\\SQLEXPRESS"); cboSQLServer.Items.Add("."); cboSQLServer.TextChanged += (s, e) => GenerateConnectionString(); Button btnScan = new Button { Text = "🔄 Quét Local", Location = new Point(430, 31), Size = new Size(100, 30), BackColor = Color.White }; btnScan.Click += (s, e) => ScanServices("sql"); Label lblDb = new Label { Text = "Database:", Location = new Point(10, 75), AutoSize = true }; txtSQLDb = new TextBox { Text = "ProductionDB", Location = new Point(115, 72), Size = new Size(150, 28) }; txtSQLDb.TextChanged += (s, e) => GenerateConnectionString(); chkSQLWindowsAuth = new CheckBox { Text = "Windows Authentication", Location = new Point(280, 75), AutoSize = true, Checked = true }; chkSQLWindowsAuth.CheckedChanged += (s, e) => { txtSQLUser.Enabled = !chkSQLWindowsAuth.Checked; txtSQLPass.Enabled = !chkSQLWindowsAuth.Checked; GenerateConnectionString(); }; Label lblUser = new Label { Text = "User:", Location = new Point(10, 110), AutoSize = true }; txtSQLUser = new TextBox { Text = "sa", Location = new Point(115, 107), Size = new Size(100, 28), Enabled = false }; txtSQLUser.TextChanged += (s, e) => GenerateConnectionString(); Label lblPass = new Label { Text = "Pass:", Location = new Point(230, 110), AutoSize = true }; txtSQLPass = new TextBox { Location = new Point(280, 107), Size = new Size(150, 28), UseSystemPasswordChar = true, Enabled = false }; txtSQLPass.TextChanged += (s, e) => GenerateConnectionString(); pnlSQL.Controls.AddRange(new Control[] { lblServer, cboSQLServer, btnScan, lblDb, txtSQLDb, chkSQLWindowsAuth, lblUser, txtSQLUser, lblPass, txtSQLPass }); }
        private void CboProvider_SelectedIndexChanged(object? sender, EventArgs e) { string provider = cboProvider.SelectedItem?.ToString() ?? "SQLServer"; pnlMySQL.Visible = (provider == "MySQL"); pnlSQL.Visible = (provider == "SQLServer"); if (provider == "SQLite") txtConnectionString.Text = "Data Source=ProductionDB.db"; else { if (string.IsNullOrWhiteSpace(txtConnectionString.Text) || txtConnectionString.Text.Contains("Data Source=ProductionDB.db")) { ScanServices(provider == "MySQL" ? "mysql" : "sql"); GenerateConnectionString(); } } }
        private void GenerateConnectionString() { string provider = cboProvider.SelectedItem?.ToString(); if (provider == "MySQL") { string host = string.IsNullOrWhiteSpace(cboMySQLHost.Text) ? "localhost" : cboMySQLHost.Text; txtConnectionString.Text = $"Server={host};Port={txtMySQLPort.Text};Database={txtMySQLDb.Text};User={txtMySQLUser.Text};Password={txtMySQLPass.Text};"; } else if (provider == "SQLServer") { string server = string.IsNullOrWhiteSpace(cboSQLServer.Text) ? "." : cboSQLServer.Text; string db = txtSQLDb.Text; if (chkSQLWindowsAuth.Checked) txtConnectionString.Text = $"Server={server};Database={db};Trusted_Connection=True;TrustServerCertificate=True;"; else txtConnectionString.Text = $"Server={server};Database={db};User Id={txtSQLUser.Text};Password={txtSQLPass.Text};TrustServerCertificate=True;"; } }
        private void ScanServices(string type) { ComboBox targetCbo = (type == "mysql") ? cboMySQLHost : cboSQLServer; targetCbo.Items.Clear(); if (type == "mysql") { targetCbo.Items.Add("localhost"); targetCbo.Items.Add("127.0.0.1"); } else { targetCbo.Items.Add("."); targetCbo.Items.Add("(local)"); } try { ServiceController[] services = ServiceController.GetServices(); foreach (var service in services) { string name = service.ServiceName.ToLower(); if (type == "mysql" && (name.Contains("mysql") || name.Contains("mariadb"))) { if (!targetCbo.Items.Contains("localhost")) targetCbo.Items.Insert(0, "localhost"); } if (type == "sql") { if (name == "mssqlserver") { if (!targetCbo.Items.Contains("localhost")) targetCbo.Items.Insert(0, "localhost"); } else if (name.StartsWith("mssql$")) { string instanceName = ".\\" + service.ServiceName.Substring(6); if (!targetCbo.Items.Contains(instanceName)) targetCbo.Items.Insert(0, instanceName); } } } targetCbo.SelectedIndex = 0; } catch { targetCbo.SelectedIndex = 0; } }
        private void InitializeSystemTray() { trayMenu = new ContextMenuStrip(); trayMenu.Items.Add("Mở giao diện", null, (s, e) => ShowApp()); trayMenu.Items.Add("-"); trayMenu.Items.Add("Thoát hoàn toàn", null, (s, e) => ExitApp()); trayIcon = new NotifyIcon(); trayIcon.Text = "Production Manager Server"; trayIcon.Icon = GetAppIcon(); trayIcon.ContextMenuStrip = trayMenu; trayIcon.Visible = true; trayIcon.DoubleClick += (s, e) => ShowApp(); }
        private void ShowApp() { this.Show(); this.WindowState = FormWindowState.Normal; this.Activate(); }
        private void ExitApp() { _isRealExit = true; this.Close(); }
        private async void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (!_isRealExit)
            {
                e.Cancel = true;
                this.Hide();
                trayIcon.ShowBalloonTip(2000, "WEB SERVER", "Đang chạy nền...\nClick icon để mở lại", ToolTipIcon.Info);
            }
            else
            {
                // 1. Hủy sự kiện đóng form ngay lập tức để không bị treo UI
                e.Cancel = true;
                this.Enabled = false; // Khóa giao diện
                WriteLog("Đang dọn dẹp và tắt Server...", Color.Orange);

                // 2. Chạy dọn dẹp trên luồng nền
                await Task.Run(async () =>
                {
                    StopNgrok();
                    StopOllama();
                    StopN8n();
                    if (_webHost != null)
                    {
                        await _webHost.StopAsync();
                        _webHost.Dispose();
                    }
                });

                // 3. Xóa Tray và Ép đóng hoàn toàn tiến trình
                trayIcon.Dispose();
                Environment.Exit(0);
            }
        }
        private void MainForm_Resize(object? sender, EventArgs e) { if (this.WindowState == FormWindowState.Minimized) this.Hide(); }
        private void ChkStartWithWin_CheckedChanged(object? sender, EventArgs e) { try { RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true); if (chkStartWithWin.Checked) rk.SetValue(AppName, Application.ExecutablePath); else rk.DeleteValue(AppName, false); } catch { } }
        private void WriteLog(string msg, Color color) { if (rtbLog.InvokeRequired) { rtbLog.Invoke(() => WriteLog(msg, color)); return; } rtbLog.SelectionStart = rtbLog.TextLength; rtbLog.SelectionLength = 0; rtbLog.SelectionColor = color; rtbLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {msg}\r\n"); rtbLog.ScrollToCaret(); }
        private void OpenBrowser(string url) { try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); } catch { } }
        private Icon GetAppIcon() { try { if (File.Exists("app.ico")) return new Icon("app.ico"); } catch { } return SystemIcons.Application; }
        private void ResetUI() { btnStart.Enabled = true; btnStop.Enabled = false; grpConfig.Enabled = true; grpNotify.Enabled = true; lblStatus.Text = "STOPPED"; lblStatus.ForeColor = Color.Red; lnkUrl.Enabled = false; btnStartNgrok.Enabled = false; }
        private void OpenFirewallPort(int port, string protocol = "TCP") { try { if (!IsRunningAsAdmin()) { WriteLog($"⚠️ Không thể mở Port {port} {protocol} do thiếu quyền Admin.", Color.Orange); return; } string ruleName = $"ProductionManager_Server_{protocol}_{port}"; var psiDelete = new ProcessStartInfo("netsh", $"advfirewall firewall delete rule name=\"{ruleName}\" protocol={protocol} localport={port}") { CreateNoWindow = true, UseShellExecute = false }; Process.Start(psiDelete)?.WaitForExit(); var psiAdd = new ProcessStartInfo("netsh", $"advfirewall firewall add rule name=\"{ruleName}\" dir=in action=allow protocol={protocol} localport={port}") { CreateNoWindow = true, UseShellExecute = false }; Process.Start(psiAdd)?.WaitForExit(); WriteLog($"🔓 Đã mở Port {port} ({protocol}) trên Firewall.", Color.LimeGreen); } catch (Exception ex) { WriteLog($"❌ Lỗi Firewall {protocol}: {ex.Message}", Color.Red); } }
        private bool IsRunningAsAdmin() { var identity = WindowsIdentity.GetCurrent(); var principal = new WindowsPrincipal(identity); return principal.IsInRole(WindowsBuiltInRole.Administrator); }
        private async Task FindAndConnectServer() { WriteLog("📡 Đang quét mạng LAN...", Color.Yellow); string serverIp = await ScanForServerIp(); if (!string.IsNullOrEmpty(serverIp)) { WriteLog($"✅ Server tại IP: {serverIp}", Color.LimeGreen); OpenBrowser($"http://{serverIp}:{WEB_PORT}"); } else { WriteLog("❌ Không tìm thấy Server.", Color.Red); MessageBox.Show("Không tìm thấy Server!", "Thất bại"); } }
        private async Task<string> ScanForServerIp() { using (var udpClient = new UdpClient()) { udpClient.EnableBroadcast = true; udpClient.Client.ReceiveTimeout = 2000; try { var requestData = Encoding.UTF8.GetBytes(REQUEST_MSG); var broadcastEp = new IPEndPoint(IPAddress.Broadcast, DISCOVERY_PORT); await udpClient.SendAsync(requestData, requestData.Length, broadcastEp); var startTime = DateTime.Now; while ((DateTime.Now - startTime).TotalMilliseconds < 2000) { if (udpClient.Available > 0) { var result = await udpClient.ReceiveAsync(); string message = Encoding.UTF8.GetString(result.Buffer); if (message == RESPONSE_MSG) return result.RemoteEndPoint.Address.ToString(); } await Task.Delay(100); } } catch { } } return null; }
        private bool IsCommandAvailable(string command, string arguments) { try { string fileName = command; if (command == "node") { string defaultPath = @"C:\Program Files\nodejs\node.exe"; if (File.Exists(defaultPath)) { fileName = defaultPath; } } var psi = new ProcessStartInfo { FileName = fileName, Arguments = arguments, CreateNoWindow = true, UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true }; using (var proc = Process.Start(psi)) { if (proc == null) return false; proc.WaitForExit(3000); return proc.ExitCode == 0; } } catch { if (command == "node" && File.Exists(@"C:\Program Files\nodejs\node.exe")) return true; return false; } }
        private async Task<bool> CheckN8nPrerequisites() { if (!IsCommandAvailable("node", "-v")) { var res = MessageBox.Show("⚠️ Bạn chưa cài Node.js.\nTải về ngay?", "Thiếu Node.js", MessageBoxButtons.YesNo, MessageBoxIcon.Warning); if (res == DialogResult.Yes) OpenBrowser("https://nodejs.org/dist/v24.13.0/node-v24.13.0-x64.msi"); return false; } if (!IsCommandAvailable("cmd", "/c n8n --version")) { var res = MessageBox.Show("✅ Đã cài Node.js, chưa cài n8n!\nTự động cài đặt N8N?", "Cài N8N", MessageBoxButtons.YesNo, MessageBoxIcon.Question); if (res == DialogResult.Yes) { await InstallN8nAuto(); return true; } return false; } return true; }
        private async Task InstallN8nAuto() { btnStart.Enabled = false; WriteLog("⏳ Bắt đầu tải và cài đặt N8N...", Color.Magenta); WriteLog("👉 Lưu ý: Quá trình này có thể mất 10-25 phút tùy mạng. Vui lòng KHÔNG TẮT ứng dụng.", Color.Orange); await Task.Run(() => { try { var psi = new ProcessStartInfo { FileName = "cmd", Arguments = "/c npm install n8n -g --verbose", UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true, StandardOutputEncoding = Encoding.UTF8, StandardErrorEncoding = Encoding.UTF8 }; using (var proc = new Process { StartInfo = psi }) { proc.OutputDataReceived += (s, e) => { if (!string.IsNullOrEmpty(e.Data)) WriteLog($"[npm] {e.Data}", Color.Gray); }; proc.ErrorDataReceived += (s, e) => { if (!string.IsNullOrEmpty(e.Data)) WriteLog($"[npm] {e.Data}", Color.Gray); }; proc.Start(); proc.BeginOutputReadLine(); proc.BeginErrorReadLine(); proc.WaitForExit(); if (proc.ExitCode == 0) { WriteLog("✅ Cài đặt n8n HOÀN TẤT! Bạn có thể Start Server ngay.", Color.LimeGreen); MessageBox.Show("Cài đặt thành công n8n!\nHệ thống đã sẵn sàng.", "Thành công"); } else { WriteLog("❌ Cài đặt thất bại. Vui lòng kiểm tra log ở trên.", Color.Red); MessageBox.Show("Có lỗi xảy ra trong quá trình cài đặt.\nXem chi tiết trong khung Log.", "Thất bại", MessageBoxButtons.OK, MessageBoxIcon.Error); } } } catch (Exception ex) { WriteLog($"❌ Lỗi hệ thống: {ex.Message}", Color.Red); } }); this.Invoke(() => btnStart.Enabled = true); }
    }

    public class ServerDiscoveryService : BackgroundService
    {
        private const int DISCOVERY_PORT = 9999; private const string REQUEST_MSG = "WHERE_ARE_YOU"; private const string RESPONSE_MSG = "I_AM_SERVER";
        protected override async Task ExecuteAsync(CancellationToken stoppingToken) { using var udpServer = new UdpClient(DISCOVERY_PORT); udpServer.EnableBroadcast = true; while (!stoppingToken.IsCancellationRequested) { try { var result = await udpServer.ReceiveAsync(stoppingToken); if (Encoding.UTF8.GetString(result.Buffer) == REQUEST_MSG) { var responseData = Encoding.UTF8.GetBytes(RESPONSE_MSG); await udpServer.SendAsync(responseData, responseData.Length, result.RemoteEndPoint); } } catch { } } }
    }
}