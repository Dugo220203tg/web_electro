using API_Web_Shop_Electronic_TD.Data;
using API_Web_Shop_Electronic_TD.Models;

namespace API_Web_Shop_Electronic_TD.Interfaces
{
	public interface IKhachHangRepository
	{
		Task<List<KhachHang>> GetAllAsync();
		Task<KhachHang?> GetByIdAsync(string MaKh);
		Task<KhachHang> CreateAsync(AdminDkMD model);
		Task<KhachHang?> UpdateAsync(string MaKh, UpdateKH model);
		Task<KhachHang?> DeleteAsync(string MaKh);
		Task<KhachHang?> GetByEmailAsync(string email);
		Task<KhachHang?> UpdatePasswordAsync(string currentPassword, string newPassword, string email);
		bool VerifyPassword(string inputPassword, string storedPassword, string randomKey);
	}
}
