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
				NgayCan = model.NgayCan ?? DateTime.MinValue, 
				NgayGiao = model.NgayGiao ?? DateTime.MinValue 
			};

			if (model.MaTrangThaiNavigation != null)
			{
				hoaDonMD.TrangThai = model.MaTrangThaiNavigation.TenTrangThai;
			}

			if (model.ChiTietHds != null)
			{
				hoaDonMD.ChiTietHds = model.ChiTietHds
					.Where(ct => ct != null) 
					.Select(ct => new ChiTietHoaDonMD
					{
						MaCT = ct.MaCt,
						MaHD = ct.MaHd,
						MaHH = ct.MaHh,
						SoLuong = ct.SoLuong, 
						DonGia = ct.DonGia,  
						MaGiamGia = (int)ct.MaGiamGia, 
						TenHangHoa = ct.MaHhNavigation?.TenHh ?? "", 
						HinhAnh = ct.MaHhNavigation?.Hinh ?? "" 
					}).ToList();
			}
			else
			{
				hoaDonMD.ChiTietHds = new List<ChiTietHoaDonMD>(); 
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
