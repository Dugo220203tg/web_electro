using API_Web_Shop_Electronic_TD.Data;
using API_Web_Shop_Electronic_TD.Models;

namespace API_Web_Shop_Electronic_TD.Interfaces
{
	public interface IKhachHangRepository
	{
		Task<List<KhachHang>> GetAllAsync();
		Task<KhachHang?> GetByIdAsync(string MaKh);
		Task<KhachHang> CreateAsync(AdminDkMD model);
		Task<KhachHang?> UpdateAdminAsync(string MaKh, UpdateKH model);
		Task<KhachHang?> DeleteAsync(string MaKh);
		Task<KhachHang?> GetByEmailAsync(string email);
		Task<KhachHang?> UpdatePasswordAsync(string currentPassword, string newPassword, string email);
		bool VerifyPassword(string inputPassword, string storedPassword, string randomKey);	
		Task<KhachHang?> Authenticate(string Email, string password);
		Task<KhachHang> CreateAccountAsync(RegisterMD model);
		Task<IEnumerable<string>> GetRoleAsync(KhachHang user);
		Task<string> GeneratePasswordResetTokenAsync(KhachHang user);
		Task<KhachHang> UpdateAsync(KhachHang user);
		Task<bool> ResetPasswordAsync(KhachHang user, string token, string newPassword);
		Task<(bool IsSuccess, string Message)> ChangePasswordAsync(KhachHang user, string currentPassword, string newPassword);
	}
}
