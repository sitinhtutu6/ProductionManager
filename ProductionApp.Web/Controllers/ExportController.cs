using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProductionApp.Web.Constants;
using ProductionApp.Web.Helpers;
using ProductionManager.Data;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ProductionApp.Web.Controllers
{
    [Authorize]
    public class ExportController : Controller
    {
        private readonly AppDbContext _context;
        private readonly FileHelper _fileHelper;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ExportController(AppDbContext context, IWebHostEnvironment webHostEnvironment, FileHelper fileHelper)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _fileHelper = fileHelper;
        }

        public async Task<IActionResult> Workflow(int id)
        {
            var shipment = await _context.ExportShipments
                .Include(s => s.Customer)
                .Include(s => s.Documents)
                .Include(s => s.StepTrackers)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (shipment == null) return NotFound();

            return View(shipment); 
        }

        // ===================================================================
        // 1. DANH SÁCH CHUYẾN XE / LÔ HÀNG (INDEX)
        // ===================================================================
        [Authorize(Policy = AppPermissions.Shipments.View)]
        public async Task<IActionResult> Index(string searchString, DateTime? fromDate, DateTime? toDate, string sortOrder, string statusFilter, string viewMode = "list")
        {
            // Kiểm tra phân quyền: Chỉ Admin, Manager, Kế toán mới được xem giá
            bool canViewPrice = User.IsInRole("Admin") || User.IsInRole("Manager") || User.IsInRole("Accountant");
            ViewBag.CanViewPrice = canViewPrice;

            // Kéo thêm ShipmentOrders và Shipments (chi tiết hàng hóa xuất thực tế)
            var query = _context.ExportShipments
                .Include(s => s.Customer)
                .Include(s => s.Shipments)
                    .ThenInclude(sh => sh.ShipmentDetails)
                        .ThenInclude(sd => sd.OrderDetail)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                query = query.Where(s => s.ShipmentCode.ToLower().Contains(searchString)
                                      || (s.VesselName != null && s.VesselName.ToLower().Contains(searchString))
                                      || (s.BookingNumber != null && s.BookingNumber.ToLower().Contains(searchString))
                                      || (s.Customer != null && s.Customer.CompanyName.ToLower().Contains(searchString))
                                      || (s.LicensePlate != null && s.LicensePlate.ToLower().Contains(searchString))
                                      || (s.Destination != null && s.Destination.ToLower().Contains(searchString))
                                      || (s.Departure != null && s.Departure.ToLower().Contains(searchString)));
            }

            if (fromDate.HasValue) query = query.Where(s => s.ETD >= fromDate.Value);
            if (toDate.HasValue) query = query.Where(s => s.ETD <= toDate.Value);

            if (!string.IsNullOrEmpty(statusFilter))
            {
                if (statusFilter == "Done") query = query.Where(s => s.ProgressPercent == 100);
                else if (statusFilter == "Running") query = query.Where(s => s.ProgressPercent < 100);
            }

            ViewData["CodeSort"] = String.IsNullOrEmpty(sortOrder) ? "code_desc" : "";
            ViewData["DateSort"] = sortOrder == "Date" ? "date_desc" : "Date";
            ViewData["ProgressSort"] = sortOrder == "Progress" ? "progress_desc" : "Progress";

            query = sortOrder switch
            {
                "code_desc" => query.OrderByDescending(s => s.ShipmentCode),
                "Date" => query.OrderBy(s => s.ETD),
                "date_desc" => query.OrderByDescending(s => s.ETD),
                "Progress" => query.OrderBy(s => s.ProgressPercent),
                "progress_desc" => query.OrderByDescending(s => s.ProgressPercent),
                _ => query.OrderByDescending(s => s.CreatedDate),
            };

            ViewData["CurrentSearch"] = searchString;
            ViewData["FromDate"] = fromDate?.ToString("yyyy-MM-dd");
            ViewData["ToDate"] = toDate?.ToString("yyyy-MM-dd");
            ViewData["StatusFilter"] = statusFilter;
            ViewBag.ViewMode = viewMode;

            return View(await query.ToListAsync());
        }

        // ===================================================================
        // XUẤT BÁO CÁO EXCEL CHUYÊN NGHIỆP (PHÂN QUYỀN GIÁ)
        // ===================================================================
        [HttpGet]
        [Authorize(Policy = AppPermissions.Shipments.View)]
        public async Task<IActionResult> ExportToExcel(string searchString, DateTime? fromDate, DateTime? toDate, string statusFilter)
        {
            bool canViewPrice = User.IsInRole("Admin") || User.IsInRole("Manager") || User.IsInRole("Accountant");

            var query = _context.ExportShipments
                .Include(s => s.Customer)
                .Include(s => s.Shipments)
                    .ThenInclude(sh => sh.ShipmentDetails)
                        .ThenInclude(sd => sd.OrderDetail)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                query = query.Where(s => s.ShipmentCode.ToLower().Contains(searchString)
                                      || s.Customer.CompanyName.ToLower().Contains(searchString));
            }
            if (fromDate.HasValue) query = query.Where(s => s.ETD >= fromDate.Value);
            if (toDate.HasValue) query = query.Where(s => s.ETD <= toDate.Value);
            if (!string.IsNullOrEmpty(statusFilter))
            {
                if (statusFilter == "Done") query = query.Where(s => s.ProgressPercent == 100);
                else if (statusFilter == "Running") query = query.Where(s => s.ProgressPercent < 100);
            }

            var list = await query.OrderByDescending(s => s.CreatedDate).ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("BaoCaoGiaoHang");

                ws.Cell("A1").Value = "BÁO CÁO CHI TIẾT CÁC CHUYẾN GIAO HÀNG / XUẤT KHẨU";
                ws.Range(1, 1, 1, canViewPrice ? 12 : 9).Merge().Style.Font.SetBold().Font.SetFontSize(16).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                int col = 1;
                ws.Cell(3, col++).Value = "Mã Lô/Chuyến";
                ws.Cell(3, col++).Value = "Khách Hàng";
                ws.Cell(3, col++).Value = "Nơi Xuất Phát";
                ws.Cell(3, col++).Value = "Nơi Đến";
                ws.Cell(3, col++).Value = "Phương Tiện";
                ws.Cell(3, col++).Value = "Ngày Đi (ETD)";
                ws.Cell(3, col++).Value = "Tiến độ";
                ws.Cell(3, col++).Value = "Tên Sản Phẩm";
                ws.Cell(3, col++).Value = "Số Lượng";

                if (canViewPrice)
                {
                    ws.Cell(3, col++).Value = "Đơn Giá";
                    ws.Cell(3, col++).Value = "Thành Tiền";
                    ws.Cell(3, col++).Value = "Cước Vận Chuyển";
                }

                // Format Header
                var headerRange = ws.Range(3, 1, 3, col - 1);
                headerRange.Style.Font.SetBold().Fill.SetBackgroundColor(XLColor.LightGray).Border.SetOutsideBorder(XLBorderStyleValues.Thin).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                int row = 4;
                decimal grandTotalShippingCost = 0;
                decimal grandTotalItemsValue = 0;

                foreach (var item in list)
                {
                    string transportInfo = item.TransportType == TransportType.Container ? $"{item.BookingNumber} / {item.VesselName}" : item.LicensePlate;

                    // Nếu chuyến xe chưa xuất kho mặt hàng nào, in thông tin chuyến xe trống
                    if (item.Shipments == null || !item.Shipments.Any(sh => sh.ShipmentDetails.Any()))
                    {
                        col = 1;
                        ws.Cell(row, col++).Value = item.ShipmentCode;
                        ws.Cell(row, col++).Value = item.Customer?.CompanyName;
                        ws.Cell(row, col++).Value = item.Departure;
                        ws.Cell(row, col++).Value = item.Destination;
                        ws.Cell(row, col++).Value = transportInfo;
                        ws.Cell(row, col++).Value = item.ETD?.ToString("dd/MM/yyyy");
                        ws.Cell(row, col++).Value = item.ProgressPercent + "%";
                        ws.Cell(row, col++).Value = "(Chưa xuất kho mặt hàng nào)";
                        ws.Cell(row, col++).Value = 0;

                        if (canViewPrice)
                        {
                            ws.Cell(row, col++).Value = 0;
                            ws.Cell(row, col++).Value = 0;
                            ws.Cell(row, col++).Value = item.ShippingCost;
                            grandTotalShippingCost += item.ShippingCost;
                        }

                        ws.Range(row, 1, row, col - 1).Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
                        row++;
                    }
                    else
                    {
                        // In chi tiết từng mặt hàng trong chuyến xe
                        bool isFirstItemInTrip = true;
                        foreach (var ship in item.Shipments)
                        {
                            foreach (var detail in ship.ShipmentDetails)
                            {
                                col = 1;
                                ws.Cell(row, col++).Value = item.ShipmentCode;
                                ws.Cell(row, col++).Value = item.Customer?.CompanyName;
                                ws.Cell(row, col++).Value = item.Departure;
                                ws.Cell(row, col++).Value = item.Destination;
                                ws.Cell(row, col++).Value = transportInfo;
                                ws.Cell(row, col++).Value = item.ETD?.ToString("dd/MM/yyyy");
                                ws.Cell(row, col++).Value = item.ProgressPercent + "%";
                                ws.Cell(row, col++).Value = detail.OrderDetail?.ProductName + (string.IsNullOrEmpty(detail.OrderDetail?.Specifications) ? "" : $" ({detail.OrderDetail?.Specifications})");
                                ws.Cell(row, col++).Value = detail.QuantityShipped;

                                if (canViewPrice)
                                {
                                    decimal price = detail.OrderDetail?.UnitPrice ?? 0;
                                    decimal itemTotal = detail.QuantityShipped * price;
                                    grandTotalItemsValue += itemTotal;

                                    ws.Cell(row, col++).Value = price;
                                    ws.Cell(row, col++).Value = itemTotal;

                                    // Chỉ in cước vận chuyển 1 lần ở dòng đầu tiên của chuyến xe để tránh cộng dồn sai
                                    if (isFirstItemInTrip)
                                    {
                                        ws.Cell(row, col++).Value = item.ShippingCost;
                                        grandTotalShippingCost += item.ShippingCost;
                                        isFirstItemInTrip = false;
                                    }
                                    else
                                    {
                                        ws.Cell(row, col++).Value = "";
                                    }
                                }

                                ws.Range(row, 1, row, col - 1).Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
                                if (canViewPrice) { ws.Cell(row, col - 3).Style.NumberFormat.Format = "#,##0"; ws.Cell(row, col - 2).Style.NumberFormat.Format = "#,##0"; ws.Cell(row, col - 1).Style.NumberFormat.Format = "#,##0"; }
                                row++;
                            }
                        }
                    }
                }

                if (canViewPrice)
                {
                    ws.Cell(row, 10).Value = "TỔNG CỘNG:"; ws.Cell(row, 10).Style.Font.SetBold().Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
                    ws.Cell(row, 11).Value = grandTotalItemsValue; ws.Cell(row, 11).Style.Font.SetBold().Font.SetFontColor(XLColor.Red).NumberFormat.Format = "#,##0";
                    ws.Cell(row, 12).Value = grandTotalShippingCost; ws.Cell(row, 12).Style.Font.SetBold().Font.SetFontColor(XLColor.Red).NumberFormat.Format = "#,##0";
                }

                ws.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Logistics_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
                }
            }
        }

        // ===================================================================
        // 2. TẠO LÔ HÀNG / CHUYẾN XE (CREATE)
        // ===================================================================
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.CustomerId = new SelectList(_context.Customers, "Id", "CompanyName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = AppPermissions.Shipments.Create)]
        public async Task<IActionResult> Create(ExportShipment shipment)
        {
            ModelState.Clear();

            // Tự Validate tay các trường bắt buộc
            if (shipment.CustomerId <= 0)
            {
                ModelState.AddModelError("CustomerId", "Vui lòng chọn Khách hàng.");
            }

            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(shipment.ShipmentCode))
                {
                    shipment.ShipmentCode = $"TRX-{DateTime.Now:yyMM}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}";
                }

                // Bổ sung các thông số ngầm định
                shipment.CreatedDate = DateTime.Now;
                shipment.ProgressPercent = 0;
                shipment.CurrentStep = ExportStep.Contract;
                shipment.Status = ExportStatus.Planning; // Đang lên kế hoạch

                _context.Add(shipment);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Đã khởi tạo chuyến xe thành công!";
                return RedirectToAction(nameof(Details), new { id = shipment.Id });
            }

            // Nếu vẫn còn lỗi, nạp lại danh sách khách hàng để form không bị sập
            ViewBag.CustomerId = new SelectList(_context.Customers, "Id", "CompanyName", shipment.CustomerId);
            return View(shipment);
        }

        // ===================================================================
        // 3. TRUNG TÂM ĐIỀU PHỐI (DETAILS)
        // ===================================================================
        public async Task<IActionResult> Details(int id)
        {
            var shipment = await _context.ExportShipments
                .Include(s => s.Customer)
                .Include(s => s.ShipmentOrders).ThenInclude(eo => eo.Order).ThenInclude(o => o.OrderDetails)
                .Include(s => s.Shipments).ThenInclude(sh => sh.ShipmentDetails).ThenInclude(sd => sd.OrderDetail)
                .Include(s => s.Documents)
                .Include(s => s.StepTrackers)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (shipment == null) return NotFound();

            // 1. Lấy tất cả các đơn hàng hợp lệ (chưa lên chuyến xe này)
            var allCustomerOrders = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails)
                .Where(o => !_context.ExportShipmentOrders.Any(eo => eo.OrderId == o.Id && eo.ExportShipmentId == id))
                .ToListAsync();

            var pendingOrders = new List<Order>();

            foreach (var o in allCustomerOrders)
            {
                bool isValidStatus = o.Status != OrderStatus.PendingApproval
                                  && o.Status != OrderStatus.Delivered
                                  && o.Status != OrderStatus.Invoiced
                                  && o.Status != OrderStatus.Canceled;

                if (isValidStatus)
                {
                    pendingOrders.Add(o);
                    continue;
                }

                // Xử lý nợ hàng lỗi
                if (o.Status == OrderStatus.Delivered)
                {
                    var detailIds = o.OrderDetails.Select(d => d.Id).ToList();

                    // 🔥 VÁ LỖI EF CORE CHỖ NÀY: Ép kiểu nullable (double?) để xử lý giá trị NULL từ SQL
                    var returnQtyDb = await _context.ShipmentDetails
                        .Where(sd => detailIds.Contains(sd.OrderDetailId) && sd.Shipment.Type == ShipmentType.Return)
                        .SumAsync(sd => (double?)sd.QuantityShipped) ?? 0;

                    var returnQty = Math.Abs(returnQtyDb); // Đưa Math.Abs ra ngoài xử lý bằng C#

                    var warrantyQty = await _context.ShipmentDetails
                        .Where(sd => detailIds.Contains(sd.OrderDetailId) && sd.Shipment.Type == ShipmentType.Warranty)
                        .SumAsync(sd => (double?)sd.QuantityShipped) ?? 0;

                    if (returnQty > warrantyQty) pendingOrders.Add(o);
                }
            }

            ViewBag.AvailableOrders = pendingOrders; 

            // 2. Lấy danh sách Khách hàng đang có đơn để làm Bộ lọc
            ViewBag.PendingCustomers = pendingOrders
                .Select(o => o.Customer)
                .Where(c => c != null)
                .GroupBy(c => c.Id).Select(g => g.First())
                .ToList();

            // 3. Lấy danh sách Khách hàng ĐÃ CÓ TRÊN XE để tự động tô sáng bộ lọc
            ViewBag.CurrentCustomerIds = shipment.ShipmentOrders
                .Where(so => so.Order != null)
                .Select(so => so.Order.CustomerId)
                .Distinct()
                .ToList();

            // 4. Danh sách toàn bộ khách cho Modal Sửa Xe
            ViewBag.Customers = await _context.Customers.Select(c => new { c.Id, c.CompanyName }).ToListAsync();

            return View(shipment);
        }

        // ===================================================================
        // 4. QUẢN LÝ KẾ HOẠCH & VẬN TẢI (LOGISTICS)
        // ===================================================================
        [HttpPost]
        public async Task<IActionResult> AddOrderToPlan(int exportId, int orderId)
        {
            if (exportId <= 0 || orderId <= 0) return BadRequest("Dữ liệu không hợp lệ.");

            // Kiểm tra xem đơn hàng đã nằm trên xe này chưa để tránh trùng lặp
            var exists = await _context.ExportShipmentOrders
                .AnyAsync(x => x.ExportShipmentId == exportId && x.OrderId == orderId);

            if (!exists)
            {
                var newPlan = new ExportShipmentOrder
                {
                    ExportShipmentId = exportId,
                    OrderId = orderId
                };

                _context.ExportShipmentOrders.Add(newPlan);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Đã ghép đơn hàng lên chuyến xe thành công!";
            }

            return RedirectToAction(nameof(Details), new { id = exportId });
        }

        // ===================================================================
        // 6. GỠ ĐƠN HÀNG KHỎI CHUYẾN XE (REMOVE FROM PLAN)
        // ===================================================================
        [HttpPost]
        public async Task<IActionResult> RemoveOrderFromPlan(int exportId, int orderId)
        {
            var planItem = await _context.ExportShipmentOrders
                .FirstOrDefaultAsync(x => x.ExportShipmentId == exportId && x.OrderId == orderId);

            if (planItem != null)
            {
                _context.ExportShipmentOrders.Remove(planItem);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Đã gỡ bỏ đơn hàng khỏi chuyến xe!";
            }

            return RedirectToAction(nameof(Details), new { id = exportId });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateLogisticsInfo(int id, ExportShipment info)
        {
            var item = await _context.ExportShipments.FindAsync(id);
            if (item == null) return NotFound();

            // Cập nhật thông tin Vận tải xe lẻ & Chi phí
            item.TransportType = info.TransportType;
            item.CarrierName = info.CarrierName;
            item.LicensePlate = info.LicensePlate;
            item.ShippingCost = info.ShippingCost;
            item.PaymentStatus = info.PaymentStatus;

            // Cập nhật thông tin Container / Tàu
            item.BookingNumber = info.BookingNumber;
            item.ContainerNumber = info.ContainerNumber;
            item.SealNumber = info.SealNumber;
            item.VesselName = info.VesselName;
            item.ETD = info.ETD;
            item.ETA = info.ETA;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Đã cập nhật thông tin vận tải!";
            return RedirectToAction(nameof(Details), new { id = id });
        }

        // ===================================================================
        // 5. UPLOAD & XÓA TÀI LIỆU
        // ===================================================================
        [HttpPost]
        public async Task<IActionResult> UploadDocument(int shipmentId, ExportStep step, IFormFile file, string note)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Vui lòng chọn file!";
                return RedirectToAction(nameof(Details), new { id = shipmentId });
            }

            var shipment = await _context.ExportShipments.FindAsync(shipmentId);
            if (shipment == null) return NotFound();

            try
            {
                string folderStructure = Path.Combine("exports", shipment.ShipmentCode, step.ToString());
                string relativePath = await _fileHelper.UploadFileAsync(file, folderStructure);

                var doc = new ExportDocument
                {
                    ExportShipmentId = shipmentId,
                    Step = step,
                    FileName = file.FileName,
                    FilePath = relativePath,
                    UploadDate = DateTime.Now,
                    Note = note
                };

                _context.ExportDocuments.Add(doc);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã tải lên tài liệu thành công!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi upload: " + ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id = shipmentId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteDocument(int id)
        {
            var doc = await _context.ExportDocuments.FindAsync(id);
            if (doc != null)
            {
                _fileHelper.DeleteFile(doc.FilePath);
                _context.ExportDocuments.Remove(doc);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã xóa tài liệu.";
                return RedirectToAction(nameof(Details), new { id = doc.ExportShipmentId });
            }
            return RedirectToAction(nameof(Index));
        }

        // ===================================================================
        // 6. CẬP NHẬT TIẾN ĐỘ & BƯỚC THỰC HIỆN
        // ===================================================================
        [HttpPost]
        public async Task<IActionResult> UpdateStepStatus(int shipmentId, ExportStep step, StepStatus status, string note)
        {
            var shipment = await _context.ExportShipments.Include(s => s.StepTrackers).FirstOrDefaultAsync(s => s.Id == shipmentId);
            if (shipment == null) return Json(new { success = false, message = "Không tìm thấy lô hàng" });

            var tracker = shipment.StepTrackers.FirstOrDefault(t => t.Step == step);
            if (tracker == null)
            {
                tracker = new ExportStepTracker { ExportShipmentId = shipmentId, Step = step };
                _context.StepTrackers.Add(tracker);
            }

            tracker.Status = status;
            tracker.Note = note;
            if (status == StepStatus.Done) tracker.CompletedDate = DateTime.Now;

            // Tính % tiến độ
            double totalScore = 0;
            foreach (ExportStep s in Enum.GetValues(typeof(ExportStep)))
            {
                var t = shipment.StepTrackers.FirstOrDefault(x => x.Step == s);
                var currentStatus = (s == step) ? status : (t?.Status ?? StepStatus.Pending);
                if (currentStatus == StepStatus.Done) totalScore += 1;
                else if (currentStatus == StepStatus.Active) totalScore += 0.5;
            }
            shipment.ProgressPercent = (int)((totalScore / 9.0) * 100);
            shipment.CurrentStep = step;

            await _context.SaveChangesAsync();
            return Json(new { success = true, percent = shipment.ProgressPercent });
        }

        // ===================================================================
        // 7. XÓA CHUYẾN XE, IN ẤN & XUẤT EXCEL
        // ===================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var shipment = await _context.ExportShipments.FindAsync(id);
            if (shipment != null)
            {
                _context.ExportShipments.Remove(shipment);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Print(int id)
        {
            var shipment = await _context.ExportShipments
                .Include(s => s.Customer)
                .Include(s => s.ShipmentOrders).ThenInclude(so => so.Order)
                .Include(s => s.Documents)
                .Include(s => s.StepTrackers)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (shipment == null) return NotFound();

            return View(shipment);
        }


        // ===================================================================
        // 8. BÁO CÁO CÔNG NỢ VẬN CHUYỂN (MỚI)
        // ===================================================================
        public async Task<IActionResult> LogisticsReport(int? month, int? year, string carrier)
        {
            var m = month ?? DateTime.Now.Month;
            var y = year ?? DateTime.Now.Year;

            var query = _context.ExportShipments
                .Where(x => x.CreatedDate.Month == m && x.CreatedDate.Year == y);

            if (!string.IsNullOrEmpty(carrier))
                query = query.Where(x => x.CarrierName.Contains(carrier));

            var data = await query.OrderByDescending(x => x.CreatedDate).ToListAsync();

            ViewBag.Month = m;
            ViewBag.Year = y;
            ViewBag.TotalCost = data.Sum(x => x.ShippingCost);
            ViewBag.TotalUnpaid = data.Where(x => x.PaymentStatus == PaymentStatuses.Unpaid).Sum(x => x.ShippingCost);

            return View(data);
        }


    }
}