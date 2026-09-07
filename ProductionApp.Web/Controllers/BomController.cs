//using ClosedXML.Excel;
//using ClosedXML.Excel.Drawings;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Hosting;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Rendering;
//using Microsoft.EntityFrameworkCore;
//using ProductionApp.Web.Constants;
//using ProductionApp.Web.Models.ViewModels;
//using ProductionManager.Data;
//using System;
//using System.Collections.Generic;
//using System.Globalization;
//using System.IO;
//using System.Linq;
//using System.Threading.Tasks;

//namespace ProductionApp.Web.Controllers
//{
//    [Authorize]
//    public class BomController : Controller
//    {
//        private readonly AppDbContext _context;
//        private readonly IWebHostEnvironment _webHostEnvironment;

//        public BomController(AppDbContext context, IWebHostEnvironment webHostEnvironment)
//        {
//            _context = context;
//            _webHostEnvironment = webHostEnvironment;
//        }

//        // ==========================================
//        // 0. HÀM DÙNG CHUNG: TÍNH TOÁN TIÊU HAO CHUẨN XÁC
//        // ==========================================
//        private double CalculateActualQuantity(double? len, double? wid, double? hei, double cutQty, double coef, string rawUnit)
//        {
//            // Nếu kích thước bị bỏ trống (Vít/Ốc) thì tự động hiểu là 0 để không lỗi toán học
//            double l = len ?? 0;
//            double w = wid ?? 0;
//            double h = hei ?? 0;

//            // Chuẩn hóa đơn vị tính (Xóa khoảng trắng, đưa về chữ thường, đổi m³ -> m3)
//            string unit = (rawUnit ?? "").ToLower().Trim().Replace(" ", "").Replace("³", "3").Replace("²", "2");

//            bool isVolume = unit.Contains("m3") || unit.Contains("khối") || unit.Contains("khoi");
//            bool isArea = unit.Contains("m2") || unit.Contains("vuông") || unit.Contains("vuong");
//            bool isLength = unit == "m" || unit.Contains("mét") || unit.Contains("met");

//            double actualQty = 0;

//            if (isVolume && l > 0 && w > 0 && h > 0)
//            {
//                // Tính Thể Tích (Khối lượng gỗ) -> Ra số Khối m3
//                actualQty = ((l * w * h) / 1_000_000_000.0) * cutQty * coef;
//            }
//            else if (isArea && l > 0 && w > 0)
//            {
//                // Tính Diện tích (Ván công nghiệp, da...) -> Ra số Mét vuông m2
//                actualQty = ((l * w) / 1_000_000.0) * cutQty * coef;
//            }
//            else if (isLength && l > 0)
//            {
//                // Tính Chiều dài (Chỉ dán cạnh, sắt hộp...) -> Ra số Mét dài m
//                actualQty = (l / 1000.0) * cutQty * coef;
//            }
//            else
//            {
//                // Tính Nguyên cái (Ốc vít, bản lề, tay nắm...) -> Đếm số lượng, làm tròn LÊN
//                actualQty = Math.Ceiling(cutQty * coef);
//            }

//            return actualQty;
//        }

//        // ==========================================
//        // 1. INDEX
//        // ==========================================
//        [Authorize(Policy = AppPermissions.Products.View)]
//        public async Task<IActionResult> Index(string keyword)
//        {
//            var query = _context.ProductBoms.Include(x => x.Product).Include(x => x.Material).AsQueryable();

//            if (!string.IsNullOrEmpty(keyword))
//            {
//                query = query.Where(x => x.Product.ProductName.Contains(keyword) || x.Product.ProductCode.Contains(keyword));
//            }

//            var data = await query.ToListAsync();

//            var groupedBoms = data.GroupBy(x => x.ProductId)
//                .Select(g => new BomProductStatsVM
//                {
//                    ProductId = g.Key,
//                    ProductCode = g.First().Product.ProductCode,
//                    ProductName = g.First().Product.ProductName,
//                    Unit = g.First().Product.Unit,
//                    MaterialCount = g.Count(),
//                    EstimatedCost = g.Sum(x => (decimal)x.Quantity * x.Material.CostPrice)
//                })
//                .OrderBy(x => x.ProductName).ToList();

//            ViewBag.Keyword = keyword;
//            return View(groupedBoms);
//        }

//        // ==========================================
//        // 2. CREATE (GET) 
//        // ==========================================
//        [HttpGet]
//        [Authorize(Policy = AppPermissions.Products.Create)]
//        public async Task<IActionResult> Create(int? productId)
//        {
//            LoadViewBags();
//            var model = new BomCreateVM();
//            ViewBag.SelectedVariant = "";

//            if (productId.HasValue)
//            {
//                var existingBoms = await _context.ProductBoms.Where(x => x.ProductId == productId.Value).ToListAsync();
//                if (existingBoms.Any())
//                {
//                    model.ProductId = productId.Value;
//                    model.Items = existingBoms.Select(x => new BomItemInput
//                    {
//                        MaterialId = x.MaterialId,
//                        ComponentName = x.ComponentName,
//                        Length = x.Length,
//                        Width = x.Width,
//                        Height = x.Height,
//                        CutQuantity = x.CutQuantity,
//                        Coefficient = x.Coefficient,
//                        Note = x.Note
//                    }).ToList();

//                    var distinctComponents = existingBoms.Where(x => !string.IsNullOrEmpty(x.ComponentName)).Select(x => x.ComponentName.Trim()).Distinct().ToList();
//                    if (distinctComponents.Count == 1)
//                    {
//                        string singleCompName = distinctComponents.First();
//                        if (await _context.ProductDetails.AnyAsync(d => d.ProductId == productId.Value && d.VariantName == singleCompName))
//                            ViewBag.SelectedVariant = singleCompName;
//                    }
//                }
//                else { model.ProductId = productId.Value; model.Items.Add(new BomItemInput()); }
//            }
//            else { model.Items.Add(new BomItemInput()); }

//            return View(model);
//        }

//        private void LoadViewBags()
//        {
//            var list = new List<SelectListItem>();
//            var parents = _context.Products.OrderBy(x => x.ProductName).ToList();
//            var details = _context.ProductDetails.Include(x => x.Product).ToList();

//            foreach (var p in parents) list.Add(new SelectListItem { Value = $"{p.Id}|", Text = $"📦 [{p.ProductCode}] {p.ProductName} (Nguyên bộ)" });
//            foreach (var d in details) { if (d.Product != null) list.Add(new SelectListItem { Value = $"{d.ProductId}|{d.VariantName}", Text = $"🔹 [{d.Product.ProductCode}] {d.Product.ProductName} - {d.VariantName}" }); }

//            ViewBag.ProductsList = list;

//            var materials = _context.WarehouseItems
//                .Select(x => new { x.Id, x.Name, x.Unit, CostPrice = x.CostPrice })
//                .OrderBy(x => x.Name).ToList();

//            ViewBag.MaterialsRaw = materials;
//            ViewBag.MatNames = materials.ToDictionary(x => x.Id, x => x.Name);
//            ViewBag.MatUnits = materials.ToDictionary(x => x.Id, x => string.IsNullOrEmpty(x.Unit) ? "Cái" : x.Unit);

//            // Serialize vật tư để Javascript dùng tính toán Real-time
//            ViewBag.MaterialsJson = System.Text.Json.JsonSerializer.Serialize(materials);
//        }

//        // ==========================================
//        // 3. CREATE (POST) - 🔥 BẢN CHUẨN: QUÉT TỰ ĐỘNG KHÔNG CẦN INDEX
//        // ==========================================
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        [Authorize(Policy = AppPermissions.Products.Create)]
//        public async Task<IActionResult> Create(BomCreateVM model)
//        {
//            // 1. Nhận diện ID Sản phẩm
//            string rawSelect = Request.Form["cbProductSelect"];
//            if (!string.IsNullOrEmpty(rawSelect))
//            {
//                var parts = rawSelect.Split('|');
//                if (parts.Length > 0 && int.TryParse(parts[0], out int pid) && pid > 0)
//                    model.ProductId = pid;
//            }

//            ModelState.Clear();
//            if (model.ProductId <= 0) ModelState.AddModelError("ProductId", "Vui lòng chọn sản phẩm.");

//            var validItems = new List<ProductBom>();

//            // 🔥 GIẢI PHÁP TỐI THƯỢNG: Tự động quét toàn bộ dữ liệu gửi lên (Bỏ qua lỗi đứt gãy Index của JS)
//            var itemKeys = Request.Form.Keys.Where(k => k.StartsWith("Items[") && k.EndsWith("].MaterialId")).ToList();
//            var indices = itemKeys.Select(k => k.Substring(6, k.IndexOf(']') - 6)).Distinct().ToList();

//            if (indices.Any())
//            {
//                // Lấy trước thông tin ĐVT vật tư từ DB để tăng tốc
//                var matIds = indices.Select(idx => { int.TryParse(Request.Form[$"Items[{idx}].MaterialId"], out int mId); return mId; }).Where(m => m > 0).ToList();
//                var matInfoMap = await _context.WarehouseItems.Where(m => matIds.Contains(m.Id)).ToDictionaryAsync(m => m.Id, m => m.Unit ?? "");

//                foreach (var idx in indices)
//                {
//                    int.TryParse(Request.Form[$"Items[{idx}].MaterialId"], out int matId);

//                    string rawQty = Request.Form[$"Items[{idx}].CutQuantity"].ToString().Replace(",", ".");
//                    double.TryParse(rawQty, NumberStyles.Any, CultureInfo.InvariantCulture, out double qty);

