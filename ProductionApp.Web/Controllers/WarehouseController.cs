using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProductionApp.Web.Constants;
using ProductionApp.Web.Models;
using ProductionManager.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ProductionApp.Web.Controllers
{
    [Authorize]
    public class WarehouseController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public WarehouseController(AppDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // ============================================================
        // 1. DANH SÁCH & API CƠ BẢN
        // ============================================================
        [Authorize(Policy = AppPermissions.Warehouse.View)]
        public async Task<IActionResult> Index()
        {
            // Lấy danh sách nhóm hàng để hiển thị thành các Tag
            ViewBag.Categories = await _context.WarehouseCategories.OrderBy(c => c.Name).ToListAsync();

            // Load vật tư (Kèm theo thông tin bảng Nhóm Hàng)
            var items = await _context.WarehouseItems
                .Include(w => w.WarehouseCategory)
                .OrderByDescending(w => w.Id)
                .ToListAsync();

            return View(items);
        }

        [HttpGet]
        public async Task<IActionResult> GetJson(int id)
        {
            var item = await _context.WarehouseItems.FindAsync(id);
            if (item == null) return NotFound();

            // Trả về CategoryId thay vì chuỗi Text cũ
            return Json(new
            {
                item.Code,
                item.Name,
                item.CategoryId,
                item.Unit,
                item.ImportUnit,
                item.ConversionRate,
                item.StockQuantity,
                item.CostPrice,
                item.Note,
                item.Image
            });
        }

        private async Task<string> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0) return null;
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "items");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }
            return uniqueFileName;
        }

        // ========================================================
        // 2. API QUẢN LÝ NHÓM HÀNG (AJAX)
        // ========================================================
        [HttpPost]
        [Authorize(Policy = AppPermissions.Warehouse.Edit)]
        public async Task<IActionResult> AddCategory(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Json(new { success = false, message = "Tên không được trống" });

            if (await _context.WarehouseCategories.AnyAsync(x => x.Name.ToLower() == name.ToLower()))
                return Json(new { success = false, message = "Nhóm hàng đã tồn tại!" });

            var cat = new WarehouseCategory { Name = name.Trim() };
            _context.WarehouseCategories.Add(cat);
            await _context.SaveChangesAsync();

            return Json(new { success = true, id = cat.Id, name = cat.Name });
        }

        [HttpPost]
        [Authorize(Policy = AppPermissions.Warehouse.Edit)]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var cat = await _context.WarehouseCategories
                .Include(c => c.WarehouseItems)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cat == null) return Json(new { success = false, message = "Không tìm thấy nhóm hàng" });

            // Ràng buộc hệ thống: Không cho xóa nếu đang có vật tư dùng nhóm này
            if (cat.WarehouseItems != null && cat.WarehouseItems.Any())
                return Json(new { success = false, message = $"Không thể xóa! Đang có {cat.WarehouseItems.Count} vật tư thuộc nhóm này." });

            _context.WarehouseCategories.Remove(cat);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        // ============================================================
        // 3. TẠO MỚI & CHỈNH SỬA VẬT TƯ
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = AppPermissions.Warehouse.Create)]
        public async Task<IActionResult> Create(WarehouseItem model, IFormFile? ImageFile)
        {
            ModelState.Remove("Id");
            ModelState.Remove("WarehouseCategory"); // Bỏ qua validate model liên kết

            if (ModelState.IsValid)
            {
                if (await _context.WarehouseItems.AnyAsync(x => x.Code == model.Code))
                    return Json(new { success = false, message = $"Mã vật tư '{model.Code}' đã tồn tại." });

                try
                {
                    if (ImageFile != null) model.Image = await UploadImage(ImageFile);

                    _context.Add(model);
                    await _context.SaveChangesAsync();

                    if (model.StockQuantity > 0)
                    {
                        _context.StockTransactions.Add(new StockTransaction
                        {
                            WarehouseItemId = model.Id,
                            Type = "Opening",
                            Quantity = model.StockQuantity,
                            CurrentStock = model.StockQuantity,
                            Price = model.CostPrice,
                            TransactionDate = DateTime.Now,
                            DocumentCode = "OPENING",
                            Receiver = "Kho",
                            Note = "Khởi tạo tồn đầu kỳ",
                            Staff = User.Identity?.Name ?? "System"
                        });
                        await _context.SaveChangesAsync();
                    }
                    return Json(new { success = true, message = "Tạo vật tư thành công!" });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
                }
            }
            return Json(new { success = false, message = "Dữ liệu không hợp lệ." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = AppPermissions.Warehouse.Edit)]
        public async Task<IActionResult> Edit(WarehouseItem model, IFormFile? ImageFile)
        {
            ModelState.Remove("WarehouseCategory");

            if (ModelState.IsValid)
            {
                var existingItem = await _context.WarehouseItems.FindAsync(model.Id);
                if (existingItem == null) return Json(new { success = false, message = "Không tìm thấy vật tư." });

                if (existingItem.Code != model.Code && await _context.WarehouseItems.AnyAsync(x => x.Code == model.Code))
                    return Json(new { success = false, message = $"Mã '{model.Code}' đã được sử dụng." });

                try
                {
                    existingItem.Code = model.Code;
                    existingItem.Name = model.Name;
                    existingItem.CategoryId = model.CategoryId; // Cập nhật Khóa ngoại Nhóm hàng
                    existingItem.Unit = model.Unit;
                    existingItem.Note = model.Note;

                    if (ImageFile != null)
                    {
                        if (!string.IsNullOrEmpty(existingItem.Image))
                        {
                            string oldPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "items", existingItem.Image);
                            if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                        }
                        existingItem.Image = await UploadImage(ImageFile);
                    }
                    _context.Update(existingItem);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Cập nhật thành công!" });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "Lỗi: " + ex.Message });
                }
            }
            return Json(new { success = false, message = "Dữ liệu nhập vào không hợp lệ." });
        }

        // ============================================================
        // 4. NHẬP / XUẤT KHO 
        // ============================================================
        [HttpGet]
        [Authorize(Policy = AppPermissions.Warehouse.Create)]
        public async Task<IActionResult> ImportExport(string type, int? poId = null)
        {
            ViewBag.Type = type;
            ViewBag.Title = type == "Import" ? "Tạo Phiếu Nhập Kho" : "Tạo Phiếu Xuất Kho";
            string prefix = type == "Import" ? "PN" : "PX";
            ViewBag.AutoCode = $"{prefix}-{DateTime.Now:yyyyMMdd}-{DateTime.Now:HHmm}";
            ViewBag.CurrentUser = User.Identity?.Name ?? "Admin";

            // Map CategoryId sang Tên để UI hiển thị dễ dàng
            var items = await _context.WarehouseItems
                .Include(x => x.WarehouseCategory)
                .Select(x => new {
                    x.Id,
                    x.Code,
                    x.Name,
                    x.Unit,
                    x.ImportUnit,
                    x.ConversionRate, 
                    Category = x.WarehouseCategory != null ? x.WarehouseCategory.Name : "Chưa phân loại",
                    x.StockQuantity,
                    x.CostPrice
                })
                .OrderBy(x => x.Code).ToListAsync();

            var activeOrders = await _context.Orders
                .Where(o => o.Status == OrderStatus.Approved || o.Status == OrderStatus.InProduction)
                .Select(o => new {
                    o.Id,
                    o.OrderCode,
                    CustomerName = o.Customer != null ? o.Customer.CompanyName : "Khách lẻ"
                })
                .OrderByDescending(o => o.Id).ToListAsync();

            ViewBag.ItemsJson = System.Text.Json.JsonSerializer.Serialize(items);
            ViewBag.OrdersJson = System.Text.Json.JsonSerializer.Serialize(activeOrders);

            // 🔥 LOGIC AUTO-FILL TỪ ĐƠN ĐẶT HÀNG (PURCHASE ORDER)
            ViewBag.PoId = poId;
            if (type == "Import" && poId.HasValue)
            {
                var po = await _context.PurchaseOrders
                    .Include(p => p.Details).ThenInclude(d => d.WarehouseItem)
                    .FirstOrDefaultAsync(p => p.Id == poId.Value);

                if (po != null)
                {
                    ViewBag.Receiver = po.SupplierName;
                    var poItems = po.Details.Select(d => new {
                        WarehouseItemId = d.WarehouseItemId,
                        Code = d.WarehouseItem.Code,
                        Name = d.WarehouseItem.Name,
                        Unit = d.WarehouseItem.Unit,
                        Quantity = (d.QuantityOrdered - d.QuantityReceived - d.QuantityBad) > 0 ? (d.QuantityOrdered - d.QuantityReceived - d.QuantityBad) : 0,

                        // 🔥 ÉP CỨNG GIÁ = 0 ĐỂ KHO KHÔNG LÀM TĂNG CÔNG NỢ (Kế toán quản lý ở PO)
                        Price = 0,

                        StockQuantity = d.WarehouseItem.StockQuantity
                    }).Where(x => x.Quantity > 0).ToList();

                    ViewBag.PoItemsJson = System.Text.Json.JsonSerializer.Serialize(poItems);
                }
            }

            ViewBag.Partners = await _context.Customers
                .Select(c => c.CompanyName)
                .Distinct()
                .ToListAsync();

            return View();
        }

        [HttpPost]
        [Authorize(Policy = AppPermissions.Warehouse.Create)]
        public async Task<IActionResult> SaveBulk([FromBody] BulkTransactionViewModel model)
        {
            if (model == null || model.Items == null || !model.Items.Any())
                return Json(new { success = false, message = "Dữ liệu trống." });

            using var transaction = _context.Database.BeginTransaction();
            try
            {
                if (string.IsNullOrEmpty(model.DocumentCode) || model.DocumentCode.Contains("XXXX"))
                {
                    string prefix = model.Type == "Import" ? "NK" : "XK";
                    model.DocumentCode = $"{prefix}-{DateTime.Now:yyMMdd}-{new Random().Next(1000, 9999)}";
                }

                foreach (var item in model.Items)
                {
                    var whItem = await _context.WarehouseItems.FindAsync(item.WarehouseItemId);
                    if (whItem == null) continue;

                    if (model.Type == "Import")
                    {
                        whItem.StockQuantity += item.Quantity;
                        if (item.Price > 0) whItem.CostPrice = item.Price;
                    }
                    else
                    {
                        whItem.StockQuantity -= item.Quantity;
                    }

                    _context.StockTransactions.Add(new StockTransaction
                    {
                        WarehouseItemId = item.WarehouseItemId,
                        Type = model.Type,
                        Quantity = item.Quantity,
                        CurrentStock = whItem.StockQuantity,
                        Price = item.Price,
                        TransactionDate = model.Date,
                        DocumentCode = model.DocumentCode,
                        Receiver = model.Receiver,
                        Note = model.Note,
                        Staff = User.Identity?.Name ?? "Hệ thống",
                        OrderId = model.OrderId,
                        Reason = model.Type == "Export" && model.Reason.HasValue ? (ExportReason)model.Reason.Value : null
                    });
                }

                // LOGIC THANH TOÁN & TRỪ ĐƠN ĐẶT HÀNG KHI NHẬP KHO
                if (model.Type == "Import")
                {
                    decimal totalAmount = model.Items.Sum(x => (decimal)x.Quantity * x.Price);
                    decimal paidAmount = model.PaymentMethod == 0 ? model.PaidAmount : totalAmount;

                    var matImport = new MaterialImport
                    {
                        ImportCode = model.DocumentCode,
                        ImportDate = model.Date,
                        SupplierName = model.Receiver,
                        TotalAmount = totalAmount,
                        PaidAmount = paidAmount,
                        PaymentStatus = paidAmount >= totalAmount ? ImportPaymentStatus.Paid : (paidAmount > 0 ? ImportPaymentStatus.Partial : ImportPaymentStatus.Unpaid)
                    };
                    _context.MaterialImports.Add(matImport);
                    await _context.SaveChangesAsync();

                    if (paidAmount > 0)
                    {
                        _context.CashEntries.Add(new CashEntry
                        {
                            TransactionDate = model.Date,
                            ReportDate = model.Date,
                            VoucherCode = $"PC-{model.DocumentCode}",
                            Type = TransactionType.Payment,
                            Category = EntryCategory.Business,
                            Amount = paidAmount,
                            TargetName = model.Receiver,
                            Description = $"Thanh toán nhập kho {model.DocumentCode}",
                            Method = model.PaymentMethod == 2 ? PaymentMethod.Transfer : PaymentMethod.Cash,
                            MaterialImportId = matImport.Id,
                            CreatedBy = User.Identity?.Name ?? "System",
                            CreatedAt = DateTime.Now
                        });
                    }

                    if (model.PurchaseOrderId.HasValue)
                    {
                        var po = await _context.PurchaseOrders.Include(p => p.Details).FirstOrDefaultAsync(p => p.Id == model.PurchaseOrderId.Value);
                        if (po != null)
                        {
                            foreach (var item in model.Items)
                            {
                                var detail = po.Details.FirstOrDefault(d => d.WarehouseItemId == item.WarehouseItemId);
                                if (detail != null)
                                {
                                    detail.QuantityReceived += item.Quantity;
                                    if (detail.QuantityReceived + detail.QuantityBad >= detail.QuantityOrdered)
                                        detail.IsFinished = true;
                                }
                            }
                            po.Status = po.Details.All(d => d.IsFinished) ? POStatus.Completed : POStatus.Partial;
                        }
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Json(new { success = true, message = "Lưu thành công!" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // ============================================================
        // 5. CHỈNH SỬA PHIẾU XUẤT/NHẬP
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> EditTransaction(string code)
        {
            var trans = await _context.StockTransactions.Where(x => x.DocumentCode == code).ToListAsync();
            if (!trans.Any()) return NotFound("Không tìm thấy phiếu.");
            var first = trans.First();
            var model = new BulkTransactionViewModel
            {
                Type = first.Type,
                DocumentCode = code,
                Receiver = first.Receiver,
                Note = first.Note,
                Date = first.TransactionDate,
                OrderId = first.OrderId,
                Reason = first.Reason.HasValue ? (int)first.Reason.Value : null,
                Items = trans.Select(x => new TransactionItemDetail { WarehouseItemId = x.WarehouseItemId, Quantity = x.Quantity, Price = x.Price }).ToList()
            };

            ViewBag.IsEditMode = true;
            ViewBag.Type = first.Type;
            ViewBag.Title = $"Sửa phiếu: {code}";
            ViewBag.AutoCode = code;

            var items = await _context.WarehouseItems.Select(x => new { x.Id, x.Code, x.Name, x.Unit, x.StockQuantity, x.CostPrice }).ToListAsync();
            var activeOrders = await _context.Orders
                .Where(o => o.Status == OrderStatus.Approved || o.Status == OrderStatus.InProduction)
                .Select(o => new {
                    o.Id,
                    o.OrderCode,
                    CustomerName = o.Customer != null ? o.Customer.CompanyName : "Khách lẻ"
                }).ToListAsync();

            ViewBag.ItemsJson = System.Text.Json.JsonSerializer.Serialize(items);
            ViewBag.OrdersJson = System.Text.Json.JsonSerializer.Serialize(activeOrders);
            ViewBag.EditDataJson = System.Text.Json.JsonSerializer.Serialize(model);

            return View("ImportExport");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateTransaction([FromBody] BulkTransactionViewModel model)
        {
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                var oldTrans = await _context.StockTransactions.Where(x => x.DocumentCode == model.DocumentCode).ToListAsync();
                if (oldTrans.Any())
                {
                    foreach (var t in oldTrans)
                    {
                        var item = await _context.WarehouseItems.FindAsync(t.WarehouseItemId);
                        if (item != null)
                        {
                            if (t.Type == "Import") item.StockQuantity -= t.Quantity;
                            else item.StockQuantity += t.Quantity;
                        }
                    }
                    _context.StockTransactions.RemoveRange(oldTrans);
                }

                foreach (var item in model.Items)
                {
                    var whItem = await _context.WarehouseItems.FindAsync(item.WarehouseItemId);
                    if (whItem != null)
                    {
                        if (model.Type == "Import") whItem.StockQuantity += item.Quantity;
                        else whItem.StockQuantity -= item.Quantity;

                        _context.StockTransactions.Add(new StockTransaction
                        {
                            WarehouseItemId = item.WarehouseItemId,
                            Type = model.Type,
                            Quantity = item.Quantity,
                            CurrentStock = whItem.StockQuantity,
                            Price = item.Price,
                            TransactionDate = model.Date,
                            DocumentCode = model.DocumentCode,
                            Receiver = model.Receiver,
                            Note = model.Note,
                            Staff = User.Identity?.Name,
                            OrderId = model.OrderId,
                            Reason = model.Type == "Export" && model.Reason.HasValue ? (ExportReason)model.Reason.Value : null
                        });
                    }
                }
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ============================================================
        // 6. LỊCH SỬ KHO (HISTORY)
        // ============================================================
        public async Task<IActionResult> History(DateTime? fromDate, DateTime? toDate, string docCode, int? orderId, string type, int? reason, string sortOrder)
        {
            var f = fromDate ?? DateTime.Today.AddDays(-30);
            var t = (toDate ?? DateTime.Today).Date.AddDays(1).AddSeconds(-1);

            var query = _context.StockTransactions
                .Include(x => x.WarehouseItem)
                .Include(x => x.Order)
                .Where(x => x.TransactionDate >= f && x.TransactionDate <= t);

            if (!string.IsNullOrEmpty(docCode)) query = query.Where(x => x.DocumentCode.Contains(docCode));
            if (orderId.HasValue) query = query.Where(x => x.OrderId == orderId);
            if (!string.IsNullOrEmpty(type)) query = query.Where(x => x.Type == type);
            if (reason.HasValue) query = query.Where(x => x.Reason == (ExportReason)reason.Value);

            ViewBag.CurrentSort = sortOrder;
            ViewBag.DateSortParm = string.IsNullOrEmpty(sortOrder) ? "date_asc" : "";
            ViewBag.TypeSortParm = sortOrder == "type_asc" ? "type_desc" : "type_asc";
            ViewBag.ItemSortParm = sortOrder == "item_asc" ? "item_desc" : "item_asc";
            ViewBag.QtySortParm = sortOrder == "qty_asc" ? "qty_desc" : "qty_asc";
            ViewBag.StockSortParm = sortOrder == "stock_asc" ? "stock_desc" : "stock_asc";

            switch (sortOrder)
            {
                case "date_asc": query = query.OrderBy(x => x.TransactionDate); break;
                case "type_asc": query = query.OrderBy(x => x.Type).ThenByDescending(x => x.TransactionDate); break;
                case "type_desc": query = query.OrderByDescending(x => x.Type).ThenByDescending(x => x.TransactionDate); break;
                case "item_asc": query = query.OrderBy(x => x.WarehouseItem.Name).ThenByDescending(x => x.TransactionDate); break;
                case "item_desc": query = query.OrderByDescending(x => x.WarehouseItem.Name).ThenByDescending(x => x.TransactionDate); break;
                case "qty_asc": query = query.OrderBy(x => x.Quantity).ThenByDescending(x => x.TransactionDate); break;
                case "qty_desc": query = query.OrderByDescending(x => x.Quantity).ThenByDescending(x => x.TransactionDate); break;
                case "stock_asc": query = query.OrderBy(x => x.CurrentStock).ThenByDescending(x => x.TransactionDate); break;
                case "stock_desc": query = query.OrderByDescending(x => x.CurrentStock).ThenByDescending(x => x.TransactionDate); break;
                default: query = query.OrderByDescending(x => x.TransactionDate); break;
            }

            var history = await query.ToListAsync();
            ViewBag.FromDate = f;
            ViewBag.ToDate = t;
            ViewBag.DocCode = docCode;
            ViewBag.OrderId = orderId;

            return View(history);
        }

        // ============================================================
        // 7. BÁO CÁO TỒN KHO THÔNG MINH
        // ============================================================
        public async Task<IActionResult> Report(DateTime? fromDate, DateTime? toDate)
        {
            var model = new StockReportViewModel { FromDate = fromDate, ToDate = toDate };
            if (fromDate == null || toDate == null) return View(model);

            var f = fromDate.Value.Date;
            var t = toDate.Value.Date.AddDays(1).AddSeconds(-1);

            // Bổ sung Include WarehouseCategory để đổ dữ liệu Phân loại
            var allItems = await _context.WarehouseItems
                .Include(x => x.WarehouseCategory)
                .OrderBy(x => x.Code).ToListAsync();

            var allTrans = await _context.StockTransactions.Where(x => x.TransactionDate <= t).ToListAsync();

            foreach (var item in allItems)
            {
                var itemTrans = allTrans.Where(x => x.WarehouseItemId == item.Id).ToList();
                var pastTrans = itemTrans.Where(x => x.TransactionDate < f).ToList();
                double openStock = 0;

                foreach (var tr in pastTrans)
                {
                    if (tr.Type == "Import" || tr.Type == "Opening") openStock += tr.Quantity;
                    else if (tr.Type == "Export") openStock -= tr.Quantity;
                    else if (tr.Type == "Adjustment") openStock = tr.CurrentStock;
                }

                var periodTrans = itemTrans.Where(x => x.TransactionDate >= f && x.TransactionDate <= t).ToList();
                double import = periodTrans.Where(x => x.Type == "Import" || x.Type == "Opening").Sum(x => x.Quantity);
                double export = periodTrans.Where(x => x.Type == "Export").Sum(x => x.Quantity);
                double closeStock = t.Date >= DateTime.Today ? item.StockQuantity : openStock + import - export;

                model.Items.Add(new StockItemReport
                {
                    Code = item.Code,
                    Name = item.Name,
                    Unit = item.Unit,
                    Category = item.WarehouseCategory?.Name ?? "Chưa phân loại", // Map từ Model Nhóm Hàng
                    OpeningStock = openStock,
                    ImportQty = import,
                    ExportQty = export,
                    ClosingStock = closeStock,
                    CostPrice = item.CostPrice
                });
            }
            model.TotalValue = model.Items.Sum(x => x.TotalValue);
            return View(model);
        }

        // ============================================================
        // 8. KIỂM KÊ KHO
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> StockTake()
        {
            var items = await _context.WarehouseItems.OrderBy(x => x.Code).ToListAsync();
            var model = new StockTakeViewModel
            {
                Date = DateTime.Now,
                Items = items.Select(x => new StockTakeItem
                {
                    WarehouseItemId = x.Id,
                    ItemCode = x.Code,
                    ItemName = x.Name,
                    SystemStock = x.StockQuantity,
                    RealStock = x.StockQuantity
                }).ToList()
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SaveBulkStockTake([FromBody] StockTakeViewModel model)
        {
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                string docCode = $"KK-{model.Date:yyyyMMdd}-{DateTime.Now:HHmm}";
                int count = 0;
                foreach (var checkItem in model.Items)
                {
                    var whItem = await _context.WarehouseItems.FindAsync(checkItem.WarehouseItemId);
                    if (whItem == null) continue;

                    double realDiff = checkItem.RealStock - whItem.StockQuantity;
                    if (Math.Abs(realDiff) > 0)
                    {
                        whItem.StockQuantity = checkItem.RealStock;
                        _context.StockTransactions.Add(new StockTransaction
                        {
                            WarehouseItemId = whItem.Id,
                            Type = "Adjustment",
                            Quantity = Math.Abs(realDiff),
                            CurrentStock = checkItem.RealStock,
                            Price = whItem.CostPrice,
                            Note = $"KK: {model.Note}. " + (realDiff > 0 ? $"Thừa {realDiff}" : $"Thiếu {Math.Abs(realDiff)}"),
                            TransactionDate = model.Date,
                            DocumentCode = docCode,
                            Receiver = "Kho",
                            Staff = User.Identity?.Name ?? "Admin"
                        });
                        count++;
                    }
                }
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Json(new { success = true, message = $"Đã điều chỉnh {count} mã." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ============================================================
        // 9. XUẤT / NHẬP EXCEL DỮ LIỆU KHO
        // ============================================================
        public async Task<IActionResult> ExportExcel()
        {
            var data = await _context.WarehouseItems
                .Include(x => x.WarehouseCategory)
                .OrderBy(x => x.Code).ToListAsync();

            using (var wb = new XLWorkbook())
            {
                var ws = wb.Worksheets.Add("TonKho");
                ws.Cell(1, 1).Value = "Mã VT";
                ws.Cell(1, 2).Value = "Tên VT";
                ws.Cell(1, 3).Value = "ĐVT";
                ws.Cell(1, 4).Value = "Tồn";
                ws.Cell(1, 5).Value = "Giá Vốn";
                ws.Cell(1, 6).Value = "Nhóm Hàng"; // Thêm cột xuất Nhóm Hàng

                int row = 2;
                foreach (var item in data)
                {
                    ws.Cell(row, 1).Value = item.Code;
                    ws.Cell(row, 2).Value = item.Name;
                    ws.Cell(row, 3).Value = item.Unit;
                    ws.Cell(row, 4).Value = item.StockQuantity;
                    ws.Cell(row, 5).Value = item.CostPrice;
                    ws.Cell(row, 6).Value = item.WarehouseCategory?.Name ?? "Chưa phân loại";
                    row++;
                }
                using (var stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "TonKho.xlsx");
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> ImportFromExcel(IFormFile file)
        {
            if (file == null || file.Length == 0) return Json(new { success = false, message = "Chọn file." });
            int success = 0;

            using var transaction = _context.Database.BeginTransaction();
            try
            {
                using (var wb = new XLWorkbook(file.OpenReadStream()))
                {
                    var rows = wb.Worksheet(1).RangeUsed().RowsUsed().Skip(1);
                    foreach (var row in rows)
                    {
                        string code = row.Cell(1).GetValue<string>()?.Trim();
                        if (string.IsNullOrEmpty(code) || await _context.WarehouseItems.AnyAsync(x => x.Code == code)) continue;

                        // 💡 THUẬT TOÁN MAP CATEGORY TỪ EXCEL: Tự động kiểm tra và thêm mới Nhóm hàng
                        string categoryName = row.Cell(6).GetValue<string>()?.Trim();
                        int? categoryId = null;

                        if (!string.IsNullOrEmpty(categoryName))
                        {
                            var cat = await _context.WarehouseCategories.FirstOrDefaultAsync(c => c.Name.ToLower() == categoryName.ToLower());
                            if (cat == null)
                            {
                                cat = new WarehouseCategory { Name = categoryName };
                                _context.WarehouseCategories.Add(cat);
                                await _context.SaveChangesAsync(); // Cần lưu ngay để lấy ID
                            }
                            categoryId = cat.Id;
                        }

                        var item = new WarehouseItem
                        {
                            Code = code,
                            Name = row.Cell(2).GetValue<string>(),
                            Unit = row.Cell(3).GetValue<string>(),
                            StockQuantity = row.Cell(4).GetValue<double>(),
                            CostPrice = row.Cell(5).GetValue<decimal>(),
                            CategoryId = categoryId // Gắn ID Khóa ngoại
                        };

                        _context.WarehouseItems.Add(item);
                        await _context.SaveChangesAsync();

                        if (item.StockQuantity > 0)
                            _context.StockTransactions.Add(new StockTransaction
                            {
                                WarehouseItemId = item.Id,
                                Type = "Opening",
                                Quantity = item.StockQuantity,
                                CurrentStock = item.StockQuantity,
                                Price = item.CostPrice,
                                TransactionDate = DateTime.Now,
                                DocumentCode = "IMPORT_EXCEL",
                                Receiver = "Kho",
                                Staff = User.Identity?.Name ?? "System"
                            });
                        success++;
                    }
                }
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Json(new { success = true, message = $"Đã nhập {success} dòng." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult DownloadTemplate()
        {
            using (var wb = new XLWorkbook())
            {
                var ws = wb.Worksheets.Add("Mau");
                ws.Cell(1, 1).Value = "Mã VT*";
                ws.Cell(1, 2).Value = "Tên VT*";
                ws.Cell(1, 3).Value = "ĐVT";
                ws.Cell(1, 4).Value = "Tồn Đầu";
                ws.Cell(1, 5).Value = "Giá Vốn";
                ws.Cell(1, 6).Value = "Phân Loại / Nhóm Hàng";

                ws.Cell(2, 1).Value = "VT-001";
                ws.Cell(2, 2).Value = "Vít";
                ws.Cell(2, 3).Value = "Cái";
                ws.Cell(2, 6).Value = "Nguyên vật liệu";

                using (var stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Mau_Import_VT.xlsx");
                }
            }
        }

        // ============================================================
        // 10. BÁO CÁO VẬT TƯ KHÁCH HÀNG CUNG CẤP (ĐÃ TÍCH HỢP BOM)
        // ============================================================
        [HttpGet]
        [Authorize(Policy = AppPermissions.Warehouse.View)]
        public async Task<IActionResult> CustomerMaterialReport(DateTime? fromDate, DateTime? toDate, int? orderId)
        {
            var f = fromDate ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var t = (toDate ?? DateTime.Now).Date.AddDays(1).AddSeconds(-1);

            var query = _context.StockTransactions
                .Include(x => x.Order).ThenInclude(o => o.Customer)
                .Include(x => x.Order).ThenInclude(o => o.OrderDetails)
                .Include(x => x.WarehouseItem)
                .Where(x => x.OrderId != null && x.TransactionDate >= f && x.TransactionDate <= t);

            if (orderId.HasValue) query = query.Where(x => x.OrderId == orderId.Value);

            var rawData = await query.ToListAsync();

            var productIds = rawData.Where(x => x.Order != null && x.Order.OrderDetails != null)
                                    .SelectMany(x => x.Order.OrderDetails)
                                    .Where(od => od.ProductId.HasValue)
                                    .Select(od => od.ProductId.Value).Distinct().ToList();

            var allBoms = await _context.ProductBoms.Where(b => productIds.Contains(b.ProductId)).ToListAsync();

            var reportData = rawData
                .GroupBy(x => new { x.OrderId, x.Order.OrderCode, CustomerName = x.Order.Customer?.CompanyName, x.WarehouseItemId, x.WarehouseItem.Code, x.WarehouseItem.Name, x.WarehouseItem.Unit })
                .Select(g => {
                    var firstTrans = g.First();
                    double requiredQty = 0;

                    if (firstTrans.Order != null && firstTrans.Order.OrderDetails != null)
                    {
                        foreach (var od in firstTrans.Order.OrderDetails.Where(d => d.ProductId.HasValue))
                        {
                            var boms = allBoms.Where(b => b.ProductId == od.ProductId.Value && b.MaterialId == g.Key.WarehouseItemId).ToList();

                            string[] nameParts = od.ProductName.Split(new[] { " - " }, StringSplitOptions.None);
                            if (nameParts.Length > 1)
                            {
                                string detailVariantName = nameParts.Last().Trim().ToLower();
                                boms = boms.Where(b => b.ComponentName != null && b.ComponentName.Trim().ToLower() == detailVariantName).ToList();
                            }

                            foreach (var b in boms)
                            {
                                requiredQty += (double)od.Quantity * b.Quantity * (b.Coefficient > 0 ? b.Coefficient : 1);
                            }
                        }
                    }

                    return new ProductionApp.Web.Models.ViewModels.CustomerMaterialReportViewModel
                    {
                        OrderId = g.Key.OrderId ?? 0,
                        OrderCode = g.Key.OrderCode,
                        CustomerName = g.Key.CustomerName ?? "Khách lẻ",
                        WarehouseItemId = g.Key.WarehouseItemId,
                        ItemCode = g.Key.Code,
                        ItemName = g.Key.Name,
                        Unit = g.Key.Unit,
                        TotalReceived = g.Where(x => x.Type == "Import" && (x.Price == 0 || (x.Receiver != null && x.Receiver.ToLower().Contains("khách")))).Sum(x => x.Quantity),
                        TotalUsed = g.Where(x => x.Type == "Export").Sum(x => x.Quantity),
                        TotalRequired = requiredQty
                    };
                })
                .Where(x => x.TotalReceived > 0 || x.TotalUsed > 0 || x.TotalRequired > 0)
                .OrderByDescending(x => x.OrderCode).ThenBy(x => x.ItemName)
                .ToList();

            var activeOrders = await _context.Orders
                .Select(o => new { o.Id, Text = $"[{o.OrderCode}] - {o.Customer.CompanyName}" })
                .ToListAsync();

            ViewBag.OrderList = new SelectList(activeOrders, "Id", "Text", orderId);
            ViewBag.FromDate = f;
            ViewBag.ToDate = toDate ?? DateTime.Now;

            return View(reportData);
        }

        [HttpGet]
        public async Task<IActionResult> ExportCustomerMaterialExcel(DateTime? fromDate, DateTime? toDate, int? orderId)
        {
            var f = fromDate ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var t = (toDate ?? DateTime.Now).Date.AddDays(1).AddSeconds(-1);

            var query = _context.StockTransactions
                .Include(x => x.Order).ThenInclude(o => o.Customer)
                .Include(x => x.Order).ThenInclude(o => o.OrderDetails)
                .Include(x => x.WarehouseItem)
                .Where(x => x.OrderId != null && x.TransactionDate >= f && x.TransactionDate <= t);

            if (orderId.HasValue) query = query.Where(x => x.OrderId == orderId.Value);

            var rawData = await query.ToListAsync();

            var productIds = rawData.Where(x => x.Order != null && x.Order.OrderDetails != null)
                                    .SelectMany(x => x.Order.OrderDetails)
                                    .Where(od => od.ProductId.HasValue)
                                    .Select(od => od.ProductId.Value).Distinct().ToList();

            var allBoms = await _context.ProductBoms.Where(b => productIds.Contains(b.ProductId)).ToListAsync();

            var reportData = rawData
                .GroupBy(x => new { x.OrderId, x.Order.OrderCode, CustomerName = x.Order.Customer?.CompanyName, x.WarehouseItemId, x.WarehouseItem.Code, x.WarehouseItem.Name, x.WarehouseItem.Unit })
                .Select(g => {
                    var firstTrans = g.First();
                    double requiredQty = 0;

                    if (firstTrans.Order != null && firstTrans.Order.OrderDetails != null)
                    {
                        foreach (var od in firstTrans.Order.OrderDetails.Where(d => d.ProductId.HasValue))
                        {
                            var boms = allBoms.Where(b => b.ProductId == od.ProductId.Value && b.MaterialId == g.Key.WarehouseItemId).ToList();

                            string[] nameParts = od.ProductName.Split(new[] { " - " }, StringSplitOptions.None);
                            if (nameParts.Length > 1)
                            {
                                string detailVariantName = nameParts.Last().Trim().ToLower();
                                boms = boms.Where(b => b.ComponentName != null && b.ComponentName.Trim().ToLower() == detailVariantName).ToList();
                            }

                            foreach (var b in boms)
                            {
                                requiredQty += (double)od.Quantity * b.Quantity * (b.Coefficient > 0 ? b.Coefficient : 1);
                            }
                        }
                    }

                    return new ProductionApp.Web.Models.ViewModels.CustomerMaterialReportViewModel
                    {
                        OrderCode = g.Key.OrderCode,
                        CustomerName = g.Key.CustomerName ?? "Khách lẻ",
                        ItemCode = g.Key.Code,
                        ItemName = g.Key.Name,
                        Unit = g.Key.Unit,
                        TotalReceived = g.Where(x => x.Type == "Import" && (x.Price == 0 || (x.Receiver != null && x.Receiver.ToLower().Contains("khách")))).Sum(x => x.Quantity),
                        TotalUsed = g.Where(x => x.Type == "Export").Sum(x => x.Quantity),
                        TotalRequired = requiredQty
                    };
                })
                .Where(x => x.TotalReceived > 0 || x.TotalUsed > 0 || x.TotalRequired > 0)
                .OrderByDescending(x => x.OrderCode).ThenBy(x => x.ItemName)
                .ToList();

            using (var wb = new XLWorkbook())
            {
                var ws = wb.Worksheets.Add("VatTuKhachGiao");
                ws.Cell(1, 1).Value = $"BÁO CÁO VẬT TƯ KHÁCH HÀNG CUNG CẤP ({f:dd/MM/yyyy} - {t:dd/MM/yyyy})";
                ws.Range("A1:I1").Merge().Style.Font.SetBold().Font.SetFontSize(14).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                var headers = new[] { "Mã Đơn", "Khách hàng", "Mã VT", "Tên Vật tư", "ĐVT", "Định mức cần (BOM)", "Khách đã giao", "Xưởng đã xuất", "Khách giao Thiếu/Dư" };
                for (int i = 0; i < headers.Length; i++) ws.Cell(3, i + 1).Value = headers[i];

                ws.Range("A3:I3").Style.Font.SetBold().Fill.SetBackgroundColor(XLColor.LightBlue).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                int row = 4;
                foreach (var item in reportData)
                {
                    ws.Cell(row, 1).Value = item.OrderCode;
                    ws.Cell(row, 2).Value = item.CustomerName;
                    ws.Cell(row, 3).Value = item.ItemCode;
                    ws.Cell(row, 4).Value = item.ItemName;
                    ws.Cell(row, 5).Value = item.Unit;
                    ws.Cell(row, 6).Value = item.TotalRequired;
                    ws.Cell(row, 7).Value = item.TotalReceived;
                    ws.Cell(row, 8).Value = item.TotalUsed;
                    ws.Cell(row, 9).Value = item.Balance;

                    if (item.Balance < 0) ws.Cell(row, 9).Style.Font.SetFontColor(XLColor.Red).Font.SetBold();
                    row++;
                }
                ws.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"VatTuKhachGiao_{DateTime.Now:ddMMyy}.xlsx");
                }
            }
        }

        // ============================================================
        // 11. BÁO CÁO NHẬP - XUẤT - TỒN (GROUP THEO CATEGORY)
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> ExportReportExcel(DateTime? fromDate, DateTime? toDate)
        {
            if (fromDate == null || toDate == null)
            {
                TempData["Error"] = "Vui lòng chọn khoảng thời gian để xuất báo cáo.";
                return RedirectToAction(nameof(Report));
            }

            var f = fromDate.Value.Date;
            var t = toDate.Value.Date.AddDays(1).AddSeconds(-1);

            // Gắn Include để kéo Phân loại ra Excel
            var allItems = await _context.WarehouseItems
                .Include(x => x.WarehouseCategory)
                .OrderBy(x => x.Code).ToListAsync();

            var allTrans = await _context.StockTransactions.Where(x => x.TransactionDate <= t).ToListAsync();

            var reportList = new List<StockItemReport>();

            foreach (var item in allItems)
            {
                var itemTrans = allTrans.Where(x => x.WarehouseItemId == item.Id).ToList();
                var pastTrans = itemTrans.Where(x => x.TransactionDate < f).ToList();
                double openStock = 0;

                foreach (var tr in pastTrans)
                {
                    if (tr.Type == "Import" || tr.Type == "Opening") openStock += tr.Quantity;
                    else if (tr.Type == "Export") openStock -= tr.Quantity;
                    else if (tr.Type == "Adjustment") openStock = tr.CurrentStock;
                }

                var periodTrans = itemTrans.Where(x => x.TransactionDate >= f && x.TransactionDate <= t).ToList();
                double import = periodTrans.Where(x => x.Type == "Import" || x.Type == "Opening").Sum(x => x.Quantity);
                double export = periodTrans.Where(x => x.Type == "Export").Sum(x => x.Quantity);
                double closeStock = t.Date >= DateTime.Today ? item.StockQuantity : openStock + import - export;

                reportList.Add(new StockItemReport
                {
                    Code = item.Code,
                    Name = item.Name,
                    Unit = item.Unit,
                    Category = item.WarehouseCategory?.Name ?? "CHƯA PHÂN LOẠI", // Ánh xạ từ DB
                    OpeningStock = openStock,
                    ImportQty = import,
                    ExportQty = export,
                    ClosingStock = closeStock,
                    CostPrice = item.CostPrice
                });
            }

            using (var wb = new XLWorkbook())
            {
                var ws = wb.Worksheets.Add("NXT_Nhom_PhanLoai");

                ws.Cell(1, 1).Value = "BÁO CÁO TỔNG HỢP NHẬP - XUẤT - TỒN KHO";
                ws.Range("A1:H1").Merge().Style.Font.SetBold().Font.SetFontSize(14).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell(2, 1).Value = $"Kỳ báo cáo: Từ {f:dd/MM/yyyy} đến {toDate.Value:dd/MM/yyyy}";
                ws.Range("A2:H2").Merge().Style.Font.SetItalic().Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                var headers = new[] { "Mã VT", "Tên Vật Tư", "ĐVT", "Tồn Đầu", "Nhập Trong Kỳ", "Xuất Trong Kỳ", "Tồn Cuối", "Giá Trị Tồn (VND)" };
                for (int i = 0; i < headers.Length; i++) ws.Cell(4, i + 1).Value = headers[i];

                var headerRange = ws.Range("A4:H4");
                headerRange.Style.Font.SetBold().Fill.SetBackgroundColor(XLColor.LightBlue).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                int row = 5;
                decimal grandTotal = 0;

                var groupedList = reportList
                    .Where(x => x.OpeningStock > 0 || x.ImportQty > 0 || x.ExportQty > 0 || x.ClosingStock > 0)
                    .GroupBy(x => string.IsNullOrEmpty(x.Category) ? "CHƯA PHÂN LOẠI" : x.Category)
                    .OrderBy(g => g.Key)
                    .ToList();

                foreach (var group in groupedList)
                {
                    ws.Range(row, 1, row, 7).Merge().Value = $"NHÓM: {group.Key.ToUpper()}";
                    ws.Range(row, 1, row, 7).Style.Font.SetBold().Fill.SetBackgroundColor(XLColor.FromHtml("#e2e8f0"));

                    decimal groupTotal = group.Sum(x => x.TotalValue);
                    ws.Cell(row, 8).Value = groupTotal;
                    ws.Cell(row, 8).Style.Font.SetBold().Fill.SetBackgroundColor(XLColor.FromHtml("#e2e8f0")).NumberFormat.Format = "#,##0";
                    row++;

                    foreach (var item in group)
                    {
                        ws.Cell(row, 1).Value = item.Code;
                        ws.Cell(row, 2).Value = item.Name;
                        ws.Cell(row, 3).Value = item.Unit;
                        ws.Cell(row, 4).Value = item.OpeningStock;
                        ws.Cell(row, 5).Value = item.ImportQty;
                        ws.Cell(row, 6).Value = item.ExportQty;
                        ws.Cell(row, 7).Value = item.ClosingStock;
                        ws.Cell(row, 8).Value = item.TotalValue;

                        grandTotal += item.TotalValue;
                        row++;
                    }
                }

                ws.Columns("D:G").Style.NumberFormat.Format = "#,##0.##";
                ws.Column(8).Style.NumberFormat.Format = "#,##0";

                ws.Range(row, 1, row, 7).Merge().Value = "TỔNG CỘNG TOÀN KHO:";
                ws.Range(row, 1, row, 7).Style.Font.SetBold().Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
                ws.Cell(row, 8).Value = grandTotal;
                ws.Cell(row, 8).Style.Font.SetBold().Font.SetFontColor(XLColor.Red).NumberFormat.Format = "#,##0";

                ws.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"NXT_{DateTime.Now:ddMMyy}.xlsx");
                }
            }
        }

    }
}