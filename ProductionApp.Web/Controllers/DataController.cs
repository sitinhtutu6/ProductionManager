using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductionManager.Data;
using Microsoft.Data.Sqlite;
using MySql.Data.MySqlClient;
using System.Data.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.SqlClient;

namespace ProductionApp.Web.Controllers
{
    // Class hỗ trợ truyền dữ liệu Lịch sử Backup ra View
    public class BackupFileInfo
    {
        public string FileName { get; set; }
        public DateTime CreatedDate { get; set; }
        public double SizeMB { get; set; }
    }

    [Authorize(Roles = "Admin")]
    public class DataController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public DataController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // ==========================================
        // 1. GIAO DIỆN CHÍNH (Đã tích hợp load lịch sử từ App_Data)
        // ==========================================
        public IActionResult Index()
        {
            // Trỏ đường dẫn tuyệt đối an toàn vào App_Data
            var backupFolder = Path.Combine(_env.ContentRootPath, "App_Data", "backups", "mysql");
            var backupFiles = new List<BackupFileInfo>();

            if (Directory.Exists(backupFolder))
            {
                var files = new DirectoryInfo(backupFolder).GetFiles("*.sql");
                backupFiles = files.Select(f => new BackupFileInfo
                {
                    FileName = f.Name,
                    CreatedDate = f.CreationTime,
                    SizeMB = Math.Round((double)f.Length / (1024 * 1024), 2)
                })
                .OrderByDescending(f => f.CreatedDate) // File mới nhất lên trên
                .ToList();
            }

            ViewBag.BackupHistory = backupFiles;
            return View();
        }