//                    // Chỉ xử lý các dòng có chọn Vật tư và Số lượng > 0
//                    if (matId > 0 && qty > 0)
//                    {
//                        string rawCoef = Request.Form[$"Items[{idx}].Coefficient"].ToString().Replace(",", ".");
//                        double.TryParse(rawCoef, NumberStyles.Any, CultureInfo.InvariantCulture, out double coef);
//                        coef = coef > 0 ? coef : 1;

//                        // Ép kiểu chuẩn Nullable cho các ô Dài/Rộng/Dày
//                        string rawL = Request.Form[$"Items[{idx}].Length"].ToString().Replace(",", ".");
//                        double? l = string.IsNullOrWhiteSpace(rawL) ? (double?)null : double.Parse(rawL, CultureInfo.InvariantCulture);

//                        string rawW = Request.Form[$"Items[{idx}].Width"].ToString().Replace(",", ".");
//                        double? w = string.IsNullOrWhiteSpace(rawW) ? (double?)null : double.Parse(rawW, CultureInfo.InvariantCulture);

//                        string rawH = Request.Form[$"Items[{idx}].Height"].ToString().Replace(",", ".");
//                        double? h = string.IsNullOrWhiteSpace(rawH) ? (double?)null : double.Parse(rawH, CultureInfo.InvariantCulture);

//                        string unit = matInfoMap.ContainsKey(matId) ? matInfoMap[matId] : "";

//                        // 🔥 ĐỌC CHỮ ĐVT MÀ SẾP GÕ TRÊN GIAO DIỆN
//                        string customUnit = Request.Form[$"Items[{idx}].Unit"].ToString().Trim();
//                        // Nếu lỡ xóa trắng thì lấy lại ĐVT gốc
//                        string finalUnit = !string.IsNullOrEmpty(customUnit) ? customUnit : (matInfoMap.ContainsKey(matId) ? matInfoMap[matId] : "");

//                        var bomEntry = new ProductBom
//                        {
//                            ProductId = model.ProductId,
//                            MaterialId = matId,
//                            Unit = finalUnit,
//                            ComponentName = Request.Form[$"Items[{idx}].ComponentName"].ToString().Trim(),
//                            Note = Request.Form[$"Items[{idx}].Note"].ToString().Trim(),
//                            Length = l,
//                            Width = w,
//                            Height = h,
//                            CutQuantity = qty,
//                            Coefficient = coef,
//                            Quantity = CalculateActualQuantity(l, w, h, qty, coef, unit)
//                        };

//                        validItems.Add(bomEntry);
//                    }
//                }
//            }

//            if (!validItems.Any()) ModelState.AddModelError("", "Vui lòng nhập ít nhất 1 dòng vật tư hợp lệ (có chọn vật tư và Số lượng > 0).");

//            if (ModelState.IsValid)
//            {
//                // Bọc Transaction an toàn để đảm bảo DB lưu 100%
//                using var transaction = await _context.Database.BeginTransactionAsync();
//                try
//                {
//                    var oldBoms = await _context.ProductBoms.Where(x => x.ProductId == model.ProductId).ToListAsync();
//                    if (oldBoms.Any())
//                    {
//                        _context.ProductBoms.RemoveRange(oldBoms);
//                    }

//                    await _context.ProductBoms.AddRangeAsync(validItems);
//                    await _context.SaveChangesAsync();

//                    await transaction.CommitAsync();

//                    TempData["Success"] = "Đã lưu định mức thành công!";
//                    return RedirectToAction(nameof(Index));
//                }
//                catch (Exception ex)
//                {
//                    await transaction.RollbackAsync();
//                    // In ra lỗi cốt lõi của Database nếu có
//                    string errorMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
//                    ModelState.AddModelError("", "Lỗi CSDL MySQL: " + errorMsg);
//                }
//            }

//            // Fallback: Nạp lại View nếu bị lỗi
//            LoadViewBags();
//            if (!string.IsNullOrEmpty(rawSelect)) { var parts = rawSelect.Split('|'); if (parts.Length > 1) ViewBag.SelectedVariant = parts[1]; }
//            model.Items = validItems.Select(x => new BomItemInput { MaterialId = x.MaterialId, ComponentName = x.ComponentName, Length = x.Length, Width = x.Width, Height = x.Height, CutQuantity = x.CutQuantity, Coefficient = x.Coefficient, Note = x.Note }).ToList();
//            if (!model.Items.Any()) model.Items.Add(new BomItemInput());

//            return View(model);
//        }

//        // ==========================================
//        // 4. DETAILS 
//        // ==========================================
//        [Authorize(Policy = AppPermissions.Products.View)]
//        public async Task<IActionResult> Details(int id)
//        {
//            var product = await _context.Products.FindAsync(id);
//            if (product == null) return NotFound();

//            var boms = await _context.ProductBoms.Include(x => x.Material).Where(x => x.ProductId == id).ToListAsync();

//            var details = boms.Select(x => new BomDetailItem
//            {
//                ComponentName = x.ComponentName,
//                MaterialName = x.Material.Name,
//                // 🔥 Ưu tiên lấy ĐVT Sếp gõ, nếu trống mới lấy ĐVT gốc
//                Unit = !string.IsNullOrEmpty(x.Unit) ? x.Unit : (x.Material?.Unit ?? ""),
//                Quantity = x.Quantity,
//                UnitPrice = x.Material.CostPrice,
//                TotalPrice = (decimal)x.Quantity * x.Material.CostPrice,
//                Note = x.Note,
//                Length = x.Length,
//                Width = x.Width,
//                Height = x.Height,
//                CutQuantity = x.CutQuantity,
//                Coefficient = x.Coefficient
//            }).ToList();

//            // 🔥 Nhóm (Group) theo cả ID Vật tư và ĐVT để không bị gộp sai
//            var summary = boms.GroupBy(x => new {
//                x.MaterialId,
//                CustomUnit = !string.IsNullOrEmpty(x.Unit) ? x.Unit : (x.Material?.Unit ?? "")
//            })
//                .Select(g => new BomSummaryItem
//                {
//                    MaterialName = g.First().Material.Name,
//                    Unit = g.Key.CustomUnit, // 🔥 Gán ĐVT đúng chuẩn vào bảng Tổng hợp
//                    TotalQuantity = g.Sum(x => x.Quantity),
//                    TotalCost = g.Sum(x => (decimal)x.Quantity * x.Material.CostPrice)
//                }).ToList();

//            var model = new BomDetailVM { Product = product, Materials = details, Summary = summary, TotalCost = summary.Sum(x => x.TotalCost) };
//            return View(model);
//        }

//        // ==========================================
//        // 5. ĐỒNG BỘ TOÀN BỘ CÔNG THỨC (QUAN TRỌNG)
//        // ==========================================
//        [HttpGet]
//        [Authorize(Policy = AppPermissions.Products.Edit)]
//        public async Task<IActionResult> RecalculateAllQuantities()
//        {
//            var allBoms = await _context.ProductBoms.Include(x => x.Material).ToListAsync();
//            int updatedCount = 0;

//            foreach (var bom in allBoms)
//            {
//                double newQuantity = CalculateActualQuantity(bom.Length, bom.Width, bom.Height, bom.CutQuantity, bom.Coefficient, bom.Material?.Unit);

//                if (Math.Abs(bom.Quantity - newQuantity) > 0.0000001)
//                {
//                    bom.Quantity = newQuantity;
//                    updatedCount++;
//                }
//            }

//            if (updatedCount > 0)
//            {
//                await _context.SaveChangesAsync();
//                TempData["Success"] = $"Đã nắn lại chuẩn xác {updatedCount} dòng vật tư tiêu hao bị sai số lượng!";
//            }
//            else
//            {
//                TempData["Info"] = "Tuyệt vời! Toàn bộ định mức BOM trong xưởng đều đã được tính đúng chuẩn.";
//            }

//            return RedirectToAction(nameof(Index));
//        }

//        // ==========================================
//        // 6. DELETE
//        // ==========================================
//        [HttpPost]
//        [Authorize(Policy = AppPermissions.Products.Delete)]
//        public async Task<IActionResult> Delete(int id)
//        {
//            var boms = _context.ProductBoms.Where(x => x.ProductId == id);
//            if (boms.Any()) { _context.ProductBoms.RemoveRange(boms); await _context.SaveChangesAsync(); TempData["Success"] = "Đã xóa định mức thành công!"; }
//            return RedirectToAction(nameof(Index));
//        }

//        // ==========================================
//        // 7. EXPORT OPTIONS & EXCEL
//        // ==========================================
//        [HttpGet]
//        [Authorize(Policy = AppPermissions.Orders.Create)]
//        public async Task<IActionResult> ExportOptions(int id)
//        {
//            var product = await _context.Products.FindAsync(id);
//            if (product == null) return NotFound();

//            string logoPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "logo.png");
//            ViewBag.HasLogo = System.IO.File.Exists(logoPath);
//            ViewBag.LogoUrl = ViewBag.HasLogo ? "/images/logo.png?v=" + DateTime.Now.Ticks : null;

//            return View(new BomExportVM { ProductId = product.Id, ProductName = product.ProductName, ProductCode = product.ProductCode, CompanyName = "CÔNG TY TNHH ABC", CompanyAddress = "Địa chỉ công ty...", WatermarkText = "LƯU HÀNH NỘI BỘ", CreatorName = User.Identity?.Name ?? "Admin", ApproverName = "Giám Đốc", ShowLogo = true });
//        }

//        [HttpPost]
//        [Authorize(Policy = AppPermissions.Products.Create)]
//        public async Task<IActionResult> ExportExcel(BomExportVM model)
//        {
//            if (model.LogoFile != null && model.LogoFile.Length > 0)
//            {
//                string folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "images");
//                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
//                using (var stream = new FileStream(Path.Combine(folderPath, "logo.png"), FileMode.Create)) { await model.LogoFile.CopyToAsync(stream); }
//            }

