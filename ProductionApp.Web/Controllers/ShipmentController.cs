using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProductionApp.Web.Constants;
using ProductionManager.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using X.PagedList;
using X.PagedList.Extensions;

namespace ProductionApp.Web.Controllers
{
    [Authorize]
    public class ShipmentController : Controller
    {
        private readonly AppDbContext _context;

        public ShipmentController(AppDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. DANH SÁCH PHIẾU XUẤT (INDEX FULL FILTER & SORT)
        // ==========================================
        [Authorize(Policy = AppPermissions.Shipments.View)]
        public async Task<IActionResult> Index(int? page, string shipmentCode, int? customerId, string productName, string vehicle, ShipmentType? type, DateTime? fromDate, DateTime? toDate, string sortOrder, string viewMode = "list", int? pageSize = null)
        {
            int currentPageSize = pageSize ?? 30;

            var startDate = fromDate ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var endDate = toDate ?? startDate.AddMonths(1).AddDays(-1);
            var endDateTime = endDate.Date.AddDays(1).AddTicks(-1);

            // Truy vấn gốc (Bao gồm đầy đủ Quan hệ)
            var query = _context.Shipments
                .Include(s => s.Customer)
                .Include(s => s.Order)
                .Include(s => s.ExportShipment)
                .Include(s => s.ShipmentDetails)
                    .ThenInclude(sd => sd.OrderDetail)
                .Where(s => s.ShipmentDate >= startDate && s.ShipmentDate <= endDateTime)
                .AsQueryable();

            // 1. BỘ LỌC CHUYÊN NGHIỆP
            if (!string.IsNullOrEmpty(shipmentCode))
                query = query.Where(s => s.ShipmentCode.Contains(shipmentCode));

            if (customerId.HasValue)
                query = query.Where(s => s.CustomerId == customerId);

            if (type.HasValue)
                query = query.Where(s => s.Type == type);

            if (!string.IsNullOrEmpty(vehicle))
                query = query.Where(s => s.VehicleNumber.Contains(vehicle) || (s.ExportShipment != null && (s.ExportShipment.LicensePlate.Contains(vehicle) || s.ExportShipment.ContainerNumber.Contains(vehicle))));

            if (!string.IsNullOrEmpty(productName))
                query = query.Where(s => s.ShipmentDetails.Any(d => d.OrderDetail.ProductName.Contains(productName)));

            // 2. TÍNH TOÁN KPI DỰA TRÊN DỮ LIỆU ĐÃ LỌC
            var allFiltered = await query.ToListAsync();
            ViewBag.TotalShipments = allFiltered.Count;
            ViewBag.TotalValue = allFiltered.Sum(s => s.ShipmentDetails.Sum(d => d.QuantityShipped * (d.OrderDetail?.UnitPrice ?? 0)));

            // 3. TÍNH NĂNG SORT THEO THỜI GIAN
            switch (sortOrder)
            {
                case "date_asc": query = query.OrderBy(s => s.ShipmentDate); break;
                default: query = query.OrderByDescending(s => s.ShipmentDate); break;
            }

            // Truyền Dữ liệu Filter xuống View
            ViewBag.CustomerList = await _context.Customers.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.CompanyName }).ToListAsync();
            ViewBag.ShipmentCode = shipmentCode;
            ViewBag.CustomerId = customerId;
            ViewBag.ProductName = productName;
            ViewBag.Vehicle = vehicle;
            ViewBag.Type = type;
            ViewBag.FromDate = startDate;
            ViewBag.ToDate = endDate;
            ViewBag.CurrentSort = sortOrder;
            ViewBag.ViewMode = viewMode;
            ViewBag.PageSize = currentPageSize;

            var pagedData = query.ToPagedList(page ?? 1, currentPageSize);
            return View(pagedData);
        }

        // ==========================================
        // TẠO PHIẾU NHẬN HÀNG LỖI (RETURN)
        // ==========================================
        [HttpGet]
        [Authorize(Policy = AppPermissions.Shipments.Create)]
        public IActionResult CreateReturn()
        {
            // Lấy danh sách khách hàng truyền xuống View (giống hệt hàm Create gốc)
            var customers = _context.Customers
                .Select(c => new { Value = c.Id, Text = c.CompanyName })
                .ToList();

            ViewBag.CustomerId = customers;

            // Khởi tạo Model với Type mặc định là Return
            var model = new Shipment { Type = ShipmentType.Return };
            return View(model);
        }

