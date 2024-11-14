using System.ComponentModel.DataAnnotations;

namespace API_Web_Shop_Electronic_TD.Models
{
	public class KhachHangsMD
	{
		[Display(Name = "User Name")]
		[MaxLength(20, ErrorMessage = "Max 20 keys")]
		[Required(ErrorMessage = "Họ tên không được để trống")]
		public string UserName { get; set; }
		[Display(Name = "Password")]
		[Required(ErrorMessage = "*")]
		[DataType(DataType.Password)]
		public string Password { get; set; }
		public int Vaitro { get; set; }
		[Required(ErrorMessage = "Email không được để trống")]
		[EmailAddress(ErrorMessage = "Email không hợp lệ")]
		public string Email { get; set; }
		public DateOnly NgaySinh { get; set; }
		public string DiaChi { get; set; }
		public string DienThoai { get; set; }
		public string RandomKey { get; set; }
		public string HoTen { get; set; }
		public bool HieuLuc {  get; set; }


	}
	public class DangNhapMD
	{
		[Display(Name = "User Name")]
		[Required(ErrorMessage = "Họ tên không được để trống")]
		[MaxLength(20, ErrorMessage = "Max 20 keys")]
		public string UserName { get; set; }
		[Display(Name = "Password")]
		[Required(ErrorMessage = "Mật khẩu không được để trống")]
		[DataType(DataType.Password)]
		public string Password { get; set; }
		public int Vaitro { get; set; }

	}
	public class DangKyMD
	{
		[Display(Name = "User Name")]
		[Required(ErrorMessage = "Họ tên không được để trống")]
		[MaxLength(20, ErrorMessage = "Max 20 keys")]
		public string UserName { get; set; }
		[Display(Name = "Password")]
		[Required(ErrorMessage = "Mật khẩu không được để trống")]
		[DataType(DataType.Password)]
		public string Password { get; set; }
		public int Vaitro { get; set; }
		public string Email { get; set; }
		public bool HieuLuc { get; set; }
		public string Hoten { get; set; }

	}
	public class AdminDkMD
	{
		[Display(Name = "User Name")]
		[Required(ErrorMessage = "Họ tên không được để trống")]
		[MaxLength(20, ErrorMessage = "Max 20 keys")]
		public string UserName { get; set; }
		[Display(Name = "Password")]
		[Required(ErrorMessage = "Mật khẩu không được để trống")]
		[DataType(DataType.Password)]
		public string Password { get; set; }
		[Required(ErrorMessage = "Email không được để trống")]
		[EmailAddress(ErrorMessage = "Email không hợp lệ")]
		public string Email { get; set; }
		public DateOnly NgaySinh {  get; set; }
		public string DiaChi { get; set; }
		public string DienThoai { get; set; }

	}
	public class UpdateKH
	{
		[Required(ErrorMessage = "Email là bắt buộc")]
		[EmailAddress(ErrorMessage = "Email không hợp lệ")]
		public string Email { get; set; }
		public DateOnly NgaySinh { get; set; }
		public string DiaChi { get; set; }
		public string DienThoai { get; set; }
	}
	public class LoginDTO
	{
		[Required(ErrorMessage = "UserName là bắt buộc")]
		public string UserName { get; set; }

		[Required(ErrorMessage = "Mật khẩu là bắt buộc")]
		[MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
		public string Password { get; set; }
	}
	public class UpdatePasswordDTO
	{
		[Required(ErrorMessage = "Email là bắt buộc")]
		[EmailAddress(ErrorMessage = "Email không hợp lệ")]
		public string Email { get; set; }

		[Required(ErrorMessage = "Mật khẩu hiện tại là bắt buộc")]
		public string CodeSend { get; set; }

		[Required(ErrorMessage = "Mật khẩu mới là bắt buộc")]
		[MinLength(6, ErrorMessage = "Mật khẩu mới phải có ít nhất 6 ký tự")]
		public string NewPassword { get; set; }
	}
	public class UpdateHieuLucVMD
	{
		public string UserName { get; set; }
		public bool HieuLuc { get; set; }
	}
}