//            var boms = await _context.ProductBoms.Include(x => x.Material).Where(x => x.ProductId == model.ProductId).ToListAsync();
//            var product = await _context.Products.FindAsync(model.ProductId);

//            // 🔥 ĐÃ ĐỒNG BỘ: Sử dụng HasValue để an toàn khi đếm gỗ
//            double totalM3PerProduct = boms.Where(x => x.Length.HasValue && x.Length > 0 && x.Width.HasValue && x.Width > 0 && x.Height.HasValue && x.Height > 0).Sum(x => x.Quantity);
//            double qtyPerOneM3 = totalM3PerProduct > 0 ? (1 / totalM3PerProduct) : 0;

//            var summary = boms.GroupBy(x => new {
//                x.MaterialId,
//                CustomUnit = !string.IsNullOrEmpty(x.Unit) ? x.Unit : (x.Material?.Unit ?? "")
//            })
//                .Select(g => new {
//                    MaterialName = g.First().Material.Name,
//                    Unit = g.Key.CustomUnit, // 🔥 Gán ĐVT khi xuất Excel
//                    TotalQuantity = g.Sum(x => x.Quantity),
//                    TotalCost = g.Sum(x => (decimal)x.Quantity * x.Material.CostPrice)
//                }).ToList();

//            using (var wb = new XLWorkbook())
//            {
//                var ws = wb.Worksheets.Add("BOM_CHI_TIET");

//                if (model.ShowLogo && System.IO.File.Exists(Path.Combine(_webHostEnvironment.WebRootPath, "images", "logo.png")))
//                    ws.AddPicture(Path.Combine(_webHostEnvironment.WebRootPath, "images", "logo.png")).MoveTo(ws.Cell(1, 1)).WithSize(120, 60);

//                ws.Range("D1:H1").Merge().Value = model.CompanyName?.ToUpper(); ws.Range("D1:H1").Style.Font.Bold = true; ws.Range("D1:H1").Style.Font.FontSize = 14; ws.Range("D1:H1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
//                ws.Range("D2:H2").Merge().Value = model.CompanyAddress; ws.Range("D2:H2").Style.Font.Italic = true; ws.Range("D2:H2").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
//                ws.Range("A4:H4").Merge().Value = "BẢNG ĐỊNH MỨC NGUYÊN VẬT LIỆU (BOM)"; ws.Range("A4:H4").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center; ws.Range("A4:H4").Style.Font.Bold = true; ws.Range("A4:H4").Style.Font.FontSize = 16;

//                if (!string.IsNullOrEmpty(product.ImagePath) && System.IO.File.Exists(Path.Combine(_webHostEnvironment.WebRootPath, "product-images", product.ImagePath)))
//                    ws.AddPicture(Path.Combine(_webHostEnvironment.WebRootPath, "product-images", product.ImagePath)).MoveTo(ws.Cell("H6")).WithSize(150, 150);

//                ws.Cell(6, 1).Value = "Tên Sản phẩm:"; ws.Cell(6, 2).Value = product.ProductName; ws.Cell(6, 2).Style.Font.Bold = true;
//                ws.Cell(7, 1).Value = "Mã Sản phẩm:"; ws.Cell(7, 2).Value = product.ProductCode; ws.Cell(7, 2).Style.Font.Bold = true;
//                ws.Cell(8, 1).Value = "Đơn vị tính:"; ws.Cell(8, 2).Value = product.Unit; ws.Cell(8, 2).Style.Font.Bold = true;

//                ws.Cell(6, 7).Value = "Ngày lập:"; ws.Cell(6, 8).Value = DateTime.Now.ToString("dd/MM/yyyy"); ws.Cell(6, 7).Style.Font.Bold = true; ws.Cell(6, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

//                ws.Range("E7:F8").Merge().Value = "Định mức SX (Est):"; ws.Range("E7:F8").Style.Font.Bold = true; ws.Range("E7:F8").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right; ws.Range("E7:F8").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
//                ws.Range("G7:H8").Merge().Value = $"{qtyPerOneM3:N2} {product.Unit} / 1 m³ gỗ"; ws.Range("G7:H8").Style.Font.Bold = true; ws.Range("G7:H8").Style.Font.FontColor = XLColor.Red; ws.Range("G7:H8").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left; ws.Range("G7:H8").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center; ws.Range("G7:H8").Style.Fill.BackgroundColor = XLColor.Yellow; ws.Range("G7:H8").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

//                if (!string.IsNullOrEmpty(model.WatermarkText)) ws.PageSetup.Header.Center.AddText($"&\"Arial\"&40&KCCCCCC {model.WatermarkText}");

//                int row = 10;
//                ws.Cell(row, 1).Value = "I. TỔNG HỢP NHU CẦU VẬT TƯ"; ws.Cell(row, 1).Style.Font.Bold = true; ws.Cell(row, 1).Style.Font.FontColor = XLColor.Blue; row++;

//                var headerStyle = ws.Style; headerStyle.Font.Bold = true; headerStyle.Alignment.Horizontal = XLAlignmentHorizontalValues.Center; headerStyle.Border.BottomBorder = XLBorderStyleValues.Thin; headerStyle.Fill.BackgroundColor = XLColor.FromHtml("#F2F2F2");
//                ws.Cell(row, 1).Value = "STT"; ws.Cell(row, 2).Value = "Tên Vật tư"; ws.Cell(row, 3).Value = "ĐVT"; ws.Cell(row, 4).Value = "Tổng lượng"; ws.Cell(row, 5).Value = "Thành tiền (Est)";
//                ws.Range(row, 1, row, 5).Style = headerStyle; row++;

//                int stt = 1; decimal grandTotal = 0;
//                foreach (var item in summary)
//                {
//                    ws.Cell(row, 1).Value = stt++; ws.Cell(row, 2).Value = item.MaterialName; ws.Cell(row, 3).Value = item.Unit; ws.Cell(row, 4).Value = item.TotalQuantity;
//                    if (!string.IsNullOrEmpty(item.Unit) && item.Unit.ToLower().Contains("m3")) ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00000";
//                    else ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0.##";

//                    ws.Cell(row, 5).Value = item.TotalCost;
//                    ws.Cell(row, 5).Style.NumberFormat.Format = "#,##0";

//                    grandTotal += item.TotalCost;
//                    ws.Range(row, 1, row, 5).Style.Border.BottomBorder = XLBorderStyleValues.Hair;
//                    row++;
//                }

//                ws.Cell(row, 4).Value = "TỔNG CỘNG:";
//                ws.Cell(row, 5).Value = grandTotal;
//                ws.Range(row, 4, row, 5).Style.Font.Bold = true;
//                ws.Range(row, 4, row, 5).Style.NumberFormat.Format = "#,##0"; ws.Range(row, 1, row, 5).Style.Border.TopBorder = XLBorderStyleValues.Thin; row += 3;

//                ws.Cell(row, 1).Value = "II. CHI TIẾT CẤU THÀNH (CUT LIST)"; ws.Cell(row, 1).Style.Font.Bold = true; ws.Cell(row, 1).Style.Font.FontColor = XLColor.Blue; row++;
//                ws.Cell(row, 1).Value = "STT"; ws.Cell(row, 2).Value = "Tên Chi tiết / Bộ phận"; ws.Cell(row, 3).Value = "Vật tư sử dụng"; ws.Cell(row, 4).Value = "Quy cách (mm)"; ws.Cell(row, 5).Value = "SL Cắt"; ws.Cell(row, 6).Value = "Hệ số"; ws.Cell(row, 7).Value = "Tổng Tiêu hao"; ws.Range(row, 1, row, 7).Style = headerStyle; row++;

//                stt = 1;
//                foreach (var item in boms)
//                {
//                    ws.Cell(row, 1).Value = stt++; ws.Cell(row, 2).Value = item.ComponentName; ws.Cell(row, 2).Style.Font.Bold = true; ws.Cell(row, 3).Value = item.Material?.Name ?? "";

//                    // 🔥 ĐÃ ĐỒNG BỘ: Sử dụng Nullable (.HasValue) an toàn khi xuất Excel
//                    ws.Cell(row, 4).Value = item.Length.HasValue && item.Length > 0 ? $"{item.Length} x {item.Width} x {item.Height}" : "-";

//                    ws.Cell(row, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center; ws.Cell(row, 5).Value = item.CutQuantity; ws.Cell(row, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center; ws.Cell(row, 6).Value = item.Coefficient; ws.Cell(row, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center; ws.Cell(row, 7).Value = item.Quantity;
//                    if (!string.IsNullOrEmpty(item.Material?.Unit) && item.Material.Unit.ToLower().Contains("m3")) ws.Cell(row, 7).Style.NumberFormat.Format = "#,##0.00000"; else ws.Cell(row, 7).Style.NumberFormat.Format = "#,##0.##";
//                    ws.Range(row, 1, row, 7).Style.Border.BottomBorder = XLBorderStyleValues.Hair; row++;
//                }

//                ws.Range(row - 1, 1, row - 1, 7).Style.Border.BottomBorder = XLBorderStyleValues.Thin; row += 3;
//                var centerStyle = ws.Style; centerStyle.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
//                ws.Cell(row, 2).Value = "Người lập phiếu"; ws.Cell(row, 6).Value = "Người duyệt"; ws.Range(row, 1, row, 8).Style = centerStyle; ws.Cell(row, 2).Style.Font.Bold = true; ws.Cell(row, 6).Style.Font.Bold = true; row++;
//                ws.Cell(row, 2).Value = "(Ký, ghi rõ họ tên)"; ws.Cell(row, 6).Value = "(Ký, đóng dấu)"; ws.Range(row, 1, row, 8).Style.Font.Italic = true; row += 4;
//                ws.Cell(row, 2).Value = model.CreatorName; ws.Cell(row, 6).Value = model.ApproverName; ws.Cell(row, 2).Style.Font.Bold = true; ws.Cell(row, 6).Style.Font.Bold = true;
//                ws.Columns().AdjustToContents(); ws.Column(1).Width = 5; ws.Column(2).Width = 30; ws.Column(3).Width = 25;