        // ==========================================
        // 2. TẢI FILE EXCEL MẪU (TEMPLATE)
        // ==========================================
        public IActionResult DownloadTemplate(string type)
        {
            using var workbook = new XLWorkbook();
            var fileName = "";

            if (type == "Customer")
            {
                var ws = workbook.Worksheets.Add("Khách hàng");
                ws.Cell(1, 1).Value = "Mã KH";
                ws.Cell(1, 2).Value = "Tên Công ty";
                ws.Cell(1, 3).Value = "Địa chỉ";
                ws.Cell(1, 4).Value = "Điện thoại";

                ws.Cell(2, 1).Value = "KH_MAU_01";
                ws.Cell(2, 2).Value = "Công ty TNHH Ví Dụ";
                ws.Cell(2, 3).Value = "123 Đường ABC, TP.HCM";
                ws.Cell(2, 4).Value = "0909123456";

                fileName = "Mau_Nhap_KhachHang.xlsx";
            }
            else if (type == "Product")
            {
                var ws = workbook.Worksheets.Add("Sản phẩm");
                ws.Cell(1, 1).Value = "Mã SP";
                ws.Cell(1, 2).Value = "Tên Sản phẩm";
                ws.Cell(1, 3).Value = "Giá bán";
                ws.Cell(1, 4).Value = "Vật liệu";
                ws.Cell(1, 5).Value = "ĐVT";

                ws.Cell(2, 1).Value = "SP_MAU_01";
                ws.Cell(2, 2).Value = "Bàn Học Sinh";
                ws.Cell(2, 3).Value = 1500000;
                ws.Cell(2, 4).Value = "Gỗ Cao Su";
                ws.Cell(2, 5).Value = "Cái";

                fileName = "Mau_Nhap_SanPham.xlsx";
            }

            var header = workbook.Worksheets.First().Row(1);
            header.Style.Font.Bold = true;
            header.Style.Fill.BackgroundColor = XLColor.Yellow;
            workbook.Worksheets.First().Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        // ==========================================
        // 3. EXPORT EXCEL (XUẤT DỮ LIỆU RA FILE)
        // ==========================================
        public async Task<IActionResult> Export(string type)
        {
            using var workbook = new XLWorkbook();
            var fileName = "";

            if (type == "Product")
            {
                var data = await _context.Products.ToListAsync();
                var ws = workbook.Worksheets.Add("Sản phẩm");

                ws.Cell(1, 1).Value = "Mã SP";
                ws.Cell(1, 2).Value = "Tên Sản phẩm";
                ws.Cell(1, 3).Value = "Giá bán";
                ws.Cell(1, 4).Value = "Vật liệu";
                ws.Cell(1, 5).Value = "ĐVT";
                ws.Row(1).Style.Font.Bold = true;

                for (int i = 0; i < data.Count; i++)
                {
                    ws.Cell(i + 2, 1).Value = data[i].ProductCode;
                    ws.Cell(i + 2, 2).Value = data[i].ProductName;
                    ws.Cell(i + 2, 3).Value = data[i].DefaultPrice;
                    ws.Cell(i + 2, 4).Value = data[i].MaterialType;
                    ws.Cell(i + 2, 5).Value = data[i].Unit;
                }
                fileName = $"Sanpham_{DateTime.Now:ddMMyyyy}.xlsx";
            }
            else if (type == "Customer")
            {
                var data = await _context.Customers.ToListAsync();
                var ws = workbook.Worksheets.Add("Khách hàng");

                ws.Cell(1, 1).Value = "Mã KH";
                ws.Cell(1, 2).Value = "Tên Công ty";
                ws.Cell(1, 3).Value = "Địa chỉ";
                ws.Cell(1, 4).Value = "Điện thoại";
                ws.Row(1).Style.Font.Bold = true;

                for (int i = 0; i < data.Count; i++)
                {
                    ws.Cell(i + 2, 1).Value = data[i].CustomerCode;
                    ws.Cell(i + 2, 2).Value = data[i].CompanyName;
                    ws.Cell(i + 2, 3).Value = data[i].Address;
                    ws.Cell(i + 2, 4).Value = data[i].PhoneNumber;
                }
                fileName = $"Khachhang_{DateTime.Now:ddMMyyyy}.xlsx";
            }
            else if (type == "Order")
            {
                var data = await _context.Orders.Include(o => o.Customer).ToListAsync();
                var ws = workbook.Worksheets.Add("Đơn hàng");

                ws.Cell(1, 1).Value = "Mã Đơn";
                ws.Cell(1, 2).Value = "Khách hàng";
                ws.Cell(1, 3).Value = "Ngày đặt";
                ws.Cell(1, 4).Value = "Tổng tiền";
                ws.Cell(1, 5).Value = "Trạng thái";
                ws.Row(1).Style.Font.Bold = true;

                for (int i = 0; i < data.Count; i++)
                {
                    ws.Cell(i + 2, 1).Value = data[i].OrderCode;
                    ws.Cell(i + 2, 2).Value = data[i].Customer?.CompanyName;
                    ws.Cell(i + 2, 3).Value = data[i].OrderDate;
                    ws.Cell(i + 2, 4).Value = data[i].TotalAmount;
                    ws.Cell(i + 2, 5).Value = data[i].Status.ToString();
                }
                fileName = $"Donhang_{DateTime.Now:ddMMyyyy}.xlsx";
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        // ==========================================
        // 4. IMPORT EXCEL (NHẬP DỮ LIỆU TỪ FILE)
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> Import(IFormFile file, string type)
        {
            if (file == null || file.Length == 0) return RedirectToAction(nameof(Index));

            try
            {
                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                using var workbook = new XLWorkbook(stream);
                var ws = workbook.Worksheet(1);
                var rows = ws.RangeUsed().RowsUsed().Skip(1);

                if (type == "Customer")
                {
                    foreach (var row in rows)
                    {
                        var code = row.Cell(1).Value.ToString();
                        if (!_context.Customers.Any(c => c.CustomerCode == code))
                        {
                            var cus = new Customer
                            {
                                CustomerCode = code,
                                CompanyName = row.Cell(2).Value.ToString(),
                                Address = row.Cell(3).Value.ToString(),
                                PhoneNumber = row.Cell(4).Value.ToString()
                            };
                            _context.Customers.Add(cus);
                        }
                    }
                }
                else if (type == "Product")
                {
                    foreach (var row in rows)
                    {
                        var code = row.Cell(1).Value.ToString();
                        if (!_context.Products.Any(p => p.ProductCode == code))
                        {
                            var prod = new Product
                            {
                                ProductCode = code,
                                ProductName = row.Cell(2).Value.ToString(),
                                DefaultPrice = decimal.TryParse(row.Cell(3).Value.ToString(), out var p) ? p : 0,
                                MaterialType = row.Cell(4).Value.ToString(),
                                Unit = row.Cell(5).Value.ToString()
                            };
                            _context.Products.Add(prod);
                        }
                    }
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = $"Đã nhập dữ liệu {type} thành công!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi nhập file: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // 5. TẠO BACKUP LƯU XUỐNG APP_DATA
        // ==========================================
        public IActionResult Backup()
        {
            var provider = _context.Database.ProviderName;

            try
            {
                // --- TRƯỜNG HỢP 1: SQLITE ---
                if (provider.Contains("Sqlite"))
                {
                    var connectionString = _context.Database.GetConnectionString();
                    var builder = new SqliteConnectionStringBuilder(connectionString);
                    var dbPath = Path.Combine(_env.ContentRootPath, builder.DataSource);

                    if (!System.IO.File.Exists(dbPath)) return NotFound("Không tìm thấy file Database.");

                    var fileName = $"Backup_SQLite_{DateTime.Now:yyyyMMdd_HHmmss}.db";

                    using (var fs = new FileStream(dbPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        using (var ms = new MemoryStream())
                        {
                            fs.CopyTo(ms);
                            return File(ms.ToArray(), "application/x-sqlite3", fileName);
                        }
                    }
                }
                // --- TRƯỜNG HỢP 2: MYSQL ---
                else if (provider.Contains("MySql") || provider.Contains("Pomelo"))
                {
                    var connectionString = _context.Database.GetConnectionString();
                    var fileName = $"Backup_MySQL_{DateTime.Now:yyyyMMdd_HHmmss}.sql";

                    // Tạo folder App_Data an toàn
                    var backupFolder = Path.Combine(_env.ContentRootPath, "App_Data", "backups", "mysql");
                    if (!Directory.Exists(backupFolder)) Directory.CreateDirectory(backupFolder);

                    var filePath = Path.Combine(backupFolder, fileName);

                    // Lưu trực tiếp file .sql vào ổ cứng máy chủ thay vì trả về RAM
                    using (var conn = new MySqlConnection(connectionString))
                    {
                        using (var cmd = new MySqlCommand())
                        {
                            using (var mb = new MySqlBackup(cmd))
                            {
                                cmd.Connection = conn;
                                conn.Open();

                                // Cấu hình chuẩn mực: Backup CẢ CẤU TRÚC lẫn DỮ LIỆU
                                mb.ExportInfo.ExportTableStructure = true;
                                mb.ExportInfo.AddDropTable = true;

                                mb.ExportToFile(filePath);
                            }
                        }
                    }

                    TempData["Success"] = $"Tạo bản sao lưu thành công: {fileName}";
                    return RedirectToAction(nameof(Index));
                }
                // --- TRƯỜNG HỢP 3: SQL SERVER ---
                else if (provider.Contains("SqlServer"))
                {
                    var connectionString = _context.Database.GetConnectionString();
                    var dbName = new SqlConnectionStringBuilder(connectionString).InitialCatalog;
                    var fileName = $"Backup_MSSQL_{DateTime.Now:yyyyMMdd_HHmmss}.bak";

                    var backupPath = Path.Combine(_env.ContentRootPath, "wwwroot", "backups", fileName);
                    Directory.CreateDirectory(Path.GetDirectoryName(backupPath));

                    var sql = $"BACKUP DATABASE [{dbName}] TO DISK = '{backupPath}' WITH FORMAT, MEDIANAME = 'Z_SQLServerBackups', NAME = 'Full Backup of {dbName}';";
                    _context.Database.ExecuteSqlRaw(sql);

                    byte[] fileBytes = System.IO.File.ReadAllBytes(backupPath);
                    System.IO.File.Delete(backupPath); // Dọn dẹp temp

                    return File(fileBytes, "application/octet-stream", fileName);
                }
                else
                {
                    TempData["Error"] = "Chức năng này hiện chỉ thiết lập cho MySQL, SQLite, hoặc SQL Server.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi Backup: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // ==========================================
        // 6. QUẢN LÝ LỊCH SỬ (RESTORE / DOWNLOAD / DELETE)
        // ==========================================

        // --- 6.1 Khôi phục thẳng từ Lịch sử trên Server (App_Data) ---
        [HttpPost]
        public IActionResult RestoreFromHistory(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return RedirectToAction(nameof(Index));

            var filePath = Path.Combine(_env.ContentRootPath, "App_Data", "backups", "mysql", fileName);
            if (!System.IO.File.Exists(filePath))
            {
                TempData["Error"] = "Không tìm thấy file backup trên Server.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var connectionString = _context.Database.GetConnectionString();
                using (var conn = new MySqlConnection(connectionString))
                {
                    using (var cmd = new MySqlCommand())
                    {
                        using (var mb = new MySqlBackup(cmd))
                        {
                            cmd.Connection = conn;
                            conn.Open();
                            mb.ImportFromFile(filePath); // Import đọc trực tiếp từ ổ đĩa
                        }
                    }
                }
                TempData["Success"] = $"Đã khôi phục dữ liệu từ bản {fileName} thành công!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi phục hồi: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // --- 6.2 Tải file Backup về máy (Vì App_Data bị khóa truy cập trực tiếp từ web) ---
        [HttpGet]
        public IActionResult DownloadBackup(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return RedirectToAction(nameof(Index));

            var filePath = Path.Combine(_env.ContentRootPath, "App_Data", "backups", "mysql", fileName);
            if (!System.IO.File.Exists(filePath))
            {
                TempData["Error"] = "Không tìm thấy file trên server.";
                return RedirectToAction(nameof(Index));
            }

            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/sql", fileName);
        }

        // --- 6.3 Xóa file Backup dọn rác ---
        [HttpPost]
        public IActionResult DeleteBackup(string fileName)
        {
            try
            {
                var filePath = Path.Combine(_env.ContentRootPath, "App_Data", "backups", "mysql", fileName);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                    TempData["Success"] = "Đã xóa bản sao lưu khỏi hệ thống.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi xóa file: " + ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // 7. PHỤC HỒI TỪ FILE UPLOAD (MÁY CÁ NHÂN)
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> Restore(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Vui lòng chọn file Backup (.sql).";
                return RedirectToAction(nameof(Index));
            }

            var provider = _context.Database.ProviderName;
            var ext = Path.GetExtension(file.FileName).ToLower();

            try
            {
                if (provider.Contains("Sqlite"))
                {
                    if (ext != ".db") { TempData["Error"] = "Đang dùng SQLite, chỉ chấp nhận file .db"; return RedirectToAction(nameof(Index)); }

                    var connectionString = _context.Database.GetConnectionString();
                    var builder = new SqliteConnectionStringBuilder(connectionString);
                    var dbPath = Path.Combine(_env.ContentRootPath, builder.DataSource);

                    SqliteConnection.ClearAllPools();
                    using (var stream = new FileStream(dbPath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    TempData["Success"] = "Phục hồi dữ liệu SQLite thành công!";
                }
                else if (provider.Contains("MySql") || provider.Contains("Pomelo"))
                {
                    if (ext != ".sql") { TempData["Error"] = "Chỉ chấp nhận file .sql"; return RedirectToAction(nameof(Index)); }

                    string scriptContent;
                    using (var reader = new StreamReader(file.OpenReadStream()))
                    {
                        scriptContent = await reader.ReadToEndAsync();
                    }

                    var connectionString = _context.Database.GetConnectionString();
                    using (var conn = new MySqlConnection(connectionString))
                    {
                        using (var cmd = new MySqlCommand())
                        {
                            using (var mb = new MySqlBackup(cmd))
                            {
                                cmd.Connection = conn;
                                conn.Open();
                                mb.ImportFromString(scriptContent);
                            }
                        }
                    }
                    TempData["Success"] = "Phục hồi dữ liệu từ file ngoài thành công!";
                }
                else
                {
                    TempData["Error"] = "Database này chưa hỗ trợ Restore tự động.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi Restore: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // 8. BÁO CÁO (REPORT) ĐỂ IN
        // ==========================================
        public async Task<IActionResult> Report()
        {
            var orders = await _context.Orders
                .Include(o => o.Customer)
                .Where(o => !o.IsLocked)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }
    }
}