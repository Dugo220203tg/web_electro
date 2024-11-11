using API_Web_Shop_Electronic_TD.Data;
using API_Web_Shop_Electronic_TD.Models;

namespace API_Web_Shop_Electronic_TD.Mappers
{
	public static class HoaDonMapper
	{
		public static HoaDonMD ToHoaDonDo(this HoaDon model)
		{
			var hoaDonMD = new HoaDonMD
			{
				MaHD = model.MaHd,
				MaKH = model.MaKh,
				HoTen = model.HoTen,
				NgayDat = model.NgayDat,
				DiaChi = model.DiaChi,
				CachThanhToan = model.CachThanhToan,
				DienThoai = model.DienThoai,
				GhiChu = model.GhiChu,
				MaTrangThai = model.MaTrangThai,
				CachVanChuyen = model.CachVanChuyen,
				PhiVanChuyen = (float?)model.PhiVanChuyen,
				NgayCan = model.NgayCan ?? DateTime.MinValue, // Gán giá trị mặc định nếu null
				NgayGiao = model.NgayGiao ?? DateTime.MinValue // Gán giá trị mặc định nếu null
			};

			// Kiểm tra nếu MaTrangThaiNavigation không null thì gán TrangThai
			if (model.MaTrangThaiNavigation != null)
			{
				hoaDonMD.TrangThai = model.MaTrangThaiNavigation.TenTrangThai;
			}

			// Kiểm tra nếu ChiTietHds không null để tránh NullReferenceException
			if (model.ChiTietHds != null)
			{
				hoaDonMD.ChiTietHds = model.ChiTietHds
					.Where(ct => ct != null) // Đảm bảo rằng ct không phải null
					.Select(ct => new ChiTietHoaDonMD
					{
						MaCT = ct.MaCt,
						MaHD = ct.MaHd,
						MaHH = ct.MaHh,
						SoLuong = ct.SoLuong, // SoLuong đã có giá trị không cần kiểm tra null
						DonGia = ct.DonGia,   // DonGia đã có giá trị không cần kiểm tra null
						MaGiamGia = (int)ct.MaGiamGia, // Nếu MaGiamGia là null thì gán 0
						TenHangHoa = ct.MaHhNavigation?.TenHh ?? "", // Kiểm tra null cho MaHhNavigation và TenHh
						HinhAnh = ct.MaHhNavigation?.Hinh ?? "" // Kiểm tra null cho Hinh
					}).ToList();
			}
			else
			{
				hoaDonMD.ChiTietHds = new List<ChiTietHoaDonMD>(); // Đảm bảo rằng danh sách không null
			}

			return hoaDonMD;
		}



		public static HoaDon ToHoaDonDTO(this HoaDonMD model)
		{
			var hoaDon = new HoaDon
			{
				MaHd = (int)model.MaHD,
				MaKh = model.MaKH,
				NgayDat = model.NgayDat,
				HoTen = model.HoTen,
				DiaChi = model.DiaChi,
				CachThanhToan = model.CachThanhToan,
				DienThoai = model.DienThoai,
				GhiChu = model.GhiChu,
				MaTrangThai = (int)model.MaTrangThai,
				CachVanChuyen = model.CachVanChuyen,
				PhiVanChuyen = (float)model.PhiVanChuyen,
				NgayCan = model.NgayCan,
				NgayGiao = model.NgayGiao,
				MaNv = model.MaNV,
			};

			// Chuyển danh sách chi tiết hóa đơn
			hoaDon.ChiTietHds = model.ChiTietHds
				.Select(ct => new ChiTietHd
				{
					MaCt = ct.MaCT,
					MaHd = ct.MaHD,
					MaHh = ct.MaHH,
					SoLuong = ct.SoLuong,
					DonGia = ct.DonGia,
					MaGiamGia = ct.MaGiamGia,
				}).ToList();

			return hoaDon;
		}

	}
}
