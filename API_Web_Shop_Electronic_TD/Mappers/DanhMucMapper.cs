using API_Web_Shop_Electronic_TD.Data;
using API_Web_Shop_Electronic_TD.Models;

namespace API_Web_Shop_Electronic_TD.Mappers
{
	public static class DanhMucMapper
	{
		public static DanhMucMD ToDanhMucDto(this DanhMucSp model)
		{
			int soLuong = model.Loais.Sum(loai => loai.HangHoas.Count);

			return new DanhMucMD
			{
				MaDanhMuc = model.MaDanhMuc,
				TenDanhMuc = model.TenDanhMuc,
				soLuong = soLuong,
				image = model.Image
			};
		}

		public static DanhMucSp ToDanhMucDTO(this DanhMucMD model)
		{
			return new DanhMucSp
			{
				MaDanhMuc = model.MaDanhMuc,
				TenDanhMuc = model.TenDanhMuc
			};
		}
	}

}
