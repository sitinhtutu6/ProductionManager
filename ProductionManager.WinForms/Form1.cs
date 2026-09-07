using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClientLauncher
{
    public partial class Form1 : Form
    {
        // CẤU HÌNH KHỚP VỚI SERVER
        private const int DISCOVERY_PORT = 9999; // Cổng tìm kiếm (UDP)
        private const int WEB_PORT = 7078;       // Cổng Web (TCP)
        private const string REQUEST_MSG = "WHERE_ARE_YOU";
        private const string RESPONSE_MSG = "I_AM_SERVER";

        private Button btnConnect;
        private Label lblStatus;
        private ProgressBar progressBar;

        public Form1()
        {
            InitializeUI();
            // Tự động tìm ngay khi mở App
            this.Load += async (s, e) => await FindAndConnectServer();
        }

        private void InitializeUI()
        {
            this.Text = "Đang tìm Server...";
            this.Size = new System.Drawing.Size(400, 200);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            lblStatus = new Label { Text = "Đang quét mạng LAN...", AutoSize = false, TextAlign = ContentAlignment.MiddleCenter, Dock = DockStyle.Top, Height = 50, Font = new System.Drawing.Font("Segoe UI", 12) };
            
            progressBar = new ProgressBar { Style = ProgressBarStyle.Marquee, Dock = DockStyle.Top, Height = 20 };
            
            btnConnect = new Button { Text = "Thử lại", Dock = DockStyle.Bottom, Height = 40, Enabled = false };
            btnConnect.Click += async (s, e) => await FindAndConnectServer();

            this.Controls.Add(progressBar);
            this.Controls.Add(lblStatus);
            this.Controls.Add(btnConnect);
        }

        private async Task FindAndConnectServer()
        {
            btnConnect.Enabled = false;
            progressBar.Style = ProgressBarStyle.Marquee;
            lblStatus.Text = "📡 Đang tìm Server trong mạng nội bộ...";
            lblStatus.ForeColor = System.Drawing.Color.Black;

            string serverIp = await ScanForServerIp();

            if (!string.IsNullOrEmpty(serverIp))
            {
                lblStatus.Text = $"✅ Đã thấy Server: {serverIp}";
                lblStatus.ForeColor = System.Drawing.Color.Green;
                progressBar.Style = ProgressBarStyle.Blocks;
                progressBar.Value = 100;

                // Tự động mở trình duyệt
                string url = $"http://{serverIp}:{WEB_PORT}";
                try
                {
                    Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
                    // Tự động đóng Tool sau 3 giây
                    await Task.Delay(5000);
                    this.Close();
                }
                catch 
                { 
                    MessageBox.Show($"Vui lòng mở trình duyệt và truy cập:\n{url}", "Thành công"); 
                }
            }
            else
            {
                lblStatus.Text = "❌ Không tìm thấy Server!";
                lblStatus.ForeColor = System.Drawing.Color.Red;
                progressBar.Style = ProgressBarStyle.Blocks;
                progressBar.Value = 0;
                btnConnect.Enabled = true;
                btnConnect.Text = "QUÉT LẠI";
            }
        }

        private async Task<string> ScanForServerIp()
        {
            using (var udpClient = new UdpClient())
            {
                udpClient.EnableBroadcast = true;
                udpClient.Client.ReceiveTimeout = 2000; // Chờ tối đa 2 giây

                try
                {
                    // 1. Gửi tín hiệu hỏi
                    var requestData = Encoding.UTF8.GetBytes(REQUEST_MSG);
                    var broadcastEp = new IPEndPoint(IPAddress.Broadcast, DISCOVERY_PORT);
                    await udpClient.SendAsync(requestData, requestData.Length, broadcastEp);

                    // 2. Chờ Server trả lời
                    // Dùng vòng lặp chờ ngắn để tránh treo UI
                    var startTime = DateTime.Now;
                    while ((DateTime.Now - startTime).TotalMilliseconds < 2000)
                    {
                        if (udpClient.Available > 0)
                        {
                            var result = await udpClient.ReceiveAsync();
                            string message = Encoding.UTF8.GetString(result.Buffer);

                            if (message == RESPONSE_MSG)
                            {
                                return result.RemoteEndPoint.Address.ToString();
                            }
                        }
                        await Task.Delay(100);
                    }
                }
                catch
                {
                    // Bỏ qua lỗi mạng
                }
            }
            return null;
        }
    }
}