//                using (var stream = new MemoryStream()) { wb.SaveAs(stream); return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"BOM_{product.ProductCode}_{DateTime.Now:yyyyMMdd}.xlsx"); }
//            }
//        }

//        // ==========================================
//        // 8. PRINT
//        // ==========================================
//        [HttpPost]
//        public async Task<IActionResult> Print(BomExportVM model)
//        {
//            var boms = await _context.ProductBoms.Include(x => x.Material).Where(x => x.ProductId == model.ProductId).ToListAsync();
//            var product = await _context.Products.FindAsync(model.ProductId);
//            var summary = boms.GroupBy(x => new {
//                x.MaterialId,
//                CustomUnit = !string.IsNullOrEmpty(x.Unit) ? x.Unit : (x.Material?.Unit ?? "")
//            })
//                .Select(g => new {
//                    MaterialName = g.First().Material.Name,
//                    Unit = g.Key.CustomUnit,
//                    TotalQuantity = g.Sum(x => x.Quantity),
//                    TotalCost = g.Sum(x => (decimal)x.Quantity * x.Material.CostPrice)
//                }).ToList();

//            ViewBag.Product = product; ViewBag.Boms = boms; ViewBag.Summary = summary;
//            ViewBag.HasLogo = System.IO.File.Exists(Path.Combine(_webHostEnvironment.WebRootPath, "images", "logo.png")) && model.ShowLogo;
//            return View(model);
//        }

//        // ==========================================
//        // 9. MASTER PLAN (MRP) - KẾ HOẠCH VẬT TƯ
//        // ==========================================
//        [HttpGet]
//        public async Task<IActionResult> MasterPlan(DateTime? fromDate, DateTime? toDate)
//        {
//            var model = new MasterPlanVM { AggregatedMaterials = new List<PlanMaterialSummary>(), Orders = new List<PlanOrder>(), Warnings = new List<string>() };

//            try
//            {
//                var start = fromDate ?? DateTime.Today; var end = toDate ?? start.AddDays(30);
//                model.FromDate = start; model.ToDate = end; ViewData["fromDate"] = start.ToString("yyyy-MM-dd"); ViewData["toDate"] = end.ToString("yyyy-MM-dd");

//                var orders = await _context.Orders.Include(o => o.Customer).Include(o => o.OrderDetails).Where(o => o.DeliveryDeadline >= start && o.DeliveryDeadline <= end && o.Status != OrderStatus.Canceled && o.Status != OrderStatus.Completed).OrderBy(o => o.DeliveryDeadline).ToListAsync();
//                var productIds = orders.SelectMany(o => o.OrderDetails).Where(od => od.ProductId.HasValue).Select(od => od.ProductId.Value).Distinct().ToList();
//                var productBoms = await _context.ProductBoms.Include(pb => pb.Material).Where(pb => productIds.Contains(pb.ProductId)).ToListAsync();
//                var materialIds = productBoms.Select(pb => pb.MaterialId).Distinct().ToList();
//                var materials = await _context.WarehouseItems.Where(m => materialIds.Contains(m.Id)).ToDictionaryAsync(m => m.Id);

//                var totalMaterialNeeds = new Dictionary<int, double>();

//                foreach (var ord in orders)
//                {
//                    var planOrder = new PlanOrder { OrderId = ord.Id, OrderCode = ord.OrderCode, CustomerName = ord.Customer?.CompanyName ?? "Khách lẻ", DeliveryDate = ord.DeliveryDeadline ?? DateTime.Now, Status = ord.Status.ToString(), ProgressColor = ord.Status == OrderStatus.InProduction ? "bg-info" : ord.Status == OrderStatus.PendingApproval ? "bg-warning" : "bg-secondary", BomRequirements = new List<PlanOrderDetailItem>(), ExportedHistory = new List<PlanOrderExportedItem>() };

//                    if (ord.OrderDetails != null)
//                    {
//                        foreach (var od in ord.OrderDetails)
//                        {
//                            if (od.ProductId == null) continue;
//                            var bomsForProduct = productBoms.Where(b => b.ProductId == od.ProductId).ToList();
//                            string[] nameParts = od.ProductName.Split(new[] { " - " }, StringSplitOptions.None);
//                            if (nameParts.Length > 1) { string detailVariantName = nameParts.Last().Trim().ToLower(); bomsForProduct = bomsForProduct.Where(b => b.ComponentName != null && b.ComponentName.Trim().ToLower() == detailVariantName).ToList(); }

//                            if (!bomsForProduct.Any()) { string msg = $"Đơn {ord.OrderCode}: SP '{od.ProductName}' chưa thiết lập BOM"; if (!model.Warnings.Contains(msg)) model.Warnings.Add(msg); continue; }

//                            foreach (var bomItem in bomsForProduct)
//                            {
//                                if (bomItem.Material == null) continue;
//                                double totalNeeded = (double)od.Quantity * bomItem.Quantity * (bomItem.Coefficient > 0 ? bomItem.Coefficient : 1);
//                                planOrder.BomRequirements.Add(new PlanOrderDetailItem { ProductId = od.ProductId.Value, ProductName = od.ProductName, MaterialName = bomItem.Material.Name ?? "Vật tư #" + bomItem.MaterialId, Unit = bomItem.Material.Unit ?? "Cái", QuantityNeeded = totalNeeded });
//                                if (totalMaterialNeeds.ContainsKey(bomItem.MaterialId)) totalMaterialNeeds[bomItem.MaterialId] += totalNeeded; else totalMaterialNeeds.Add(bomItem.MaterialId, totalNeeded);
//                            }
//                        }
//                    }
//                    model.Orders.Add(planOrder);
//                }

//                foreach (var matId in totalMaterialNeeds.Keys)
//                {
//                    if (materials.ContainsKey(matId)) { var mat = materials[matId]; model.AggregatedMaterials.Add(new PlanMaterialSummary { MaterialId = mat.Id, MaterialName = mat.Name, Unit = mat.Unit, TotalNeeded = totalMaterialNeeds[matId], CurrentStock = mat.StockQuantity }); }
//                }
//            }
//            catch (Exception ex) { model.Warnings.Add($"LỖI HỆ THỐNG: {ex.Message}"); }
//            return View(model);
//        }

//        // ==========================================
//        // 10. EXPORT MASTER PLAN TO EXCEL (MRP)
//        // ==========================================
//        [HttpPost]
//        public async Task<IActionResult> ExportMasterPlanExcel(DateTime? fromDate, DateTime? toDate)
//        {
//            try
//            {
//                var start = fromDate ?? DateTime.Today; var end = toDate ?? start.AddDays(30);
//                var orders = await _context.Orders.Include(o => o.Customer).Include(o => o.OrderDetails).Where(o => o.DeliveryDeadline >= start && o.DeliveryDeadline <= end && o.Status != OrderStatus.Canceled && o.Status != OrderStatus.Completed).OrderBy(o => o.DeliveryDeadline).ToListAsync();
//                var productIds = orders.SelectMany(o => o.OrderDetails).Where(od => od.ProductId.HasValue).Select(od => od.ProductId.Value).Distinct().ToList();
//                var productBoms = await _context.ProductBoms.Include(pb => pb.Material).Where(pb => productIds.Contains(pb.ProductId)).ToListAsync();

//                var totalMaterialNeeds = new Dictionary<int, double>();
//                var materialDetails = new Dictionary<int, WarehouseItem>();

//                using (var workbook = new XLWorkbook())
//                {
//                    var wsDetail = workbook.Worksheets.Add("Chi tiết Đơn hàng");
//                    var headers = new[] { "Mã Đơn", "Khách Hàng", "Hạn Giao", "Sản Phẩm", "SL Đơn", "Vật tư", "Định mức/SP", "Tổng tiêu hao", "ĐVT" };
//                    for (int i = 0; i < headers.Length; i++) { wsDetail.Cell(1, i + 1).Value = headers[i]; wsDetail.Cell(1, i + 1).Style.Font.Bold = true; wsDetail.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray; }

//                    int row2 = 2;
//                    foreach (var ord in orders)
//                    {
//                        if (ord.OrderDetails == null) continue;
//                        foreach (var od in ord.OrderDetails)
//                        {
//                            if (od.ProductId == null) continue;
//                            var bomsForProduct = productBoms.Where(b => b.ProductId == od.ProductId).ToList();
//                            string[] nameParts = od.ProductName.Split(new[] { " - " }, StringSplitOptions.None);
//                            if (nameParts.Length > 1) { string detailVariantName = nameParts.Last().Trim().ToLower(); bomsForProduct = bomsForProduct.Where(b => b.ComponentName != null && b.ComponentName.Trim().ToLower() == detailVariantName).ToList(); }

//                            foreach (var bom in bomsForProduct)
//                            {
//                                if (bom.Material == null) continue;
//                                if (!materialDetails.ContainsKey(bom.MaterialId)) materialDetails.Add(bom.MaterialId, bom.Material);

//                                double orderQty = (double)od.Quantity;
//                                double totalQty = orderQty * bom.Quantity * (bom.Coefficient > 0 ? bom.Coefficient : 1);

//                                if (totalMaterialNeeds.ContainsKey(bom.MaterialId)) totalMaterialNeeds[bom.MaterialId] += totalQty; else totalMaterialNeeds.Add(bom.MaterialId, totalQty);

