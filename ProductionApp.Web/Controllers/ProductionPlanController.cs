using ClosedXML.Excel;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductionApp.Web.Constants;
using ProductionManager.Data;
using ProductionManager.Data.Entities;
using System.Text.RegularExpressions;

namespace ProductionApp.Web.Controllers
{
    [Authorize(Roles = "Admin, Manager")]
    public class ProductionPlanController : Controller
    {
        private readonly AppDbContext _context;

        public ProductionPlanController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        // 1. API LẤY DỮ LIỆU (Chia làm 2 nhóm: Đã xếp & Chưa xếp)

        [HttpGet]
        public async Task<IActionResult> GetProductionData()
        {
            var orders = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails) // Include thêm để đếm số lượng
                .Where(x => x.Status == OrderStatus.Approved || x.Status == OrderStatus.InProduction)
                .ToListAsync();

            // Nhóm 1: Chưa xếp lịch
            var unplanned = orders.Where(x => x.ProductionStartDate == null).Select(x => new
            {
                id = x.Id,
                content = x.OrderCode,
                customer = x.Customer?.CompanyName ?? "Khách lẻ",
                deadline = x.DeliveryDeadline?.ToString("dd/MM"),
                qty = x.OrderDetails?.Sum(d => d.Quantity) ?? 0,
                // Cờ báo trễ cho list chờ
                isLate = x.DeliveryDeadline.HasValue && x.DeliveryDeadline.Value.Date < DateTime.Now.Date
            });

            // Nhóm 2: Đã xếp lịch
            var planned = orders.Where(x => x.ProductionStartDate != null).Select(x => {

                string className = "orange-event"; // Mặc định
                string style = ""; // Style tùy chỉnh

                // Logic cũ: Check trễ hạn & trạng thái
                bool isLate = x.DeliveryDeadline.HasValue && x.DeliveryDeadline.Value.Date < DateTime.Now.Date;
                if (isLate) className = "red-event";
                else if (x.Status == OrderStatus.InProduction) className = "blue-event";

                // 🔥 LOGIC MỚI: Nếu người dùng đã chọn màu riêng -> Ghi đè tất cả
                if (!string.IsNullOrEmpty(x.PlanColor))
                {
                    className = ""; // Bỏ class mặc định
                                    // Áp dụng màu nền trực tiếp (CSS Inline)
                    style = $"background-color: {x.PlanColor}; border-color: {x.PlanColor}; color: white;";
                }

                return new
                {
                    id = x.Id,
                    content = $"<b>{x.OrderCode}</b> <span style='font-size:0.8em'>({x.Customer?.CompanyName})</span>",
                    start = x.ProductionStartDate,
                    end = x.ProductionEndDate ?? x.ProductionStartDate.Value.AddDays(2),
                    title = $"Hạn giao: {x.DeliveryDeadline:dd/MM/yyyy}",
                    className = className,
                    style = style,


                    orderCode = x.OrderCode,
                    customer = x.Customer?.CompanyName ?? "Khách lẻ",
                    deadline = x.DeliveryDeadline?.ToString("dd/MM"),
                    qty = x.OrderDetails?.Sum(d => d.Quantity) ?? 0,
                    isLate = isLate
                };
            });

            return Json(new { unplanned, planned });
        }

