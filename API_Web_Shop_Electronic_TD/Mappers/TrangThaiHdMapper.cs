using API_Web_Shop_Electronic_TD.Data;
using API_Web_Shop_Electronic_TD.Models;

namespace API_Web_Shop_Electronic_TD.Mappers
{
	public static class TrangThaiHdMapper
	{
		public static TrangThaiHoaDonMD ToTrangThaiDo(this TrangThai Model)
		{
			return new TrangThaiHoaDonMD
			{
				MaTrangThai = Model.MaTrangThai,
				TenTrangThai = Model.TenTrangThai,
			};
		}
	}
}