        // ==========================================
        // 2. TẠO PHIẾU - GIAO DIỆN (GET)
        // ==========================================
        [Authorize(Policy = AppPermissions.Shipments.Create)]
        public async Task<IActionResult> Create(int? customerId, int? orderId, int? exportId)
        {
            // Lấy danh sách khách hàng có đơn chưa hoàn thành
            var customers = await _context.Orders
                .Where(o => !o.IsLocked && o.Status != OrderStatus.Completed && o.Status != OrderStatus.Canceled)
                .Select(o => o.Customer)
                .Distinct()
                .Select(c => new { c.Id, DisplayText = $"{c.CustomerCode} - {c.CompanyName}" })
                .ToListAsync();

            ViewData["CustomerId"] = customers;

            // Pre-select dữ liệu
            if (customerId.HasValue)
            {
                ViewBag.SelectedCustomerId = customerId.Value;
            }
            if (orderId.HasValue)
            {
                var order = await _context.Orders.FindAsync(orderId);
                if (order != null)
                {
                    ViewBag.SelectedCustomerId = order.CustomerId;
                    ViewBag.PreSelectOrderId = orderId;
                }
            }
            if (exportId.HasValue)
            {
                ViewBag.PreSelectExportId = exportId.Value;
            }

            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = AppPermissions.Shipments.Create)]
        public async Task<IActionResult> Create(Shipment shipment)
        {
            ModelState.Remove("Customer"); ModelState.Remove("Order"); ModelState.Remove("ExportShipment"); ModelState.Remove("ShipmentCode");
            foreach (var key in ModelState.Keys) { if (key.Contains("ShipmentDetails") && (key.EndsWith(".Shipment") || key.EndsWith(".OrderDetail"))) ModelState.Remove(key); }

            if (shipment.ShipmentDetails != null)
                shipment.ShipmentDetails = shipment.ShipmentDetails.Where(x => x.OrderDetailId > 0 && x.QuantityShipped > 0).ToList();

            if (shipment.ShipmentDetails == null || !shipment.ShipmentDetails.Any())
                ModelState.AddModelError("", "Vui lòng nhập số lượng cho ít nhất một mục.");

            if (ModelState.IsValid)
            {
                using var transaction = _context.Database.BeginTransaction();
                try
                {
                    // Đặt tiền tố mã phiếu: PX (Xuất mới), PNT (Nhập trả), PXB (Xuất bù)
                    string prefix = shipment.Type == ShipmentType.Standard ? "PX" : (shipment.Type == ShipmentType.Return ? "PNT" : "PXB");
                    if (string.IsNullOrEmpty(shipment.ShipmentCode))
                        shipment.ShipmentCode = $"{prefix}-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}";

                    var distinctOrderIds = await _context.OrderDetails.Where(od => shipment.ShipmentDetails.Select(v => v.OrderDetailId).Contains(od.Id)).Select(od => od.OrderId).Distinct().ToListAsync();
                    shipment.OrderId = distinctOrderIds.Count == 1 ? distinctOrderIds.First() : null;

                    if (shipment.Type == ShipmentType.Return) shipment.Notes = "[NHẬP HÀNG LỖI] " + shipment.Notes;
                    if (shipment.Type == ShipmentType.Warranty) shipment.Notes = "[XUẤT BẢO HÀNH] " + shipment.Notes;

                    // ĐIỀU PHỐI LOGISTICS
                    if (shipment.ExportShipmentId.HasValue)
                    {
                        var export = await _context.ExportShipments.FindAsync(shipment.ExportShipmentId);
                        if (export != null)
                        {
                            export.Status = ExportStatus.Loading; // Đang xếp hàng
                            if (string.IsNullOrEmpty(shipment.VehicleNumber))
                            {
                                shipment.VehicleNumber = export.TransportType == TransportType.Truck ? export.LicensePlate : export.ContainerNumber;
                            }
                        }
                    }

                    // 1. LƯU PHIẾU VÀO DB
                    _context.Add(shipment);
                    await _context.SaveChangesAsync();

                    // 🔥 2. GỌI HÀM CẬP NHẬT TRẠNG THÁI ĐƠN HÀNG
                    foreach (var oId in distinctOrderIds)
                    {
                        await UpdateOrderStatus(oId);
                    }

                    // 3. LƯU TẤT CẢ VÀ HOÀN TẤT GIAO DỊCH
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    TempData["Success"] = "Đã lưu phiếu xuất kho thành công!";

                    if (shipment.ExportShipmentId.HasValue)
                    {
                        return RedirectToAction("Details", "Export", new { id = shipment.ExportShipmentId });
                    }

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("", "Lỗi lưu dữ liệu: " + ex.Message);
                }
            }

            return View(shipment);
        }

        // ==========================================
        // 4. XEM CHI TIẾT, XÓA & IN
        // ==========================================
        [Authorize(Policy = AppPermissions.Shipments.View)]
        public async Task<IActionResult> Details(int id)
        {
            var shipment = await _context.Shipments
                .Include(s => s.Customer)
                .Include(s => s.ExportShipment) // Kéo thông tin chuyến xe
                .Include(s => s.ShipmentDetails)
                    .ThenInclude(sd => sd.OrderDetail)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (shipment == null) return NotFound();

            // Map OrderCode để hiển thị
            var detailIds = shipment.ShipmentDetails.Select(x => x.OrderDetailId).ToList();
            var orderMapping = await _context.OrderDetails
                .Include(od => od.Order)
                .Where(od => detailIds.Contains(od.Id))
                .ToDictionaryAsync(k => k.Id, v => v.Order.OrderCode);

            ViewBag.OrderCodes = orderMapping;
            return View(shipment);
        }

        [Authorize(Policy = AppPermissions.Shipments.Delete)]
        public async Task<IActionResult> Delete(int id)
        {
            var shipment = await _context.Shipments
                .Include(s => s.ShipmentDetails)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (shipment != null)
            {
                var distinctOrderIds = await _context.OrderDetails
                    .Where(od => shipment.ShipmentDetails.Select(v => v.OrderDetailId).Contains(od.Id))
                    .Select(od => od.OrderId)
                    .Distinct()
                    .ToListAsync();

                _context.Shipments.Remove(shipment);
                await _context.SaveChangesAsync();

                foreach (var oId in distinctOrderIds)
                {
                    await UpdateOrderStatus(oId);
                }

                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // SỬA THÔNG TIN PHIẾU GIAO HÀNG (GET)
        // ==========================================
        [HttpGet]
        [Authorize(Policy = AppPermissions.Shipments.Edit)]
        public async Task<IActionResult> Edit(int id)
        {
            var shipment = await _context.Shipments
                .FirstOrDefaultAsync(m => m.Id == id);

            if (shipment == null) return NotFound();
            return View(shipment);
        }

        // ==========================================
        // SỬA THÔNG TIN PHIẾU GIAO HÀNG (POST)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = AppPermissions.Shipments.Edit)]
        public async Task<IActionResult> Edit(int id, Shipment shipment)
        {
            if (id != shipment.Id) return NotFound();

            var existingShipment = await _context.Shipments.FindAsync(id);
            if (existingShipment == null) return NotFound();

            // CHỈ cập nhật thông tin vận tải và ghi chú (Tuyệt đối không đụng số lượng)
            existingShipment.ShipmentDate = shipment.ShipmentDate;
            existingShipment.VehicleNumber = shipment.VehicleNumber;
            existingShipment.Notes = shipment.Notes;

            _context.Update(existingShipment);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã cập nhật thông tin phiếu thành công!";

            // Quay về lại trang Chi tiết của phiếu đó
            return RedirectToAction(nameof(Details), new { id = shipment.Id });
        }

        [Authorize(Policy = AppPermissions.Shipments.Create)]
        public async Task<IActionResult> Print(int id)
        {
            var shipment = await _context.Shipments
                .Include(s => s.Customer)
                .Include(s => s.ShipmentDetails)
                    .ThenInclude(sd => sd.OrderDetail)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (shipment == null) return NotFound();

            var detailIds = shipment.ShipmentDetails.Select(x => x.OrderDetailId).ToList();
            var orderMapping = await _context.OrderDetails
                .Include(od => od.Order)
                .Where(od => detailIds.Contains(od.Id))
                .ToDictionaryAsync(k => k.Id, v => v.Order.OrderCode);

            ViewBag.OrderCodes = orderMapping;

            return View(shipment);
        }

        // ==========================================
        // 5. API HELPER (AJAX) CHO GIAO DIỆN
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> GetOrdersByCustomer(int customerId)
        {
            // 1. Kéo danh sách đơn hàng (Bao gồm cả đơn Đang Giao - Delivered và Đã xuất HĐ - Invoiced)
            var orders = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.ShipmentDetails)
                        .ThenInclude(sd => sd.Shipment)
                .Where(o => o.CustomerId == customerId
                            && o.Status != OrderStatus.Canceled
                            && o.Status != OrderStatus.PendingApproval
                            && o.Status != OrderStatus.Completed) // Bỏ chặn Delivered và Invoiced
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            var result = new List<object>();

            foreach (var o in orders)
            {
                // 2. Tính tổng số lượng yêu cầu của đơn này
                double totalOrdered = o.OrderDetails.Sum(d => d.Quantity);

                // 3. Tính tổng số lượng đã giao (Chỉ tính các phiếu xuất chuẩn - Standard)
                double totalShipped = o.OrderDetails.Sum(od =>
                    od.ShipmentDetails
                      .Where(sd => sd.Shipment.Type == ShipmentType.Standard)
                      .Sum(sd => sd.QuantityShipped));

                // 🔥 4. CHỈ HIỂN THỊ NẾU VẪN CÒN HÀNG CHƯA GIAO (totalShipped < totalOrdered)
                if (totalShipped < totalOrdered)
                {
                    int progress = totalOrdered > 0 ? (int)((totalShipped / totalOrdered) * 100) : 0;

                    result.Add(new
                    {
                        id = o.Id,
                        orderCode = o.OrderCode,
                        orderDate = o.OrderDate.ToString("dd/MM/yyyy"),
                        itemCount = o.OrderDetails?.Count ?? 0,
                        progress = progress // Truyền luôn phần trăm tiến độ ra UI
                    });
                }
            }

            return Json(result);
        }

        private int CalculateProgress(Order order)
        {
            return 0; // Để tạm logic này
        }



        // Lấy danh sách chuyến xe (Logistics) đang chờ cho Dropdown
        [HttpGet]
        public async Task<IActionResult> GetActiveExportsByCustomer(int customerId)
        {
            var exports = await _context.ExportShipments
                .Where(e => e.CustomerId == customerId && (e.Status == ExportStatus.Planning || e.Status == ExportStatus.Loading))
                .Select(e => new
                {
                    id = e.Id,
                    code = e.ShipmentCode,
                    vehicle = e.TransportType == TransportType.Truck ? e.LicensePlate : e.ContainerNumber,
                    type = e.TransportType == TransportType.Truck ? "Xe tải" : "Container"
                })
                .ToListAsync();

            return Json(exports);
        }

        // =========================================================
        // HÀM TỰ ĐỘNG CẬP NHẬT TRẠNG THÁI ĐƠN HÀNG (AUTOMATION)
        // =========================================================
        private async Task UpdateOrderStatus(int orderId)
        {
            var order = await _context.Orders.Include(o => o.OrderDetails).ThenInclude(od => od.ShipmentDetails).FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null) return;

            if (order.Status == OrderStatus.Completed || order.Status == OrderStatus.Invoiced || order.Status == OrderStatus.Canceled) return;

            // 🔥 LOGIC LỌC: Nếu nhóm có Con -> Lấy Con tính tiến độ. Nếu đứng 1 mình (Mua lẻ) -> Lấy chính nó.
            var validToShipIds = new List<int>();
            var groups = order.OrderDetails.GroupBy(x => string.IsNullOrEmpty(x.BundleCode) ? x.Id.ToString() : x.BundleCode);
            foreach (var g in groups)
            {
                var comps = g.Where(x => x.IsComponent).ToList();
                if (comps.Any()) validToShipIds.AddRange(comps.Select(x => x.Id));
                else validToShipIds.AddRange(g.Select(x => x.Id));
            }

            double totalShippable = order.OrderDetails.Where(x => validToShipIds.Contains(x.Id)).Sum(x => x.Quantity);
            double totalShipped = order.OrderDetails.Where(x => validToShipIds.Contains(x.Id)).Sum(x => x.ShipmentDetails?.Sum(s => s.QuantityShipped) ?? 0);

            if (totalShipped >= totalShippable && totalShippable > 0) order.Status = OrderStatus.Delivered; // Đã giao 100%
            else if (totalShipped > 0) order.Status = OrderStatus.InProduction; // Mới giao 1 phần

            _context.Update(order);
        }

        // ==========================================
        // API LẤY DANH SÁCH HÀNG CẦN XUẤT CHO MÀN HÌNH "TẠO PHIẾU"
        // ==========================================
        [HttpPost]
        public IActionResult GetPendingItemsByOrderIds(List<int> orderIds, ShipmentType type = ShipmentType.Standard)
        {
            var allDetails = _context.OrderDetails.Include(d => d.Order).Where(d => orderIds.Contains(d.OrderId)).ToList();

            // 🔥 LOGIC LỌC TƯƠNG TỰ: Giấu Mẹ đi nếu có Con, Cho phép xuất Mẹ nếu không có Con
            var validToShipIds = new List<int>();
            var groups = allDetails.GroupBy(x => string.IsNullOrEmpty(x.BundleCode) ? x.Id.ToString() : x.BundleCode);
            foreach (var g in groups)
            {
                var comps = g.Where(x => x.IsComponent).ToList();
                if (comps.Any()) validToShipIds.AddRange(comps.Select(x => x.Id));
                else validToShipIds.AddRange(g.Select(x => x.Id));
            }

            var rawItems = allDetails.Where(d => validToShipIds.Contains(d.Id)).Select(d => new
            {
                id = d.Id,
                orderCode = d.Order.OrderCode,
                productName = d.ProductName,
                specifications = d.Specifications,
                unitPrice = d.UnitPrice,
                quantityOrdered = d.Quantity,
                isComponent = d.IsComponent, // Truyền cờ này xuống UI để vẽ icon thụt lề

                shippedTotal = _context.ShipmentDetails.Where(sd => sd.OrderDetailId == d.Id && sd.Shipment.Type == ShipmentType.Standard).Sum(sd => (int?)sd.QuantityShipped) ?? 0,
                returnedTotal = _context.ShipmentDetails.Where(sd => sd.OrderDetailId == d.Id && sd.Shipment.Type == ShipmentType.Return).Sum(sd => (int?)Math.Abs(sd.QuantityShipped)) ?? 0,
                warrantyTotal = _context.ShipmentDetails.Where(sd => sd.OrderDetailId == d.Id && sd.Shipment.Type == ShipmentType.Warranty).Sum(sd => (int?)sd.QuantityShipped) ?? 0
            }).ToList();

            var finalItems = rawItems.Select(x =>
            {
                double pendingQty = type == ShipmentType.Standard ? x.quantityOrdered - x.shippedTotal : (type == ShipmentType.Return ? x.shippedTotal - x.returnedTotal : x.returnedTotal - x.warrantyTotal);
                return new { x.id, x.orderCode, x.productName, x.specifications, x.unitPrice, x.quantityOrdered, x.shippedTotal, x.returnedTotal, x.warrantyTotal, pending = pendingQty, x.isComponent };
            }).Where(x => x.pending > 0).OrderBy(x => x.orderCode).ToList();

            return Json(finalItems);
        }

        [Authorize(Policy = AppPermissions.Shipments.View)]
        public async Task<IActionResult> ReturnHistory(DateTime? fromDate, DateTime? toDate)
        {
            var startDate = fromDate ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var endDate = toDate ?? startDate.AddMonths(1).AddDays(-1);
            var endDateTime = endDate.Date.AddDays(1).AddTicks(-1);

            // 1. Lấy danh sách phiếu Nhập Trả (Return)
            var query = _context.Shipments
                .Include(s => s.Customer)
                .Include(s => s.Order)
                .Include(s => s.ShipmentDetails).ThenInclude(sd => sd.OrderDetail)
                .Include(s => s.ParentShipment)
                .Where(s => s.Type == ShipmentType.Return && s.ShipmentDate >= startDate && s.ShipmentDate <= endDateTime)
                .OrderByDescending(s => s.ShipmentDate);

            var returns = await query.ToListAsync();

            // 2. Lấy tất cả OrderDetailId (Mã sản phẩm trong đơn) đang bị lỗi
            var defectiveOrderDetailIds = returns
                .SelectMany(s => s.ShipmentDetails)
                .Select(sd => sd.OrderDetailId)
                .Distinct()
                .ToList();

            // 3. 🔥 TÌM TẤT CẢ CÁC LẦN XUẤT BÙ (WARRANTY) CỦA CÁC SẢN PHẨM NÀY
            var compensations = await _context.ShipmentDetails
                .Include(sd => sd.Shipment)
                .Where(sd => defectiveOrderDetailIds.Contains(sd.OrderDetailId)
                          && sd.Shipment.Type == ShipmentType.Warranty)
                .ToListAsync();

            // 4. Group by theo OrderDetailId và đẩy sang View
            var compDict = compensations
                .GroupBy(sd => sd.OrderDetailId)
                .ToDictionary(g => g.Key, g => g.ToList());

            ViewBag.Compensations = compDict;
            ViewBag.FromDate = startDate;
            ViewBag.ToDate = endDate;
            ViewBag.TotalDefectiveItems = returns.Sum(s => s.ShipmentDetails.Sum(d => Math.Abs(d.QuantityShipped)));

            return View(returns);
        }

        // ==========================================
        // IMPORT EXCEL - LỊCH SỬ GIAO HÀNG CŨ
        // ==========================================

        [HttpPost]
        [Authorize(Policy = AppPermissions.Shipments.Create)]
        public async Task<IActionResult> PreviewImport(IFormFile file)
        {
            if (file == null || file.Length == 0) { TempData["Error"] = "Vui lòng chọn file Excel."; return RedirectToAction(nameof(Index)); }

            var previewList = new List<ShipmentImportDto>();

            try
            {
                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                using var wb = new ClosedXML.Excel.XLWorkbook(stream);
                var ws = wb.Worksheet(1);
                var rows = ws.RowsUsed().Skip(1); // Bỏ qua Header dòng 1

                // 1. TẢI SẴN DỮ LIỆU ĐỂ ĐỐI CHIẾU (TỐI ƯU TỐC ĐỘ, TRÁNH LỖI N+1 QUERIES)
                var customers = await _context.Customers.ToListAsync();
                var activeOrders = await _context.Orders.Include(o => o.OrderDetails).ThenInclude(d => d.ShipmentDetails).ThenInclude(sd => sd.Shipment).ToListAsync();
                var existingShipmentCodes = await _context.Shipments.Select(s => s.ShipmentCode).ToListAsync();

                int rowIndex = 2;
                foreach (var row in rows)
                {
                    var item = new ShipmentImportDto { RowIndex = rowIndex++ };

                    // Đọc dữ liệu từ cột (Cấu trúc: A:Mã phiếu, B:Loại, C:Khách, D:Mã Đơn, E:Sản phẩm, F:SL, G:Ngày, H:Xe, I:Ghi chú)
                    item.ShipmentCode = row.Cell(1).GetValue<string>()?.Trim();
                    item.TypeName = row.Cell(2).GetValue<string>()?.Trim();
                    item.CustomerName = row.Cell(3).GetValue<string>()?.Trim();
                    item.OrderCode = row.Cell(4).GetValue<string>()?.Trim();
                    item.ProductName = row.Cell(5).GetValue<string>()?.Trim();
                    item.Quantity = row.Cell(6).TryGetValue<double>(out var q) ? q : 0;

                    string dateStr = row.Cell(7).GetValue<string>()?.Trim();
                    string[] formats = { "dd/MM/yyyy", "d/M/yyyy", "yyyy-MM-dd" };
                    item.ShipmentDate = DateTime.TryParseExact(dateStr, formats, null, System.Globalization.DateTimeStyles.None, out var d) ? d : DateTime.Now;

                    item.Vehicle = row.Cell(8).GetValue<string>()?.Trim();
                    item.Note = row.Cell(9).GetValue<string>()?.Trim();

                    // Xác định loại phiếu
                    if (!string.IsNullOrEmpty(item.TypeName) && item.TypeName.ToLower().Contains("trả")) item.Type = ShipmentType.Return;
                    else if (!string.IsNullOrEmpty(item.TypeName) && (item.TypeName.ToLower().Contains("bảo hành") || item.TypeName.ToLower().Contains("bù"))) item.Type = ShipmentType.Warranty;

                    // ================= BỘ LỌC KIỂM TRA (VALIDATION) =================

                    if (string.IsNullOrEmpty(item.ShipmentCode)) { item.IsValid = false; item.Errors.Add("Mã phiếu trống"); }
                    else if (existingShipmentCodes.Contains(item.ShipmentCode)) { item.IsValid = false; item.Errors.Add("Mã phiếu đã tồn tại trong DB"); }

                    if (string.IsNullOrEmpty(item.CustomerName)) { item.IsValid = false; item.Errors.Add("Tên Khách hàng trống"); }
                    else
                    {
                        var cust = customers.FirstOrDefault(c => c.CompanyName.ToLower() == item.CustomerName.ToLower());
                        if (cust == null) { item.IsValid = false; item.Errors.Add("Khách hàng không tồn tại"); }
                        else item.CustomerId = cust.Id;
                    }

                    if (string.IsNullOrEmpty(item.OrderCode)) { item.IsValid = false; item.Errors.Add("Mã đơn (PO) trống"); }
                    else
                    {
                        var order = activeOrders.FirstOrDefault(o => o.OrderCode.ToLower() == item.OrderCode.ToLower());
                        if (order == null) { item.IsValid = false; item.Errors.Add("Đơn hàng không tồn tại"); }
                        else if (order.CustomerId != item.CustomerId && item.CustomerId > 0) { item.IsValid = false; item.Errors.Add("Đơn hàng không thuộc Khách này"); }
                        else
                        {
                            item.OrderId = order.Id;

                            // Kiểm tra Sản phẩm
                            if (string.IsNullOrEmpty(item.ProductName)) { item.IsValid = false; item.Errors.Add("Tên SP trống"); }
                            else
                            {
                                var detail = order.OrderDetails.FirstOrDefault(od => od.ProductName.ToLower() == item.ProductName.ToLower());
                                if (detail == null) { item.IsValid = false; item.Errors.Add("Sản phẩm không có trong Đơn này"); }
                                else
                                {
                                    item.OrderDetailId = detail.Id;

                                    if (item.Quantity <= 0) { item.IsValid = false; item.Errors.Add("Số lượng phải > 0"); }
                                    else if (item.Type == ShipmentType.Standard) // Nếu là xuất bán, check số lượng tồn
                                    {
                                        double alreadyShipped = detail.ShipmentDetails?.Where(sd => sd.Shipment?.Type == ShipmentType.Standard).Sum(sd => sd.QuantityShipped) ?? 0;
                                        double shippedInExcel = previewList.Where(x => x.IsValid && x.OrderDetailId == detail.Id).Sum(x => x.Quantity); // Trừ hao những dòng đã đọc trước đó trong file Excel
                                        double remain = detail.Quantity - alreadyShipped - shippedInExcel;

                                        if (item.Quantity > remain) { item.IsValid = false; item.Errors.Add($"Xuất quá lố! (Chỉ còn {remain} / {detail.Quantity})"); }
                                    }
                                }
                            }
                        }
                    }

                    previewList.Add(item);
                }
            }
            catch (Exception ex) { TempData["Error"] = "Lỗi đọc cấu trúc file Excel: " + ex.Message; return RedirectToAction(nameof(Index)); }

            return View("ImportPreview", previewList);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = AppPermissions.Shipments.Create)]
        public async Task<IActionResult> ConfirmImport(string jsonData)
        {
            var items = System.Text.Json.JsonSerializer.Deserialize<List<ShipmentImportDto>>(jsonData);
            if (items == null || !items.Any()) return RedirectToAction(nameof(Index));

            var validItems = items.Where(x => x.IsValid).ToList();
            var invalidItems = items.Where(x => !x.IsValid).ToList();

            if (!validItems.Any())
            {
                TempData["Error"] = "Không có dòng dữ liệu hợp lệ nào để ghi vào Database.";
                return View("ImportResult", new { SuccessCount = 0, FailedItems = invalidItems });
            }

            int successCount = 0;
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Gom nhóm các dòng Excel chung 1 Mã Phiếu
                var shipmentGroups = validItems.GroupBy(x => x.ShipmentCode);

                foreach (var group in shipmentGroups)
                {
                    var first = group.First();
                    var newShipment = new Shipment
                    {
                        ShipmentCode = first.ShipmentCode,
                        Type = first.Type,
                        ShipmentDate = first.ShipmentDate,
                        CustomerId = first.CustomerId,
                        OrderId = first.OrderId,
                        VehicleNumber = first.Vehicle,
                        Notes = "[IMPORT EXCEL] " + string.Join(" | ", group.Select(g => g.Note).Where(n => !string.IsNullOrEmpty(n)).Distinct()),
                        ShipmentDetails = new List<ShipmentDetail>()
                    };

                    foreach (var d in group)
                    {
                        newShipment.ShipmentDetails.Add(new ShipmentDetail
                        {
                            OrderDetailId = d.OrderDetailId,
                            QuantityShipped = (int)d.Quantity  
                        });
                    }

                    _context.Shipments.Add(newShipment);
                    successCount++;
                }

                await _context.SaveChangesAsync();

                // Cập nhật trạng thái các Order vừa được xuất hàng
                var orderIds = validItems.Select(x => x.OrderId).Distinct().ToList();
                foreach (var id in orderIds) await UpdateOrderStatus(id);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Trả về trang Kết quả thay vì Redirect để Sếp xem dòng nào lỗi
                ViewBag.SuccessCount = successCount;
                return View("ImportResult", invalidItems);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = "Lỗi hệ thống khi lưu: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // ==========================================
        // 2. XUẤT BÁO CÁO EXCEL CHUYÊN NGHIỆP
        // ==========================================
        [HttpGet]
        [Authorize(Policy = AppPermissions.Shipments.View)]
        public async Task<IActionResult> ExportExcel(string shipmentCode, int? customerId, string productName, string vehicle, ShipmentType? type, DateTime? fromDate, DateTime? toDate)
        {
            var startDate = fromDate ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var endDateTime = (toDate ?? startDate.AddMonths(1).AddDays(-1)).Date.AddDays(1).AddTicks(-1);

            // Dùng chung bộ lọc với Index
            var query = _context.Shipments
                .Include(s => s.Customer).Include(s => s.Order).Include(s => s.ExportShipment)
                .Include(s => s.ShipmentDetails).ThenInclude(sd => sd.OrderDetail)
                .Where(s => s.ShipmentDate >= startDate && s.ShipmentDate <= endDateTime)
                .AsQueryable();

            if (!string.IsNullOrEmpty(shipmentCode)) query = query.Where(s => s.ShipmentCode.Contains(shipmentCode));
            if (customerId.HasValue) query = query.Where(s => s.CustomerId == customerId);
            if (type.HasValue) query = query.Where(s => s.Type == type);
            if (!string.IsNullOrEmpty(vehicle)) query = query.Where(s => s.VehicleNumber.Contains(vehicle) || (s.ExportShipment != null && (s.ExportShipment.LicensePlate.Contains(vehicle) || s.ExportShipment.ContainerNumber.Contains(vehicle))));
            if (!string.IsNullOrEmpty(productName)) query = query.Where(s => s.ShipmentDetails.Any(d => d.OrderDetail.ProductName.Contains(productName)));

            var data = await query.OrderByDescending(s => s.ShipmentDate).ToListAsync();

            using (var wb = new XLWorkbook())
            {
                var ws = wb.Worksheets.Add("LichSuXuatKho");

                // Tiêu đề
                ws.Cell("A1").Value = "BÁO CÁO LỊCH SỬ XUẤT KHO / GIAO HÀNG";
                ws.Range("A1:J1").Merge().Style.Font.SetBold().Font.SetFontSize(16).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell("A2").Value = $"Kỳ báo cáo: Từ {startDate:dd/MM/yyyy} đến {toDate?.ToString("dd/MM/yyyy") ?? startDate.AddMonths(1).AddDays(-1).ToString("dd/MM/yyyy")}";
                ws.Range("A2:J2").Merge().Style.Font.SetItalic().Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                // Header Cột
                string[] headers = { "STT", "Mã Phiếu", "Loại Xuất", "Thời Gian", "Khách Hàng / Đơn", "Sản Phẩm", "Số Lượng", "Đơn Giá", "Thành Tiền", "Chuyến Xe / Biển Số" };
                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = ws.Cell(4, i + 1);
                    cell.Value = headers[i];
                    cell.Style.Font.SetBold().Fill.SetBackgroundColor(XLColor.Teal).Font.SetFontColor(XLColor.White)
                        .Border.SetOutsideBorder(XLBorderStyleValues.Thin).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                }

                int row = 5; int stt = 1; decimal grandTotal = 0;

                // Đổ Dữ liệu chi tiết từng dòng (Grid Excel Mode)
                foreach (var s in data)
                {
                    string typeName = s.Type == ShipmentType.Standard ? "Xuất Bán" : (s.Type == ShipmentType.Return ? "Nhập Trả" : "Xuất Bù/BH");
                    string vehicleInfo = s.ExportShipment != null ? (s.ExportShipment.TransportType == TransportType.Truck ? s.ExportShipment.LicensePlate : s.ExportShipment.ContainerNumber) : (s.VehicleNumber ?? "Kho");

                    foreach (var d in s.ShipmentDetails)
                    {
                        decimal price = d.OrderDetail?.UnitPrice ?? 0;
                        decimal total = d.QuantityShipped * price;
                        grandTotal += total;

                        ws.Cell(row, 1).Value = stt++;
                        ws.Cell(row, 2).Value = s.ShipmentCode;
                        ws.Cell(row, 3).Value = typeName;
                        ws.Cell(row, 4).Value = s.ShipmentDate.ToString("dd/MM/yyyy HH:mm");
                        ws.Cell(row, 5).Value = s.Customer?.CompanyName + (s.OrderId.HasValue ? $" (PO: {s.Order?.OrderCode})" : "");
                        ws.Cell(row, 6).Value = d.OrderDetail?.ProductName + (string.IsNullOrEmpty(d.OrderDetail?.Specifications) ? "" : $" ({d.OrderDetail.Specifications})");
                        ws.Cell(row, 7).Value = d.QuantityShipped;
                        ws.Cell(row, 8).Value = price;
                        ws.Cell(row, 9).Value = total;
                        ws.Cell(row, 10).Value = vehicleInfo;

                        ws.Range(row, 1, row, 10).Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
                        ws.Cell(row, 7).Style.Font.SetBold().Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                        ws.Cell(row, 8).Style.NumberFormat.Format = "#,##0";
                        ws.Cell(row, 9).Style.NumberFormat.Format = "#,##0";
                        row++;
                    }
                }

                // Dòng Tổng Cộng
                ws.Cell(row, 8).Value = "TỔNG CỘNG:";
                ws.Cell(row, 8).Style.Font.SetBold().Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
                ws.Cell(row, 9).Value = grandTotal;
                ws.Cell(row, 9).Style.Font.SetBold().Font.SetFontColor(XLColor.Red).NumberFormat.Format = "#,##0";

                ws.Columns().AdjustToContents();
                ws.Column(6).Width = 35; // Cột Sản phẩm nới rộng
                ws.Column(5).Width = 30; // Cột Khách hàng nới rộng

                using (var stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"LichSuXuatKho_{DateTime.Now:ddMMyy}.xlsx");
                }
            }
        }
        // ====================================================================
        // TẢI FILE MẪU ĐỂ IMPORT EXCEL
        // ====================================================================
        [HttpGet]
        public IActionResult DownloadTemplate()
        {
            using (var wb = new ClosedXML.Excel.XLWorkbook())
            {
                var ws = wb.Worksheets.Add("Mau_NhapXuatKho");

                // 1. Tạo Dòng Tiêu Đề (Header)
                string[] headers = {
                    "Mã Phiếu", "Loại Phiếu", "Tên Khách Hàng", "Mã Đơn (PO)",
                    "Tên Sản Phẩm", "Số Lượng", "Ngày Xuất (dd/MM/yyyy)", "Biển Số Xe", "Ghi Chú"
                };

                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = ws.Cell(1, i + 1);
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.Teal;
                    cell.Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
                    cell.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
                    cell.Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                }

                // 2. Tạo một dòng dữ liệu ví dụ (Dummy Data)
                ws.Cell(2, 1).Value = "PX-260722-001";
                ws.Cell(2, 2).Value = "Xuất bán"; // Gợi ý: Xuất bán, Nhập trả, Xuất bảo hành
                ws.Cell(2, 3).Value = "Công ty TNHH ABC";
                ws.Cell(2, 4).Value = "PO-00123";
                ws.Cell(2, 5).Value = "Bàn làm việc giám đốc";
                ws.Cell(2, 6).Value = 15;
                ws.Cell(2, 7).Value = DateTime.Now.ToString("dd/MM/yyyy");
                ws.Cell(2, 8).Value = "51C-123.45";
                ws.Cell(2, 9).Value = "Giao trước 10h sáng";

                // Định dạng chữ in nghiêng màu xám cho dòng ví dụ để người dùng biết
                ws.Range("A2:I2").Style.Font.Italic = true;
                ws.Range("A2:I2").Style.Font.FontColor = ClosedXML.Excel.XLColor.Gray;

                // 3. Thêm dòng lưu ý đỏ ở dưới
                var noteCell = ws.Cell(4, 1);
                noteCell.Value = "LƯU Ý: Không sửa đổi/xóa dòng tiêu đề (Dòng 1). Tên Khách Hàng và Tên Sản Phẩm phải gõ chính xác 100% như trong hệ thống phần mềm.";
                ws.Range("A4:I4").Merge();
                noteCell.Style.Font.FontColor = ClosedXML.Excel.XLColor.Red;
                noteCell.Style.Font.Bold = true;

                // Tự động căn chỉnh độ rộng cột
                ws.Columns().AdjustToContents();

                // Trả file về cho trình duyệt tải xuống
                using (var stream = new System.IO.MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "FileMau_ImportXuatKho.xlsx");
                }
            }
        }
    }

    // ----------------------
    public class ShipmentImportDto
    {
        public int RowIndex { get; set; }
        public string ShipmentCode { get; set; }
        public string TypeName { get; set; }
        public string CustomerName { get; set; }
        public string OrderCode { get; set; }
        public string ProductName { get; set; }
        public double Quantity { get; set; }
        public DateTime ShipmentDate { get; set; }
        public string Vehicle { get; set; }
        public string Note { get; set; }

        // Trạng thái kiểm duyệt
        public bool IsValid { get; set; } = true;
        public List<string> Errors { get; set; } = new List<string>();

        // Khóa ngoại lưu nháp
        public ShipmentType Type { get; set; } = ShipmentType.Standard;
        public int CustomerId { get; set; }
        public int OrderId { get; set; }
        public int OrderDetailId { get; set; }
    }

}