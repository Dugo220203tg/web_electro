using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;
using TDProjectMVC.Data;
using TDProjectMVC.Helpers;
using TDProjectMVC.ViewModels;

namespace TDProjectMVC.Controllers
{
    public class DonHangController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly Hshop2023Context db;
        public DonHangController(ILogger<HomeController> logger, Hshop2023Context context)
        {
            _logger = logger;
            db = context;

        }
        [Authorize]
        [HttpGet]
        public IActionResult Index(int page = 1, int pageSize = 10)
        {
            // Validate input parameters
            page = Math.Max(1, page);
            pageSize = Math.Max(1, pageSize);

            // Get current user's customer ID
            var maKh = User.Identity.Name;

            // Query to get all orders for the customer
            var ordersQuery = db.HoaDons
                .Where(hd => string.IsNullOrEmpty(maKh) || hd.MaKh == maKh) // Filter by customer ID
                .Where(hd => new[] { 0, 1, 2 }.Contains(hd.MaTrangThai))   // Filter by allowed statuses
                .OrderByDescending(hd => hd.NgayDat)                      // Order by the most recent date
                .Select(hd => new DonHangVM
                {
                    MaHD = hd.MaHd,
                    MaKH = hd.MaKh,
                    NgayDat = hd.NgayDat,
                    HoTen = hd.HoTen,
                    DiaChi = hd.DiaChi,
                    CachThanhToan = hd.CachThanhToan,
                    CachVanChuyen = hd.CachVanChuyen,
                    PhiVanChuyen = (int)hd.PhiVanChuyen,
                    MaTrangThai = hd.MaTrangThai,
                    DienThoai = hd.DienThoai,
                    TrangThai = hd.MaTrangThaiNavigation.TenTrangThai,
                    ChiTietHds = db.ChiTietHds
                        .Where(ct => ct.MaHd == hd.MaHd)
                        .Select(ct => new ChiTietHoaDonMD
                        {
                            MaCT = ct.MaCt,
                            MaHD = ct.MaHd,
                            MaHH = ct.MaHh,
                            SoLuong = ct.SoLuong,
                            DonGia = ct.DonGia,
                            TenHangHoa = db.HangHoas.FirstOrDefault(hh => hh.MaHh == ct.MaHh).TenHh,
                            MaGiamGia = (int)ct.MaGiamGia,
                            HinhAnh = db.HangHoas.FirstOrDefault(hh => hh.MaHh == ct.MaHh).Hinh
                        }).ToList()
                });

            // Get the most recent order
            var mostRecentOrder = ordersQuery.Take(1).ToList();

            // Paginate the results for other orders
            var paginatedList = PaginatedList<DonHangVM>.CreateAsync(ordersQuery.Skip(1), page, pageSize);

            // Prepare view data
            ViewBag.TotalPages = paginatedList.TotalPages;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;

            // Return both the most recent order and the paginated list of other orders
            return View(new Tuple<DonHangVM, PaginatedList<DonHangVM>>(
                mostRecentOrder.FirstOrDefault(), paginatedList));
        }
        [Authorize]
        [HttpGet]
        public IActionResult GetOrderDetails(int maHD)
        {
            // Lấy thông tin hóa đơn và chi tiết liên quan từ database
            var order = db.HoaDons
                .Include(hd => hd.ChiTietHds) // Include bảng ChiTietHds
                .ThenInclude(ct => ct.MaHhNavigation) // Include bảng HangHoa để lấy thông tin sản phẩm
                .Include(hd => hd.MaTrangThaiNavigation) // Include bảng trạng thái đơn hàng
                .Where(hd => hd.MaHd == maHD) // Lọc theo mã hóa đơn
                .Select(hd => new DonHangVM
                {
                    MaHD = hd.MaHd,
                    NgayDat = hd.NgayDat,
                    HoTen = hd.HoTen,
                    DiaChi = hd.DiaChi,
                    CachThanhToan = hd.CachThanhToan,
                    CachVanChuyen = hd.CachVanChuyen,
                    PhiVanChuyen = (int)hd.PhiVanChuyen,
                    MaTrangThai = hd.MaTrangThai,
                    TrangThai = hd.MaTrangThaiNavigation.TenTrangThai,
                    ChiTietHds = hd.ChiTietHds.Select(ct => new ChiTietHoaDonMD
                    {
                        MaCT = ct.MaCt,
                        MaHH = ct.MaHh,
                        TenHangHoa = ct.MaHhNavigation.TenHh, // Lấy thông tin từ HangHoa
                        SoLuong = ct.SoLuong,
                        DonGia = ct.DonGia,
                        HinhAnh = ct.MaHhNavigation.Hinh, // Lấy hình ảnh từ HangHoa
                        MaGiamGia = (int)ct.MaGiamGia
                    }).ToList()
                })
                .FirstOrDefault();

            // Kiểm tra nếu không tìm thấy đơn hàng
            if (order == null)
            {
                return NotFound(new { message = "Order not found." });
            }

            // Trả về dữ liệu JSON
            return Json(order);
        }


        [Authorize]
        [HttpGet]
        public async Task<IActionResult> ChiTietDonHang(int MaHD)
        {
            if (MaHD <= 0)
            {
                return BadRequest("Mã đơn hàng không hợp lệ.");
            }

            var MaKh = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(MaKh))
            {
                return Unauthorized("Không thể xác định người dùng.");
            }

