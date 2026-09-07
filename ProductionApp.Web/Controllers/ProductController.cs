using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductionApp.Web.Constants;
using ProductionApp.Web.Models;
using ProductionApp.Web.Models.ViewModels;
using ProductionManager.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ProductionApp.Web.Controllers
{
    [Authorize]
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(AppDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // ==========================================
        // 1. INDEX: Hiển thị Sản phẩm
        // ==========================================
 
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .Include(p => p.ProductDetails)
                .Include(p => p.Materials)
                    .ThenInclude(m => m.WarehouseItem)
                .Include(p => p.WarehouseItem)
                .OrderByDescending(p => p.Id)
                .ToListAsync();

            return View(products);
        }

        // ==========================================
        // 2. PHẦN QUẢN LÝ SẢN PHẨM
        // ==========================================

        [Authorize(Policy = AppPermissions.Products.View)]
        public IActionResult Create() => View();

        //[HttpPost]
        //[Authorize(Policy = AppPermissions.Products.Create)]
        //public async Task<IActionResult> Create(Product product, IFormFile? imageFile)
        //{
        //    if (await _context.Products.AnyAsync(p => p.ProductCode == product.ProductCode))
        //    {
        //        ModelState.AddModelError("ProductCode", "Mã sản phẩm này đã tồn tại!");
        //        return View(product);
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        // Xử lý upload ảnh
        //        if (imageFile != null)
        //        {
        //            product.ImagePath = await UploadImage(imageFile);
        //        }

        //        _context.Add(product);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }
        //    return View(product);
        //}

        // Trong ProductController.cs

        [HttpPost]
        [Authorize(Policy = AppPermissions.Products.Create)]
        public async Task<IActionResult> Create(Product product, IFormFile? imageFile)
        {
            if (await _context.Products.AnyAsync(p => p.ProductCode == product.ProductCode))
            {
                ModelState.AddModelError("ProductCode", "Mã sản phẩm này đã tồn tại!");
                return View(product);
            }

            if (ModelState.IsValid)
            {
                // 1. Upload ảnh (Code cũ)
                if (imageFile != null) product.ImagePath = await UploadImage(imageFile);

                // 2. 🔥 TẠO WAREHOUSE ITEM TỰ ĐỘNG 🔥
                var whItem = new WarehouseItem
                {
                    Code = product.ProductCode,        // Dùng chung mã
                    Name = product.ProductName,        // Dùng chung tên
                    Unit = product.Unit ?? "Cái",
                    CategoryId = 3,           // Phân loại riêng
                    ItemType = "Product",              // Đánh dấu là Thành phẩm
                    StockQuantity = 0,                 // Mới tạo thì tồn = 0
                    CostPrice = product.DefaultPrice,  // Giá vốn tạm tính = Giá bán (hoặc 0)
                    Image = product.ImagePath,
                    Note = "Được tạo tự động từ Module Sản Phẩm"
                };

                _context.WarehouseItems.Add(whItem);
                await _context.SaveChangesAsync(); // Lưu để lấy ID

                // 3. Link ID kho vào Sản phẩm
                product.WarehouseItemId = whItem.Id;

                _context.Add(product);
                await _context.SaveChangesAsync();

                // Cập nhật ngược lại LinkedProductId cho kho (để dễ trace)
                whItem.LinkedProductId = product.Id;
                _context.Update(whItem);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> SyncDetailToWarehouse(int detailId)
        {
            var detail = await _context.ProductDetails.FindAsync(detailId);
            if (detail == null) return NotFound();

            if (detail.WarehouseItemId != null)
                return Json(new { success = false, message = "Chi tiết này đã có trong kho rồi!" });

            // Tạo item kho
            var whItem = new WarehouseItem
            {
                Code = detail.DetailCode,
                Name = detail.VariantName,
                Unit = "Cái", // Hoặc lấy từ product cha
                CategoryId = 2,
                ItemType = "SemiProduct",
                StockQuantity = 0,
                CostPrice = detail.Price, // Giá thành chi tiết
                Note = $"Chi tiết của SP ID: {detail.ProductId}"
            };

            _context.WarehouseItems.Add(whItem);
            await _context.SaveChangesAsync();

            // Link lại
            detail.WarehouseItemId = whItem.Id;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã tạo mã kho cho chi tiết này!" });
        }

        // Edit (GET)
        [Authorize(Policy = AppPermissions.Products.Edit)]
        public async Task<IActionResult> Edit(int id)
        {
            var p = await _context.Products
                .Include(x => x.ProductDetails)
                .Include(x => x.PriceHistories.OrderByDescending(h => h.ChangedDate))
                .Include(x => x.Materials).ThenInclude(m => m.WarehouseItem)
                .FirstOrDefaultAsync(x => x.Id == id);

            return p == null ? NotFound() : View(p);
        }

        // Edit (POST)
        [HttpPost]
        [Authorize(Policy = AppPermissions.Products.Edit)]
        public async Task<IActionResult> Edit(int id, Product product, IFormFile? imageFile, string? priceNote)
        {
            if (id != product.Id) return NotFound();

            var existing = await _context.Products.FindAsync(id);
            if (existing == null) return NotFound();

            // Xử lý ảnh: Nếu có upload ảnh mới
            if (imageFile != null)
            {
                // Xóa ảnh cũ nếu có
                if (!string.IsNullOrEmpty(existing.ImagePath))
                {
                    DeleteImageFile(existing.ImagePath);
                }
                // Lưu ảnh mới
                existing.ImagePath = await UploadImage(imageFile);
            }

            // Ghi lịch sử giá
            if (existing.DefaultPrice != product.DefaultPrice)
            {
                _context.ProductPriceHistories.Add(new ProductPriceHistory
                {
                    ProductId = id,
                    OldPrice = existing.DefaultPrice,
                    NewPrice = product.DefaultPrice,
                    Note = priceNote ?? "Cập nhật giá",
                    ChangedDate = DateTime.Now
                });
            }

            // Cập nhật thông tin
            existing.ProductCode = product.ProductCode;
            existing.ProductName = product.ProductName;
            existing.DefaultPrice = product.DefaultPrice;
            existing.Thick = product.Thick;
            existing.Width = product.Width;
            existing.Length = product.Length;
            existing.Color = product.Color;
            existing.MaterialType = product.MaterialType;
            existing.Unit = product.Unit;
            existing.Note = product.Note;
            existing.HasVariants = product.HasVariants;

            _context.Update(existing);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Cập nhật thông tin thành công!";
            return RedirectToAction("Edit", new { id = id });
        }

        // Delete Product
        [Authorize(Policy = AppPermissions.Products.Delete)]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products
                .Include(p => p.ProductDetails)
                .Include(p => p.Materials)
                .Include(p => p.PriceHistories)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product != null)
            {
                // Xóa file ảnh vật lý
                if (!string.IsNullOrEmpty(product.ImagePath))
                {
                    DeleteImageFile(product.ImagePath);
                }

                if (product.ProductDetails != null) _context.ProductDetails.RemoveRange(product.ProductDetails);
                if (product.PriceHistories != null) _context.ProductPriceHistories.RemoveRange(product.PriceHistories);
                if (product.Materials != null) _context.Materials.RemoveRange(product.Materials);

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã xóa sản phẩm!";
            }
            return RedirectToAction(nameof(Index));
        }

        // 🔥 ACTION MỚI: Xóa riêng ảnh trong trang Edit
        [HttpPost]
        [Authorize(Policy = AppPermissions.Products.Delete)]
        public async Task<IActionResult> DeleteImageOnly(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null && !string.IsNullOrEmpty(product.ImagePath))
            {
                DeleteImageFile(product.ImagePath); // Xóa file
                product.ImagePath = null; // Xóa đường dẫn trong DB
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        // --- HELPER FUNCTIONS ---
        private async Task<string> UploadImage(IFormFile file)
        {
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "product-images");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }
            return uniqueFileName;
        }

        private void DeleteImageFile(string fileName)
        {
            string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "product-images", fileName);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }

        // --- Logic Variants (Chi tiết con) ---
        [HttpPost]
        public async Task<IActionResult> SaveVariant(ProductDetail detail)
        {
            if (detail.Id == 0) // Thêm mới
            {
                if (string.IsNullOrEmpty(detail.DetailCode))
                {
                    string randomSuffix = Guid.NewGuid().ToString().Substring(0, 4).ToUpper();
                    detail.DetailCode = $"CT-{DateTime.Now:yyMMdd}-{randomSuffix}";
                }

                // Kiểm tra trùng lần cuối cho chắc chắn
                while (await _context.ProductDetails.AnyAsync(x => x.DetailCode == detail.DetailCode))
                {
                    string randomSuffix = Guid.NewGuid().ToString().Substring(0, 4).ToUpper();
                    detail.DetailCode = $"CT-{DateTime.Now:yyMMdd}-{randomSuffix}";
                }

                _context.ProductDetails.Add(detail);
            }
            else 
            {
                var existing = await _context.ProductDetails.FindAsync(detail.Id);
                if (existing != null)
                {
                    // existing.DetailCode = detail.DetailCode; // Không cho sửa mã khi đã tạo
                    existing.VariantName = detail.VariantName;
                    existing.Quantity = detail.Quantity;
                    existing.Price = detail.Price;
                    existing.Thick = detail.Thick;
                    existing.Width = detail.Width;
                    existing.Length = detail.Length;
                    existing.Color = detail.Color;
                    existing.MaterialType = detail.MaterialType;
                    existing.Note = detail.Note;
                    _context.Update(existing);
                }
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("Edit", new { id = detail.ProductId });
        }

        public async Task<IActionResult> DeleteVariant(int id)
        {
            var v = await _context.ProductDetails.FindAsync(id);
            if (v != null)
            {
                _context.ProductDetails.Remove(v);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Edit), new { id = v.ProductId });
            }
            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // 3. QUẢN LÝ VẬT TƯ (MATERIAL) - FIX QUAN TRỌNG
        // ==========================================

        // A. Mở Form Thêm Vật tư (GET)
        [HttpGet]
        [Authorize(Policy = AppPermissions.Products.Create)]
        public async Task<IActionResult> AddMaterial(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return NotFound();

            // Load danh sách kho để chọn
            ViewBag.WarehouseList = await _context.WarehouseItems
                .Select(w => new { w.Id, Display = $"{w.Code} - {w.Name} (Tồn: {w.StockQuantity} {w.Unit})" })
                .ToListAsync();

            var material = new Material { ProductId = productId };
            return View("AddMaterial", material);
        }

        // B. Mở Form Sửa Vật tư (GET) - FIX LỖI TRỐNG LIST
        [HttpGet]
        [Authorize(Policy = AppPermissions.Products.Edit)]
        public async Task<IActionResult> EditMaterial(int id)
        {
            var m = await _context.Materials.FindAsync(id);
            if (m == null) return NotFound();

            // --- FIX: Phải load lại danh sách kho thì Dropdown mới có dữ liệu ---
            ViewBag.WarehouseList = await _context.WarehouseItems
                .Select(w => new { w.Id, Display = $"{w.Code} - {w.Name} (Tồn: {w.StockQuantity} {w.Unit})" })
                .ToListAsync();
            // -------------------------------------------------------------------

            return View("AddMaterial", m);
        }

        // C. Lưu Vật tư (POST)
        [HttpPost]
        public async Task<IActionResult> SaveMaterial(Material material)
        {
            // Bỏ qua validate các object quan hệ
            ModelState.Remove("Product");
            ModelState.Remove("WarehouseItem");

            // Kiểm tra trùng (trừ chính nó)
            bool exists = await _context.Materials.AnyAsync(m => m.ProductId == material.ProductId
                                                            && m.WarehouseItemId == material.WarehouseItemId
                                                            && m.Id != material.Id);
            if (exists)
            {
                TempData["Error"] = "Vật tư này đã có trong định mức sản phẩm!";
                return RedirectToAction(nameof(AddMaterial), new { productId = material.ProductId });
            }

            if (material.Id == 0) _context.Materials.Add(material);
            else _context.Materials.Update(material);

            await _context.SaveChangesAsync();
            TempData["Success"] = "Đã lưu định mức vật tư!";

            // Quay về trang Edit Product
            return RedirectToAction("Edit", new { id = material.ProductId });
        }

        // D. Xóa Vật tư
        public async Task<IActionResult> DeleteMaterial(int id)
        {
            var m = await _context.Materials.FindAsync(id);
            if (m != null)
            {
                int pId = m.ProductId;
                _context.Materials.Remove(m);
                await _context.SaveChangesAsync();
                return RedirectToAction("Edit", new { id = pId }); // Quay về trang Edit
            }
            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // 4. API JSON & EXCEL
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> GetProductsJson()
        {
            var data = await _context.Products
                .Select(p => new
                {
                    p.Id,
                    p.ProductName,
                    p.ProductCode,
                    p.HasVariants,
                    p.DefaultPrice,
                    p.MaterialType,
                    p.Unit,
                    Specs = $"{p.Thick}x{p.Width}x{p.Length}"
                })
                .ToListAsync();
            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetVariantsJson(int productId)
        {
            var data = await _context.ProductDetails
                .Where(x => x.ProductId == productId)
                .Select(x => new
                {
                    x.Id,
                    x.VariantName,
                    x.Price,
                    x.MaterialType,
                    Specs = $"{x.Thick}x{x.Width}x{x.Length}"
                })
                .ToListAsync();
            return Json(data);
        }

        public async Task<IActionResult> ExportBomExcel(int id)
        {
            var product = await _context.Products
                .Include(p => p.ProductDetails)
                .Include(p => p.Materials)
                    .ThenInclude(m => m.WarehouseItem)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            using (var workbook = new XLWorkbook())
            {
                var sheet = workbook.Worksheets.Add("BOM " + product.ProductCode);

                // --- CẤU HÌNH STYLE CHUNG ---
                var titleStyle = workbook.Style;
                titleStyle.Font.Bold = true;
                titleStyle.Font.FontSize = 14;
                titleStyle.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // 1. HEADER
                sheet.Range("A1:H1").Merge().Value = "BẢNG CHI TIẾT SẢN PHẨM";
                sheet.Cell("A1").Style = titleStyle;

                // 🔥 [FIX LỖI TẠI ĐÂY]: Thêm .Font vào giữa
                sheet.Cell("A1").Style.Font.FontSize = 16;

                sheet.Range("A2:H2").Merge().Value = $"Sản phẩm: {product.ProductName} ({product.ProductCode})";
                sheet.Cell("A2").Style = titleStyle;

                sheet.Range("A3:H3").Merge().Value = $"Quy cách tổng: {product.Length} x {product.Width} x {product.Thick} (mm)";
                sheet.Cell("A3").Style = titleStyle;
                sheet.Cell("A3").Style.Font.FontSize = 12;

                int currentRow = 5;

                // =========================================================
                // PHẦN 1: CHI TIẾT GỖ
                // =========================================================
                sheet.Cell(currentRow, 1).Value = "I. CHI TIẾT SẢN PHẨM";
                sheet.Cell(currentRow, 1).Style.Font.Bold = true;
                sheet.Cell(currentRow, 1).Style.Font.FontColor = XLColor.Blue;
                currentRow++;

                string[] headers1 = { "STT", "Mã Chi Tiết", "Tên Chi Tiết", "Quy Cách (mm)", "Số Lượng", "Số Khối (m3)", "Vật Liệu", "Ghi Chú" };

                for (int i = 0; i < headers1.Length; i++)
                {
                    var cell = sheet.Cell(currentRow, i + 1);
                    cell.Value = headers1[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.LightGray;
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }
                currentRow++;

                double totalQty = 0;
                double totalVol = 0;

                if (product.ProductDetails != null && product.ProductDetails.Any())
                {
                    int stt = 1;
                    foreach (var item in product.ProductDetails)
                    {
                        double vol = (item.Length * item.Width * item.Thick * item.Quantity) / 1000000000.0;
                        totalQty += item.Quantity;
                        totalVol += vol;

                        sheet.Cell(currentRow, 1).Value = stt++;
                        sheet.Cell(currentRow, 2).Value = item.DetailCode;
                        sheet.Cell(currentRow, 3).Value = item.VariantName;
                        sheet.Cell(currentRow, 4).Value = $"{item.Length} x {item.Width} x {item.Thick}";

                        sheet.Cell(currentRow, 5).Value = item.Quantity;
                        sheet.Cell(currentRow, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        sheet.Cell(currentRow, 6).Value = vol;
                        sheet.Cell(currentRow, 6).Style.NumberFormat.Format = "0.0000";

                        sheet.Cell(currentRow, 7).Value = $"{item.MaterialType} - {item.Color}";
                        sheet.Cell(currentRow, 8).Value = item.Note;

                        sheet.Range(currentRow, 1, currentRow, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        currentRow++;
                    }

                    // DÒNG TỔNG CỘNG
                    sheet.Cell(currentRow, 1).Value = "TỔNG CỘNG";
                    sheet.Range(currentRow, 1, currentRow, 4).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    sheet.Range(currentRow, 1, currentRow, 4).Style.Font.Bold = true;

                    sheet.Cell(currentRow, 5).Value = totalQty;
                    sheet.Cell(currentRow, 5).Style.Font.Bold = true;
                    sheet.Cell(currentRow, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    sheet.Cell(currentRow, 6).Value = totalVol;
                    sheet.Cell(currentRow, 6).Style.Font.Bold = true;
                    sheet.Cell(currentRow, 6).Style.NumberFormat.Format = "0.0000";

                    sheet.Range(currentRow, 1, currentRow, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    sheet.Range(currentRow, 1, currentRow, 8).Style.Fill.BackgroundColor = XLColor.AliceBlue;
                    currentRow++;
                }
                else
                {
                    sheet.Cell(currentRow, 1).Value = "(Chưa có dữ liệu)";
                    sheet.Range(currentRow, 1, currentRow, 8).Merge();
                    currentRow++;
                }

                currentRow += 2;

                // =========================================================
                // PHẦN 2: VẬT TƯ
                // =========================================================
                sheet.Cell(currentRow, 1).Value = "II. VẬT TƯ ĐI KÈM";
                sheet.Cell(currentRow, 1).Style.Font.Bold = true;
                sheet.Cell(currentRow, 1).Style.Font.FontColor = XLColor.DarkOrange;
                currentRow++;

                string[] headers2 = { "STT", "Mã Vật Tư", "Tên Vật Tư", "Đơn Vị", "Định Mức", "Giá Vốn", "Thành Tiền", "Ghi Chú" };
                for (int i = 0; i < headers2.Length; i++)
                {
                    var cell = sheet.Cell(currentRow, i + 1);
                    cell.Value = headers2[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.LightYellow;
                    cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }
                currentRow++;

                if (product.Materials != null && product.Materials.Any())
                {
                    int stt = 1;
                    foreach (var item in product.Materials)
                    {
                        string mName = item.WarehouseItem?.Name ?? item.MaterialName;
                        string mCode = item.WarehouseItem?.Code ?? item.MaterialCode;
                        string mUnit = item.WarehouseItem?.Unit ?? item.Unit;
                        decimal totalCost = (decimal)item.Quantity * item.CostPrice;

                        sheet.Cell(currentRow, 1).Value = stt++;
                        sheet.Cell(currentRow, 2).Value = mCode;
                        sheet.Cell(currentRow, 3).Value = mName;
                        sheet.Cell(currentRow, 4).Value = mUnit;
                        sheet.Cell(currentRow, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        sheet.Cell(currentRow, 5).Value = item.Quantity;
                        sheet.Cell(currentRow, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        sheet.Cell(currentRow, 6).Value = item.CostPrice;
                        sheet.Cell(currentRow, 6).Style.NumberFormat.Format = "#,##0";

                        sheet.Cell(currentRow, 7).Value = totalCost;
                        sheet.Cell(currentRow, 7).Style.NumberFormat.Format = "#,##0";

                        sheet.Cell(currentRow, 8).Value = item.Note;

                        sheet.Range(currentRow, 1, currentRow, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        currentRow++;
                    }
                }
                else
                {
                    sheet.Cell(currentRow, 1).Value = "(Chưa có dữ liệu)";
                    sheet.Range(currentRow, 1, currentRow, 8).Merge();
                }

                sheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    string fileName = $"BOM_{product.ProductCode}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
        }


        //-------------------------------------------------
        // =========================================================
        // 1. TRANG HỒ SƠ SẢN PHẨM (TIMELINE)
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Profile(int id)
        {
            var product = await _context.Products
                .Include(p => p.PriceHistories)
                .Include(p => p.ProductDocuments.Where(d => d.IsActive)) // Chỉ lấy tài liệu chưa bị xóa
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            var viewModel = new ProductProfileViewModel { Product = product };

            // 1. Lấy lịch sử giá
            if (product.PriceHistories != null)
            {
                foreach (var price in product.PriceHistories)
                {
                    viewModel.Timeline.Add(new TimelineItem
                    {
                        Id = price.Id,
                        Date = price.ChangedDate,
                        Type = "Price",
                        Title = "Cập nhật Giá bán",
                        Detail = $"Từ {price.OldPrice:N0}đ ➔ {price.NewPrice:N0}đ {(string.IsNullOrEmpty(price.Note) ? "" : $"(Lý do: {price.Note})")}",
                        Icon = "fa-tags",
                        ColorClass = "text-success bg-success"
                    });
                }
            }

            // 2. Lấy lịch sử Tài liệu / Bản vẽ
            if (product.ProductDocuments != null)
            {
                foreach (var doc in product.ProductDocuments)
                {
                    viewModel.Timeline.Add(new TimelineItem
                    {
                        Id = doc.Id,
                        Date = doc.CreatedDate,
                        Type = "Document",
                        Title = doc.Title,
                        Detail = doc.Note ?? "Không có ghi chú",
                        FilePath = doc.FilePath,
                        Icon = doc.FileExtension != null && (doc.FileExtension.Contains("jpg") || doc.FileExtension.Contains("png")) ? "fa-image" : "fa-file-lines",
                        ColorClass = "text-primary bg-primary"
                    });
                }
            }

            // Sắp xếp giảm dần theo thời gian (Mới nhất lên đầu)
            viewModel.Timeline = viewModel.Timeline.OrderByDescending(t => t.Date).ToList();

            return View(viewModel);
        }

        // =========================================================
        // 2. THÊM / SỬA TÀI LIỆU VÀ LƯU FILE VẬT LÝ
        // =========================================================
        [HttpPost]
        public async Task<IActionResult> SaveDocument(int ProductId, int DocumentId, string Title, string Note, IFormFile FileUpload)
        {
            try
            {
                ProductDocument doc;

                // THÊM MỚI
                if (DocumentId == 0)
                {
                    // 1. Lấy thông tin Sản phẩm để tạo tên thư mục
                    var product = await _context.Products.FindAsync(ProductId);
                    if (product == null) return NotFound("Không tìm thấy sản phẩm");

                    // 2. Làm sạch Tên Sản Phẩm (Loại bỏ các ký tự cấm tạo thư mục như \ / : * ? " < > | )
                    string safeFolderName = $"{product.ProductCode}_{product.ProductName}";
                    foreach (char c in Path.GetInvalidFileNameChars())
                    {
                        safeFolderName = safeFolderName.Replace(c, '_');
                    }
                    safeFolderName = safeFolderName.Replace(" ", "-"); // Đổi khoảng trắng thành gạch ngang cho URL đẹp hơn

                    doc = new ProductDocument
                    {
                        ProductId = ProductId,
                        Title = Title,
                        Note = Note,
                        CreatedDate = DateTime.Now,
                        IsActive = true
                    };

                    // 3. Xử lý lưu file vật lý theo cấu trúc mới
                    if (FileUpload != null && FileUpload.Length > 0)
                    {
                        string currentYear = DateTime.Now.Year.ToString();

                        // CẤU TRÚC FOLDER MỚI: wwwroot/sanpham/[MaSP_TenSP]/[Nam]
                        string folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "sanpham", safeFolderName, currentYear);

                        // Tự động tạo cây thư mục nếu chưa có
                        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                        string ext = Path.GetExtension(FileUpload.FileName).ToLower();
                        string uniqueName = Guid.NewGuid().ToString().Substring(0, 8) + "_" + FileUpload.FileName;
                        string physicalPath = Path.Combine(folderPath, uniqueName);

                        using (var stream = new FileStream(physicalPath, FileMode.Create))
                        {
                            await FileUpload.CopyToAsync(stream);
                        }

                        // Lưu đường dẫn tương đối vào Database
                        doc.FilePath = $"sanpham/{safeFolderName}/{currentYear}/{uniqueName}";
                        doc.FileExtension = ext;
                    }

                    _context.ProductDocuments.Add(doc);
                    TempData["Success"] = "Đã thêm tài liệu mới thành công!";
                }
                // CHỈNH SỬA (Chỉ sửa text, không cho sửa file)
                else
                {
                    doc = await _context.ProductDocuments.FirstOrDefaultAsync(x => x.Id == DocumentId);
                    if (doc != null)
                    {
                        doc.Title = Title;
                        doc.Note = Note;
                        TempData["Success"] = "Đã cập nhật thông tin tài liệu!";
                    }
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi xử lý file: " + ex.Message;
            }

            return RedirectToAction(nameof(Profile), new { id = ProductId });
        }

        // =========================================================
        // 3. XÓA GIẢ TÀI LIỆU (SOFT DELETE)
        // =========================================================
        [HttpPost]
        public async Task<IActionResult> DeleteDocument(int id)
        {
            var doc = await _context.ProductDocuments.FirstOrDefaultAsync(x => x.Id == id);
            if (doc != null)
            {
                doc.IsActive = false; // Xóa giả, vẫn giữ nguyên file vật lý và record trong DB
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Không tìm thấy dữ liệu." });
        }


    }
}