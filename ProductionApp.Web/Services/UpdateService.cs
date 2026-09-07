using System.Diagnostics;
using System.Reflection;
using System.Net.Http.Headers;
using System.Text.Json;

namespace ProductionManager.WinForms
{
    public class UpdateService
    {
        // 🔥 CẤU HÌNH: Sửa lại thông tin GitHub của bạn tại đây
        // Ví dụ link repo là: https://github.com/nguyenvana/QuanLySanXuat
        private const string Owner = "sitinhtutu6"; // Ví dụ: nguyenvana
        private const string Repo = "ProductionManageUpdates";   // Ví dụ: QuanLySanXuat

        // Class hứng dữ liệu JSON từ GitHub API
        public class GitHubRelease
        {
            public string tag_name { get; set; } // Tên version (vd: v1.0.2)
            public string html_url { get; set; } // Link xem trên web
            public string body { get; set; }     // Nội dung ghi chú update
            public List<GitHubAsset> assets { get; set; } // File đính kèm
        }

        public class GitHubAsset
        {
            public string name { get; set; }             // Tên file (vd: Setup.exe)
            public string browser_download_url { get; set; } // Link tải trực tiếp
        }

        public class UpdateInfo
        {
            public bool IsUpdateAvailable { get; set; }
            public string CurrentVersion { get; set; }
            public string LatestVersion { get; set; }
            public string DownloadUrl { get; set; }
            public string Changelog { get; set; }
        }

        /// <summary>
        /// Hàm chính: Kiểm tra phiên bản mới
        /// </summary>
        public async Task<UpdateInfo> CheckForUpdatesAsync()
        {
            var result = new UpdateInfo();

            // 1. Lấy Version hiện tại của phần mềm đang chạy
            Version currentVer = Assembly.GetExecutingAssembly().GetName().Version;
            result.CurrentVersion = $"{currentVer.Major}.{currentVer.Minor}.{currentVer.Build}";

            try
            {
                // 2. Gọi API GitHub để lấy Release mới nhất
                using (var client = new HttpClient())
                {
                    // GitHub bắt buộc phải có User-Agent, nếu không sẽ báo lỗi 403
                    client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("ProductionManager", "1.3.0"));

                    string url = $"https://api.github.com/repos/{Owner}/{Repo}/releases/latest";
                    var response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var release = JsonSerializer.Deserialize<GitHubRelease>(json);

                        if (release != null)
                        {
                            // 3. Xử lý Version từ GitHub (Thường có chữ 'v' ở đầu, vd: v1.0.5 -> 1.0.5)
                            string cleanTagName = release.tag_name.TrimStart('v', 'V');

                            if (Version.TryParse(cleanTagName, out Version latestVer))
                            {
                                result.LatestVersion = $"{latestVer.Major}.{latestVer.Minor}.{latestVer.Build}";
                                result.Changelog = release.body;

                                // 4. So sánh: Nếu GitHub > Local => Có Update
                                if (latestVer > currentVer)
                                {
                                    result.IsUpdateAvailable = true;

                                    // Tìm link tải file .exe hoặc .msi
                                    var asset = release.assets.FirstOrDefault(a => a.name.EndsWith(".exe") || a.name.EndsWith(".msi"));
                                    result.DownloadUrl = asset != null ? asset.browser_download_url : release.html_url;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Lỗi Check Update: " + ex.Message);
            }

            return result;
        }

        /// <summary>
        /// Mở trình duyệt để tải file
        /// </summary>
        public void OpenDownloadLink(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch { }
        }
    }
}