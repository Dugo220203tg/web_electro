using API_Web_Shop_Electronic_TD.Data;
using API_Web_Shop_Electronic_TD.Helpers;
using API_Web_Shop_Electronic_TD.Models;
using AutoMapper;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace API_Web_Shop_Electronic_TD.Mappers
{
	public static class KhachHangsMapper
	{

		public static KhachHangsMD ToKhachHangDo(this KhachHang Model)
		{
			return new KhachHangsMD
			{
				UserName = Model.MaKh,
				Password = Model.MatKhau,
				Vaitro = Model.VaiTro,
				Email = Model.Email,
				RandomKey = Model.RandomKey,
				HoTen = Model.HoTen,
				HieuLuc = Model.HieuLuc,
				DiaChi = Model.DiaChi,
				DienThoai = Model.DienThoai,
			};
		}
		public static KhachHang ToKhachHangCreateDTO(this DangKyMD Model)
		{
			return new KhachHang
			{
				MaKh = Model.UserName,
				MatKhau = Model.Password,
				VaiTro = Model.Vaitro,
				Email = Model.Email,
				HoTen = Model.Hoten,
				HieuLuc = Model.HieuLuc,

			};
		}
		public static KhachHang ToAdminCreateDTO(this AdminDkMD Model)
		{
			KhachHang newKhachHang = new KhachHang
			{
				MaKh = Model.UserName,
				RandomKey = MyUtil.GenerateRamdomKey(),
				VaiTro = 1,
				Email = Model.Email,
				DiaChi = Model.DiaChi,
				NgaySinh = Model.NgaySinh,
				DienThoai = Model.DienThoai,
				HieuLuc = true,
				HoTen = "admin"
			};

			newKhachHang.MatKhau = Model.Password.ToMd5Hash(newKhachHang.RandomKey);

			return newKhachHang;
		}
		public static KhachHang ToAccountCreateDTO(this RegisterMD Model)
		{
			// Generate random key (salt)
			byte[] randomKey = GenerateSecureRandomKey();

			KhachHang newKhachHang = new KhachHang
			{
				MaKh = Model.UserName,
				RandomKey = Convert.ToBase64String(randomKey),
				VaiTro = Model.Role,
				Email = Model.Email,
				DiaChi = "HN",
				DienThoai = "0123456789",
				HieuLuc = true,
				HoTen = Model.UserName,
				NgayTao = DateTime.Now,		
			};

			// Use HMACSHA512 for secure password hashing
			newKhachHang.MatKhau = HashPassword(Model.Password, randomKey);

			return newKhachHang;
		}
		private static byte[] GenerateSecureRandomKey()
		{
			byte[] randomKey = new byte[64]; 
			using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
			{
				rng.GetBytes(randomKey);
			}
			return randomKey;
		}
		private static string HashPassword(string password, byte[] randomKey)
		{
			using (var hmac = new System.Security.Cryptography.HMACSHA512(randomKey))
			{
				byte[] passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
				return Convert.ToBase64String(passwordHash);
			}
		}
		public static KhachHang UpdateKhachHangInfo(this UpdateKH model, KhachHang khachHang)
		{
			// Chỉ cập nhật các trường được cung cấp trong model
			if (!string.IsNullOrEmpty(model.Email))
				khachHang.Email = model.Email;
			if (model.NgaySinh != default)
				khachHang.NgaySinh = model.NgaySinh;
			if (!string.IsNullOrEmpty(model.DiaChi))
				khachHang.DiaChi = model.DiaChi;
			if (!string.IsNullOrEmpty(model.DienThoai))
				khachHang.DienThoai = model.DienThoai;

			// Không thay đổi các trường khác
			return khachHang;
		}

		public class AutoMapperProfile : Profile
		{
			public AutoMapperProfile()
			{
				CreateMap<KhachHangsMD, KhachHang>();
				//.ForMember(kh => kh.HoTen, option => option.MapFrom(RegisterVM => RegisterVM.HoTen))
				//.ReverseMap();
			}
		}
	}
}
