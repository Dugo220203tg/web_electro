using API_Web_Shop_Electronic_TD.Data;
using API_Web_Shop_Electronic_TD.Interfaces;
using API_Web_Shop_Electronic_TD.Models;

namespace API_Web_Shop_Electronic_TD.Mappers
{
	public static class ChiTietHoaDonMapper
	{
		public static ChiTietHoaDonMD ToCtHoaDonDo(this ChiTietHd Model)
		{
			return new ChiTietHoaDonMD
			{
				MaCT = Model.MaCt,
				MaHD = Model.MaHd,
				MaHH = Model.MaHh,
				DonGia = Model.MaHhNavigation?.DonGia ?? 0,
				SoLuong = Model.SoLuong,
				MaGiamGia = (int)Model.MaGiamGia,
				TenHangHoa = Model.MaHhNavigation?.TenHh ?? string.Empty,
				MaDanhMuc = (int)Model.MaHhNavigation.MaLoaiNavigation.DanhMucId,
				HinhAnh = Model.MaHhNavigation.Hinh
			};
		}
		public static PostChiTietHoaDonMD ToChiTietHoaDonResult(this ChiTietHd model)
		{
			return new PostChiTietHoaDonMD
			{
				MaHD = model.MaHd,
				MaHH = model.MaHh,
				DonGia = model.DonGia,
				SoLuong = model.SoLuong,
				MaGiamGia = (int)model.MaGiamGia
			};
		}
		public static ChiTietHd ToCtHoaDonDTO(this PostChiTietHoaDonMD Model)
		{
			return new ChiTietHd
			{
				MaHd = Model.MaHD,
				MaHh = Model.MaHH,
				SoLuong = Model.SoLuong,
				MaGiamGia = Model.MaGiamGia,
				DonGia = Model.DonGia
			};
		}

	}
}