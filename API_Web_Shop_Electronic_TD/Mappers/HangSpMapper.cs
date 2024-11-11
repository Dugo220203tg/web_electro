using API_Web_Shop_Electronic_TD.Data;
using API_Web_Shop_Electronic_TD.Models;

namespace API_Web_Shop_Electronic_TD.Mappers
{
	public static class HangSpMapper
	{
		public static HangSpMD ToHangSpDo(this NhaCungCap Model)
		{
			return new HangSpMD
			{
				TenCongTy = Model.TenCongTy,
				MaNCC = Model.MaNcc,
				Logo = Model.Logo,
				Mota = Model.MoTa,
				Email = Model.Email,
				NguoiLienLac = Model.NguoiLienLac,
				DiaChi = Model.DiaChi,
				DienThoai = Model.DienThoai
			};
		}
		public static NhaCungCap TohangSpDTO(this HangSpMD Model)
		{

			return new NhaCungCap
			{
				TenCongTy = Model.TenCongTy,
				MaNcc = Model.MaNCC,
				Logo = Model.Logo,
				MoTa = Model.Mota,
				Email = Model.Email,
				NguoiLienLac = Model.NguoiLienLac,
				DiaChi = Model.DiaChi,
				DienThoai = Model.DienThoai
			};
		}
	}
}