            var hoaDon = await db.HoaDons
                .Include(hd => hd.MaTrangThaiNavigation)
                .Include(hd => hd.ChiTietHds)
                    .ThenInclude(ct => ct.MaHhNavigation)
                .FirstOrDefaultAsync(hd => hd.MaHd == MaHD && hd.MaKh == MaKh); // Added check for MaKh

            if (hoaDon == null)
            {
                return NotFound($"Không tìm thấy đơn hàng với mã {MaHD} hoặc không thuộc về khách hàng hiện tại.");
            }

            var result = new CtDonHangVM
            {
                MaHD = hoaDon.MaHd,
                MaKH = hoaDon.MaKh,
                NgayDat = hoaDon.NgayDat,
                //NgayCan = hoaDon.NgayCan,
                //NgayGiao = hoaDon.NgayGiao,
                HoTen = hoaDon.HoTen,
                DiaChi = hoaDon.DiaChi,
                CachThanhToan = hoaDon.CachThanhToan,
                CachVanChuyen = hoaDon.CachVanChuyen,
                PhiVanChuyen = (int)hoaDon.PhiVanChuyen, // Giá trị mặc định
                MaTrangThai = hoaDon.MaTrangThai,
                DienThoai = hoaDon.DienThoai,
                TrangThai = hoaDon.MaTrangThaiNavigation?.TenTrangThai,
                GhiChu = hoaDon.GhiChu,
                ChiTietHds = hoaDon.ChiTietHds.Select(ct => new ChiTietHoaDonMD
                {
                    MaCT = ct.MaCt,
                    MaHD = ct.MaHd,
                    MaHH = ct.MaHh,
                    SoLuong = ct.SoLuong,
                    DonGia = ct.DonGia,
                    TenHangHoa = ct.MaHhNavigation?.TenHh,
                    MaGiamGia = (int)ct.MaGiamGia,
                    HinhAnh = ct.MaHhNavigation?.Hinh
                }).ToList()
            };

            return View(result);
        }
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> AllDonHang(int? page, int? pagesize)
        {
            page ??= 1;
            pagesize ??= 5;

            // Thiết lập giá trị mặc định cho ViewBag.PageSize
            ViewBag.PageSize = pagesize;

            // Lấy mã khách hàng từ User.Identity
            var MaKh = User.Identity.Name;

            // Truy vấn hóa đơn từ database
            var hoaDons = db.HoaDons.AsQueryable();

            // Nếu MaKh không null hoặc rỗng, lọc theo khách hàng
            if (!string.IsNullOrEmpty(MaKh))
            {
                hoaDons = hoaDons.Where(hd => hd.MaKh == MaKh);
            }

            // Lấy chi tiết hóa đơn liên kết với các hóa đơn vừa truy vấn
            var result = await hoaDons.Select(hd => new DonHangVM
            {
                MaHD = hd.MaHd,
                MaKH = hd.MaKh,
                NgayDat = hd.NgayDat,
                HoTen = hd.HoTen,
                DiaChi = hd.DiaChi,
                CachThanhToan = hd.CachThanhToan,
                CachVanChuyen = hd.CachVanChuyen,
                PhiVanChuyen = (int)hd.PhiVanChuyen, // Xử lý null và chuyển kiểu
                MaTrangThai = hd.MaTrangThai,
                DienThoai = hd.DienThoai,
                TrangThai = hd.MaTrangThaiNavigation.TenTrangThai,
                ChiTietHds = db.ChiTietHds
                    .Where(ct => ct.MaHd == hd.MaHd)
                    .Select(ct => new ChiTietHoaDonMD
                    {
                        MaCT = ct.MaCt,
                        MaHD = ct.MaHd,
                        MaHH = ct.MaHh,
                        SoLuong = ct.SoLuong,
                        DonGia = ct.DonGia,
                        TenHangHoa = db.HangHoas.FirstOrDefault(hh => hh.MaHh == ct.MaHh).TenHh,
                        MaGiamGia = Convert.ToInt32(ct.MaGiamGia), // Xử lý null và chuyển kiểu
                        HinhAnh = db.HangHoas.FirstOrDefault(hh => hh.MaHh == ct.MaHh).Hinh
                    }).ToList()
            }).ToListAsync(); // Truy vấn bất đồng bộ

            // Tính tổng số hóa đơn
            int totalItems = result.Count();

            // Tạo danh sách phân trang
            var paginatedList = PaginatedList<DonHangVM>.CreateAsync(result.AsQueryable(), page.Value, pagesize.Value);

            // Truyền dữ liệu ra View
            ViewBag.Page = page;
            ViewBag.TotalPages = paginatedList.TotalPages;
            ViewBag.TotalItems = totalItems;
            return View(paginatedList);
        }

        [Authorize]
        [HttpPost]
        public JsonResult HuyDonHang([FromBody] DonHangVM model)
        {
            try
            {
                // Kiểm tra dữ liệu
                if (model == null || model.MaHD <= 0)
                {
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ!" });
                }

                // Logic xử lý hủy đơn hàng
                var hoaDon = db.HoaDons.FirstOrDefault(hd => hd.MaHd == model.MaHD);
                if (hoaDon == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đơn hàng!" });
                }

                // Cập nhật trạng thái đơn hàng
                hoaDon.MaTrangThai = 4; 
                db.SaveChanges();

                return Json(new { success = true, message = "Hủy đơn hàng thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }


    }
}