//                                wsDetail.Cell(row2, 1).Value = ord.OrderCode; wsDetail.Cell(row2, 2).Value = ord.Customer?.CompanyName; wsDetail.Cell(row2, 3).Value = ord.DeliveryDeadline?.ToString("dd/MM/yyyy"); wsDetail.Cell(row2, 4).Value = od.ProductName; wsDetail.Cell(row2, 5).Value = orderQty; wsDetail.Cell(row2, 6).Value = bom.Material.Name ?? "Vật tư #" + bom.MaterialId; wsDetail.Cell(row2, 7).Value = bom.Quantity; wsDetail.Cell(row2, 8).Value = totalQty; wsDetail.Cell(row2, 9).Value = bom.Material.Unit;
//                                row2++;
//                            }
//                        }
//                    }
//                    wsDetail.Columns().AdjustToContents();

//                    var wsSummary = workbook.Worksheets.Add("Tổng hợp Vật tư");
//                    var sumHeaders = new[] { "Mã Vật tư", "Tên Vật tư", "ĐVT", "Tổng Cần Mua/Xuất", "Tồn kho hiện tại" };
//                    for (int i = 0; i < sumHeaders.Length; i++) { wsSummary.Cell(1, i + 1).Value = sumHeaders[i]; wsSummary.Cell(1, i + 1).Style.Font.Bold = true; wsSummary.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.Yellow; }

//                    int row1 = 2;
//                    foreach (var mat in totalMaterialNeeds)
//                    {
//                        var matInfo = materialDetails[mat.Key];
//                        wsSummary.Cell(row1, 1).Value = matInfo.Id; wsSummary.Cell(row1, 2).Value = matInfo.Name; wsSummary.Cell(row1, 3).Value = matInfo.Unit; wsSummary.Cell(row1, 4).Value = mat.Value; wsSummary.Cell(row1, 5).Value = matInfo.StockQuantity;
//                        row1++;
//                    }
//                    wsSummary.Columns().AdjustToContents();

//                    using (var stream = new MemoryStream()) { workbook.SaveAs(stream); return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"MRP_Report_{DateTime.Now:yyyyMMdd_HHmm}.xlsx"); }
//                }
//            }
//            catch (Exception ex) { return BadRequest($"Lỗi xuất Excel: {ex.Message}"); }
//        }
//    }
//}


