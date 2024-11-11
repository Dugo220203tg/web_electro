using API_Web_Shop_Electronic_TD.Data;
using API_Web_Shop_Electronic_TD.Models;

namespace API_Web_Shop_Electronic_TD.Mappers
{
	public static class DanhMucMapper
	{
		 public static DanhMucMD ToDanhMucDo(this DanhMucSp Model)
		{
			return new DanhMucMD
			{
				MaDanhMuc = Model.MaDanhMuc,
				TenDanhMuc = Model.TenDanhMuc
			};
		}
		public static DanhMucSp ToDanhMucDTO(this DanhMucMD Model)
		{

			return new DanhMucSp
			{
				MaDanhMuc = Model.MaDanhMuc,
				TenDanhMuc = Model.TenDanhMuc,
			};
		}
	}
}
