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
		public int Roles { get; set; }
		public bool EmailConfirmed { get; set; }
		public int AccessFailedCount { get; set; }
		public bool HieuLuc { get; set; }
		public bool GioiTinh { get; set; }
		public string Hoten { get; set; }
		public string DienThoai { get; set; }
		public string DiaChi { get; set; }
		public DateTime NgayTao { get; set; }
	}
	public class DetailAccoutResponse
	{

		public string fullname { get; set; }
		public string email { get; set; }
		public int roles { get; set; }
		public int accessFailedCount { get; set; }
		public string phoneNumber { get; set; }
	}
	public class RegisterMD
	{
		[Display(Name = "User Name")]
		[Required(ErrorMessage = "Họ tên không được để trống")]
		[MaxLength(20, ErrorMessage = "Max 20 keys")]
		public string UserName { get; set; }
		[Display(Name = "Password")]
		[Required(ErrorMessage = "Mật khẩu không được để trống")]
		[DataType(DataType.Password)]
		public string Password { get; set; }
		public int Role { get; set; }
		public string Email { get; set; }
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
		public string Email { get; set; }

		[Required(ErrorMessage = "Mật khẩu là bắt buộc")]
		[MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
		public string Password { get; set; }
	}
	public class RefreshTokenDTO
	{
		public string RefreshToken { get; set; } = null!;
		public string Token { get; set; } = null!;
		public string Email { get; set; } = null!;

	}

	public class ForgotPasswordDto
	{
		[Required]
		[EmailAddress]
		public string Email { get; set; }
	}
	public class ResetPasswordDto
	{
		[Required]
		[EmailAddress]
		public string Email { get; set; } = string.Empty;
		public string Token { get; set; } = string.Empty;
		public string NewPassword { get; set; } = string.Empty;
	}
	public class AuthResponseDto
	{
		public bool IsSuccess { get; set; }
		public string Message { get; set; }
		public string Token { get; set; }
		public string RefreshToken { get; set; }
		public string Email { get; set; }

	}
	public class ResponeseDtoForgotPassword
	{
		public bool IsSuccess { get; set; }
		public string Message { get; set; }
		public string Token { get; set; }
	}
	public class ChangePasswordDto
	{
		[Required]
		[EmailAddress]
		public string Email { get; set; } = string.Empty;
		[Required]
		public string NewPassword { get; set; } = string.Empty;
		[Required]
		public string CurrentPassword { get; set; } = string.Empty;
	}
		public class LoginResDto
	{
		public string Token { get; set; }
		public bool isSuccess { get; set; }
		public string Message { get; set; }
		public string RefreshToken { get; set; }
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
	public class ErrorResponse
	{
		public bool IsSuccess { get; set; }
		public string Message { get; set; }
		public List<string> Errors { get; set; }
	}
	public class WishListReport
	{
		public int maYt { get; set; }
		public int maHH { get; set; }
		public string tenHH { get; set; }
		public double donGia { get; set; }
		public string hinh { get; set; }
		public string tenNCC { get; set; }
	}
	public class WishListRequest
	{
		public string maKh {  get; set; }
		public int maHh {  get; set; }
		public DateTime ngay { get; set; }
	}
}
