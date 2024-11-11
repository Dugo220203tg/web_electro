using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;
using TDProjectMVC.Data;
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
        public IActionResult Index()
        {
            var MaKh = User.Identity.Name;
            // Truy vấn hóa đơn từ database
            var hoaDons = db.HoaDons.AsQueryable();

            // Nếu MaKh không null hoặc rỗng, lọc theo khách hàng
            if (!string.IsNullOrEmpty(MaKh))
            {
                hoaDons = hoaDons.Where(hd => hd.MaKh == MaKh);
            }
            hoaDons = hoaDons.Where(hd => hd.MaTrangThai == 1 || hd.MaTrangThai == 2 || hd.MaTrangThai == 0);
            // Lấy chi tiết hóa đơn liên kết với các hóa đơn vừa truy vấn
            var result = hoaDons.Select(hd => new DonHangVM
            {
                MaHD = hd.MaHd,
                MaKH = hd.MaKh,
                NgayDat = hd.NgayDat,
                //NgayCan = hd.NgayCan,
                //NgayGiao = hd.NgayGiao,
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
            }).ToList();

            // Trả về kết quả cho View
            return View(result);
        }
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
                PhiVanChuyen = (float)hoaDon.PhiVanChuyen ,
                MaTrangThai = hoaDon.MaTrangThai,
                DienThoai = hoaDon.DienThoai,
                TrangThai = hoaDon.MaTrangThaiNavigation?.TenTrangThai,
                //MaNV = hoaDon.MaNV,
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

        public IActionResult AllDonHang()
        {
            var MaKh = User.Identity.Name;
            // Truy vấn hóa đơn từ database
            var hoaDons = db.HoaDons.AsQueryable();

            // Nếu MaKh không null hoặc rỗng, lọc theo khách hàng
            if (!string.IsNullOrEmpty(MaKh))
            {
                hoaDons = hoaDons.Where(hd => hd.MaKh == MaKh);
            }
            // Lấy chi tiết hóa đơn liên kết với các hóa đơn vừa truy vấn
            var result = hoaDons.Select(hd => new CtDonHangVM
            {
                MaHD = hd.MaHd,
                MaKH = hd.MaKh,
                NgayDat = hd.NgayDat,
                //NgayCan = hd.NgayCan,
                //NgayGiao = hd.NgayGiao,
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
            }).ToList();

            // Trả về kết quả cho View
            return View(result);
        }
        public IActionResult HuyDonHang([FromBody] HoaDonUpdateStatusModel model)
        {
            var donhang = db.HoaDons.Find(model.MaHD);
            if (donhang != null)
            {
                donhang.MaTrangThai = 4;  // Cập nhật trạng thái thành hủy
                db.SaveChanges();
                return Ok(new { success = true, message = "Hủy đơn hàng thành công!" });
            }
            return BadRequest(new { success = false, message = "Không tìm thấy đơn hàng." });
        }

    }
}
