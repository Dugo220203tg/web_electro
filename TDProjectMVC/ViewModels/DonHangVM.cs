using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TDProjectMVC.ViewModels
{
    public class ChiTietHoaDonMD
    {
        public int MaCT { get; set; }
        public int MaHD { get; set; }
        public int MaHH { get; set; }
        public int SoLuong { get; set; }
        public double DonGia { get; set; }
        public string? TenHangHoa { get; set; }
        public int MaGiamGia { get; set; }
        public string? HinhAnh { get; set; }
        public double ThanhTien => SoLuong * DonGia * (1 - (double)MaGiamGia / 100);

    }
    public class DonHangVM
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaHD { get; set; }

        [Required(ErrorMessage = "Mã khách hàng là bắt buộc")]
        public string MaKH { get; set; }

        [Required(ErrorMessage = "Ngày đặt là bắt buộc")]
        public DateTime NgayDat { get; set; }

        public DateTime NgayCan { get; set; }
        public DateTime NgayGiao { get; set; }

        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        public string HoTen { get; set; }

        [Required(ErrorMessage = "Địa chỉ là bắt buộc")]
        public string DiaChi { get; set; }

        [Required(ErrorMessage = "Cách thanh toán là bắt buộc")]
        public string CachThanhToan { get; set; }

        public string? CachVanChuyen { get; set; }
        public float? PhiVanChuyen { get; set; }

        [Required(ErrorMessage = "Mã trạng thái là bắt buộc")]
        public int MaTrangThai { get; set; }

        public string? MaNV { get; set; }

        public string? GhiChu { get; set; }

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        public string DienThoai { get; set; }
        public string? TrangThai { get; set; }

        // Chi tiết hóa đơn
        public List<ChiTietHoaDonMD> ChiTietHds { get; set; } = new List<ChiTietHoaDonMD>();
    }
    public class CtDonHangVM
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaHD { get; set; }

        [Required(ErrorMessage = "Mã khách hàng là bắt buộc")]
        public string MaKH { get; set; }

        [Required(ErrorMessage = "Ngày đặt là bắt buộc")]
        public DateTime NgayDat { get; set; }

        public DateTime NgayCan { get; set; }
        public DateTime NgayGiao { get; set; }

        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        public string HoTen { get; set; }

        [Required(ErrorMessage = "Địa chỉ là bắt buộc")]
        public string DiaChi { get; set; }

        [Required(ErrorMessage = "Cách thanh toán là bắt buộc")]
        public string CachThanhToan { get; set; }

        public string? CachVanChuyen { get; set; }
        public float? PhiVanChuyen { get; set; }

        [Required(ErrorMessage = "Mã trạng thái là bắt buộc")]
        public int MaTrangThai { get; set; }

        public string? MaNV { get; set; }

        public string? GhiChu { get; set; }

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        public string DienThoai { get; set; }
        public string? TrangThai { get; set; }

        // Chi tiết hóa đơn
        public List<ChiTietHoaDonMD> ChiTietHds { get; set; } = new List<ChiTietHoaDonMD>();
    }
    public class HoaDonUpdateStatusModel
    {
        public int MaHD { get; set; }
        public int MaTrangThai { get; set; }
    }
}