        // 2. API LƯU TẤT CẢ (Manual Save)
        [HttpPost]
        [Authorize(Policy = AppPermissions.Plans.Create)]
        public async Task<IActionResult> SaveAllPlans([FromBody] List<PlanItemModel> items)
        {
            try
            {
                // 1. Lấy danh sách ID các đơn đang hiển thị trên Timeline (Do Client gửi lên)
                var activeIds = items.Select(x => x.Id).ToList();

                // 2. Lấy TẤT CẢ đơn hàng cần quan tâm (Đang SX hoặc Chờ duyệt) từ Database
                // Để mình rà soát xem đơn nào bị xóa
                var dbOrders = await _context.Orders
                    .Where(x => x.Status == OrderStatus.Approved || x.Status == OrderStatus.InProduction)
                    .ToListAsync();

                foreach (var order in dbOrders)
                {
                    // Kiểm tra xem đơn hàng trong DB có nằm trong danh sách Timeline gửi lên không?
                    var timelineItem = items.FirstOrDefault(x => x.Id == order.Id);

                    if (timelineItem != null)
                    {
                        // A. CÓ TRÊN TIMELINE -> Cập nhật ngày mới
                        order.ProductionStartDate = timelineItem.Start;
                        order.ProductionEndDate = timelineItem.End;
                    }
                    else
                    {
                        // B. KHÔNG CÓ TRÊN TIMELINE (Tức là người dùng đã xóa) -> Reset về NULL
                        // Chỉ reset nếu nó đang có ngày (để tránh update dư thừa)
                        if (order.ProductionStartDate != null)
                        {
                            order.ProductionStartDate = null;
                            order.ProductionEndDate = null;
                        }
                    }
                }

                await _context.SaveChangesAsync();
                return Ok(new { message = "Đã cập nhật kế hoạch thành công!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }


        // 3. THÊM API CẬP NHẬT MÀU (POST)
        [HttpPost]
        [Authorize(Policy = AppPermissions.Plans.Create)]
        public async Task<IActionResult> UpdateOrderColor([FromBody] ColorUpdateModel model)
        {
            try
            {
                var order = await _context.Orders.FindAsync(model.Id);
                if (order != null)
                {
                    order.PlanColor = model.Color; // Lưu màu (hoặc null để reset)
                    await _context.SaveChangesAsync();
                    return Ok(new { message = "Đã đổi màu!" });
                }
                return NotFound();
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // ==============================================================
        // 5. XUẤT EXCEL (EXPORT) - ĐÃ CẬP NHẬT CỘT NGÀY
        // ==============================================================
        [HttpGet]
        [Authorize(Policy = AppPermissions.Plans.Create)]
        public async Task<IActionResult> ExportToExcel()
        {
            // Lấy tất cả đơn hàng cần làm việc (Chờ duyệt hoặc Đang SX)
            var orders = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails)
                .Where(x => x.Status == OrderStatus.Approved || x.Status == OrderStatus.InProduction)
                .OrderBy(x => x.ProductionIndex)
                .ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("KeHoachSanXuat");

                // --- A. TẠO HEADER (ĐỒNG BỘ VỚI IMPORT) ---
                worksheet.Cell(1, 1).Value = "STT";
                worksheet.Cell(1, 2).Value = "Khách Hàng";
                worksheet.Cell(1, 3).Value = "Mã Đơn";         // KEY ĐỂ TÌM KIẾM
                worksheet.Cell(1, 4).Value = "Tên Sản Phẩm";
                worksheet.Cell(1, 5).Value = "Tổng SL";
                worksheet.Cell(1, 6).Value = "Ngày Nhận";
                worksheet.Cell(1, 7).Value = "Hạn Giao";       // Cột 7: Deadline
                worksheet.Cell(1, 8).Value = "Ghi Chú";        // Cột 8: Note

                // 🔥 CỘT QUAN TRỌNG CHO TIMELINE 🔥
                worksheet.Cell(1, 9).Value = "Bắt Đầu SX";     // Cột 9: StartDate
                worksheet.Cell(1, 10).Value = "Kết Thúc SX";   // Cột 10: EndDate

                // Style cho Header đẹp mắt
                var header = worksheet.Range("A1:J1");
                header.Style.Font.Bold = true;
                header.Style.Fill.BackgroundColor = XLColor.CornflowerBlue;
                header.Style.Font.FontColor = XLColor.White;

                // --- B. ĐỔ DỮ LIỆU ---
                int row = 2;
                int stt = 1;
                foreach (var item in orders)
                {
                    string productNames = item.OrderDetails != null ? string.Join(", ", item.OrderDetails.Select(p => p.ProductName)) : "";

                    worksheet.Cell(row, 1).Value = stt;
                    worksheet.Cell(row, 2).Value = item.Customer?.CompanyName;
                    worksheet.Cell(row, 3).Value = item.OrderCode; // Quan trọng nhất
                    worksheet.Cell(row, 4).Value = productNames;
                    worksheet.Cell(row, 5).Value = item.OrderDetails?.Sum(x => x.Quantity) ?? 0;
                    worksheet.Cell(row, 6).Value = item.OrderDate;
                    worksheet.Cell(row, 7).Value = item.DeliveryDeadline;
                    worksheet.Cell(row, 8).Value = item.Notes;

                    // Xuất dữ liệu ngày tháng hiện tại (nếu có)
                    worksheet.Cell(row, 9).Value = item.ProductionStartDate;
                    worksheet.Cell(row, 10).Value = item.ProductionEndDate;

                    // Format ngày tháng cho dễ nhìn trong Excel
                    worksheet.Cell(row, 6).Style.DateFormat.Format = "dd/MM/yyyy";
                    worksheet.Cell(row, 7).Style.DateFormat.Format = "dd/MM/yyyy";
                    worksheet.Cell(row, 9).Style.DateFormat.Format = "dd/MM/yyyy";
                    worksheet.Cell(row, 10).Style.DateFormat.Format = "dd/MM/yyyy";

                    row++;
                    stt++;
                }

                // Tự động chỉnh độ rộng cột
                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"KeHoachSX_{DateTime.Now:ddMMyy}.xlsx");
                }
            }
        }

        // ==============================================================
        // 6. NHẬP EXCEL (IMPORT) - ĐỒNG BỘ LOGIC
        // ==============================================================
        [HttpPost]
        [Authorize(Policy = AppPermissions.Plans.Create)]
        public async Task<IActionResult> ImportFromExcel(IFormFile fileExcel)
        {
            if (fileExcel == null || fileExcel.Length == 0) return Json(new { success = false, message = "Vui lòng chọn file Excel!" });

            try
            {
                using (var stream = new MemoryStream())
                {
                    await fileExcel.CopyToAsync(stream);
                    using (var workbook = new XLWorkbook(stream))
                    {
                        var worksheet = workbook.Worksheet(1); // Lấy Sheet đầu tiên
                        var rows = worksheet.RangeUsed().RowsUsed().Skip(1); // Bỏ qua dòng Header (Dòng 1)

                        int updatedCount = 0; // Tổng số đơn tìm thấy
                        int plannedCount = 0; // Tổng số đơn ĐÃ XẾP LỊCH

                        foreach (var row in rows)
                        {
                            // 1. Tìm đơn hàng dựa trên Mã Đơn (Cột 3)
                            string orderCode = row.Cell(3).GetValue<string>();
                            if (string.IsNullOrEmpty(orderCode)) continue;

                            var order = await _context.Orders.FirstOrDefaultAsync(x => x.OrderCode == orderCode);

                            if (order != null)
                            {
                                // 2. Cập nhật thông tin phụ (Deadline, Ghi chú)
                                // Cột 7: Hạn giao
                                if (!row.Cell(7).IsEmpty() && row.Cell(7).TryGetValue(out DateTime deadline))
                                {
                                    order.DeliveryDeadline = deadline;
                                }

                                // Cột 8: Ghi chú
                                if (!row.Cell(8).IsEmpty())
                                {
                                    order.Notes = row.Cell(8).GetValue<string>();
                                }

                                // 3. 🔥 XỬ LÝ LỊCH SẢN XUẤT (CỘT 9 & 10) 🔥
                                var startCell = row.Cell(9);
                                var endCell = row.Cell(10);

                                // Nếu Excel có điền Ngày Bắt Đầu
                                if (!startCell.IsEmpty() && startCell.TryGetValue(out DateTime startDate))
                                {
                                    order.ProductionStartDate = startDate;

                                    // Xử lý Ngày Kết Thúc (Cột 10)
                                    if (!endCell.IsEmpty() && endCell.TryGetValue(out DateTime endDate))
                                    {
                                        order.ProductionEndDate = endDate;
                                    }
                                    else
                                    {
                                        // Nếu quên điền ngày kết thúc -> Tự động +2 ngày
                                        order.ProductionEndDate = startDate.AddDays(2);
                                    }
                                    plannedCount++;
                                }
                                else
                                {
                                    // Nếu Excel KHÔNG điền ngày (để trống)
                                    // -> Giữ nguyên lịch cũ (nếu có) HOẶC Reset về NULL (bỏ khỏi lịch)
                                    // Ở đây ta chọn cách an toàn: GIỮ NGUYÊN (Không làm gì cả)
                                    // Nếu bạn muốn Reset về list chờ thì bỏ comment dòng dưới:
                                    // order.ProductionStartDate = null; order.ProductionEndDate = null;
                                }

                                updatedCount++;
                            }
                        }

                        await _context.SaveChangesAsync();

                        string msg = $"Tìm thấy {updatedCount} đơn hàng.";
                        if (plannedCount > 0) msg += $" Đã cập nhật lịch cho {plannedCount} đơn.";
                        else msg += " (Lưu ý: Không có đơn nào được xếp lịch vì thiếu cột 'Bắt Đầu SX')";

                        return Json(new { success = true, message = msg });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi xử lý file: " + ex.Message });
            }

        }

        // ==============================================================
        // 1. TRẠM GIÁM SÁT (ĐÃ NÂNG CẤP BỘ DÒ TÌM SÓT 1 PHẦN)
        // ==============================================================
        [HttpGet]
        public async Task<IActionResult> Tracking()
        {
            var pendingOrders = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails)
                .Where(o => o.Status == OrderStatus.Approved ||
                            (o.Status == OrderStatus.InProduction &&
                                (
                                    (o.OrderDetails.Any() && _context.WorkOrders.Count(w => w.OrderId == o.Id && w.Status != WorkOrderStatus.Canceled) < o.OrderDetails.Count) ||
                                    (!o.OrderDetails.Any() && !_context.WorkOrders.Any(w => w.OrderId == o.Id && w.Status != WorkOrderStatus.Canceled))
                                )
                            ))
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(pendingOrders);
        }

        /*ApproveAndCreateWorkOrder
        /--------------------------------*/

        //[HttpPost]
        //public async Task<IActionResult> ApproveAndCreateWorkOrder(int orderId)
        //{
        //    var order = await _context.Orders.Include(o => o.OrderDetails).FirstOrDefaultAsync(o => o.Id == orderId);

        //    if (order == null)
        //        return Json(new { success = false, message = "Đơn hàng không tồn tại!" });

        //    // 🔥 FIX LỖI: Thay vì chặn theo Status, ta kiểm tra thực tế xem đơn này đã có Lệnh SX nào chưa
        //    bool hasWorkOrders = await _context.WorkOrders.AnyAsync(w => w.OrderId == orderId);

        //    if (hasWorkOrders)
        //        return Json(new { success = false, message = "Đơn này đã được tạo Lệnh Sản Xuất từ trước rồi!" });

        //    using var transaction = await _context.Database.BeginTransactionAsync();
        //    try
        //    {
        //        int index = 1;
        //        foreach (var detail in order.OrderDetails)
        //        {
        //            var wo = new WorkOrder
        //            {
        //                OrderId = order.Id,
        //                WOCode = $"LSX-{order.OrderCode}-{index:D2}",
        //                ProductName = detail.ProductName,
        //                TargetQuantity = detail.Quantity,
        //                EndDate = order.DeliveryDeadline,
        //                Notes = detail.Notes,
        //                ProductionIndex = 999, // Đẩy xuống cuối bảng
        //                Status = WorkOrderStatus.Pending // Đảm bảo luôn hiện lên Bảng Kế Hoạch
        //            };
        //            _context.WorkOrders.Add(wo);
        //            index++;
        //        }

        //        // Cập nhật đơn gốc thành Đang sản xuất
        //        order.Status = OrderStatus.InProduction;

        //        // Ghi nhật ký
        //        _context.OrderHistories.Add(new OrderHistory
        //        {
        //            Action = "Tạo Lệnh Sản Xuất",
        //            NewValue = $"Sinh ra {order.OrderDetails.Count} lệnh từ đơn {order.OrderCode}",
        //            ChangedBy = User.Identity?.Name ?? "System"
        //        });

        //        await _context.SaveChangesAsync();
        //        await transaction.CommitAsync();

        //        return Json(new { success = true, message = "Đã sinh Lệnh Sản Xuất chuyển xuống Xưởng!" });
        //    }
        //    catch (Exception ex)
        //    {
        //        await transaction.RollbackAsync();
        //        // Moi lỗi thực sự từ InnerException ra
        //        string realError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        //        return Json(new { success = false, message = "Lỗi DB: " + realError });
        //    }
        //}

        // ==============================================================
        // 2. BẢNG KẾ HOẠCH SẢN XUẤT (Kéo thả, Tô màu)
        // ==============================================================

        // ==============================================================
        // 3. BỘ SINH LỆNH THÔNG MINH (CHỈ TẠO LỆNH BỊ THIẾU)
        // ==============================================================
        [HttpPost]
        public async Task<IActionResult> ApproveAndCreateWorkOrder(int orderId)
        {
            var order = await _context.Orders.Include(o => o.OrderDetails).FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null) return Json(new { success = false, message = "Đơn hàng không tồn tại!" });

            var existingWOs = await _context.WorkOrders
                .Where(w => w.OrderId == orderId && w.Status != WorkOrderStatus.Canceled)
                .ToListAsync();

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                int newlyCreated = 0;

                // CÓ CHI TIẾT
                if (order.OrderDetails != null && order.OrderDetails.Any())
                {
                    // Chỉ sinh lệnh cho những sản phẩm CHƯA CÓ LỆNH SX
                    var missingDetails = order.OrderDetails
                        .Where(d => !existingWOs.Any(w => w.ProductName == d.ProductName))
                        .ToList();

                    if (!missingDetails.Any()) return Json(new { success = false, message = "Đơn này đã sinh đủ lệnh SX!" });

                    int index = existingWOs.Count + 1;
                    foreach (var detail in missingDetails)
                    {
                        _context.WorkOrders.Add(new WorkOrder
                        {
                            OrderId = order.Id,
                            WOCode = $"LSX-{order.OrderCode}-{index:D2}",
                            ProductName = detail.ProductName,
                            TargetQuantity = detail.Quantity,
                            EndDate = order.DeliveryDeadline,
                            Notes = detail.Notes,
                            ProductionIndex = 999,
                            Status = WorkOrderStatus.Pending
                        });
                        index++; newlyCreated++;
                    }
                }
                // ĐƠN TỔNG KHÔNG CHI TIẾT
                else
                {
                    if (existingWOs.Any()) return Json(new { success = false, message = "Đơn này đã được tạo Lệnh tổng!" });

                    _context.WorkOrders.Add(new WorkOrder
                    {
                        OrderId = order.Id,
                        WOCode = $"LSX-{order.OrderCode}-FULL",
                        ProductName = "Lệnh Sản Xuất Tổng (Chưa có chi tiết)",
                        TargetQuantity = 1,
                        EndDate = order.DeliveryDeadline,
                        Notes = "Đơn hàng này không có danh sách chi tiết sản phẩm.",
                        ProductionIndex = 999,
                        Status = WorkOrderStatus.Pending
                    });
                    newlyCreated++;
                }

                order.Status = OrderStatus.InProduction;

                _context.OrderHistories.Add(new OrderHistory
                {
                    OrderId = order.Id,
                    Action = "Bổ sung Lệnh SX",
                    OldValue = "Bị sót / Chưa có lệnh",
                    NewValue = $"Đã đẩy {newlyCreated} lệnh xuống xưởng",
                    ChangedBy = User.Identity?.Name ?? "System"
                });

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { success = true, message = $"Đã đẩy thành công {newlyCreated} lệnh xuống xưởng!" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "Lỗi DB: " + (ex.InnerException?.Message ?? ex.Message) });
            }
        }

