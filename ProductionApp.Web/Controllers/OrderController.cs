using ClosedXML.Excel;
using DocumentFormat.OpenXml.ExtendedProperties;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProductionApp.Web.Constants;
using ProductionApp.Web.Helpers;
using ProductionApp.Web.Services;
using ProductionManager.Data;
using System.Globalization;
using System.Security.Claims;
using X.PagedList;
using X.PagedList.Extensions;

namespace ProductionApp.Web.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ServerConfig _config;
        private readonly UserManager<AppUser> _userManager;
         
        private readonly AutomationService _automation; 

        public OrderController(AppDbContext context,
                               IWebHostEnvironment webHostEnvironment,
                               ServerConfig config,
                               UserManager<AppUser> userManager, AutomationService automation)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _config = config;
            _userManager = userManager;
            _automation = automation;
        }

        // ==========================================
        // 1. INDEX (QUẢN LÝ ĐƠN HÀNG)
        // ==========================================
        [Authorize]
        public async Task<IActionResult> Index(int? page, string status, DateTime? fromDate, DateTime? toDate, int? customerId,
            string sortOrder, string colCode, string colCustomer, string colProduct, string viewMode = "list", int? pageSize = null)
        {
            int currentPageSize = pageSize ?? 15;

            // 1. Lưu lại các tham số vào ViewBag để trả về giao diện
            ViewBag.Status = status;
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;
            ViewBag.CustomerId = customerId?.ToString();
            ViewBag.CurrentSort = sortOrder;
            ViewBag.ColCode = colCode;
            ViewBag.ColCustomer = colCustomer;
            ViewBag.ColProduct = colProduct;
            ViewBag.ViewMode = viewMode;
            ViewBag.PageSize = currentPageSize;

            ViewBag.CustomerList = await _context.Customers
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.CompanyName })
                .ToListAsync();

            // 2. KHỞI TẠO QUERY
            var query = _context.Orders
                .AsNoTracking()
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails).ThenInclude(od => od.ShipmentDetails)
                .Include(o => o.InvoiceDetails).ThenInclude(id => id.Invoice)
                .AsQueryable();

            // 3. XỬ LÝ LỌC DỮ LIỆU (FILTER)
            //if (!string.IsNullOrEmpty(status) && int.TryParse(status, out int st))
            //    query = query.Where(o => (int)o.Status == st);

            // Thay thế đoạn lọc status cũ bằng đoạn này:
            if (!string.IsNullOrEmpty(status))
            {
                // Cắt chuỗi "2,3,4" thành mảng số nguyên
                var statusList = status.Split(',').Select(int.Parse).ToList();
                query = query.Where(o => statusList.Contains((int)o.Status));
            }

            if (fromDate.HasValue) query = query.Where(o => o.OrderDate >= fromDate.Value);
            if (toDate.HasValue) query = query.Where(o => o.OrderDate <= toDate.Value);
            if (customerId.HasValue) query = query.Where(o => o.CustomerId == customerId.Value);

            // MySQL mặc định LIKE là không phân biệt hoa/thường, chỉ cần dùng Contains
            if (!string.IsNullOrEmpty(colCode))
                query = query.Where(o => o.OrderCode.Contains(colCode) || o.PaperOrderCode.Contains(colCode));

            if (!string.IsNullOrEmpty(colCustomer))
                query = query.Where(o => o.Customer.CompanyName.Contains(colCustomer));

            if (!string.IsNullOrEmpty(colProduct))
                query = query.Where(o => o.OrderDetails.Any(d => d.ProductName.Contains(colProduct)));

            // 4. TÍNH TOÁN KPI
            ViewBag.TotalRevenue = await query.SumAsync(o => o.TotalAmount);

            var paymentMap = await _context.CashEntries
                .Where(c => (int)c.Type == 1 && c.OrderId != null)
                .GroupBy(c => c.OrderId.Value)
                .Select(g => new { OrderId = g.Key, TotalPaid = g.Sum(c => c.Amount) })
                .ToDictionaryAsync(k => k.OrderId, v => v.TotalPaid);
            ViewBag.PaymentMap = paymentMap;

            // 5. XỬ LÝ SẮP XẾP DỮ LIỆU (SORT)
            switch (sortOrder)
            {
                // Sort trực tiếp theo chuỗi OrderCode thay vì Id
                case "code_asc":
                    query = query.OrderBy(o => o.OrderCode);
                    break;
                case "code_desc":
                    query = query.OrderByDescending(o => o.OrderCode);
                    break;

                // Sort Khách hàng (Nếu trùng tên thì xếp Đơn mới nhất lên trước)
                case "cus_asc":
                    query = query.OrderBy(o => o.Customer.CompanyName).ThenByDescending(o => o.OrderDate);
                    break;
                case "cus_desc":
                    query = query.OrderByDescending(o => o.Customer.CompanyName).ThenByDescending(o => o.OrderDate);
                    break;

                // Sort Ngày nhận (Nếu trùng ngày thì xếp theo Mã đơn)
                case "date_asc":
                    query = query.OrderBy(o => o.OrderDate).ThenBy(o => o.OrderCode);
                    break;
                case "date_desc":
                    query = query.OrderByDescending(o => o.OrderDate).ThenByDescending(o => o.OrderCode);
                    break;

                // Sort Tổng tiền (Nếu trùng số tiền thì đơn mới nhất xếp trước)
                case "total_asc":
                    query = query.OrderBy(o => o.TotalAmount).ThenByDescending(o => o.OrderDate);
                    break;
                case "total_desc":
                    query = query.OrderByDescending(o => o.TotalAmount).ThenByDescending(o => o.OrderDate);
                    break;

                // Mặc định: Ưu tiên Ngày nhận mới nhất -> Mã đơn Z-A
                default:
                    query = query.OrderByDescending(o => o.OrderDate).ThenByDescending(o => o.OrderCode);
                    break;
            }

            // 6. PHÂN TRANG
            int pageNumber = page ?? 1;
            var pagedData = query.ToPagedList(pageNumber, currentPageSize);

            // Xóa block rẽ nhánh PartialView gây lỗi, luôn trả về View chuẩn (AJAX Javascript ở file Index.cshtml sẽ tự động bóc tách vùng dữ liệu cần thiết)
            return View(pagedData);
        }

        [Authorize(Policy = AppPermissions.Orders.View)]
        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.ShipmentDetails)
                        .ThenInclude(sd => sd.Shipment)
                // 🔥 THÊM 2 DÒNG NÀY ĐỂ LẤY DỮ LIỆU HÓA ĐƠN CHO GIAO DIỆN
                .Include(o => o.InvoiceDetails)
                    .ThenInclude(id => id.Invoice)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (order == null) return NotFound();

            ViewBag.PaidAmount = await _context.CashEntries
            .Where(c => c.OrderId == id && c.Type == TransactionType.Receipt)
            .SumAsync(c => c.Amount);

            return View(order);
        }

        // ==========================================
        // 3. CREATE
        // ==========================================
        [HttpGet]
        [Authorize(Policy = AppPermissions.Orders.Create)]
        public async Task<IActionResult> Create()
        {
            LoadCustomerList();
            ViewBag.ProductList = await GetFullProductList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = AppPermissions.Orders.Create)]
        public async Task<IActionResult> Create(Order order, IFormFile? designFile)
        {
            // Bỏ qua validate các trường hệ thống
            ModelState.Remove("ShipmentDetails");
            ModelState.Remove("StockTransactions");
            ModelState.Remove("Customer");
            ModelState.Remove("OrderCode"); // Bỏ validate OrderCode vì ta sẽ tự sinh

            if (ModelState.IsValid)
            {
                try
                {
                    // 1. TỰ ĐỘNG SINH MÃ HỆ THỐNG (Định danh tuyệt đối không trùng)
                    // Định dạng: SYS-NămThángNgày-Random (VD: SYS-260727-4582)
                    order.OrderCode = $"SYS-{DateTime.Now:yyMMdd}-{new Random().Next(1000, 9999)}";

                    // 2. MÃ ĐƠN GIẤY (PaperOrderCode)
                    // Sếp sẽ nhập "DH-001" từ giao diện vào biến order.PaperOrderCode. 
                    // Hệ thống cho phép trùng lặp thoải mái nên không cần hàm Check Exists nữa.
                    if (string.IsNullOrEmpty(order.PaperOrderCode))
                    {
                        order.PaperOrderCode = "Chưa có mã giấy";
                    }

                    order.DesignFile = await UploadFile(designFile);
                    order.Status = OrderStatus.PendingApproval;

                    // 3. TÍNH TỔNG TIỀN
                    if (order.OrderDetails != null)
                    {
                        order.TotalAmount = order.OrderDetails.Sum(x => (x.Quantity * x.UnitPrice) * (1 + (decimal)x.VatPercent / 100));
                    }

                    _context.Add(order);
                    await _context.SaveChangesAsync();

                    await SendOrderMessage(order, "📦 Đơn Hàng Mới");
                    await _automation.Trigger("new-order", new { OrderId = order.Id, Code = order.PaperOrderCode, SystemCode = order.OrderCode, Customer = order.Customer, Total = order.TotalAmount, Date = DateTime.Now });

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi: " + ex.Message);
                }
            }

            LoadCustomerList(order.CustomerId);
            ViewBag.ProductList = await GetFullProductList();
            return View(order);
        }

        // ==========================================
        // 4. EDIT
        // ==========================================
        [HttpGet]
        [Authorize(Policy = AppPermissions.Orders.Edit)]
        public async Task<IActionResult> Edit(int id)
        {
            //if (!UserHasPermission(AppPermissions.Orders.Edit)) return AccessDenied();
            var order = await _context.Orders.Include(o => o.OrderDetails).FirstOrDefaultAsync(x => x.Id == id);
            if (order == null || order.IsLocked || order.Status == OrderStatus.Completed) return RedirectToAction(nameof(Index));
            LoadCustomerList(order.CustomerId); ViewBag.ProductList = await GetFullProductList();
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = AppPermissions.Orders.Edit)]
        public async Task<IActionResult> Edit(int id, Order order, IFormFile? designFile)
        {
            // 1. Kiểm tra ID
            if (id != order.Id) return NotFound();

            // 2. Bỏ qua validate Customer
            ModelState.Remove("Customer");

            if (ModelState.IsValid)
            {
                // 3. Lấy đơn hàng cũ từ DB (Kèm chi tiết)
                var existingOrder = await _context.Orders
                    .Include(o => o.OrderDetails)
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (existingOrder == null) return NotFound();

                // --- CẬP NHẬT THÔNG TIN CHUNG ---
                existingOrder.OrderCode = order.OrderCode;
                existingOrder.DeliveryDeadline = order.DeliveryDeadline;
                existingOrder.Notes = order.Notes;
                existingOrder.CustomerId = order.CustomerId;
                if (designFile != null) existingOrder.DesignFile = await UploadFile(designFile);

                // --- XỬ LÝ CHI TIẾT ĐƠN HÀNG ---
                // A. Xóa các dòng đã bị user xóa trên giao diện
                var submittedIds = order.OrderDetails?.Select(x => x.Id).Where(x => x > 0).ToList() ?? new List<int>();
                var itemsToDelete = existingOrder.OrderDetails.Where(x => !submittedIds.Contains(x.Id)).ToList();
                if (itemsToDelete.Any()) _context.OrderDetails.RemoveRange(itemsToDelete);

                // B. Cập nhật hoặc Thêm mới
                if (order.OrderDetails != null)
                {
                    var allProducts = await _context.Products.ToListAsync();
                    var allDetails = await _context.ProductDetails.Include(x => x.Product).ToListAsync();

                    foreach (var item in order.OrderDetails)
                    {
                        int? pId = null;

                        // Tìm ID
                        var parent = allProducts.FirstOrDefault(x => x.ProductName == item.ProductName);
                        if (parent != null) pId = parent.Id;
                        else
                        {
                            var child = allDetails.FirstOrDefault(x => $"{x.Product.ProductName} - {x.VariantName}" == item.ProductName);
                            if (child != null) pId = child.ProductId;
                        }

                        if (item.Id > 0)
                        {
                            // --- CASE 1: CẬP NHẬT DÒNG CŨ ---
                            var exist = existingOrder.OrderDetails.FirstOrDefault(x => x.Id == item.Id);
                            if (exist != null)
                            {
                                exist.ProductName = item.ProductName;
                                exist.ProductId = pId;
                                exist.Specifications = item.Specifications;
                                exist.Quantity = item.Quantity;
                                exist.UnitPrice = item.UnitPrice;
                                exist.VatPercent = item.VatPercent;
                                exist.Notes = item.Notes;

                                // 🔥 ĐÃ FIX LỖI TẠI ĐÂY: Lưu lại cờ phân biệt Mẹ / Con
                                exist.IsComponent = item.IsComponent;
                                exist.BundleCode = item.BundleCode;
                                exist.ExcludedMaterials = item.ExcludedMaterials;
                            }
                        }
                        else
                        {
                            // --- CASE 2: THÊM DÒNG MỚI ---
                            existingOrder.OrderDetails.Add(new OrderDetail
                            {
                                ProductName = item.ProductName,
                                ProductId = pId,
                                Specifications = item.Specifications,
                                Quantity = item.Quantity,
                                UnitPrice = item.UnitPrice,
                                VatPercent = item.VatPercent,
                                Notes = item.Notes,

                                // 🔥 ĐÃ FIX LỖI TẠI ĐÂY: Lưu lại cờ phân biệt Mẹ / Con
                                IsComponent = item.IsComponent,
                                BundleCode = item.BundleCode,
                                ExcludedMaterials = item.ExcludedMaterials
                            });
                        }
                    }
                }

                // 4. TÍNH LẠI TỔNG TIỀN (Công thức có VAT)
                existingOrder.TotalAmount = existingOrder.OrderDetails.Sum(x =>
                    (x.Quantity * x.UnitPrice) * (1 + (decimal)x.VatPercent / 100)
                );

                _context.Update(existingOrder);
                await _context.SaveChangesAsync();
              
                await SendOrderMessage(existingOrder, "✍️ Đơn Hàng Có Thay Đổi");

                // return RedirectToAction(nameof(Index));
                // 2. 🔥 GỌI ĐỒNG BỘ SANG WORKORDER NGAY LẬP TỨC
                return RedirectToAction("SyncOrderToWorkOrders", "ProductionPlan", new { orderId = existingOrder.Id });
            }

            // Nếu lỗi Validation -> Load lại dữ liệu để hiển thị Form
            LoadCustomerList(order.CustomerId);
            ViewBag.ProductList = await GetFullProductList();
            return View(order);
        }

        // ==========================================
        // 5. DELETE
        // ==========================================
        /*[HttpGet]
        [Authorize(Policy = AppPermissions.Orders.Delete)]
        public async Task<IActionResult> Delete(int id)
        {
            var order = await _context.Orders.Include(o => o.OrderDetails).Include(o => o.Shipments).FirstOrDefaultAsync(x => x.Id == id);
            if (order != null && order.Status != OrderStatus.Completed && !order.IsLocked)
            {
                if (!string.IsNullOrEmpty(order.DesignFile))
                {
                    string p = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", order.DesignFile);
                    if (System.IO.File.Exists(p)) System.IO.File.Delete(p);
                }
                if (order.OrderDetails != null) _context.OrderDetails.RemoveRange(order.OrderDetails);
                if (order.Shipments != null) _context.Shipments.RemoveRange(order.Shipments);
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();

                SendOrderMessage(order, "❌Đơn Hàng Bị Huỷ");
            }
            return RedirectToAction(nameof(Index));
        }
        */

        [HttpGet] // Note: Trong source gốc của bạn nó đang là HttpGet, bạn có thể đổi thành HttpPost ở Route nếu view dùng POST
        [Authorize(Policy = AppPermissions.Orders.Delete)]
        public async Task<IActionResult> Delete(int id)
        {
            var order = await _context.Orders
                .Include(o => o.InvoiceDetails)
                .Include(o => o.OrderDetails)
                .Include(o => o.Shipments)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (order != null && order.Status != OrderStatus.Completed && !order.IsLocked)
            {
                // 🔥 KHIÊN BẢO VỆ
                if (order.InvoiceDetails.Any())
                    return Json(new { success = false, message = "LỖI: Đơn hàng này ĐÃ XUẤT HÓA ĐƠN. Bạn phải xóa Hóa đơn trước!" });

                bool hasPayments = await _context.CashEntries.AnyAsync(c => c.OrderId == id);
                if (hasPayments)
                    return Json(new { success = false, message = "LỖI: Đơn hàng này đã CÓ PHIẾU THU TIỀN. Vui lòng xóa phiếu thu trước!" });

                if (!string.IsNullOrEmpty(order.DesignFile))
                {
                    string p = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", order.DesignFile);
                    if (System.IO.File.Exists(p)) System.IO.File.Delete(p);
                }

                if (order.OrderDetails != null) _context.OrderDetails.RemoveRange(order.OrderDetails);
                if (order.Shipments != null) _context.Shipments.RemoveRange(order.Shipments);

                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();

                await SendOrderMessage(order, "❌Đơn Hàng Bị Huỷ");
            }
            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // 6. APPROVE
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = AppPermissions.Orders.Approve)]
        public async Task<IActionResult> ApproveOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            if (order.Status == OrderStatus.PendingApproval)
            {
                order.Status = OrderStatus.InProduction;
                await _context.SaveChangesAsync();

                string msg = $"*Thông Tin*:\n" +
                    $"PO: `{order.OrderCode}`\n" +
                    $"✅Đã Duyệt\n" +
                    $"Ghi Chú: `{order.Notes}`\n";
                _ = NotificationHelper.SendTelegram(_config.TelegramToken, _config.TelegramChatId, msg);
                _ = NotificationHelper.SendDiscord(_config.DiscordWebhook, msg);
            }
            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // 7. INVOICE ACTIONS 
        // ==========================================
        [HttpPost]
        [Authorize(Policy = AppPermissions.Orders.Edit)]
        public async Task<IActionResult> UploadInvoice(int id, string invoiceNumber, DateTime invoiceDate, decimal billedAmount, IFormFile invoiceFile)
        {
            var order = await _context.Orders.Include(o => o.InvoiceDetails).FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return Json(new { success = false, message = "Không tìm thấy đơn hàng!" });

            // Ràng buộc chống xuất lố tiền
            decimal alreadyBilled = order.InvoiceDetails.Sum(d => d.BilledAmount);
            if (alreadyBilled + billedAmount > order.TotalAmount)
            {
                return Json(new { success = false, message = $"Lỗi: Tổng tiền xuất HĐ ({alreadyBilled + billedAmount:N0}) vượt quá giá trị đơn hàng ({order.TotalAmount:N0})!" });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var invoice = new Invoice
                {
                    InvoiceNumber = invoiceNumber,
                    InvoiceDate = invoiceDate,
                    TotalAmount = billedAmount
                };

                if (invoiceFile != null && invoiceFile.Length > 0)
                {
                    string yearFolder = invoiceDate.ToString("yyyy");
                    string monthFolder = invoiceDate.ToString("MM");
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "invoices", yearFolder, monthFolder);
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    string uniqueFileName = $"INV_{order.OrderCode}_{DateTime.Now.Ticks}{Path.GetExtension(invoiceFile.FileName)}";
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await invoiceFile.CopyToAsync(fileStream);
                    }
                    invoice.InvoiceFile = $"{yearFolder}/{monthFolder}/{uniqueFileName}";
                }

                _context.Invoices.Add(invoice);
                await _context.SaveChangesAsync();

                var detail = new InvoiceDetail
                {
                    InvoiceId = invoice.Id,
                    OrderId = order.Id,
                    BilledAmount = billedAmount
                };
                _context.InvoiceDetails.Add(detail);

                if (alreadyBilled + billedAmount >= order.TotalAmount)
                {
                    if (order.Status != OrderStatus.Completed && order.Status != OrderStatus.Canceled)
                        order.Status = OrderStatus.Invoiced;
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

        // Delete post
        /*
        [HttpPost]
        [Authorize(Policy = AppPermissions.Orders.Delete)]
        public async Task<IActionResult> DeleteInvoice(int detailId)
        {
            var detail = await _context.InvoiceDetails.Include(d => d.Invoice).Include(d => d.Order).FirstOrDefaultAsync(d => d.Id == detailId);
            if (detail == null) return Json(new { success = false, message = "Không tìm thấy bản ghi hóa đơn này!" });

            try
            {
                var order = detail.Order;
                var invoice = detail.Invoice;

                // 1. Xóa bản ghi phân bổ
                _context.InvoiceDetails.Remove(detail);
                await _context.SaveChangesAsync();

                // 2. Nếu tờ Hóa đơn gốc không còn Đơn hàng nào liên kết -> Xóa luôn tờ Hóa đơn & File vật lý
                var remainingDetails = await _context.InvoiceDetails.AnyAsync(d => d.InvoiceId == invoice.Id);
                if (!remainingDetails)
                {
                    if (!string.IsNullOrEmpty(invoice.InvoiceFile))
                    {
                        string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "invoices", invoice.InvoiceFile);
                        if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
                    }
                    _context.Invoices.Remove(invoice);
                }

                // 3. Cập nhật lại trạng thái Đơn hàng (Nếu xóa HĐ làm số tiền xuất < Tổng tiền -> Lùi trạng thái)
                var totalBilledNow = await _context.InvoiceDetails.Where(d => d.OrderId == order.Id).SumAsync(d => d.BilledAmount);
                if (totalBilledNow < order.TotalAmount && order.Status == OrderStatus.Invoiced)
                {
                    // 🔥 AUTO SET LOGIC: Kiểm tra số lượng đã xuất kho thực tế để quyết định trạng thái lùi về
                    var orderDetails = await _context.OrderDetails.Where(od => od.OrderId == order.Id).ToListAsync();
                    double totalOrdered = orderDetails.Sum(od => od.Quantity);

                    var detailIds = orderDetails.Select(od => od.Id).ToList();
                    double totalShipped = await _context.ShipmentDetails
                        .Where(sd => detailIds.Contains(sd.OrderDetailId) && sd.Shipment.Type == ShipmentType.Standard) // Tính theo phiếu xuất hàng chuẩn
                        .SumAsync(sd => (double?)sd.QuantityShipped) ?? 0;

                    if (totalShipped >= totalOrdered && totalOrdered > 0)
                        order.Status = OrderStatus.Delivered; // Đã giao đủ -> Lùi về Đã giao hàng
                    else
                        order.Status = OrderStatus.InProduction; // Chưa giao đủ -> Lùi về Đang sản xuất

                    _context.Update(order);
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Đã xóa hóa đơn thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        */

        [HttpPost]
        [Authorize(Policy = AppPermissions.Orders.Delete)]
        public async Task<IActionResult> DeleteInvoice(int detailId)
        {
            var detail = await _context.InvoiceDetails.Include(d => d.Invoice).Include(d => d.Order).FirstOrDefaultAsync(d => d.Id == detailId);
            if (detail == null) return Json(new { success = false, message = "Không tìm thấy bản ghi hóa đơn này!" });

            try
            {
                var order = detail.Order;
                var invoice = detail.Invoice;

                _context.InvoiceDetails.Remove(detail);
                await _context.SaveChangesAsync();

                var remainingDetails = await _context.InvoiceDetails.AnyAsync(d => d.InvoiceId == invoice.Id);
                if (!remainingDetails)
                {
                    if (!string.IsNullOrEmpty(invoice.InvoiceFile))
                    {
                        string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "invoices", invoice.InvoiceFile);
                        if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
                    }
                    _context.Invoices.Remove(invoice);
                }

                // 🔥 LÙI TRẠNG THÁI
                var totalBilledNow = await _context.InvoiceDetails.Where(d => d.OrderId == order.Id).SumAsync(d => d.BilledAmount);
                if (totalBilledNow < order.TotalAmount && order.Status == OrderStatus.Invoiced)
                {
                    var orderDetails = await _context.OrderDetails.Where(od => od.OrderId == order.Id).ToListAsync();
                    double totalOrdered = orderDetails.Sum(od => od.Quantity);

                    var detailIds = orderDetails.Select(od => od.Id).ToList();
                    double totalShipped = await _context.ShipmentDetails
                        .Where(sd => detailIds.Contains(sd.OrderDetailId) && sd.Shipment.Type == ShipmentType.Standard)
                        .SumAsync(sd => (double?)sd.QuantityShipped) ?? 0;

                    if (totalShipped >= totalOrdered && totalOrdered > 0)
                        order.Status = OrderStatus.Delivered;
                    else
                        order.Status = OrderStatus.InProduction;

                    _context.Update(order);
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Đã xóa hóa đơn thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }



        [HttpPost]
        [Authorize(Policy = AppPermissions.Orders.Edit)]

        public async Task<IActionResult> UpdateInvoice(int detailId, string invoiceNumber, DateTime invoiceDate, decimal billedAmount, IFormFile? invoiceFile)
        {
            var detail = await _context.InvoiceDetails
                .Include(d => d.Invoice)
                .Include(d => d.Order)
                .FirstOrDefaultAsync(d => d.Id == detailId);

            if (detail == null) return Json(new { success = false, message = "Không tìm thấy bản ghi hóa đơn!" });

            // Kiểm tra không được xuất lố cho Đơn hàng này (Tính tổng các lần xuất khác + lần cập nhật này)
            decimal otherBilledForOrder = await _context.InvoiceDetails
                .Where(d => d.OrderId == detail.OrderId && d.Id != detailId)
                .SumAsync(d => d.BilledAmount);

            if (otherBilledForOrder + billedAmount > detail.Order.TotalAmount)
            {
                return Json(new { success = false, message = $"Lỗi: Vượt quá giá trị đơn hàng. Tối đa chỉ có thể nhập {(detail.Order.TotalAmount - otherBilledForOrder):N0} đ!" });
            }

            // 🔥 BỌC TRANSACTION ĐỂ BẢO VỆ TÍNH TOÀN VẸN DỮ LIỆU
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Cập nhật số tiền mới cho bảng trung gian (InvoiceDetail)
                detail.BilledAmount = billedAmount;
                _context.Update(detail);

                // 2. Cập nhật Hóa đơn gốc
                var invoice = detail.Invoice;
                invoice.InvoiceNumber = invoiceNumber;
                invoice.InvoiceDate = invoiceDate;

                // 🔥 FIX LỖI: TÍNH LẠI TỔNG TIỀN CỦA HÓA ĐƠN LÔ (INVOICE)
                decimal otherDetailsInThisInvoice = await _context.InvoiceDetails
                    .Where(d => d.InvoiceId == invoice.Id && d.Id != detailId)
                    .SumAsync(d => d.BilledAmount);

                // Tổng Hóa đơn = Tiền của các đơn khác trong lô + Tiền mới sửa của đơn này
                invoice.TotalAmount = otherDetailsInThisInvoice + billedAmount;

                // 3. Xử lý Upload File mới (nếu có)
                if (invoiceFile != null && invoiceFile.Length > 0)
                {
                    // Xóa file cũ
                    if (!string.IsNullOrEmpty(invoice.InvoiceFile))
                    {
                        string oldPath = Path.Combine(_webHostEnvironment.WebRootPath, "invoices", invoice.InvoiceFile);
                        if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                    }

                    // Up file mới
                    string yearFolder = invoiceDate.ToString("yyyy");
                    string monthFolder = invoiceDate.ToString("MM");
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "invoices", yearFolder, monthFolder);
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    string uniqueFileName = $"INV_{detail.Order.OrderCode}_{DateTime.Now.Ticks}{Path.GetExtension(invoiceFile.FileName)}";
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await invoiceFile.CopyToAsync(fileStream);
                    }
                    invoice.InvoiceFile = $"{yearFolder}/{monthFolder}/{uniqueFileName}";
                }

                // 4. Cập nhật trạng thái Order nếu đổi tiền làm đủ/thiếu (Tự lùi trạng thái an toàn)
                if (otherBilledForOrder + billedAmount >= detail.Order.TotalAmount && detail.Order.Status != OrderStatus.Completed && detail.Order.Status != OrderStatus.Canceled)
                {
                    detail.Order.Status = OrderStatus.Invoiced;
                }
                else if (otherBilledForOrder + billedAmount < detail.Order.TotalAmount && detail.Order.Status == OrderStatus.Invoiced)
                {
                    // AUTO SET LOGIC: Tính lại trạng thái chính xác dựa vào số lượng đã xuất kho
                    var orderDetails = await _context.OrderDetails.Where(od => od.OrderId == detail.Order.Id).ToListAsync();
                    double totalOrdered = orderDetails.Sum(od => od.Quantity);

                    var detailIds = orderDetails.Select(od => od.Id).ToList();
                    double totalShipped = await _context.ShipmentDetails
                        .Where(sd => detailIds.Contains(sd.OrderDetailId) && sd.Shipment.Type == ShipmentType.Standard) // Tính theo phiếu xuất hàng chuẩn
                        .SumAsync(sd => (double?)sd.QuantityShipped) ?? 0;

                    if (totalShipped >= totalOrdered && totalOrdered > 0)
                        detail.Order.Status = OrderStatus.Delivered;
                    else
                        detail.Order.Status = OrderStatus.InProduction;
                }

                _context.Update(invoice);
                _context.Update(detail.Order);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync(); // Xác nhận giao dịch thành công

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(); // Rollback nếu có bất kỳ lỗi nào
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ==========================================
        // HELPERS
        // ==========================================
        private void LoadCustomerList(int? selectedId = null)
        {
            var raw = _context.Customers.Select(c => new { c.Id, c.CompanyName, c.CustomerCode }).ToList();
            ViewData["CustomerId"] = new SelectList(raw.Select(c => new { Id = c.Id, DisplayText = $"{c.CustomerCode} - {c.CompanyName}" }), "Id", "DisplayText", selectedId);
        }

        // ==========================================
        // 1. LẤY DANH SÁCH SP & ĐỊNH MỨC CON
        // ==========================================
        private async Task<object> GetFullProductList()
        {
            var list = new List<object>();
            var products = await _context.Products.Include(p => p.ProductDetails).ToListAsync();

            foreach (var p in products)
            {
                var childrenList = p.ProductDetails?.Select(c => new {
                    Id = c.Id,
                    Name = c.VariantName,
                    Price = c.Price,
                    Specs = $"{c.Thick}x{c.Width}x{c.Length}",
                    BaseQty = c.Quantity > 0 ? c.Quantity : 1
                }).ToList();

                list.Add(new
                {
                    Id = p.Id,
                    ProductName = p.ProductName ?? "",
                    DefaultPrice = p.DefaultPrice,
                    Specs = $"{p.Thick}x{p.Width}x{p.Length}",
                    Type = "PARENT",
                    Children = childrenList
                });
            }
            return list.OrderBy(x => ((dynamic)x).ProductName).ToList();
        }

        // ==========================================
        // 2. API LẤY VẬT TƯ GỐC CHO MINI-TAB TẠO ĐƠN (Phân biệt Mẹ / Con)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> GetProductMaterials(int productId, bool isComponent)
        {
            try
            {
                if (!isComponent)
                {
                    var product = await _context.Products.Include(p => p.Materials).ThenInclude(m => m.WarehouseItem).FirstOrDefaultAsync(p => p.Id == productId);
                    if (product == null || product.Materials == null || !product.Materials.Any())
                        return Json(new { success = true, data = new List<object>() });

                    var materials = product.Materials.Select(m => new {
                        id = m.WarehouseItemId,
                        name = m.WarehouseItem?.Name ?? "N/A",
                        unit = m.WarehouseItem?.Unit ?? "Cái",
                        norm = m.Quantity
                    }).ToList();

                    // 🔥 ĐÃ FIX LỖI THIẾU DẤU BẰNG Ở DÒNG NÀY:
                    return Json(new { success = true, data = materials });
                }
                else
                {
                    // Lấy vật tư cho Chi tiết con (Dùng reflection an toàn nếu DB hỗ trợ)
                    var detailHasMaterials = typeof(ProductDetail).GetProperty("Materials") != null;
                    if (detailHasMaterials)
                    {
                        var materials = await _context.Set<Material>().Include(m => m.WarehouseItem).Where(m => EF.Property<int?>(m, "ProductDetailId") == productId).Select(m => new {
                            id = m.WarehouseItemId,
                            name = m.WarehouseItem.Name,
                            unit = m.WarehouseItem.Unit,
                            norm = m.Quantity
                        }).ToListAsync();
                        return Json(new { success = true, data = materials });
                    }
                    return Json(new { success = true, data = new List<object>() });
                }
            }
            catch (Exception ex) { return Json(new { success = false, message = "Lỗi Material: " + ex.Message }); }
        }

        // ==========================================
        // 3. TÍNH DỰ TOÁN TỔNG HỢP CHO INDEX & DETAILS (Không bị x2)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> GetOrderBom(int id)
        {
            try
            {
                var orderDetails = await _context.OrderDetails.Where(x => x.OrderId == id).ToListAsync();
                if (!orderDetails.Any()) return Json(new { success = false, message = "Đơn hàng trống" });

                var materialMap = new Dictionary<string, (string Unit, double Qty, double Stock)>();

                // Chuẩn bị dữ liệu
                var allProducts = await _context.Products.Include(p => p.Materials).ThenInclude(m => m.WarehouseItem).ToListAsync();
                bool detailHasMaterials = typeof(ProductDetail).GetProperty("Materials") != null;
                var allChildMaterials = new List<Material>();
                if (detailHasMaterials) allChildMaterials = await _context.Set<Material>().Include(m => m.WarehouseItem).Where(m => EF.Property<int?>(m, "ProductDetailId") != null).ToListAsync();

                foreach (var item in orderDetails)
                {
                    if (!item.ProductId.HasValue) continue;

                    var excludedIds = string.IsNullOrEmpty(item.ExcludedMaterials) ? new List<int>() : item.ExcludedMaterials.Split(',').Where(x => !string.IsNullOrEmpty(x)).Select(int.Parse).ToList();

                    IEnumerable<Material> boms = new List<Material>();
                    if (!item.IsComponent)
                    {
                        var p = allProducts.FirstOrDefault(x => x.Id == item.ProductId.Value);
                        if (p != null && p.Materials != null) boms = p.Materials;
                    }
                    else if (detailHasMaterials)
                    {
                        boms = allChildMaterials.Where(m => EF.Property<int?>(m, "ProductDetailId") == item.ProductId.Value);
                    }

                    foreach (var m in boms)
                    {
                        if (m.WarehouseItemId == 0 || excludedIds.Contains(m.WarehouseItemId)) continue;

                        double neededQty = m.Quantity * item.Quantity; // SL Đặt * Định Mức
                        string mName = m.WarehouseItem?.Name ?? "Vật tư không tên";

                        if (materialMap.ContainsKey(mName))
                        {
                            var current = materialMap[mName];
                            materialMap[mName] = (current.Unit, current.Qty + neededQty, current.Stock);
                        }
                        else
                        {
                            materialMap[mName] = (m.WarehouseItem?.Unit ?? "Cái", neededQty, m.WarehouseItem?.StockQuantity ?? 0);
                        }
                    }
                }

                if (!materialMap.Any()) return Json(new { success = false, message = "Sản phẩm trong đơn không có vật tư gốc cấu thành." });

                var result = materialMap.Select(x => new { name = x.Key, unit = x.Value.Unit, qty = x.Value.Qty, stock = x.Value.Stock }).OrderBy(x => x.name).ToList();
                return Json(new { success = true, data = result });
            }
            catch (Exception ex) { return Json(new { success = false, message = "Lỗi hệ thống BOM: " + ex.Message }); }
        }


        private async Task<string?> UploadFile(IFormFile? file)
        {
            if (file != null && file.Length > 0)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
                using (var fileStream = new FileStream(Path.Combine(uploadsFolder, uniqueFileName), FileMode.Create)) { await file.CopyToAsync(fileStream); }
                return uniqueFileName;
            }
            return null;
        }

        private string GetLocalIpAddress()
        {
            var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) return ip.ToString();
            }
            return "localhost";
        }

        


        private async Task SendOrderMessage(Order order, string header)
        {
            // --- NOTIFICATION ---
            var currentUser = await _userManager.GetUserAsync(User);
            string creatorName = currentUser?.FullName;

            string domain = Request.Host.Value;
            string scheme = Request.Scheme;
            if (domain.Contains("localhost") || domain.Contains("127.0.0.1"))
            {
                domain = $"{GetLocalIpAddress()}:{Request.Host.Port}";
                scheme = "http";
            }

            var detailsUrl = $"{scheme}://{domain}/Order/Details/{order.Id}";
            var customerName = "Khách lẻ";
            if (order.CustomerId > 0)
            {
                var cust = await _context.Customers.FindAsync(order.CustomerId);
                if (cust != null) customerName = cust.CompanyName;
            }

            // Sửa lại đoạn này
            string telegramMsg = $@"
                *{header}*
                ──────────────
                🆔 *Mã đơn:* `{order.OrderCode}`
                🏢 *Khách hàng:* `{customerName}`
                📅 *Ngày đặt:* {order.OrderDate:dd/MM/yyyy}
                📦 *Ngày giao:* {order.DeliveryDeadline:dd/MM/yyyy}
                👤 *Người tạo:* {creatorName}
                ──────────────
                👉 [Xem chi tiết]({detailsUrl})
                ";

            string discordMsg = $@"
                **{header}**
                ────────────────
                🆔 **Mã đơn:** `{order.OrderCode}`
                🏢 **Khách hàng:** `{customerName}`
                📅 **Ngày đặt:** {order.OrderDate:dd/MM/yyyy}
                📦 **Ngày giao:** {order.DeliveryDeadline:dd/MM/yyyy}
                👤 **Người tạo:** **{creatorName}**
                ────────────────
                👉 [Xem chi tiết]({detailsUrl})
                ";

            _ = NotificationHelper.SendTelegram(_config.TelegramToken, _config.TelegramChatId, telegramMsg);
            _ = NotificationHelper.SendDiscord(_config.DiscordWebhook, discordMsg);
        }

        // ==========================================
        // 8. API TÌM KIẾM SẢN PHẨM (REAL-TIME SELECT2)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> SearchProducts(string term)
        {
            if (string.IsNullOrWhiteSpace(term)) return Json(new { results = new object[] { } });

            term = term.ToLower();

            // 1. Tìm trong Sản phẩm cha (Parent)
            var parents = await _context.Products
                .Where(p => p.ProductName.ToLower().Contains(term))
                .Select(p => new
                {
                    Id = p.Id,
                    Type = "PARENT",
                    Name = p.ProductName,
                    Price = p.DefaultPrice,
                    // Tự động ghép quy cách: Dày x Rộng x Dài
                    Specs = $"{p.Thick}x{p.Width}x{p.Length}",
                    Text = $"📦 {p.ProductName} ({p.DefaultPrice:N0}đ)"
                })
                .Take(20) // Lấy tối đa 20 kết quả
                .ToListAsync();

            // 2. Tìm trong Biến thể (Child)
            var variants = await _context.ProductDetails
                .Include(v => v.Product)
                .Where(v => v.VariantName.ToLower().Contains(term) || (v.Product != null && v.Product.ProductName.ToLower().Contains(term)))
                .Select(v => new
                {
                    Id = v.Id,
                    Type = "CHILD",
                    Name = $"{v.Product.ProductName} - {v.VariantName}",
                    Price = v.Price,
                    Specs = $"{v.Thick}x{v.Width}x{v.Length}",
                    Text = $"🔹 {v.Product.ProductName} - {v.VariantName} ({v.Price:N0}đ)"
                })
                .Take(20)
                .ToListAsync();

            // Gộp lại
            var results = parents.Cast<object>().Concat(variants).Take(20);

            return Json(new { results = results });
        }

        // ==========================================
        // 9. QUICK EDIT (AJAX RIGHT CLICK)
        // ==========================================
        [HttpPost]
        [Authorize(Policy = AppPermissions.Orders.Edit)]
        public async Task<IActionResult> QuickUpdate(int id, string field, string value)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return Json(new { success = false, message = "Không tìm thấy đơn hàng" });
            if (order.IsLocked) return Json(new { success = false, message = "Đơn hàng đã bị khóa, không thể sửa." });

            try
            {
                switch (field)
                {
                    case "Status":
                        if (Enum.TryParse(value, out OrderStatus status)) order.Status = status;
                        break;
                    case "Customer":
                        if (int.TryParse(value, out int custId)) order.CustomerId = custId;
                        break;
                    case "OrderDate":
                        if (DateTime.TryParse(value, out DateTime date)) order.OrderDate = date;
                        break;
                    case "DeliveryDeadline":
                        if (DateTime.TryParse(value, out DateTime dl)) order.DeliveryDeadline = dl;
                        break;
                    case "PaperOrderCode":
                        order.PaperOrderCode = value;
                        break;
                    default:
                        return Json(new { success = false, message = "Trường dữ liệu không hợp lệ" });
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Thêm thư viện: using ClosedXML.Excel;

        /*
        [HttpGet]
        public async Task<IActionResult> ExportExcel(DateTime? fromDate, DateTime? toDate, int? customerId, OrderStatus? status, string keyword)
        {
            // 1. Tái sử dụng logic lọc dữ liệu giống hệt trang Index
            var query = _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails)
                .AsQueryable();

            if (fromDate.HasValue) query = query.Where(x => x.OrderDate >= fromDate.Value);
            if (toDate.HasValue) query = query.Where(x => x.OrderDate <= toDate.Value);
            if (customerId.HasValue) query = query.Where(x => x.CustomerId == customerId);
            if (status.HasValue)
                query = query.Where(o => o.Status == status.Value);
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(x => x.OrderCode.Contains(keyword) || x.Customer.CompanyName.Contains(keyword));
            }

            var data = await query.OrderByDescending(x => x.OrderDate).ToListAsync();

            // 2. Tạo file Excel
            using (var wb = new XLWorkbook())
            {
                var ws = wb.Worksheets.Add("DonHang");

                // Header
                var headers = new[] { "Mã Đơn", "Chứng từ gốc", "Khách hàng", "Ngày đặt", "Ngày hẹn", "Tổng tiền", "Đã thu", "Còn lại", "Tiến độ", "Trạng thái" };
                for (int i = 0; i < headers.Length; i++)
                {
                    ws.Cell(1, i + 1).Value = headers[i];
                }

                // Style Header
                var headerRange = ws.Range(1, 1, 1, headers.Length);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#4e73df");
                headerRange.Style.Font.FontColor = XLColor.White;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Data
                int row = 2;
                foreach (var item in data)
                {
                    ws.Cell(row, 1).Value = item.OrderCode;
                    ws.Cell(row, 2).Value = item.PaperOrderCode;
                    ws.Cell(row, 3).Value = item.Customer?.CompanyName;
                    ws.Cell(row, 4).Value = item.OrderDate;
                    ws.Cell(row, 5).Value = item.DeliveryDeadline;
                    ws.Cell(row, 6).Value = item.TotalAmount;

                    // Tính toán tài chính (giả định logic lấy PaidAmount)
                    // Lưu ý: Nếu bạn có bảng Payment riêng, cần Include và Sum ở trên. 
                    // Ở đây mình để tạm PaidAmount = 0 hoặc lấy từ field nếu có.
                    decimal paid = 0; // Thay bằng logic thực tế của bạn
                    ws.Cell(row, 7).Value = paid;
                    ws.Cell(row, 8).Value = item.TotalAmount - paid;

                    // Tiến độ SX (giả định logic)
                    // ws.Cell(row, 9).Value = ...

                    ws.Cell(row, 10).Value = item.Status.ToString();

                    row++;
                }

                // Format
                ws.Columns().AdjustToContents();
                ws.Column(4).Style.DateFormat.Format = "dd/MM/yyyy";
                ws.Column(5).Style.DateFormat.Format = "dd/MM/yyyy";
                ws.Columns(6, 8).Style.NumberFormat.Format = "#,##0";

                using (var stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"DS_DonHang_{DateTime.Now:ddMMyy}.xlsx");
                }
            }
        }
        */

        // ==========================================
        // 9. EXPORT EXCEL (THEO ĐÚNG FORMAT CỘT MỚI)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> ExportExcel(DateTime? fromDate, DateTime? toDate, int? customerId, OrderStatus? status)
        {
            var query = _context.Orders.Include(o => o.Customer).Include(o => o.OrderDetails).AsQueryable();
            if (fromDate.HasValue) query = query.Where(x => x.OrderDate >= fromDate.Value);
            if (toDate.HasValue) query = query.Where(x => x.OrderDate <= toDate.Value);
            if (customerId.HasValue) query = query.Where(x => x.CustomerId == customerId);
            if (status.HasValue) query = query.Where(o => o.Status == status.Value);

            var data = await query.OrderByDescending(x => x.OrderDate).ToListAsync();

            using (var wb = new XLWorkbook())
            {
                var ws = wb.Worksheets.Add("DonHang");

                // Cố định 11 cột đúng theo Format yêu cầu
                string[] headers = { "STT", "Đơn hàng", "Khách Hàng", "Tên sản phẩm", "Số Lượng Tự Nhiên", "Số Lượng Màu", "Tổng cộng", "Loại gỗ", "Ngày giao", "Ngày Nhận", "Ghi chú" };
                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = ws.Cell(1, i + 1);
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#E2E8F0");
                    cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }

                int row = 2; int stt = 1;
                foreach (var order in data)
                {
                    foreach (var detail in order.OrderDetails)
                    {
                        ws.Cell(row, 1).Value = stt++;
                        ws.Cell(row, 2).Value = order.OrderCode;
                        ws.Cell(row, 3).Value = order.Customer?.CompanyName;
                        ws.Cell(row, 4).Value = detail.ProductName;
                        ws.Cell(row, 5).Value = ""; // Số Lượng Tự Nhiên (Hệ thống không có cột này, để trống)
                        ws.Cell(row, 6).Value = ""; // Số Lượng Màu (Hệ thống không có cột này, để trống)
                        ws.Cell(row, 7).Value = detail.Quantity; // SỐ LƯỢNG HỆ THỐNG NẰM Ở CỘT TỔNG CỘNG
                        ws.Cell(row, 8).Value = ""; // Loại gỗ (Để trống)
                        ws.Cell(row, 9).Value = order.DeliveryDeadline?.ToString("dd/MM/yyyy"); // Ngày giao
                        ws.Cell(row, 10).Value = order.OrderDate.ToString("dd/MM/yyyy");        // Ngày nhận
                        ws.Cell(row, 11).Value = detail.Notes;
                        row++;
                    }
                }
                ws.Columns().AdjustToContents();
                using (var stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Orders_{DateTime.Now:ddMMyy}.xlsx");
                }
            }
        }

        // Hàm phụ trợ phân tích ngày Excel
        private DateTime? GetExcelDate(IXLCell cell)
        {
            if (cell.IsEmpty()) return null;
            if (cell.DataType == XLDataType.DateTime) return cell.GetDateTime();
            string str = cell.GetValue<string>()?.Trim();
            string[] formats = { "dd/MM/yyyy", "d/M/yyyy", "yyyy-MM-dd", "dd-MM-yyyy", "MM/dd/yyyy" };
            if (DateTime.TryParseExact(str, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                return date;
            return null;
        }

        // ==========================================
        // 9.1 PREVIEW IMPORT (CÓ KIỂM TRA SẢN PHẨM / CHI TIẾT)
        // ==========================================
        [HttpPost]
        [Authorize(Policy = AppPermissions.Orders.Create)]
        public async Task<IActionResult> PreviewImport(IFormFile file)
        {
            if (file == null || file.Length == 0) return RedirectToAction(nameof(Index));
            var previewData = new List<OrderImportPreviewModel>();

            // 1. Tải trước dữ liệu Khách hàng
            var customers = await _context.Customers.ToListAsync();

            // 2. Tải trước TẤT CẢ Tên Sản phẩm Mẹ và Tên Chi tiết Con để validate
            var validProductNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var allProducts = await _context.Products.Select(p => p.ProductName).ToListAsync();
            foreach (var p in allProducts) validProductNames.Add(p);

            var allVariants = await _context.ProductDetails.Include(v => v.Product).ToListAsync();
            foreach (var v in allVariants)
            {
                if (v.Product != null)
                {
                    // Tên chi tiết con thường được lưu theo dạng "Tên Mẹ - Tên Con"
                    validProductNames.Add($"{v.Product.ProductName} - {v.VariantName}");
                }
            }

            try
            {
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    using (var wb = new XLWorkbook(stream))
                    {
                        var ws = wb.Worksheet(1);
                        var rows = ws.RowsUsed().Skip(1); // Bỏ qua Header

                        foreach (var row in rows)
                        {
                            string orderCode = row.Cell(2).GetValue<string>()?.Trim();
                            if (string.IsNullOrEmpty(orderCode)) continue;

                            string customerName = row.Cell(3).GetValue<string>()?.Trim();
                            string productName = row.Cell(4).GetValue<string>()?.Trim();

                            string slTuNhien = row.Cell(5).GetValue<string>()?.Trim();
                            string slMau = row.Cell(6).GetValue<string>()?.Trim();
                            double totalQty = row.Cell(7).TryGetValue<double>(out var val7) ? val7 : 0;
                            string woodType = row.Cell(8).GetValue<string>()?.Trim();

                            var customer = customers.FirstOrDefault(c => c.CompanyName.ToLower() == customerName?.ToLower());
                            var isDup = await _context.Orders.AnyAsync(o => o.OrderCode == orderCode);

                            // 🔥 KIỂM TRA SẢN PHẨM CÓ TỒN TẠI KHÔNG
                            bool isProductValid = !string.IsNullOrEmpty(productName) && validProductNames.Contains(productName);

                            previewData.Add(new OrderImportPreviewModel
                            {
                                OrderCode = orderCode,
                                CustomerName = customerName,
                                CustomerId = customer?.Id ?? 0,
                                ProductName = productName,
                                NaturalQtyStr = slTuNhien,
                                ColorQtyStr = slMau,
                                TotalQty = totalQty,
                                WoodType = woodType,
                                Deadline = GetExcelDate(row.Cell(9)),
                                OrderDate = GetExcelDate(row.Cell(10)) ?? DateTime.Now,
                                Note = row.Cell(11).GetValue<string>(),
                                IsDuplicate = isDup,
                                CustomerExists = customer != null,
                                ProductExists = isProductValid // Ghi nhận trạng thái SP
                            });
                        }
                    }
                }
            }
            catch (Exception ex) { TempData["Error"] = "Lỗi đọc file: " + ex.Message; return RedirectToAction(nameof(Index)); }

            return View("ImportPreview", previewData);
        }

        // ==========================================
        // 9.2 LƯU DỮ LIỆU SAU KHI PREVIEW
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> ConfirmImport(string jsonData)
        {
            var items = System.Text.Json.JsonSerializer.Deserialize<List<OrderImportPreviewModel>>(jsonData);
            if (items == null || !items.Any()) return RedirectToAction(nameof(Index));

            // 🔥 BỘ LỌC CỨNG: Chỉ lấy dòng đúng Khách, đúng SP và Không bị trùng Mã đơn
            var validItems = items.Where(x => x.CustomerExists && x.ProductExists && x.CustomerId > 0 && !x.IsDuplicate).ToList();

            if (!validItems.Any())
            {
                TempData["Error"] = "Không có dữ liệu hợp lệ nào để lưu. Vui lòng kiểm tra lại file Excel (Tên Khách và Sản phẩm phải khớp 100%).";
                return RedirectToAction(nameof(Index));
            }

            var orderGroups = validItems.GroupBy(x => x.OrderCode);
            int count = 0;

            foreach (var group in orderGroups)
            {
                if (await _context.Orders.AnyAsync(o => o.OrderCode == group.Key)) continue;

                var first = group.First();
                var order = new Order
                {
                    OrderCode = group.Key,
                    CustomerId = first.CustomerId,
                    OrderDate = first.OrderDate,
                    DeliveryDeadline = first.Deadline,
                    Status = OrderStatus.PendingApproval,
                    OrderDetails = group.Select(d => {
                        var extraInfoList = new List<string>();
                        if (!string.IsNullOrEmpty(d.NaturalQtyStr)) extraInfoList.Add($"Tự nhiên: {d.NaturalQtyStr}");
                        if (!string.IsNullOrEmpty(d.ColorQtyStr)) extraInfoList.Add($"Màu: {d.ColorQtyStr}");
                        if (!string.IsNullOrEmpty(d.WoodType)) extraInfoList.Add($"Gỗ: {d.WoodType}");

                        string extraInfo = extraInfoList.Any() ? $"[{string.Join(", ", extraInfoList)}] " : "";

                        return new OrderDetail
                        {
                            ProductName = d.ProductName,
                            Quantity = (int)d.TotalQty,
                            Notes = (extraInfo + d.Note).Trim()
                        };
                    }).ToList()
                };

                _context.Orders.Add(order);
                count++;
            }

            await _context.SaveChangesAsync();

            if (count > 0) TempData["Success"] = $"Đã nhập thành công {count} đơn hàng hợp lệ!";
            else TempData["Warning"] = "Không có đơn hàng mới nào được thêm (Có thể các mã đơn đã tồn tại sẵn).";

            return RedirectToAction(nameof(Index));
        }

        // Cập nhật Model thêm trường ProductExists
        public class OrderImportPreviewModel
        {
            public string OrderCode { get; set; }
            public string CustomerName { get; set; }
            public int CustomerId { get; set; }
            public string ProductName { get; set; }
            public string NaturalQtyStr { get; set; }
            public string ColorQtyStr { get; set; }
            public double TotalQty { get; set; }
            public string WoodType { get; set; }
            public DateTime? Deadline { get; set; }
            public DateTime OrderDate { get; set; }
            public string Note { get; set; }
            public bool IsDuplicate { get; set; }
            public bool CustomerExists { get; set; }
            public bool ProductExists { get; set; } // <--- Thêm dòng này
        }


        // ==========================================
        // 10. BÁO CÁO TIẾN ĐỘ GIAO HÀNG (DELIVERY PROGRESS)
        // ==========================================

        // Lớp chứa dữ liệu báo cáo
        public class DeliveryProgressVM
        {
            public string OrderCode { get; set; }
            public string CustomerName { get; set; }
            public DateTime OrderDate { get; set; }
            public DateTime? DeliveryDeadline { get; set; }
            public string ProductName { get; set; }
            public string Specifications { get; set; }
            public double QuantityOrdered { get; set; }
            public double QuantityShipped { get; set; }
            public double QuantityRemaining => QuantityOrdered - QuantityShipped;
            public double ProgressPercent => QuantityOrdered > 0 ? (QuantityShipped / QuantityOrdered) * 100 : 0;
            public string Status { get; set; }
        }

        [HttpGet]
        [Authorize(Policy = AppPermissions.Orders.View)]
        public async Task<IActionResult> DeliveryProgressReport(DateTime? fromDate, DateTime? toDate, int? customerId)
        {
            var start = fromDate ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var end = toDate ?? start.AddMonths(1).AddDays(-1);
            var endDateTime = end.Date.AddDays(1).AddTicks(-1);

            // Truy vấn lấy chi tiết đơn hàng kèm theo lịch sử xuất kho (Chỉ lấy xuất bán Standard)
            var query = _context.OrderDetails
                .Include(od => od.Order).ThenInclude(o => o.Customer)
                .Include(od => od.ShipmentDetails).ThenInclude(sd => sd.Shipment)
                .Where(od => od.Order.OrderDate >= start && od.Order.OrderDate <= endDateTime
                          && od.Order.Status != OrderStatus.Canceled) // Bỏ qua đơn hủy
                .AsQueryable();

            if (customerId.HasValue)
            {
                query = query.Where(od => od.Order.CustomerId == customerId.Value);
            }

            var rawData = await query.ToListAsync();

            var reportData = rawData.Select(od => new DeliveryProgressVM
            {
                OrderCode = od.Order.OrderCode,
                CustomerName = od.Order.Customer?.CompanyName ?? "Khách lẻ",
                OrderDate = od.Order.OrderDate,
                DeliveryDeadline = od.Order.DeliveryDeadline,
                ProductName = od.ProductName,
                Specifications = od.Specifications,
                QuantityOrdered = od.Quantity,
                // 🔥 Chỉ đếm những lần xuất hàng bình thường (Standard), không tính xuất bù bảo hành
                QuantityShipped = od.ShipmentDetails.Where(sd => sd.Shipment.Type == ShipmentType.Standard).Sum(sd => sd.QuantityShipped),
                Status = od.Order.Status.ToString()
            })
            .OrderByDescending(x => x.OrderDate)
            .ThenBy(x => x.OrderCode)
            .ToList();

            // Load ViewBags cho bộ lọc
            var customers = await _context.Customers.Select(c => new { c.Id, c.CompanyName, c.CustomerCode }).ToListAsync();
            ViewBag.CustomerList = new SelectList(customers.Select(c => new { Id = c.Id, DisplayText = $"{c.CustomerCode} - {c.CompanyName}" }), "Id", "DisplayText", customerId);

            ViewBag.FromDate = start;
            ViewBag.ToDate = end;

            return View(reportData);
        }

        [HttpGet]
        [Authorize(Policy = AppPermissions.Orders.View)]
        public async Task<IActionResult> ExportDeliveryProgressExcel(DateTime? fromDate, DateTime? toDate, int? customerId)
        {
            var start = fromDate ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var end = toDate ?? start.AddMonths(1).AddDays(-1);
            var endDateTime = end.Date.AddDays(1).AddTicks(-1);

            var query = _context.OrderDetails
                .Include(od => od.Order).ThenInclude(o => o.Customer)
                .Include(od => od.ShipmentDetails).ThenInclude(sd => sd.Shipment)
                .Where(od => od.Order.OrderDate >= start && od.Order.OrderDate <= endDateTime && od.Order.Status != OrderStatus.Canceled)
                .AsQueryable();

            if (customerId.HasValue) query = query.Where(od => od.Order.CustomerId == customerId.Value);

            var rawData = await query.ToListAsync();
            var reportData = rawData.Select(od => new DeliveryProgressVM
            {
                OrderCode = od.Order.OrderCode,
                CustomerName = od.Order.Customer?.CompanyName ?? "Khách lẻ",
                OrderDate = od.Order.OrderDate,
                DeliveryDeadline = od.Order.DeliveryDeadline,
                ProductName = od.ProductName,
                Specifications = od.Specifications,
                QuantityOrdered = od.Quantity,
                QuantityShipped = od.ShipmentDetails.Where(sd => sd.Shipment.Type == ShipmentType.Standard).Sum(sd => sd.QuantityShipped)
            }).OrderByDescending(x => x.OrderDate).ToList();

            using (var wb = new XLWorkbook())
            {
                var ws = wb.Worksheets.Add("TienDoGiaoHang");

                ws.Cell(1, 1).Value = "BÁO CÁO TIẾN ĐỘ GIAO HÀNG TỔNG HỢP";
                ws.Range("A1:I1").Merge().Style.Font.SetBold().Font.SetFontSize(14).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell(2, 1).Value = $"Kỳ báo cáo: {start:dd/MM/yyyy} - {end:dd/MM/yyyy}";
                ws.Range("A2:I2").Merge().Style.Font.SetItalic().Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                var headers = new[] { "Mã Đơn Hàng", "Khách Hàng", "Ngày Đặt", "Hạn Giao", "Tên Sản Phẩm", "Quy Cách", "SL Đặt", "Đã Giao", "Còn Nợ (Chưa Giao)" };
                for (int i = 0; i < headers.Length; i++) ws.Cell(4, i + 1).Value = headers[i];

                ws.Range("A4:I4").Style.Font.SetBold().Fill.SetBackgroundColor(XLColor.LightBlue).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                int row = 5;
                foreach (var item in reportData)
                {
                    ws.Cell(row, 1).Value = item.OrderCode;
                    ws.Cell(row, 2).Value = item.CustomerName;
                    ws.Cell(row, 3).Value = item.OrderDate.ToString("dd/MM/yyyy");
                    ws.Cell(row, 4).Value = item.DeliveryDeadline?.ToString("dd/MM/yyyy") ?? "";
                    ws.Cell(row, 5).Value = item.ProductName;
                    ws.Cell(row, 6).Value = item.Specifications;

                    ws.Cell(row, 7).Value = item.QuantityOrdered;
                    ws.Cell(row, 8).Value = item.QuantityShipped;
                    ws.Cell(row, 9).Value = item.QuantityRemaining;

                    // Đổi màu đỏ nếu còn nợ hàng
                    if (item.QuantityRemaining > 0)
                        ws.Cell(row, 9).Style.Font.SetFontColor(XLColor.Red).Font.SetBold();
                    else
                        ws.Cell(row, 9).Style.Font.SetFontColor(XLColor.Green).Font.SetBold();

                    row++;
                }

                ws.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"TienDoGiaoHang_{DateTime.Now:ddMMyy}.xlsx");
                }
            }
        }


        // ==========================================
        // 7.1 XUẤT HÓA ĐƠN LÔ (GOM NHIỀU ĐƠN HÀNG)
        // ==========================================
        [HttpGet]
        [Authorize(Policy = AppPermissions.Orders.Edit)]
        public async Task<IActionResult> BatchInvoice()
        {
            // BỎ LỌC THEO TYPE CŨ. 
            // Logic mới: Chỉ lấy ra danh sách các Khách hàng ĐÃ CÓ ĐƠN HÀNG
            var customerIdsWithOrders = await _context.Orders
                .Where(o => o.Status != OrderStatus.Canceled)
                .Select(o => o.CustomerId)
                .Distinct()
                .ToListAsync();

            var customers = await _context.Customers
                .Where(c => customerIdsWithOrders.Contains(c.Id))
                .Select(c => new { c.Id, Text = $"[{c.CustomerCode}] {c.CompanyName}" })
                .ToListAsync();

            ViewBag.CustomersJson = System.Text.Json.JsonSerializer.Serialize(customers);
            return View();
        }

        // Lấy các đơn hàng CHƯA XUẤT HẾT HÓA ĐƠN của Khách hàng
        [HttpGet]
        public async Task<IActionResult> GetUninvoicedOrders(int customerId)
        {
            var orders = await _context.Orders
                .Include(o => o.InvoiceDetails)
                .Where(o => o.CustomerId == customerId && o.Status != OrderStatus.Canceled)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            var result = orders.Select(o => {
                decimal billed = o.InvoiceDetails.Sum(d => d.BilledAmount);
                decimal remain = o.TotalAmount - billed;
                return new
                {
                    id = o.Id,
                    orderCode = o.OrderCode,
                    orderDate = o.OrderDate.ToString("dd/MM/yyyy"),
                    totalAmount = o.TotalAmount,
                    billedAmount = billed,
                    remainAmount = remain
                };
            }).Where(x => x.remainAmount > 0).ToList(); // Chỉ lấy đơn còn dư địa xuất HĐ

            return Json(result);
        }

        public class BatchInvoiceItem
        {
            public int OrderId { get; set; }
            public decimal BilledAmount { get; set; }
        }

        [HttpPost]
        [Authorize(Policy = AppPermissions.Orders.Edit)]
        public async Task<IActionResult> ProcessBatchInvoice([FromForm] string dataJson, [FromForm] string invoiceNumber, [FromForm] DateTime invoiceDate, IFormFile? invoiceFile)
        {
            if (string.IsNullOrWhiteSpace(dataJson)) return Json(new { success = false, message = "Không có dữ liệu đơn hàng." });
            if (string.IsNullOrWhiteSpace(invoiceNumber)) return Json(new { success = false, message = "Vui lòng nhập Số hóa đơn." });

            var items = System.Text.Json.JsonSerializer.Deserialize<List<BatchInvoiceItem>>(dataJson);
            if (items == null || !items.Any()) return Json(new { success = false, message = "Danh sách trống." });

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Tạo Hóa Đơn Gốc (Invoice)
                var invoice = new Invoice
                {
                    InvoiceNumber = invoiceNumber,
                    InvoiceDate = invoiceDate,
                    TotalAmount = items.Sum(x => x.BilledAmount)
                };

                // Upload File nếu có
                if (invoiceFile != null && invoiceFile.Length > 0)
                {
                    string yearFolder = invoiceDate.ToString("yyyy");
                    string monthFolder = invoiceDate.ToString("MM");
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "invoices", yearFolder, monthFolder);
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    string uniqueFileName = $"INV_BATCH_{DateTime.Now.Ticks}{Path.GetExtension(invoiceFile.FileName)}";
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await invoiceFile.CopyToAsync(fileStream);
                    }
                    invoice.InvoiceFile = $"{yearFolder}/{monthFolder}/{uniqueFileName}";
                }

                _context.Invoices.Add(invoice);
                await _context.SaveChangesAsync(); // Lưu để lấy ID

                // 2. Lưu chi tiết InvoiceDetail và Cập nhật trạng thái từng Đơn hàng
                foreach (var item in items.Where(x => x.BilledAmount > 0))
                {
                    var order = await _context.Orders.Include(o => o.InvoiceDetails).FirstOrDefaultAsync(o => o.Id == item.OrderId);
                    if (order == null) continue;

                    // Thêm dòng chi tiết hóa đơn
                    _context.InvoiceDetails.Add(new InvoiceDetail
                    {
                        InvoiceId = invoice.Id,
                        OrderId = order.Id,
                        BilledAmount = item.BilledAmount
                    });

                    // Cập nhật trạng thái
                    decimal alreadyBilled = order.InvoiceDetails.Sum(d => d.BilledAmount);
                    if (alreadyBilled + item.BilledAmount >= order.TotalAmount)
                    {
                        if (order.Status != OrderStatus.Completed && order.Status != OrderStatus.Canceled)
                            order.Status = OrderStatus.Invoiced;
                    }
                    _context.Update(order);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { success = true, message = "Đã xuất hóa đơn lô thành công!" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        // ==========================================
        // IN HỒ SƠ ĐƠN HÀNG (A4 KHỔ DỌC)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> PrintOrder(int id)
        {
            // 1. Lấy thông tin đơn hàng + Khách hàng + Sản phẩm + Lịch sử giao hàng + Hóa đơn
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.ShipmentDetails)
                        .ThenInclude(sd => sd.Shipment) 
                .Include(o => o.InvoiceDetails)
                    .ThenInclude(inv => inv.Invoice)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound("Không tìm thấy đơn hàng!");

            // 2. Lấy lịch sử thanh toán (Thu tiền)
            var payments = await _context.CashEntries
                .Where(c => c.OrderId == id && (int)c.Type == 1) // 1 = Thu
                .OrderBy(c => c.TransactionDate)
                .ToListAsync();

            ViewBag.Payments = payments;

            return View(order);
        }

        // ==========================================
        // 11. ĐỔI TRẠNG THÁI HÀNG LOẠT (BULK ACTION)
        // ==========================================
        [HttpPost]
        [Authorize(Policy = AppPermissions.Orders.Edit)]
        public async Task<IActionResult> BulkUpdateStatus(string ids, int newStatus)
        {
            if (string.IsNullOrEmpty(ids)) return Json(new { success = false, message = "Chưa chọn đơn hàng nào." });

            try
            {
                var idList = ids.Split(',').Select(int.Parse).ToList();
                var orders = await _context.Orders.Where(o => idList.Contains(o.Id)).ToListAsync();

                foreach (var order in orders)
                {
                    if (!order.IsLocked && order.Status != OrderStatus.Completed && order.Status != OrderStatus.Canceled)
                    {
                        order.Status = (OrderStatus)newStatus;
                    }
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ==========================================
        // 12. API TÌM ID ĐƠN HÀNG QUA MÃ (CHUYỂN TRANG NHANH)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> GetOrderIdByCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return Json(new { success = false, message = "Vui lòng nhập mã đơn." });

            // Tìm đơn hàng khớp mã (Không phân biệt hoa/thường)
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.OrderCode.ToLower() == code.Trim().ToLower()
                                       || (o.PaperOrderCode != null && o.PaperOrderCode.ToLower() == code.Trim().ToLower()));

            if (order != null)
            {
                return Json(new { success = true, id = order.Id });
            }

            return Json(new { success = false, message = $"Không tìm thấy đơn hàng nào mang mã '{code}'!" });
        }

        // ==========================================
        // 13. QUẢN LÝ NHIỀU FILE ĐÍNH KÈM (BẰNG CHUỖI JSON)
        // ==========================================

        // Hàm phụ trợ: Đọc dữ liệu từ cột DesignFile
        private List<Dictionary<string, string>> GetAttachments(string json)
        {
            if (string.IsNullOrEmpty(json)) return new List<Dictionary<string, string>>();
            try
            {
                if (json.Trim().StartsWith("["))
                {
                    return System.Text.Json.JsonSerializer.Deserialize<List<Dictionary<string, string>>>(json);
                }
                else
                {
                    // Tương thích ngược: Đơn hàng cũ chỉ có 1 file
                    return new List<Dictionary<string, string>> {
                        new Dictionary<string, string> { { "Id", Guid.NewGuid().ToString() }, { "FileName", json }, { "FilePath", json } }
                    };
                }
            }
            catch { return new List<Dictionary<string, string>>(); }
        }

        [HttpPost]
        [Authorize(Policy = AppPermissions.Orders.Edit)]
        public async Task<IActionResult> UploadMultipleDesignFiles(int id, List<IFormFile> designFiles)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return Json(new { success = false, message = "Không tìm thấy đơn!" });
            if (order.IsLocked) return Json(new { success = false, message = "Đơn hàng đã khóa!" });

            if (designFiles == null || !designFiles.Any())
                return Json(new { success = false, message = "Vui lòng chọn file." });

            var attachments = GetAttachments(order.DesignFile);

            foreach (var file in designFiles)
            {
                if (file.Length > 0)
                {
                    string newPath = await UploadFile(file); // Gọi lại hàm Upload có sẵn của Sếp
                    string originalName = file.FileName;

                    // Nếu trùng tên trong JSON, tự động thêm timestamp để phân biệt
                    if (attachments.Any(a => a["FileName"].Equals(originalName, StringComparison.OrdinalIgnoreCase)))
                    {
                        string ext = Path.GetExtension(originalName);
                        string nameWithoutExt = Path.GetFileNameWithoutExtension(originalName);
                        originalName = $"{nameWithoutExt}_{DateTime.Now.ToString("HHmmss")}{ext}";
                    }

                    attachments.Add(new Dictionary<string, string> {
                        { "Id", Guid.NewGuid().ToString() },
                        { "FileName", originalName },
                        { "FilePath", newPath }
                    });
                }
            }

            order.DesignFile = System.Text.Json.JsonSerializer.Serialize(attachments);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã tải file lên thành công!" });
        }

        [HttpPost]
        [Authorize(Policy = AppPermissions.Orders.Edit)]
        public async Task<IActionResult> RenameDesignFile(int id, string fileId, string newName)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null || order.IsLocked) return Json(new { success = false, message = "Lỗi quyền truy cập!" });

            // Đọc danh sách file hiện tại từ chuỗi JSON
            var attachments = new List<Dictionary<string, string>>();
            try
            {
                if (order.DesignFile.Trim().StartsWith("["))
                {
                    attachments = System.Text.Json.JsonSerializer.Deserialize<List<Dictionary<string, string>>>(order.DesignFile);
                }
                else
                {
                    attachments.Add(new Dictionary<string, string> { { "Id", Guid.NewGuid().ToString() }, { "FileName", order.DesignFile }, { "FilePath", order.DesignFile } });
                }
            }
            catch { }

            var file = attachments.FirstOrDefault(a => a["Id"] == fileId);
            if (file == null) return Json(new { success = false, message = "Không tìm thấy file!" });

            // Giữ lại định dạng đuôi file gốc (VD: .pdf)
            string ext = Path.GetExtension(file["FilePath"]);
            if (!newName.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
            {
                newName += ext;
            }

            // Kiểm tra chống trùng tên
            if (attachments.Any(a => a["Id"] != fileId && a["FileName"].Equals(newName, StringComparison.OrdinalIgnoreCase)))
            {
                return Json(new { success = false, message = $"Tên file '{newName}' đã bị trùng. Vui lòng đặt tên khác!" });
            }

            // Lưu tên mới và đóng gói JSON
            file["FileName"] = newName;
            order.DesignFile = System.Text.Json.JsonSerializer.Serialize(attachments);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpPost]
        [Authorize(Policy = AppPermissions.Orders.Edit)]
        public async Task<IActionResult> DeleteDesignFileItem(int id, string fileId)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null || order.IsLocked) return Json(new { success = false, message = "Lỗi quyền truy cập!" });

            var attachments = GetAttachments(order.DesignFile);
            var file = attachments.FirstOrDefault(a => a["Id"] == fileId);

            if (file != null)
            {
                // Xóa vật lý trên ổ cứng
                string path = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", file["FilePath"]);
                if (System.IO.File.Exists(path)) System.IO.File.Delete(path);

                attachments.Remove(file);

                // Nếu xóa sạch file, trả về NULL để đỡ tốn dung lượng SQL
                if (!attachments.Any()) order.DesignFile = null;
                else order.DesignFile = System.Text.Json.JsonSerializer.Serialize(attachments);

                await _context.SaveChangesAsync();
            }
            return Json(new { success = true });
        }


        // ==========================================
        // 14. XUẤT EXCEL CHI TIẾT MỘT ĐƠN HÀNG (FULL THÔNG TIN NHƯ BẢN IN A4)
        // ==========================================
        [HttpGet]
        [Authorize(Policy = AppPermissions.Orders.View)]
        public async Task<IActionResult> ExportExcelDetail(int id)
        {
            // 1. Lấy dữ liệu Đơn hàng (Kèm Sản phẩm, Khách hàng, Giao hàng, Hóa đơn)
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.ShipmentDetails)
                        .ThenInclude(sd => sd.Shipment)
                .Include(o => o.InvoiceDetails)
                    .ThenInclude(inv => inv.Invoice)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound("Không tìm thấy đơn hàng.");

            // 2. Lấy dữ liệu Thu tiền
            var payments = await _context.CashEntries
                .Where(c => c.OrderId == id && (int)c.Type == 1) // Type 1: Thu tiền
                .OrderBy(c => c.TransactionDate)
                .ToListAsync();

            decimal totalPaid = payments.Sum(p => p.Amount);
            decimal totalBilled = order.InvoiceDetails?.Sum(i => i.BilledAmount) ?? 0;
            decimal remain = order.TotalAmount - totalPaid;

            // 3. Khởi tạo file Excel
            using (var wb = new XLWorkbook())
            {
                var ws = wb.Worksheets.Add($"HSDH_{order.OrderCode}");
                int currentRow = 1;

                // ================= HEADER: THÔNG TIN CÔNG TY =================
                ws.Cell(currentRow, 1).Value = "CÔNG TY SẢN XUẤT NỘI THẤT";
                ws.Cell(currentRow, 1).Style.Font.SetBold().Font.SetFontSize(12);
                ws.Cell(currentRow + 1, 1).Value = "Địa chỉ: Khu Công Nghiệp ABC, TP.HCM";
                ws.Cell(currentRow + 2, 1).Value = "Hotline: 0909.xxx.xxx - MST: 031xxxxxxx";

                ws.Cell(currentRow, 5).Value = "Mẫu số: 01-HD/ERP";
                ws.Cell(currentRow, 5).Style.Font.SetItalic().Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws.Range(currentRow, 5, currentRow, 6).Merge();
                ws.Cell(currentRow + 1, 5).Value = $"Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm}";
                ws.Cell(currentRow + 1, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws.Range(currentRow + 1, 5, currentRow + 1, 6).Merge();
                currentRow += 4;

                // ================= TITLE =================
                ws.Cell(currentRow, 1).Value = "HỒ SƠ CHI TIẾT ĐƠN HÀNG";
                ws.Range(currentRow, 1, currentRow, 6).Merge().Style.Font.SetBold().Font.SetFontSize(16).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                currentRow++;

                ws.Cell(currentRow, 1).Value = $"Mã số: {order.OrderCode} | Ngày nhận: {order.OrderDate:dd/MM/yyyy} | Hạn giao: {(order.DeliveryDeadline?.ToString("dd/MM/yyyy") ?? "___")}";
                ws.Range(currentRow, 1, currentRow, 6).Merge().Style.Font.SetItalic().Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                currentRow += 2;

                // ================= 1. THÔNG TIN KHÁCH HÀNG =================
                ws.Cell(currentRow, 1).Value = "Khách hàng:"; ws.Cell(currentRow, 1).Style.Font.SetBold();
                ws.Cell(currentRow, 2).Value = order.Customer?.CompanyName; ws.Range(currentRow, 2, currentRow, 3).Merge();
                ws.Cell(currentRow, 4).Value = "Điện thoại:"; ws.Cell(currentRow, 4).Style.Font.SetBold();
                ws.Cell(currentRow, 5).Value = order.Customer?.PhoneNumber; ws.Range(currentRow, 5, currentRow, 6).Merge();
                currentRow++;

                ws.Cell(currentRow, 1).Value = "Địa chỉ:"; ws.Cell(currentRow, 1).Style.Font.SetBold();
                ws.Cell(currentRow, 2).Value = order.Customer?.Address; ws.Range(currentRow, 2, currentRow, 6).Merge();
                currentRow++;

                ws.Cell(currentRow, 1).Value = "PO khách:"; ws.Cell(currentRow, 1).Style.Font.SetBold();
                ws.Cell(currentRow, 2).Value = order.PaperOrderCode; ws.Range(currentRow, 2, currentRow, 3).Merge();
                ws.Cell(currentRow, 4).Value = "Trạng thái:"; ws.Cell(currentRow, 4).Style.Font.SetBold();
                ws.Cell(currentRow, 5).Value = order.Status.ToString(); ws.Range(currentRow, 5, currentRow, 6).Merge();
                currentRow++;

                ws.Cell(currentRow, 1).Value = "Ghi chú đơn:"; ws.Cell(currentRow, 1).Style.Font.SetBold();
                ws.Cell(currentRow, 2).Value = order.Notes; ws.Range(currentRow, 2, currentRow, 6).Merge();
                currentRow += 2;

                // ================= 2. CHI TIẾT SẢN PHẨM =================
                ws.Cell(currentRow, 1).Value = "I. DANH SÁCH SẢN PHẨM ĐẶT HÀNG";
                ws.Range(currentRow, 1, currentRow, 6).Merge().Style.Font.SetBold().Font.SetFontSize(12).Border.BottomBorder = XLBorderStyleValues.Thick;
                currentRow++;

                string[] prodHeaders = { "STT", "Tên sản phẩm", "ĐVT", "Số lượng", "Đơn giá", "Thành tiền" };
                for (int i = 0; i < prodHeaders.Length; i++)
                {
                    var cell = ws.Cell(currentRow, i + 1);
                    cell.Value = prodHeaders[i];
                    cell.Style.Font.SetBold().Fill.SetBackgroundColor(XLColor.LightGray).Border.SetOutsideBorder(XLBorderStyleValues.Thin).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                }
                currentRow++;

                int sttSp = 1;
                if (order.OrderDetails != null)
                {
                    foreach (var sp in order.OrderDetails)
                    {
                        string spName = string.IsNullOrEmpty(sp.Notes) ? sp.ProductName : $"{sp.ProductName} ({sp.Notes})";
                        ws.Cell(currentRow, 1).Value = sttSp++;
                        ws.Cell(currentRow, 2).Value = spName;
                        ws.Cell(currentRow, 3).Value = "Cái";
                        ws.Cell(currentRow, 4).Value = sp.Quantity;
                        ws.Cell(currentRow, 5).Value = sp.UnitPrice;
                        ws.Cell(currentRow, 6).Value = sp.Quantity * sp.UnitPrice;

                        ws.Range(currentRow, 1, currentRow, 6).Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
                        ws.Cell(currentRow, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                        ws.Cell(currentRow, 3).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                        ws.Cell(currentRow, 4).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center).Font.SetBold();
                        ws.Cell(currentRow, 5).Style.NumberFormat.Format = "#,##0";
                        ws.Cell(currentRow, 6).Style.NumberFormat.Format = "#,##0";
                        ws.Cell(currentRow, 6).Style.Font.SetBold();
                        currentRow++;
                    }
                }

                ws.Cell(currentRow, 5).Value = "TỔNG CỘNG TRỊ GIÁ ĐƠN HÀNG:";
                ws.Cell(currentRow, 5).Style.Font.SetBold().Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
                ws.Cell(currentRow, 6).Value = order.TotalAmount;
                ws.Cell(currentRow, 6).Style.Font.SetBold().Font.SetFontSize(12).NumberFormat.Format = "#,##0";
                currentRow += 3;

                // ================= 3. LỊCH SỬ GIAO HÀNG =================
                ws.Cell(currentRow, 1).Value = "II. TIẾN ĐỘ XUẤT KHO / GIAO HÀNG";
                ws.Range(currentRow, 1, currentRow, 6).Merge().Style.Font.SetBold().Font.SetFontSize(12).Border.BottomBorder = XLBorderStyleValues.Thick;
                currentRow++;

                string[] shipHeaders = { "Ngày xuất", "Số phiếu XK", "Biển số xe", "Sản phẩm", "Phân loại", "SL Giao" };
                for (int i = 0; i < shipHeaders.Length; i++)
                {
                    var cell = ws.Cell(currentRow, i + 1);
                    cell.Value = shipHeaders[i];
                    cell.Style.Font.SetBold().Fill.SetBackgroundColor(XLColor.LightGray).Border.SetOutsideBorder(XLBorderStyleValues.Thin).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                }
                currentRow++;

                bool hasShipment = false;
                if (order.OrderDetails != null)
                {
                    foreach (var sp in order.OrderDetails)
                    {
                        if (sp.ShipmentDetails != null && sp.ShipmentDetails.Any())
                        {
                            hasShipment = true;
                            foreach (var ship in sp.ShipmentDetails)
                            {
                                ws.Cell(currentRow, 1).Value = ship.Shipment?.ShipmentDate.ToString("dd/MM/yyyy") ?? "-";
                                ws.Cell(currentRow, 2).Value = ship.Shipment?.ShipmentCode ?? "N/A";
                                ws.Cell(currentRow, 3).Value = ship.Shipment?.VehicleNumber ?? "-";
                                ws.Cell(currentRow, 4).Value = sp.ProductName;

                                string typeName = "-";
                                if (ship.Shipment?.Type == ProductionManager.Data.ShipmentType.Standard) typeName = "Xuất hàng";
                                else if (ship.Shipment?.Type == ProductionManager.Data.ShipmentType.Warranty) typeName = "Bảo hành";
                                else if (ship.Shipment?.Type == ProductionManager.Data.ShipmentType.Return) typeName = "Nhập trả";
                                ws.Cell(currentRow, 5).Value = typeName;

                                ws.Cell(currentRow, 6).Value = ship.QuantityShipped;

                                ws.Range(currentRow, 1, currentRow, 6).Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
                                ws.Cell(currentRow, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                                ws.Cell(currentRow, 2).Style.Font.SetBold();
                                ws.Cell(currentRow, 5).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                                ws.Cell(currentRow, 6).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center).Font.SetBold();
                                currentRow++;
                            }
                        }
                    }
                }
                if (!hasShipment)
                {
                    ws.Cell(currentRow, 1).Value = "Chưa có dữ liệu xuất kho / giao hàng.";
                    ws.Range(currentRow, 1, currentRow, 6).Merge().Style.Font.SetItalic().Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                    currentRow++;
                }
                currentRow += 2;

                // ================= 4. HÓA ĐƠN & THU TIỀN (CHIA 2 CỘT) =================
                int startFinanceRow = currentRow;

                // Cột trái: Hóa Đơn
                ws.Cell(startFinanceRow, 1).Value = "III. CHỨNG TỪ HÓA ĐƠN (VAT)";
                ws.Range(startFinanceRow, 1, startFinanceRow, 3).Merge().Style.Font.SetBold().Border.BottomBorder = XLBorderStyleValues.Thick;

                ws.Cell(startFinanceRow + 1, 1).Value = "Ngày HĐ";
                ws.Cell(startFinanceRow + 1, 2).Value = "Số HĐ";
                ws.Cell(startFinanceRow + 1, 3).Value = "Trị giá HĐ";
                ws.Range(startFinanceRow + 1, 1, startFinanceRow + 1, 3).Style.Font.SetBold().Fill.SetBackgroundColor(XLColor.LightGray).Border.SetOutsideBorder(XLBorderStyleValues.Thin).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                int invRow = startFinanceRow + 2;
                if (order.InvoiceDetails != null && order.InvoiceDetails.Any())
                {
                    foreach (var inv in order.InvoiceDetails)
                    {
                        ws.Cell(invRow, 1).Value = inv.Invoice?.InvoiceDate.ToString("dd/MM/yyyy");
                        ws.Cell(invRow, 2).Value = inv.Invoice?.InvoiceNumber;
                        ws.Cell(invRow, 3).Value = inv.BilledAmount;

                        ws.Range(invRow, 1, invRow, 3).Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
                        ws.Cell(invRow, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                        ws.Cell(invRow, 2).Style.Font.SetBold();
                        ws.Cell(invRow, 3).Style.NumberFormat.Format = "#,##0";
                        invRow++;
                    }
                    ws.Cell(invRow, 2).Value = "TỔNG XUẤT HĐ:"; ws.Cell(invRow, 2).Style.Font.SetBold().Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
                    ws.Cell(invRow, 3).Value = totalBilled; ws.Cell(invRow, 3).Style.Font.SetBold().NumberFormat.Format = "#,##0";
                }
                else
                {
                    ws.Cell(invRow, 1).Value = "Chưa xuất hóa đơn";
                    ws.Range(invRow, 1, invRow, 3).Merge().Style.Font.SetItalic().Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                }

                // Cột phải: Thu tiền
                ws.Cell(startFinanceRow, 4).Value = "IV. LỊCH SỬ THU TIỀN";
                ws.Range(startFinanceRow, 4, startFinanceRow, 6).Merge().Style.Font.SetBold().Border.BottomBorder = XLBorderStyleValues.Thick;

                ws.Cell(startFinanceRow + 1, 4).Value = "Ngày thu";
                ws.Cell(startFinanceRow + 1, 5).Value = "Số phiếu";
                ws.Cell(startFinanceRow + 1, 6).Value = "Số tiền thu";
                ws.Range(startFinanceRow + 1, 4, startFinanceRow + 1, 6).Style.Font.SetBold().Fill.SetBackgroundColor(XLColor.LightGray).Border.SetOutsideBorder(XLBorderStyleValues.Thin).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                int payRow = startFinanceRow + 2;
                if (payments.Any())
                {
                    foreach (var pay in payments)
                    {
                        ws.Cell(payRow, 4).Value = pay.TransactionDate.ToString("dd/MM/yyyy");
                        ws.Cell(payRow, 5).Value = pay.VoucherCode;
                        ws.Cell(payRow, 6).Value = pay.Amount;

                        ws.Range(payRow, 4, payRow, 6).Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
                        ws.Cell(payRow, 4).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                        ws.Cell(payRow, 6).Style.NumberFormat.Format = "#,##0";
                        payRow++;
                    }
                    ws.Cell(payRow, 5).Value = "TỔNG ĐÃ THU:"; ws.Cell(payRow, 5).Style.Font.SetBold().Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
                    ws.Cell(payRow, 6).Value = totalPaid; ws.Cell(payRow, 6).Style.Font.SetBold().NumberFormat.Format = "#,##0";
                }
                else
                {
                    ws.Cell(payRow, 4).Value = "Chưa có dữ liệu thu tiền";
                    ws.Range(payRow, 4, payRow, 6).Merge().Style.Font.SetItalic().Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                }

                currentRow = Math.Max(invRow, payRow) + 3;

                // ================= 5. TỔNG KẾT CÔNG NỢ =================
                ws.Cell(currentRow, 1).Value = "V. TỔNG KẾT CÔNG NỢ";
                ws.Range(currentRow, 1, currentRow, 6).Merge().Style.Font.SetBold().Font.SetFontSize(12);
                currentRow++;

                ws.Cell(currentRow, 2).Value = "TỔNG TRỊ GIÁ ĐƠN:"; ws.Cell(currentRow, 2).Style.Font.SetBold().Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
                ws.Cell(currentRow, 3).Value = order.TotalAmount; ws.Cell(currentRow, 3).Style.Font.SetBold().NumberFormat.Format = "#,##0";

                ws.Cell(currentRow, 4).Value = "ĐÃ THANH TOÁN:"; ws.Cell(currentRow, 4).Style.Font.SetBold().Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
                ws.Cell(currentRow, 5).Value = totalPaid; ws.Cell(currentRow, 5).Style.Font.SetBold().NumberFormat.Format = "#,##0";
                currentRow++;

                ws.Cell(currentRow, 4).Value = "CÒN PHẢI THU (NỢ):"; ws.Cell(currentRow, 4).Style.Font.SetBold().Font.SetFontColor(XLColor.Red).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
                ws.Cell(currentRow, 5).Value = remain; ws.Cell(currentRow, 5).Style.Font.SetBold().Font.SetFontColor(XLColor.Red).NumberFormat.Format = "#,##0";

                currentRow += 3;

                // ================= CHỮ KÝ =================
                ws.Cell(currentRow, 1).Value = "NGƯỜI LẬP BIỂU";
                ws.Cell(currentRow, 3).Value = "KẾ TOÁN";
                ws.Cell(currentRow, 4).Value = "KHO / VẬN CHUYỂN";
                ws.Cell(currentRow, 6).Value = "GIÁM ĐỐC";

                ws.Range(currentRow, 1, currentRow, 6).Style.Font.SetBold().Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                ws.Cell(currentRow + 1, 1).Value = "(Ký, ghi rõ họ tên)";
                ws.Cell(currentRow + 1, 3).Value = "(Ký, ghi rõ họ tên)";
                ws.Cell(currentRow + 1, 4).Value = "(Ký, ghi rõ họ tên)";
                ws.Cell(currentRow + 1, 6).Value = "(Ký, đóng dấu)";

                ws.Range(currentRow + 1, 1, currentRow + 1, 6).Style.Font.SetItalic().Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                // ================= FORMAT CỘT =================
                ws.Columns().AdjustToContents();
                ws.Column(2).Width = 35; // Nới rộng cột Tên sản phẩm / Tên Khách hàng
                ws.Column(4).Width = 20; // Nới rộng cột Số lượng / Phân loại

                using (var stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"HSDH_{order.OrderCode}.xlsx");
                }
            }
        }



    }
}