using API_Web_Shop_Electronic_TD.Data;
using API_Web_Shop_Electronic_TD.Models;

namespace API_Web_Shop_Electronic_TD.Mappers
{
	public static class LoaiSpMapper
	{
		public static LoaiSpMD ToLoaiDo(this Loai Model)
		{
			return new LoaiSpMD
			{
				MaLoai = Model.MaLoai,
				TenLoai = Model.TenLoai,
				Hinh = Model.Hinh,
				Mota = Model.MoTa,
				DanhMuc_id = (int)Model.DanhMucId
			};
		}
		public static Loai ToLoaiDTO(this CreateLoaiSpMD Model)
		{

			return new Loai
			{
				TenLoai = Model.TenLoai,
				Hinh = Model.Hinh,
				MoTa = Model.Mota,
				DanhMucId = Model.DanhMuc_id
			};
		}
	}
}