using ClosedXML.Excel;
using ClosedXML.Excel.Drawings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProductionApp.Web.Constants;
using ProductionApp.Web.Models.ViewModels;
using ProductionManager.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ProductionApp.Web.Controllers
{
    [Authorize]
    public class BomController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public BomController(AppDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // ==========================================
        // 0. HÀM DÙNG CHUNG: TÍNH TOÁN TIÊU HAO CHUẨN XÁC
        // ==========================================
        private double CalculateActualQuantity(double l, double w, double h, double cutQty, double coef, string rawUnit)
        {
            string unit = (rawUnit ?? "").ToLower().Trim().Replace(" ", "").Replace("³", "3").Replace("²", "2");

            bool isVolume = unit.Contains("m3") || unit.Contains("khối") || unit.Contains("khoi");
            bool isArea = unit.Contains("m2") || unit.Contains("vuông") || unit.Contains("vuong");
            bool isLength = unit == "m" || unit.Contains("mét") || unit.Contains("met");

            // 🔥 THÊM ĐIỀU KIỆN NHẬN DIỆN TRỌNG LƯỢNG
            bool isWeight = unit.Contains("kg") || unit.Contains("gam");

            double actualQty = 0;

            if (isVolume && l > 0 && w > 0 && h > 0)
            {
                actualQty = ((l * w * h) / 1_000_000_000.0) * cutQty * coef;
            }
            else if (isArea && l > 0 && w > 0)
            {
                actualQty = ((l * w) / 1_000_000.0) * cutQty * coef;
            }
            else if (isLength && l > 0)
            {
                actualQty = (l / 1000.0) * cutQty * coef;
            }
            else if (isWeight && l > 0 && w > 0)
            {
                // 🔥 LOGIC QUY ĐỔI MỚI DÀNH CHO VÍT/ỐC (KG -> CON)
                // l = Khối lượng mẫu (VD: 8 kg)
                // w = Số lượng tương ứng (VD: 100 con)
                // cutQty = Số lượng con cần dùng (VD: 4 con)
                // => Tiêu hao = (8 / 100) * 4 = 0.32 Kg
                actualQty = (l / w) * cutQty * coef;
            }
            else
            {
                actualQty = cutQty * coef;
            }

            return actualQty;
        }

        // ==========================================
        // 1. INDEX
        // ==========================================
        [Authorize(Policy = AppPermissions.Products.View)]
        public async Task<IActionResult> Index(string keyword)
        {
            var query = _context.ProductBoms.Include(x => x.Product).Include(x => x.Material).AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(x => x.Product.ProductName.Contains(keyword) || x.Product.ProductCode.Contains(keyword));

            var data = await query.ToListAsync();

            var groupedBoms = data.GroupBy(x => x.ProductId)
                .Select(g => new BomProductStatsVM
                {
                    ProductId = g.Key,
                    ProductCode = g.First().Product.ProductCode,
                    ProductName = g.First().Product.ProductName,
                    Unit = g.First().Product.Unit,
                    MaterialCount = g.Count(),
                    EstimatedCost = g.Sum(x => (decimal)x.Quantity * x.Material.CostPrice),
                    ImagePath = g.First().Product.ImagePath
                })
                .OrderBy(x => x.ProductName).ToList();

            ViewBag.Keyword = keyword;
            return View(groupedBoms);
        }

        // ==========================================
        // 2. CREATE (GET) 
        // ==========================================
        [HttpGet]
        [Authorize(Policy = AppPermissions.Products.Create)]
        public async Task<IActionResult> Create(int? productId)
        {
            LoadViewBags();
            var model = new BomCreateVM();
            ViewBag.SelectedVariant = "";

            if (productId.HasValue)
            {
                var existingBoms = await _context.ProductBoms.Where(x => x.ProductId == productId.Value).ToListAsync();
                if (existingBoms.Any())
                {
                    model.ProductId = productId.Value;
                    model.Items = existingBoms.Select(x => new BomItemInput
                    {
                        MaterialId = x.MaterialId,
                        ComponentName = x.ComponentName,
                        Unit = !string.IsNullOrEmpty(x.Unit) ? x.Unit : x.Material?.Unit, // Tải ĐVT
                        Length = x.Length,
                        Width = x.Width,
                        Height = x.Height,
                        CutQuantity = x.CutQuantity,
                        Coefficient = x.Coefficient,
                        Note = x.Note
                    }).ToList();

                    var distinctComponents = existingBoms.Where(x => !string.IsNullOrEmpty(x.ComponentName)).Select(x => x.ComponentName.Trim()).Distinct().ToList();
                    if (distinctComponents.Count == 1)
                    {
                        string singleCompName = distinctComponents.First();
                        if (await _context.ProductDetails.AnyAsync(d => d.ProductId == productId.Value && d.VariantName == singleCompName))
                            ViewBag.SelectedVariant = singleCompName;
                    }
                }
                else { model.ProductId = productId.Value; model.Items.Add(new BomItemInput()); }
            }
            else { model.Items.Add(new BomItemInput()); }

            return View(model);
        }

        private void LoadViewBags()
        {
            var list = new List<SelectListItem>();
            var parents = _context.Products.OrderBy(x => x.ProductName).ToList();
            var details = _context.ProductDetails.Include(x => x.Product).ToList();

            foreach (var p in parents) list.Add(new SelectListItem { Value = $"{p.Id}|", Text = $"📦 [{p.ProductCode}] {p.ProductName} (Nguyên bộ)" });
            foreach (var d in details) { if (d.Product != null) list.Add(new SelectListItem { Value = $"{d.ProductId}|{d.VariantName}", Text = $"🔹 [{d.Product.ProductCode}] {d.Product.ProductName} - {d.VariantName}" }); }

            ViewBag.ProductsList = list;

            var materials = _context.WarehouseItems
            .Select(x => new { x.Id, x.Name, x.Unit, CostPrice = x.CostPrice })
            .OrderBy(x => x.Name).ToList();

            ViewBag.MaterialsRaw = materials;
            ViewBag.MatNames = materials.ToDictionary(x => x.Id, x => x.Name);
            ViewBag.MatUnits = materials.ToDictionary(x => x.Id, x => string.IsNullOrEmpty(x.Unit) ? "Cái" : x.Unit);
            ViewBag.MaterialsJson = System.Text.Json.JsonSerializer.Serialize(materials);
        }

        // ==========================================
        // 3. CREATE (POST) - ĐÃ FIX LỖI NHẬN DIỆN DỮ LIỆU
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = AppPermissions.Products.Create)]
        public async Task<IActionResult> Create(BomCreateVM model)
        {
            // 1. Nhận diện ID Sản phẩm từ Dropdown
            string rawSelect = Request.Form["cbProductSelect"];
            if (!string.IsNullOrEmpty(rawSelect))
            {
                var parts = rawSelect.Split('|');
                if (parts.Length > 0 && int.TryParse(parts[0], out int pid)) model.ProductId = pid;
            }

            ModelState.Clear();
            if (model.ProductId <= 0) ModelState.AddModelError("ProductId", "Vui lòng chọn sản phẩm.");

            var validItems = new List<ProductBom>();

            // 🔥 THUẬT TOÁN QUÉT ĐỘNG: Tìm tất cả các Index có chứa MaterialId được gửi lên
            var itemKeys = Request.Form.Keys.Where(k => k.StartsWith("Items[") && k.EndsWith("].MaterialId")).ToList();
            var indices = itemKeys.Select(k => k.Substring(6, k.IndexOf(']') - 6)).Distinct().ToList();

            if (indices.Any())
            {
                // Lấy thông tin Nhóm ĐVT để dự phòng
                var matIds = indices.Select(idx => int.TryParse(Request.Form[$"Items[{idx}].MaterialId"], out int mId) ? mId : 0).Where(m => m > 0).ToList();
                var matInfoMap = await _context.WarehouseItems.Where(m => matIds.Contains(m.Id)).ToDictionaryAsync(m => m.Id, m => m.Unit ?? "");

                foreach (var iStr in indices)
                {
                    string i = iStr.Trim();
                    int.TryParse(Request.Form[$"Items[{i}].MaterialId"], out int matId);

                    // Parse chuẩn xác số lượng cắt
                    double.TryParse(Request.Form[$"Items[{i}].CutQuantity"].ToString().Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out double qty);

                    // Chỉ nhận những dòng có Tên Vật Tư và Số lượng > 0
                    if (matId > 0 && qty > 0)
                    {
                        double.TryParse(Request.Form[$"Items[{i}].Coefficient"].ToString().Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out double coef);
                        double.TryParse(Request.Form[$"Items[{i}].Length"].ToString().Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out double l);
                        double.TryParse(Request.Form[$"Items[{i}].Width"].ToString().Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out double w);
                        double.TryParse(Request.Form[$"Items[{i}].Height"].ToString().Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out double h);

                        coef = coef > 0 ? coef : 1;
                        string customUnit = Request.Form[$"Items[{i}].Unit"].ToString().Trim();
                        string finalUnit = !string.IsNullOrEmpty(customUnit) ? customUnit : (matInfoMap.ContainsKey(matId) ? matInfoMap[matId] : "");

                        var bomEntry = new ProductBom
                        {
                            ProductId = model.ProductId,
                            MaterialId = matId,
                            ComponentName = Request.Form[$"Items[{i}].ComponentName"].ToString().Trim(),
                            Note = Request.Form[$"Items[{i}].Note"].ToString().Trim(),
                            Unit = finalUnit,
                            Length = l,
                            Width = w,
                            Height = h,
                            CutQuantity = qty,
                            Coefficient = coef,
                            Quantity = CalculateActualQuantity(l, w, h, qty, coef, finalUnit)
                        };

                        validItems.Add(bomEntry);
                    }
                }
            }

            // Nếu không có dòng nào quét thành công, báo lỗi cho người dùng
            if (!validItems.Any()) ModelState.AddModelError("", "Vui lòng nhập ít nhất 1 dòng vật tư hợp lệ (Đã tìm vật tư và điền số lượng cắt > 0).");

            if (ModelState.IsValid)
            {
                try
                {
                    // Xóa BOM cũ và ghi đè BOM mới (Sạch sẽ CSDL)
                    var oldBoms = _context.ProductBoms.Where(x => x.ProductId == model.ProductId);
                    _context.ProductBoms.RemoveRange(oldBoms);
                    await _context.ProductBoms.AddRangeAsync(validItems);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Đã lưu định mức thành công!";
                    // return RedirectToAction(nameof(Index));
                    return RedirectToAction("Details", new { id = model.ProductId });
                }
                catch (Exception ex) { ModelState.AddModelError("", "Lỗi hệ thống CSDL: " + ex.Message); }
            }

            // Fallback load lại view nếu có lỗi
            LoadViewBags();
            if (!string.IsNullOrEmpty(rawSelect)) { var parts = rawSelect.Split('|'); if (parts.Length > 1) ViewBag.SelectedVariant = parts[1]; }

            model.Items = validItems.Select(x => new BomItemInput
            {
                MaterialId = x.MaterialId,
                ComponentName = x.ComponentName,
                Unit = x.Unit,
                Length = x.Length,
                Width = x.Width,
                Height = x.Height,
                CutQuantity = x.CutQuantity,
                Coefficient = x.Coefficient,
                Note = x.Note
            }).ToList();

            // Nếu rỗng thì luôn để sẵn 1 dòng trắng
            if (!model.Items.Any()) model.Items.Add(new BomItemInput());

            return View(model);
        }

        // ==========================================
        // 4. DETAILS 
        // ==========================================
        [Authorize(Policy = AppPermissions.Products.View)]
        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            var boms = await _context.ProductBoms.Include(x => x.Material).Where(x => x.ProductId == id).ToListAsync();

            var details = boms.Select(x => new BomDetailItem
            {
                ComponentName = x.ComponentName,
                MaterialName = x.Material.Name,
                Unit = !string.IsNullOrEmpty(x.Unit) ? x.Unit : (x.Material?.Unit ?? ""),
                Quantity = x.Quantity,
                UnitPrice = x.Material.CostPrice,
                TotalPrice = (decimal)x.Quantity * x.Material.CostPrice,
                Note = x.Note,
                Length = x.Length,
                Width = x.Width,
                Height = x.Height,
                CutQuantity = x.CutQuantity,
                Coefficient = x.Coefficient
            }).ToList();

            var summary = boms.GroupBy(x => new {
                x.MaterialId,
                CustomUnit = !string.IsNullOrEmpty(x.Unit) ? x.Unit : (x.Material?.Unit ?? "")
            })
                .Select(g => new BomSummaryItem
                {
                    MaterialName = g.First().Material.Name,
                    Unit = g.Key.CustomUnit,
                    TotalQuantity = g.Sum(x => x.Quantity),
                    TotalCost = g.Sum(x => (decimal)x.Quantity * x.Material.CostPrice)
                }).ToList();

            var model = new BomDetailVM { Product = product, Materials = details, Summary = summary, TotalCost = summary.Sum(x => x.TotalCost) };
            return View(model);
        }

        // ==========================================
        // 5. ĐỒNG BỘ TOÀN BỘ CÔNG THỨC (QUAN TRỌNG)
        // ==========================================
        [HttpGet]
        [Authorize(Policy = AppPermissions.Products.Edit)]
        public async Task<IActionResult> RecalculateAllQuantities()
        {
            var allBoms = await _context.ProductBoms.Include(x => x.Material).ToListAsync();
            int updatedCount = 0;

            foreach (var bom in allBoms)
            {
                // 1. Lấy ĐVT an toàn (Ưu tiên ĐVT Sếp gõ, nếu không có thì lấy ĐVT gốc của vật tư)
                string u = !string.IsNullOrEmpty(bom.Unit) ? bom.Unit : (bom.Material?.Unit ?? "");

                // 2. ÉP KIỂU AN TOÀN 100% (Khắc phục hoàn toàn lỗi double? sang double)
                double l = Convert.ToDouble(bom.Length);
                double w = Convert.ToDouble(bom.Width);
                double h = Convert.ToDouble(bom.Height);

                // 3. Truyền biến đã ép kiểu vào hàm tính
                double newQuantity = CalculateActualQuantity(l, w, h, bom.CutQuantity, bom.Coefficient, u);

                // Kiểm tra sai số và cập nhật nếu bị lệch
                if (Math.Abs(bom.Quantity - newQuantity) > 0.0000001)
                {
                    bom.Quantity = newQuantity;
                    updatedCount++;
                }
            }

            if (updatedCount > 0)
            {
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Đã nắn lại chuẩn xác {updatedCount} dòng vật tư tiêu hao bị sai số lượng!";
            }
            else
            {
                TempData["Info"] = "Tuyệt vời! Toàn bộ định mức BOM trong xưởng đều đã được tính đúng, không có dữ liệu rác.";
            }

            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // 6. DELETE
        // ==========================================
        [HttpPost]
        [Authorize(Policy = AppPermissions.Products.Delete)]
        public async Task<IActionResult> Delete(int id)
        {
            var boms = _context.ProductBoms.Where(x => x.ProductId == id);
            if (boms.Any()) { _context.ProductBoms.RemoveRange(boms); await _context.SaveChangesAsync(); TempData["Success"] = "Đã xóa định mức thành công!"; }
            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // 7. EXPORT OPTIONS & EXCEL
        // ==========================================
        [HttpGet]
        [Authorize(Policy = AppPermissions.Orders.Create)]
        public async Task<IActionResult> ExportOptions(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            string logoPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "logo.png");
            ViewBag.HasLogo = System.IO.File.Exists(logoPath);
            ViewBag.LogoUrl = ViewBag.HasLogo ? "/images/logo.png?v=" + DateTime.Now.Ticks : null;

            return View(new BomExportVM { ProductId = product.Id, ProductName = product.ProductName, ProductCode = product.ProductCode, CompanyName = "CÔNG TY TNHH ABC", CompanyAddress = "Địa chỉ công ty...", WatermarkText = "LƯU HÀNH NỘI BỘ", CreatorName = User.Identity?.Name ?? "Admin", ApproverName = "Giám Đốc", ShowLogo = true });
        }

        [HttpPost]
        [Authorize(Policy = AppPermissions.Products.Create)]
        public async Task<IActionResult> ExportExcel(BomExportVM model)
        {
            if (model.LogoFile != null && model.LogoFile.Length > 0)
            {
                string folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
                using (var stream = new FileStream(Path.Combine(folderPath, "logo.png"), FileMode.Create)) { await model.LogoFile.CopyToAsync(stream); }
            }

            var boms = await _context.ProductBoms.Include(x => x.Material).Where(x => x.ProductId == model.ProductId).ToListAsync();
            var product = await _context.Products.FindAsync(model.ProductId);

            double totalM3PerProduct = boms.Where(x => x.Length > 0 && x.Width > 0 && x.Height > 0).Sum(x => x.Quantity);
            double qtyPerOneM3 = totalM3PerProduct > 0 ? (1 / totalM3PerProduct) : 0;

            var summary = boms.GroupBy(x => new {
                x.MaterialId,
                CustomUnit = !string.IsNullOrEmpty(x.Unit) ? x.Unit : (x.Material?.Unit ?? "")
            })
                .Select(g => new {
                    MaterialName = g.First().Material.Name,
                    Unit = g.Key.CustomUnit,
                    TotalQuantity = g.Sum(x => x.Quantity),
                    TotalCost = g.Sum(x => (decimal)x.Quantity * x.Material.CostPrice)
                }).ToList();

            using (var wb = new XLWorkbook())
            {
                var ws = wb.Worksheets.Add("BOM_CHI_TIET");

                if (model.ShowLogo && System.IO.File.Exists(Path.Combine(_webHostEnvironment.WebRootPath, "images", "logo.png")))
                    ws.AddPicture(Path.Combine(_webHostEnvironment.WebRootPath, "images", "logo.png")).MoveTo(ws.Cell(1, 1)).WithSize(120, 60);

                ws.Range("D1:H1").Merge().Value = model.CompanyName?.ToUpper(); ws.Range("D1:H1").Style.Font.Bold = true; ws.Range("D1:H1").Style.Font.FontSize = 14; ws.Range("D1:H1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws.Range("D2:H2").Merge().Value = model.CompanyAddress; ws.Range("D2:H2").Style.Font.Italic = true; ws.Range("D2:H2").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws.Range("A4:H4").Merge().Value = "BẢNG ĐỊNH MỨC NGUYÊN VẬT LIỆU (BOM)"; ws.Range("A4:H4").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center; ws.Range("A4:H4").Style.Font.Bold = true; ws.Range("A4:H4").Style.Font.FontSize = 16;

                ws.Cell(6, 1).Value = "Tên Sản phẩm:"; ws.Cell(6, 2).Value = product.ProductName; ws.Cell(6, 2).Style.Font.Bold = true;
                ws.Cell(7, 1).Value = "Mã Sản phẩm:"; ws.Cell(7, 2).Value = product.ProductCode; ws.Cell(7, 2).Style.Font.Bold = true;
                ws.Cell(8, 1).Value = "Đơn vị tính:"; ws.Cell(8, 2).Value = product.Unit; ws.Cell(8, 2).Style.Font.Bold = true;

                ws.Cell(6, 7).Value = "Ngày lập:"; ws.Cell(6, 8).Value = DateTime.Now.ToString("dd/MM/yyyy"); ws.Cell(6, 7).Style.Font.Bold = true; ws.Cell(6, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                ws.Range("E7:F8").Merge().Value = "Định mức SX (Est):"; ws.Range("E7:F8").Style.Font.Bold = true; ws.Range("E7:F8").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right; ws.Range("E7:F8").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Range("G7:H8").Merge().Value = $"{qtyPerOneM3:N2} {product.Unit} / 1 m³ gỗ"; ws.Range("G7:H8").Style.Font.Bold = true; ws.Range("G7:H8").Style.Font.FontColor = XLColor.Red; ws.Range("G7:H8").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left; ws.Range("G7:H8").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center; ws.Range("G7:H8").Style.Fill.BackgroundColor = XLColor.Yellow; ws.Range("G7:H8").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                int row = 10;
                ws.Cell(row, 1).Value = "I. TỔNG HỢP NHU CẦU VẬT TƯ"; ws.Cell(row, 1).Style.Font.Bold = true; ws.Cell(row, 1).Style.Font.FontColor = XLColor.Blue; row++;

                var headerStyle = ws.Style; headerStyle.Font.Bold = true; headerStyle.Alignment.Horizontal = XLAlignmentHorizontalValues.Center; headerStyle.Border.BottomBorder = XLBorderStyleValues.Thin; headerStyle.Fill.BackgroundColor = XLColor.FromHtml("#F2F2F2");
                ws.Cell(row, 1).Value = "STT"; ws.Cell(row, 2).Value = "Tên Vật tư"; ws.Cell(row, 3).Value = "ĐVT"; ws.Cell(row, 4).Value = "Tổng lượng"; ws.Cell(row, 5).Value = "Thành tiền (Est)";
                ws.Range(row, 1, row, 5).Style = headerStyle; row++;

                int stt = 1; decimal grandTotal = 0;
                foreach (var item in summary)
                {
                    ws.Cell(row, 1).Value = stt++; ws.Cell(row, 2).Value = item.MaterialName; ws.Cell(row, 3).Value = item.Unit; ws.Cell(row, 4).Value = item.TotalQuantity;
                    if (!string.IsNullOrEmpty(item.Unit) && item.Unit.ToLower().Contains("m3")) ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00000"; else ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0.##";
                    ws.Cell(row, 5).Value = item.TotalCost; ws.Cell(row, 5).Style.NumberFormat.Format = "#,##0"; grandTotal += item.TotalCost; ws.Range(row, 1, row, 5).Style.Border.BottomBorder = XLBorderStyleValues.Hair; row++;
                }

                ws.Cell(row, 4).Value = "TỔNG CỘNG:"; ws.Cell(row, 5).Value = grandTotal; ws.Range(row, 4, row, 5).Style.Font.Bold = true; ws.Range(row, 4, row, 5).Style.NumberFormat.Format = "#,##0"; ws.Range(row, 1, row, 5).Style.Border.TopBorder = XLBorderStyleValues.Thin; row += 3;

                ws.Cell(row, 1).Value = "II. CHI TIẾT CẤU THÀNH (CUT LIST)"; ws.Cell(row, 1).Style.Font.Bold = true; ws.Cell(row, 1).Style.Font.FontColor = XLColor.Blue; row++;
                ws.Cell(row, 1).Value = "STT"; ws.Cell(row, 2).Value = "Tên Chi tiết / Bộ phận"; ws.Cell(row, 3).Value = "Vật tư sử dụng"; ws.Cell(row, 4).Value = "Quy cách"; ws.Cell(row, 5).Value = "SL Cắt"; ws.Cell(row, 6).Value = "Hệ số"; ws.Cell(row, 7).Value = "Tổng Tiêu hao"; ws.Range(row, 1, row, 7).Style = headerStyle; row++;

                stt = 1;
                foreach (var item in boms)
                {
                    ws.Cell(row, 1).Value = stt++; ws.Cell(row, 2).Value = item.ComponentName; ws.Cell(row, 2).Style.Font.Bold = true; ws.Cell(row, 3).Value = item.Material?.Name ?? "";

                    // 🔥 In quy cách thông minh ra Excel
                    string quyCach = "-";
                    string unit = !string.IsNullOrEmpty(item.Unit) ? item.Unit.ToLower() : "";
                    if ((unit.Contains("kg") || unit.Contains("gam")) && item.Length > 0 && item.Width > 0)
                    {
                        quyCach = $"{item.Length} {unit} = {item.Width} con";
                    }
                    else if (item.Length > 0 && item.Width > 0 && item.Height > 0)
                    {
                        quyCach = $"{item.Length} x {item.Width} x {item.Height}";
                    }

                    ws.Cell(row, 4).Value = quyCach; ws.Cell(row, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center; ws.Cell(row, 5).Value = item.CutQuantity; ws.Cell(row, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center; ws.Cell(row, 6).Value = item.Coefficient; ws.Cell(row, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center; ws.Cell(row, 7).Value = item.Quantity;
                    if (!string.IsNullOrEmpty(item.Unit) && item.Unit.ToLower().Contains("m3")) ws.Cell(row, 7).Style.NumberFormat.Format = "#,##0.00000"; else ws.Cell(row, 7).Style.NumberFormat.Format = "#,##0.##";
                    ws.Range(row, 1, row, 7).Style.Border.BottomBorder = XLBorderStyleValues.Hair; row++;
                }

                ws.Range(row - 1, 1, row - 1, 7).Style.Border.BottomBorder = XLBorderStyleValues.Thin; row += 3;
                var centerStyle = ws.Style; centerStyle.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(row, 2).Value = "Người lập phiếu"; ws.Cell(row, 6).Value = "Người duyệt"; ws.Range(row, 1, row, 8).Style = centerStyle; ws.Cell(row, 2).Style.Font.Bold = true; ws.Cell(row, 6).Style.Font.Bold = true; row++;
                ws.Cell(row, 2).Value = "(Ký, ghi rõ họ tên)"; ws.Cell(row, 6).Value = "(Ký, đóng dấu)"; ws.Range(row, 1, row, 8).Style.Font.Italic = true; row += 4;
                ws.Cell(row, 2).Value = model.CreatorName; ws.Cell(row, 6).Value = model.ApproverName; ws.Cell(row, 2).Style.Font.Bold = true; ws.Cell(row, 6).Style.Font.Bold = true;
                ws.Columns().AdjustToContents(); ws.Column(1).Width = 5; ws.Column(2).Width = 30; ws.Column(3).Width = 25;

                using (var stream = new MemoryStream()) { wb.SaveAs(stream); return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"BOM_{product.ProductCode}_{DateTime.Now:yyyyMMdd}.xlsx"); }
            }
        }

        // ==========================================
        // 8. PRINT (POST)
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> Print(BomExportVM model)
        {
            var boms = await _context.ProductBoms.Include(x => x.Material).Where(x => x.ProductId == model.ProductId).ToListAsync();
            var product = await _context.Products.FindAsync(model.ProductId);
            var summary = boms.GroupBy(x => new { x.MaterialId, CustomUnit = !string.IsNullOrEmpty(x.Unit) ? x.Unit : (x.Material?.Unit ?? "") }).Select(g => new { MaterialName = g.First().Material.Name, Unit = g.Key.CustomUnit, TotalQuantity = g.Sum(x => x.Quantity), TotalCost = g.Sum(x => (decimal)x.Quantity * x.Material.CostPrice) }).ToList();

            ViewBag.Product = product;
            ViewBag.Boms = boms;
            ViewBag.Summary = summary;

            string logoPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "logo.png");
            ViewBag.HasLogo = System.IO.File.Exists(logoPath) && model.ShowLogo;

            return View(model);
        }

        // ==========================================
        //        // 9. MASTER PLAN (MRP) - KẾ HOẠCH VẬT TƯ
        //        // ==========================================
        [HttpGet]
        public async Task<IActionResult> MasterPlan(DateTime? fromDate, DateTime? toDate)
        {
            var model = new MasterPlanVM { AggregatedMaterials = new List<PlanMaterialSummary>(), Orders = new List<PlanOrder>(), Warnings = new List<string>() };

            try
            {
                var start = fromDate ?? DateTime.Today; var end = toDate ?? start.AddDays(30);
                model.FromDate = start; model.ToDate = end; ViewData["fromDate"] = start.ToString("yyyy-MM-dd"); ViewData["toDate"] = end.ToString("yyyy-MM-dd");

                var orders = await _context.Orders.Include(o => o.Customer).Include(o => o.OrderDetails).Where(o => o.DeliveryDeadline >= start && o.DeliveryDeadline <= end && o.Status != OrderStatus.Canceled && o.Status != OrderStatus.Completed).OrderBy(o => o.DeliveryDeadline).ToListAsync();
                var productIds = orders.SelectMany(o => o.OrderDetails).Where(od => od.ProductId.HasValue).Select(od => od.ProductId.Value).Distinct().ToList();
                var productBoms = await _context.ProductBoms.Include(pb => pb.Material).Where(pb => productIds.Contains(pb.ProductId)).ToListAsync();
                var materialIds = productBoms.Select(pb => pb.MaterialId).Distinct().ToList();
                var materials = await _context.WarehouseItems.Where(m => materialIds.Contains(m.Id)).ToDictionaryAsync(m => m.Id);

                var totalMaterialNeeds = new Dictionary<int, double>();

                foreach (var ord in orders)
                {
                    var planOrder = new PlanOrder { OrderId = ord.Id, OrderCode = ord.OrderCode, CustomerName = ord.Customer?.CompanyName ?? "Khách lẻ", DeliveryDate = ord.DeliveryDeadline ?? DateTime.Now, Status = ord.Status.ToString(), ProgressColor = ord.Status == OrderStatus.InProduction ? "bg-info" : ord.Status == OrderStatus.PendingApproval ? "bg-warning" : "bg-secondary", BomRequirements = new List<PlanOrderDetailItem>(), ExportedHistory = new List<PlanOrderExportedItem>() };

                    if (ord.OrderDetails != null)
                    {
                        foreach (var od in ord.OrderDetails)
                        {
                            if (od.ProductId == null) continue;
                            var bomsForProduct = productBoms.Where(b => b.ProductId == od.ProductId).ToList();
                            string[] nameParts = od.ProductName.Split(new[] { " - " }, StringSplitOptions.None);
                            if (nameParts.Length > 1) { string detailVariantName = nameParts.Last().Trim().ToLower(); bomsForProduct = bomsForProduct.Where(b => b.ComponentName != null && b.ComponentName.Trim().ToLower() == detailVariantName).ToList(); }

                            if (!bomsForProduct.Any()) { string msg = $"Đơn {ord.OrderCode}: SP '{od.ProductName}' chưa thiết lập BOM"; if (!model.Warnings.Contains(msg)) model.Warnings.Add(msg); continue; }

                            foreach (var bomItem in bomsForProduct)
                            {
                                if (bomItem.Material == null) continue;
                                double totalNeeded = (double)od.Quantity * bomItem.Quantity * (bomItem.Coefficient > 0 ? bomItem.Coefficient : 1);
                                planOrder.BomRequirements.Add(new PlanOrderDetailItem { ProductId = od.ProductId.Value, ProductName = od.ProductName, MaterialName = bomItem.Material.Name ?? "Vật tư #" + bomItem.MaterialId, Unit = bomItem.Material.Unit ?? "Cái", QuantityNeeded = totalNeeded });
                                if (totalMaterialNeeds.ContainsKey(bomItem.MaterialId)) totalMaterialNeeds[bomItem.MaterialId] += totalNeeded; else totalMaterialNeeds.Add(bomItem.MaterialId, totalNeeded);
                            }
                        }
                    }
                    model.Orders.Add(planOrder);
                }

                foreach (var matId in totalMaterialNeeds.Keys)
                {
                    if (materials.ContainsKey(matId)) { var mat = materials[matId]; model.AggregatedMaterials.Add(new PlanMaterialSummary { MaterialId = mat.Id, MaterialName = mat.Name, Unit = mat.Unit, TotalNeeded = totalMaterialNeeds[matId], CurrentStock = mat.StockQuantity }); }
                }
            }
            catch (Exception ex) { model.Warnings.Add($"LỖI HỆ THỐNG: {ex.Message}"); }
            return View(model);
        }

        // ==========================================
        // 10. EXPORT MASTER PLAN TO EXCEL (MRP)
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> ExportMasterPlanExcel(DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                var start = fromDate ?? DateTime.Today; var end = toDate ?? start.AddDays(30);
                var orders = await _context.Orders.Include(o => o.Customer).Include(o => o.OrderDetails).Where(o => o.DeliveryDeadline >= start && o.DeliveryDeadline <= end && o.Status != OrderStatus.Canceled && o.Status != OrderStatus.Completed).OrderBy(o => o.DeliveryDeadline).ToListAsync();
                var productIds = orders.SelectMany(o => o.OrderDetails).Where(od => od.ProductId.HasValue).Select(od => od.ProductId.Value).Distinct().ToList();
                var productBoms = await _context.ProductBoms.Include(pb => pb.Material).Where(pb => productIds.Contains(pb.ProductId)).ToListAsync();

                var totalMaterialNeeds = new Dictionary<int, double>();
                var materialDetails = new Dictionary<int, WarehouseItem>();

                using (var workbook = new XLWorkbook())
                {
                    var wsDetail = workbook.Worksheets.Add("Chi tiết Đơn hàng");
                    var headers = new[] { "Mã Đơn", "Khách Hàng", "Hạn Giao", "Sản Phẩm", "SL Đơn", "Vật tư", "Định mức/SP", "Tổng tiêu hao", "ĐVT" };
                    for (int i = 0; i < headers.Length; i++) { wsDetail.Cell(1, i + 1).Value = headers[i]; wsDetail.Cell(1, i + 1).Style.Font.Bold = true; wsDetail.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray; }

                    int row2 = 2;
                    foreach (var ord in orders)
                    {
                        if (ord.OrderDetails == null) continue;
                        foreach (var od in ord.OrderDetails)
                        {
                            if (od.ProductId == null) continue;
                            var bomsForProduct = productBoms.Where(b => b.ProductId == od.ProductId).ToList();
                            string[] nameParts = od.ProductName.Split(new[] { " - " }, StringSplitOptions.None);
                            if (nameParts.Length > 1) { string detailVariantName = nameParts.Last().Trim().ToLower(); bomsForProduct = bomsForProduct.Where(b => b.ComponentName != null && b.ComponentName.Trim().ToLower() == detailVariantName).ToList(); }

                            foreach (var bom in bomsForProduct)
                            {
                                if (bom.Material == null) continue;
                                if (!materialDetails.ContainsKey(bom.MaterialId)) materialDetails.Add(bom.MaterialId, bom.Material);

                                double orderQty = (double)od.Quantity;
                                double totalQty = orderQty * bom.Quantity * (bom.Coefficient > 0 ? bom.Coefficient : 1);

                                if (totalMaterialNeeds.ContainsKey(bom.MaterialId)) totalMaterialNeeds[bom.MaterialId] += totalQty; else totalMaterialNeeds.Add(bom.MaterialId, totalQty);

                                wsDetail.Cell(row2, 1).Value = ord.OrderCode; wsDetail.Cell(row2, 2).Value = ord.Customer?.CompanyName; wsDetail.Cell(row2, 3).Value = ord.DeliveryDeadline?.ToString("dd/MM/yyyy"); wsDetail.Cell(row2, 4).Value = od.ProductName; wsDetail.Cell(row2, 5).Value = orderQty; wsDetail.Cell(row2, 6).Value = bom.Material.Name ?? "Vật tư #" + bom.MaterialId; wsDetail.Cell(row2, 7).Value = bom.Quantity; wsDetail.Cell(row2, 8).Value = totalQty; wsDetail.Cell(row2, 9).Value = bom.Material.Unit;
                                row2++;
                            }
                        }
                    }
                    wsDetail.Columns().AdjustToContents();

                    var wsSummary = workbook.Worksheets.Add("Tổng hợp Vật tư");
                    var sumHeaders = new[] { "Mã Vật tư", "Tên Vật tư", "ĐVT", "Tổng Cần Mua/Xuất", "Tồn kho hiện tại" };
                    for (int i = 0; i < sumHeaders.Length; i++) { wsSummary.Cell(1, i + 1).Value = sumHeaders[i]; wsSummary.Cell(1, i + 1).Style.Font.Bold = true; wsSummary.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.Yellow; }

                    int row1 = 2;
                    foreach (var mat in totalMaterialNeeds)
                    {
                        var matInfo = materialDetails[mat.Key];
                        wsSummary.Cell(row1, 1).Value = matInfo.Id; wsSummary.Cell(row1, 2).Value = matInfo.Name; wsSummary.Cell(row1, 3).Value = matInfo.Unit; wsSummary.Cell(row1, 4).Value = mat.Value; wsSummary.Cell(row1, 5).Value = matInfo.StockQuantity;
                        row1++;
                    }
                    wsSummary.Columns().AdjustToContents();

                    using (var stream = new MemoryStream()) { workbook.SaveAs(stream); return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"MRP_Report_{DateTime.Now:yyyyMMdd_HHmm}.xlsx"); }
                }
            }
            catch (Exception ex) { return BadRequest($"Lỗi xuất Excel: {ex.Message}"); }
        }
    }
}