        // ==============================================================
        // 2. BẢNG KẾ HOẠCH (ĐỒNG BỘ BỘ ĐẾM BADGE ĐỎ & TÁCH NĂM)
        // ==============================================================
        [HttpGet]
        public async Task<IActionResult> TablePlan()
        {
            // Đếm số đơn sót cho Quả cầu đỏ trên Toolbar
            int missingCount = await _context.Orders
                .Where(o => o.Status == OrderStatus.Approved ||
                            (o.Status == OrderStatus.InProduction &&
                                (
                                    (o.OrderDetails.Any() && _context.WorkOrders.Count(w => w.OrderId == o.Id && w.Status != WorkOrderStatus.Canceled) < o.OrderDetails.Count) ||
                                    (!o.OrderDetails.Any() && !_context.WorkOrders.Any(w => w.OrderId == o.Id && w.Status != WorkOrderStatus.Canceled))
                                )
                            ))
                .CountAsync();
            ViewBag.MissingCount = missingCount;

            // 1. DỮ LIỆU TAB 1: KÉO THẢ (Các Lệnh Đang Sản Xuất)
            var workOrders = await _context.WorkOrders
                .Include(w => w.Order).ThenInclude(o => o.Customer)
                .Where(w => (w.Status == WorkOrderStatus.Pending || w.Status == WorkOrderStatus.InProgress)
                         && w.Order != null
                         && w.Order.Status == OrderStatus.InProduction)
                .OrderBy(w => w.ProductionIndex)
                .ToListAsync();

            // 2. DỮ LIỆU TAB 2: KIỂM TOÁN CHUỖI MÃ ĐƠN (Phát hiện nhảy cóc theo NĂM)
            var allOrderCodesWithYear = await _context.Orders
                .Where(o => !string.IsNullOrEmpty(o.OrderCode))
                .Select(o => new Tuple<string, int>(o.OrderCode, o.OrderDate.Year))
                .Distinct()
                .ToListAsync();

            ViewBag.MissingSequences = AnalyzeSequenceGaps(allOrderCodesWithYear);

            // 3. DỮ LIỆU TAB 2: ĐỐI CHIẾU TIẾN ĐỘ THỜI GIAN
            var sixMonthsAgo = DateTime.Now.AddMonths(-6);
            var auditOrders = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails)
                .Where(o => (o.Status == OrderStatus.Approved || o.Status == OrderStatus.InProduction) && o.OrderDate >= sixMonthsAgo)
                .OrderBy(o => o.OrderDate)
                .ToListAsync();

