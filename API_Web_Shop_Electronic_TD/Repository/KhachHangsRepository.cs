using API_Web_Shop_Electronic_TD.Data;
using API_Web_Shop_Electronic_TD.Helpers;
using API_Web_Shop_Electronic_TD.Interfaces;
using API_Web_Shop_Electronic_TD.Mappers;
using API_Web_Shop_Electronic_TD.Models;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace API_Web_Shop_Electronic_TD.Repository
{

	public class KhachHangsRepository : IKhachHangRepository
	{
		private readonly Hshop2023Context db;

		public KhachHangsRepository(Hshop2023Context db)
		{
			this.db = db;

		}
		public async Task<List<KhachHang>> GetAllAsync()
		{
			return await db.KhachHangs.ToListAsync();
		}
		public async Task<KhachHang> CreateAsync(AdminDkMD model)
		{
			if (string.IsNullOrEmpty(model.UserName))
			{
				throw new ArgumentException("Chưa nhập đủ thông tin: Họ tên không được để trống");
			}
			var khachHangexit = await db.KhachHangs.SingleOrDefaultAsync(kh => kh.MaKh == model.UserName);
			if (khachHangexit != null)
			{
				throw new ArgumentException("username đã tồn tại");
			}
			if (string.IsNullOrEmpty(model.Password))
			{
				throw new ArgumentException("Chưa nhập đủ thông tin: Mật khẩu không được để trống");
			}
			var khachHangemail = await db.KhachHangs.SingleOrDefaultAsync(kh => kh.Email == model.Email);
			if (khachHangemail != null)
			{
				throw new ArgumentException("Email đã tồn tại");
			}
			if (string.IsNullOrEmpty(model.Email))
			{
				throw new ArgumentException("Chưa nhập đủ thông tin: Email không được để trống");
			}
			if (model.NgaySinh == default(DateOnly))
			{
				throw new ArgumentException("Chưa nhập đủ thông tin: Ngày sinh không được để trống");
			}
			if (model.DienThoai.Length < 9)
			{
				throw new ArgumentException("Nhập sai định dạng số điện thoại");
			}

			var khachHang = model.ToAdminCreateDTO(); // Sử dụng mapper để chuyển đổi từ KhachHangsMD sang KhachHang
			await db.KhachHangs.AddAsync(khachHang);
			await db.SaveChangesAsync();
			return khachHang; // Trả về đối tượng KhachHang sau khi thêm vào cơ sở dữ liệu
		}
		public async Task<KhachHang?> DeleteAsync(string username)
		{
			try
			{
				var model = await db.KhachHangs.FirstOrDefaultAsync(x => x.MaKh == username);
				if (model == null)
				{
					return null;
				}
				db.KhachHangs.Remove(model);
				await db.SaveChangesAsync();
				return model;
			}
			catch (Exception ex)
			{
				throw new Exception($"Lỗi khi xóa khách hàng: {ex.Message}");
			}
		}

		public async Task<KhachHang?> GetByIdAsync(string MaKh)
		{
			return await db.KhachHangs.FindAsync(MaKh);

		}
		public async Task<KhachHang?> UpdateAdminAsync(string MaKh, UpdateKH model)
		{
			var khachHangModel = await db.KhachHangs.FirstOrDefaultAsync(x => x.MaKh == MaKh);
			if (khachHangModel == null)
			{
				throw new KeyNotFoundException($"Không tìm thấy khách hàng với mã {MaKh}");
			}

			khachHangModel = model.UpdateKhachHangInfo(khachHangModel);

			db.Entry(khachHangModel).State = EntityState.Modified;
			await db.SaveChangesAsync();
			return khachHangModel;
		}
		public async Task<KhachHang?> GetByEmailAsync(string email)
		{
			return await db.KhachHangs.FirstOrDefaultAsync(kh => kh.Email == email);
		}
		public bool VerifyPassword(string inputPassword, string storedPassword, string randomKey)
		{
			string hashedInputPassword = inputPassword.ToMd5Hash(randomKey);
			return hashedInputPassword == storedPassword;
		}
		public async Task<KhachHang?> UpdatePasswordAsync(string codeSend, string newPassword, string email)
		{
			var khachHang = await db.KhachHangs.FirstOrDefaultAsync(kh => kh.Email == email);
			if (khachHang == null)
			{
				return null;
			}

			if (khachHang.RandomKey != codeSend)
			{
				return null;
			}
			khachHang.MatKhau = HashPassword(newPassword, Convert.FromBase64String(khachHang.RandomKey));

			await db.SaveChangesAsync();

			return khachHang;
		}
		private static string HashPassword(string password, byte[] randomKey)
		{
			using (var hmac = new System.Security.Cryptography.HMACSHA512(randomKey))
			{
				byte[] passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
				return Convert.ToBase64String(passwordHash);
			}
		}
		public async Task<KhachHang?> Authenticate(string email, string passwordText)
		{
			var khachhang = await db.KhachHangs
				.FirstOrDefaultAsync(x => x.Email.ToLower() == email.ToLower());

			if (khachhang == null)
			{
				return null;
			}

			if (!VerifyPasswordHash(passwordText, khachhang.MatKhau, khachhang.RandomKey))
			{
				return null;
			}

			return khachhang;
		}
		private bool VerifyPasswordHash(string passwordText, string storedPassword, string passwordKey)
		{
			byte[] storedPasswordBytes = Convert.FromBase64String(storedPassword);
			byte[] passwordKeyBytes = Convert.FromBase64String(passwordKey);

			// Convert password text to byte array before hashing
			using (var hmac = new HMACSHA512(passwordKeyBytes))
			{
				var computedPasswordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(passwordText));

				// Ensure equal length before comparison
				if (computedPasswordHash.Length != storedPasswordBytes.Length)
				{
					return false;
				}

				// Constant-time comparison
				uint difference = 0;
				for (int i = 0; i < computedPasswordHash.Length; i++)
				{
					difference |= (uint)(computedPasswordHash[i] ^ storedPasswordBytes[i]);
				}

				return difference == 0;
			}
		}
		private (string passwordHash, string randomKey) CreatePasswordHash(string password)
		{
			// Generate a new random key
			byte[] randomKey = new byte[64]; // 64-byte key
			using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
			{
				rng.GetBytes(randomKey);
			}

			// Create hash using the random key
			using (var hmac = new HMACSHA512(randomKey))
			{
				byte[] passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));

				// Convert to Base64 strings for storage
				return (
					Convert.ToBase64String(passwordHash),
					Convert.ToBase64String(randomKey)
				);
			}
		}
		public async Task<KhachHang> CreateAccountAsync(RegisterMD model)
		{
			// Input validation
			//ValidateRegistrationModel(model);

			// Check for existing username
			var khachHangexit = await db.KhachHangs.SingleOrDefaultAsync(kh => kh.MaKh == model.UserName);
			if (khachHangexit != null)
			{
				throw new ArgumentException("Username đã tồn tại");
			}

			// Check for existing email
			var khachHangemail = await db.KhachHangs.SingleOrDefaultAsync(kh => kh.Email == model.Email);
			if (khachHangemail != null)
			{
				throw new ArgumentException("Email đã tồn tại");
			}

			// Create new customer
			var khachHang = model.ToAccountCreateDTO();

			await db.KhachHangs.AddAsync(khachHang);
			await db.SaveChangesAsync();

			return khachHang;
		}
		private void ValidateRegistrationModel(DangKyMD model)
		{
			if (string.IsNullOrWhiteSpace(model.UserName))
			{
				throw new ArgumentException("Chưa nhập đủ thông tin: Họ tên không được để trống");
			}

			if (string.IsNullOrWhiteSpace(model.Password))
			{
				throw new ArgumentException("Chưa nhập đủ thông tin: Mật khẩu không được để trống");
			}

			if (string.IsNullOrWhiteSpace(model.Email))
			{
				throw new ArgumentException("Chưa nhập đủ thông tin: Email không được để trống");
			}

			if (model.DienThoai.Length < 9)
			{
				throw new ArgumentException("Nhập sai định dạng số điện thoại");
			}
		}
		public async Task<IEnumerable<string>> GetRoleAsync(KhachHang user)
		{
			// Simple implementation based on VaiTro field
			// You might want to expand this based on your specific role system
			var roles = new List<string>();

			switch (user.VaiTro)
			{
				case 1:
					roles.Add("Admin");
					break;
				case 2:
					roles.Add("Dev");
					break;
				case 3:
					roles.Add("Customer");
					break;
				default:
					roles.Add("Guest");
					break;
			}

			return roles;
		}
		public async Task<string> GeneratePasswordResetTokenAsync(KhachHang user)
		{
			if (user == null)
			{
				throw new ArgumentNullException(nameof(user));
			}

			// Generate a secure random token
			var randomBytes = new byte[32];
			using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
			{
				rng.GetBytes(randomBytes);
			}

			// Convert to a URL-safe string
			var token = Convert.ToBase64String(randomBytes)
				.Replace("+", "-")
				.Replace("/", "_")
				.Replace("=", "");

			// Store the token in the user's RandomKey field
			// This reuses the existing RandomKey field since it's already being used
			// for password reset verification in the ResetPasswordAsync method
			user.RandomKey = token;

			// Update the user in the database
			db.Entry(user).State = EntityState.Modified;
			await db.SaveChangesAsync();

			return token;
		}
		public async Task<KhachHang> UpdateAsync(KhachHang user)
		{
			// Find the existing user in the database
			var existingUser = await db.KhachHangs.FirstOrDefaultAsync(u => u.MaKh == user.MaKh);

			if (existingUser == null)
			{
				throw new KeyNotFoundException($"Không tìm thấy khách hàng với mã {user.MaKh}");
			}

			// Update specific properties that can be modified
			existingUser.RefreshToken = user.RefreshToken;
			existingUser.RefreshTokenExpiryTime = user.RefreshTokenExpiryTime;

			// Optional: Add more properties to update as needed
			// existingUser.Email = user.Email;
			// existingUser.HoTen = user.HoTen;

			// Mark the entity as modified
			db.Entry(existingUser).State = EntityState.Modified;

			// Save changes to the database
			await db.SaveChangesAsync();

			return existingUser;
		}
		public async Task<bool> ResetPasswordAsync(KhachHang user, string token, string newPassword)
		{
			var existingUser = await db.KhachHangs.FirstOrDefaultAsync(u => u.MaKh == user.MaKh);

			if (existingUser == null)
			{
				throw new KeyNotFoundException($"Không tìm thấy khách hàng với mã {user.MaKh}");
			}

			// Check if the token matches
			if (existingUser.RandomKey != token)
			{
				throw new UnauthorizedAccessException("Token không hợp lệ hoặc đã hết hạn.");
			}

			// Generate new password hash and key
			var (passwordHash, randomKey) = CreatePasswordHash(newPassword);

			// Update the user's password and key
			existingUser.MatKhau = passwordHash;
			existingUser.RandomKey = randomKey;

			db.Entry(existingUser).State = EntityState.Modified;

			// Save changes to the database
			var result = await db.SaveChangesAsync();
			return result > 0; // Return true if the password was successfully reset
		}
		public async Task<(bool IsSuccess, string Message)> ChangePasswordAsync(KhachHang user, string currentPassword, string newPassword)
		{
			if (user == null)
			{
				return (false, "User not found");
			}

			// Validate the current password
			if (!VerifyPasswordHash(currentPassword, user.MatKhau, user.RandomKey))
			{
				return (false, "Current password is incorrect");
			}

			// Validate the new password
			if (string.IsNullOrWhiteSpace(newPassword))
			{
				return (false, "New password cannot be empty");
			}

			if (newPassword.Length < 6)
			{
				return (false, "New password must be at least 6 characters long");
			}

			try
			{
				// Generate new password hash and key
				var (passwordHash, randomKey) = CreatePasswordHash(newPassword);

				// Update the user's password and key
				user.MatKhau = passwordHash;
				user.RandomKey = randomKey;

				// Mark the entity as modified
				db.Entry(user).State = EntityState.Modified;

				// Save changes to the database
				await db.SaveChangesAsync();

				return (true, "Password changed successfully");
			}
			catch (Exception ex)
			{
				return (false, $"Failed to change password: {ex.Message}");
			}
		}

	}
}