            var auditDelayList = new List<dynamic>();
            foreach (var order in auditOrders)
            {
                int woCount = _context.WorkOrders.Count(w => w.OrderId == order.Id && w.Status != WorkOrderStatus.Canceled);
                bool isMissing = order.OrderDetails.Any() ? woCount < order.OrderDetails.Count : woCount == 0;

                auditDelayList.Add(new
                {
                    OrderDate = order.OrderDate,
                    OrderCode = order.OrderCode,
                    CustomerName = order.Customer?.CompanyName ?? "Khách lẻ",
                    Details = order.OrderDetails.ToList(),
                    IsMissing = isMissing
                });
            }
            ViewBag.AuditDelayList = auditDelayList;

            return View(workOrders);
        }

        // HÀM HELPER: DÒ LỖ HỔNG MÃ (ĐÃ NÂNG CẤP CHIA THEO NĂM)
        private List<dynamic> AnalyzeSequenceGaps(List<Tuple<string, int>> codesWithYear)
        {
            var missingGroups = new List<dynamic>();
            if (codesWithYear == null || !codesWithYear.Any()) return missingGroups;

            var regex = new Regex(@"^(.*?)(\d+)$");
            var parsed = codesWithYear.Select(c => {
                var match = regex.Match(c.Item1.Trim());
                // C.Item1 = OrderCode, C.Item2 = Year
                return match.Success ? new
                {
                    Prefix = match.Groups[1].Value,
                    Number = int.Parse(match.Groups[2].Value),
                    Pad = match.Groups[2].Value.Length,
                    Year = c.Item2
                } : null;
            }).Where(x => x != null)
              .GroupBy(x => new { x.Year, x.Prefix, x.Pad }) // Gom nhóm theo cả Tiền tố VÀ Năm
              .ToList();

            foreach (var group in parsed)
            {
                var numbers = group.Select(g => g.Number).OrderBy(n => n).ToList();
                if (numbers.Count < 2) continue;

                var missingInGroup = new List<string>();
                for (int i = 0; i < numbers.Count - 1; i++)
                {
                    if (numbers[i + 1] - numbers[i] > 1) // Phát hiện nhảy cóc
                    {
                        for (int gap = numbers[i] + 1; gap < numbers[i + 1]; gap++)
                            missingInGroup.Add($"{group.Key.Prefix}{gap.ToString("D" + group.Key.Pad)}");
                    }
                }

                if (missingInGroup.Any())
                {
                    string groupName = string.IsNullOrEmpty(group.Key.Prefix) ? "Mã chỉ có số" : group.Key.Prefix.TrimEnd('.', '-');
                    missingGroups.Add(new
                    {
                        GroupName = groupName,
                        Year = group.Key.Year,
                        MissingCodes = missingInGroup
                    });
                }
            }
            return missingGroups;
        }


        [HttpPost]
        public async Task<IActionResult> SaveOrderIndex([FromBody] List<int> woIds)
        {
            if (woIds == null || !woIds.Any()) return Json(new { success = false });
            int currentIndex = 1;
            foreach (var id in woIds)
            {
                var wo = await _context.WorkOrders.FindAsync(id);
                if (wo != null) { wo.ProductionIndex = currentIndex; _context.Update(wo); currentIndex++; }
            }
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePlanStatus(int id, WorkOrderStatus newStatus)
        {
            var wo = await _context.WorkOrders.FindAsync(id);
            if (wo == null) return Json(new { success = false, message = "Không tìm thấy lệnh SX" });

            wo.Status = newStatus;

            if (newStatus == WorkOrderStatus.Completed)
            {
                var relatedWOs = await _context.WorkOrders.Where(w => w.OrderId == wo.OrderId && w.Id != wo.Id).ToListAsync();
                bool isAllCompleted = relatedWOs.All(w => w.Status == WorkOrderStatus.Completed || w.Status == WorkOrderStatus.Canceled);
                if (isAllCompleted)
                {
                    var parentOrder = await _context.Orders.FindAsync(wo.OrderId);
                    if (parentOrder != null) parentOrder.Status = OrderStatus.Completed;
                }
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePlanColor(int id, string color)
        {
            var wo = await _context.WorkOrders.FindAsync(id);
            if (wo != null) { wo.PlanColor = color; await _context.SaveChangesAsync(); }
            return Json(new { success = true });
        }

        // ==============================================================
        // 3. XEM NHẬT KÝ & XUẤT EXCEL
        // ==============================================================
        [HttpGet]
        public async Task<IActionResult> GetHistory()
        {
            var logs = await _context.OrderHistories.OrderByDescending(h => h.ChangedAt).Take(50).ToListAsync();
            return PartialView("_HistoryPartial", logs);
        }

        [HttpGet]
        public async Task<IActionResult> ExportTablePlanExcel()
        {
            using (var wb = new XLWorkbook())
            {
                // ==========================================
                // SHEET 1: KẾ HOẠCH SẢN XUẤT XƯỞNG (MODE 1)
                // ==========================================
                var workOrders = await _context.WorkOrders
                    .Include(w => w.Order).ThenInclude(o => o.Customer)
                    .Where(w => (w.Status == WorkOrderStatus.Pending || w.Status == WorkOrderStatus.InProgress)
                             && w.Order != null && w.Order.Status == OrderStatus.InProduction)
                    .OrderBy(w => w.ProductionIndex)
                    .ToListAsync();

                var ws1 = wb.Worksheets.Add("KeHoach_SanXuat");
                ws1.Cell(1, 1).Value = "KẾ HOẠCH SẢN XUẤT XƯỞNG";
                ws1.Range("A1:I1").Merge().Style.Font.SetBold().Font.SetFontSize(14).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                var headers1 = new[] { "STT", "Mã Lệnh SX", "Mã Đơn Gốc", "Tên Sản Phẩm", "Số Lượng", "Ngày Nhận", "Hạn Giao", "Ghi Chú", "Trạng Thái" };
                for (int i = 0; i < headers1.Length; i++) ws1.Cell(3, i + 1).Value = headers1[i];
                ws1.Range("A3:I3").Style.Font.SetBold().Fill.SetBackgroundColor(XLColor.CornflowerBlue).Font.SetFontColor(XLColor.White);

                int row1 = 4;
                var groupedOrders = workOrders.GroupBy(x => x.Order?.Customer?.CompanyName ?? "Khách lẻ").ToList();
                foreach (var group in groupedOrders)
                {
                    ws1.Cell(row1, 1).Value = "NHÀ MÁY: " + group.Key.ToUpper();
                    ws1.Range(row1, 1, row1, 9).Merge().Style.Font.SetBold().Fill.SetBackgroundColor(XLColor.LightGray);
                    row1++;

                    int stt = 1;
                    foreach (var item in group)
                    {
                        ws1.Cell(row1, 1).Value = stt;
                        ws1.Cell(row1, 2).Value = item.WOCode;
                        ws1.Cell(row1, 3).Value = item.Order?.OrderCode;
                        ws1.Cell(row1, 4).Value = item.ProductName;
                        ws1.Cell(row1, 5).Value = item.TargetQuantity;
                        ws1.Cell(row1, 6).Value = item.Order?.OrderDate.ToString("dd/MM/yyyy");
                        ws1.Cell(row1, 7).Value = item.EndDate?.ToString("dd/MM/yyyy") ?? "";
                        ws1.Cell(row1, 8).Value = item.Notes;

                        string status = item.Status == WorkOrderStatus.Pending ? "Chờ máy" : "Đang gia công";
                        ws1.Cell(row1, 9).Value = status;

                        ws1.Range(row1, 1, row1, 9).Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
                        row1++; stt++;
                    }
                }
                ws1.Columns().AdjustToContents();
                ws1.Column(8).Width = 30; // Rộng hơn cho Ghi chú
                ws1.Column(8).Style.Alignment.SetWrapText(true);

                // ==========================================
                // SHEET 2: KIỂM TOÁN CHUỖI ĐƠN (MODE 2)
                // ==========================================
                var sixMonthsAgo = DateTime.Now.AddMonths(-6);
                var auditOrders = await _context.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.OrderDetails)
                    .Where(o => (o.Status == OrderStatus.Approved || o.Status == OrderStatus.InProduction) && o.OrderDate >= sixMonthsAgo)
                    .OrderBy(o => o.OrderDate)
                    .ToListAsync();

                var ws2 = wb.Worksheets.Add("KiemToan_LoHong");
                ws2.Cell(1, 1).Value = "BÁO CÁO ĐỐI CHIẾU TIẾN ĐỘ & KIỂM TOÁN LỖ HỔNG ĐƠN HÀNG";
                ws2.Range("A1:E1").Merge().Style.Font.SetBold().Font.SetFontSize(14).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                var headers2 = new[] { "Ngày Khách Đặt", "Mã Đơn Gốc", "Khách Hàng", "Chi Tiết Sản Phẩm (Cần SX)", "Kết Quả Đối Chiếu" };
                for (int i = 0; i < headers2.Length; i++) ws2.Cell(3, i + 1).Value = headers2[i];
                ws2.Range("A3:E3").Style.Font.SetBold().Fill.SetBackgroundColor(XLColor.DarkOrange).Font.SetFontColor(XLColor.White);

                int row2 = 4;
                foreach (var order in auditOrders)
                {
                    int woCount = _context.WorkOrders.Count(w => w.OrderId == order.Id && w.Status != WorkOrderStatus.Canceled);
                    bool isMissing = order.OrderDetails.Any() ? woCount < order.OrderDetails.Count : woCount == 0;

                    ws2.Cell(row2, 1).Value = order.OrderDate.ToString("dd/MM/yyyy");
                    ws2.Cell(row2, 2).Value = order.OrderCode;
                    ws2.Cell(row2, 3).Value = order.Customer?.CompanyName ?? "Khách lẻ";

                    // Nối danh sách sản phẩm thành 1 ô có xuống dòng
                    var detailsStr = string.Join("\n", order.OrderDetails.Select(d => $"- {d.ProductName} (SL: {d.Quantity})"));
                    ws2.Cell(row2, 4).Value = detailsStr;
                    ws2.Cell(row2, 4).Style.Alignment.SetWrapText(true);

                    if (isMissing)
                    {
                        ws2.Cell(row2, 5).Value = "XƯỞNG ĐANG SÓT LỆNH";
                        // Bôi đỏ toàn bộ dòng bị sót để nổi bật trong Excel
                        ws2.Range(row2, 1, row2, 5).Style.Fill.SetBackgroundColor(XLColor.MistyRose).Font.SetFontColor(XLColor.DarkRed).Font.SetBold();
                    }
                    else
                    {
                        ws2.Cell(row2, 5).Value = "Đã đầy đủ lệnh";
                        ws2.Cell(row2, 5).Style.Font.SetFontColor(XLColor.DarkGreen);
                    }

                    ws2.Range(row2, 1, row2, 5).Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
                    row2++;
                }

                ws2.Columns().AdjustToContents();
                ws2.Column(4).Width = 45; // Chi tiết sản phẩm để rộng để không bị tràn

                // ==========================================
                // LƯU VÀ TRẢ VỀ FILE EXCEL CHUẨN XÁC
                // ==========================================
                using (var stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"BaoCao_KeHoach_KiemToan_{DateTime.Now:ddMMyy}.xlsx");
                }
            }
        }

        [HttpGet]
        public async Task<IActionResult> SyncOldOrders()
        {
            // Tìm các đơn hàng ĐANG SẢN XUẤT nhưng CHƯA CÓ lệnh WorkOrder nào dưới xưởng
            var oldOrders = await _context.Orders
                .Include(o => o.OrderDetails)
                .Where(o => (o.Status == OrderStatus.InProduction || o.Status == OrderStatus.Approved) && !_context.WorkOrders.Any(w => w.OrderId == o.Id))
                .ToListAsync();

            if (!oldOrders.Any())
                return Content("Không có đơn hàng cũ nào cần đồng bộ!");

            int countWO = 0;
            foreach (var order in oldOrders)
            {
                int index = 1;
                foreach (var detail in order.OrderDetails)
                {
                    var wo = new WorkOrder
                    {
                        OrderId = order.Id,
                        WOCode = $"LSX-{order.OrderCode}-OLD{index:D2}", // Đánh dấu 'OLD' để dễ nhận biết
                        ProductName = detail.ProductName,
                        TargetQuantity = detail.Quantity,
                        EndDate = order.DeliveryDeadline,
                        Notes = detail.Notes,
                        Status = WorkOrderStatus.InProgress // Set luôn là đang làm
                    };
                    _context.WorkOrders.Add(wo);
                    index++;
                    countWO++;
                }
            }

            await _context.SaveChangesAsync();
            return Content($"Đã đồng bộ thành công! Tạo ra {countWO} lệnh SX từ {oldOrders.Count} đơn hàng cũ.");
        }

        [HttpGet]
        public async Task<IActionResult> SyncOrderToWorkOrders(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) return RedirectToAction("Index", "Order");

            var workOrders = await _context.WorkOrders.Where(w => w.OrderId == orderId).ToListAsync();
            if (!workOrders.Any()) return RedirectToAction("Index", "Order");

            bool isChanged = false;

            if (order.Status == OrderStatus.Canceled)
            {
                foreach (var wo in workOrders)
                {
                    if (wo.Status != WorkOrderStatus.Completed && wo.Status != WorkOrderStatus.Canceled)
                    {
                        wo.Status = WorkOrderStatus.Canceled;
                        wo.Notes = $"[HỦY TỰ ĐỘNG THEO ĐƠN GỐC {DateTime.Now:dd/MM HH:mm}] " + wo.Notes;
                        isChanged = true;
                    }
                }
            }
            else
            {
                foreach (var wo in workOrders)
                {
                    if (wo.Status == WorkOrderStatus.Completed || wo.Status == WorkOrderStatus.Canceled) continue;

                    if (wo.EndDate != order.DeliveryDeadline)
                    {
                        wo.EndDate = order.DeliveryDeadline;
                        isChanged = true;
                    }

                    var detail = order.OrderDetails?.FirstOrDefault(d => d.ProductName == wo.ProductName);
                    if (detail != null)
                    {
                        if (wo.TargetQuantity != detail.Quantity)
                        {
                            wo.TargetQuantity = detail.Quantity;
                            isChanged = true;
                        }
                        if (wo.Notes != detail.Notes)
                        {
                            wo.Notes = detail.Notes;
                            isChanged = true;
                        }
                    }
                }
            }

            if (isChanged) await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Order");
        }


        // 1. Trang hiển thị danh sách hàng thiếu
        [HttpGet]
        public async Task<IActionResult> ShortagePlan()
        {
            // Lấy tất cả lệnh SX thuộc đơn hàng ĐANG SẢN XUẤT (để có chỗ nhập số liệu)
            // CỘNG THÊM các lệnh dù đơn đã xong nhưng vẫn còn bị NỢ (ShortageQuantity < 0)
            var data = await _context.WorkOrders
                .Include(w => w.Order).ThenInclude(o => o.Customer)
                .Where(w => w.ShortageQuantity < 0 || w.Order.Status == OrderStatus.InProduction)
                .ToListAsync();

            return View(data);
        }

        // 2. Cập nhật số lượng thiếu và ghi log
        // CẬP NHẬT SỐ LƯỢNG VÀ GHI NHẬT KÝ THẬT
        [HttpPost]
        public async Task<IActionResult> UpdateShortage(int id, int newQuantity, string note)
        {
            var item = await _context.WorkOrders.FindAsync(id);
            if (item == null) return Json(new { success = false });

            int oldQuantity = item.ShortageQuantity;
            if (newQuantity > 0) newQuantity = -newQuantity; // Ép số âm

            // 1. Cập nhật thông tin đơn hàng
            item.ShortageQuantity = newQuantity;
            item.Notes = note;

            // 2. Tạo bản ghi nhật ký riêng cho đơn này
            var log = new WorkOrderLog
            {
                WorkOrderId = id,
                UserName = User.Identity.Name ?? "Quản lý",
                ChangeDetail = $"{oldQuantity} ➔ {newQuantity}",
                Note = note ?? "Cập nhật số lượng"
            };
            _context.WorkOrderLogs.Add(log);

            await _context.SaveChangesAsync();
            return Json(new { success = true, newVal = newQuantity });
        }

        // LẤY LỊCH SỬ RIÊNG BIỆT CHO TỪNG ID
        [HttpGet]
        public async Task<IActionResult> GetShortageHistory(int id)
        {
            // Lọc chính xác các log thuộc về WorkOrderId này
            var history = await _context.WorkOrderLogs
                .Where(l => l.WorkOrderId == id)
                .OrderByDescending(l => l.CreatedAt)
                .Select(l => new {
                    time = l.CreatedAt.ToString("dd/MM HH:mm"),
                    user = l.UserName,
                    change = l.ChangeDetail,
                    reason = l.Note
                })
                .ToListAsync();

            return Json(history);
        }

        [HttpGet]
        public async Task<IActionResult> ExportShortageExcel()
        {
            using (var wb = new XLWorkbook())
            {
                // 1. Lấy dữ liệu (Chỉ lấy các đơn đang nợ hàng: ShortageQuantity < 0)
                var data = await _context.WorkOrders
                    .Include(w => w.Order).ThenInclude(o => o.Customer)
                    .Where(w => w.ShortageQuantity < 0)
                    .OrderBy(w => w.Order.Customer.CompanyName)
                    .ToListAsync();

                var ws = wb.Worksheets.Add("BaoCaoNoHang");

                // 2. TẠO TIÊU ĐỀ TRANG TRỌNG
                ws.Cell(1, 1).Value = "BÁO CÁO CHI TIẾT CÔNG NỢ HÀNG HÓA (HÀNG THIẾU)";
                ws.Range("A1:F1").Merge().Style
                    .Font.SetBold()
                    .Font.SetFontSize(16)
                    .Font.SetFontColor(XLColor.DarkRed)
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                ws.Cell(2, 1).Value = $"Ngày xuất báo cáo: {DateTime.Now:dd/MM/yyyy HH:mm}";
                ws.Range("A2:F2").Merge().Style
                    .Font.SetItalic()
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                // 3. ĐỊNH DẠNG ĐẦU MỤC CỘT (HEADERS)
                var headers = new[] { "STT", "MÃ ĐƠN HÀNG", "TÊN SẢN PHẨM", "KHÁCH HÀNG", "SỐ LƯỢNG THIẾU", "GHI CHÚ" };
                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = ws.Cell(4, i + 1);
                    cell.Value = headers[i];
                    cell.Style.Font.SetBold()
                        .Fill.SetBackgroundColor(XLColor.FromHtml("#334155")) // Slate-700
                        .Font.SetFontColor(XLColor.White)
                        .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                        .Border.SetOutsideBorder(XLBorderStyleValues.Thin);
                }

                // 4. ĐIỀN DỮ LIỆU & GOM NHÓM (GROUPING)
                int currentRow = 5;
                var grouped = data.GroupBy(x => x.Order?.Customer?.CompanyName ?? "Khách lẻ");

                foreach (var group in grouped)
                {
                    // Dòng tiêu đề Nhóm Khách Hàng
                    var groupHeader = ws.Range(currentRow, 1, currentRow, 6);
                    groupHeader.Merge().Value = $"🏢 KHÁCH HÀNG: {group.Key.ToUpper()}";
                    groupHeader.Style
                        .Font.SetBold()
                        .Fill.SetBackgroundColor(XLColor.FromHtml("#F1F5F9")) // Slate-100
                        .Border.SetOutsideBorder(XLBorderStyleValues.Thin);
                    currentRow++;

                    int sttInGroup = 1;
                    foreach (var item in group)
                    {
                        ws.Cell(currentRow, 1).Value = sttInGroup;
                        ws.Cell(currentRow, 2).Value = item.Order?.OrderCode;
                        ws.Cell(currentRow, 3).Value = item.ProductName;
                        ws.Cell(currentRow, 4).Value = group.Key;

                        // 🔥 Cột Số lượng: Định dạng số âm và màu đỏ
                        var qtyCell = ws.Cell(currentRow, 5);
                        qtyCell.Value = item.ShortageQuantity;
                        qtyCell.Style.Font.SetBold().Font.SetFontColor(XLColor.Red);
                        qtyCell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                        ws.Cell(currentRow, 6).Value = item.Notes;

                        // Kẻ border cho dòng dữ liệu
                        ws.Range(currentRow, 1, currentRow, 6).Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);

                        currentRow++;
                        sttInGroup++;
                    }
                }

                // 5. TỔNG KẾT & ĐỊNH DẠNG CỘT
                ws.Columns().AdjustToContents(); // Tự động dãn độ rộng cột
                ws.Column(6).Width = 40; // Ghi chú cho rộng ra
                ws.Column(6).Style.Alignment.SetWrapText(true);

                // Trả file về trình duyệt
                using (var stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"BaoCao_HangThieu_{DateTime.Now:ddMMyy}.xlsx");
                }
            }
        }


        //--------------------------------
        public class PlanItemModel
        {
            public int Id { get; set; }
            public DateTime Start { get; set; }
            public DateTime End { get; set; }
        }

        public class ColorUpdateModel
        {
            public int Id { get; set; }
            public string Color { get; set; } // Hex code (VD: #ff0000)
        }
    }
}