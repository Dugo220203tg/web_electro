using API_Web_Shop_Electronic_TD.Data;
using API_Web_Shop_Electronic_TD.Helpers;
using API_Web_Shop_Electronic_TD.Interfaces;
using API_Web_Shop_Electronic_TD.Mappers;
using API_Web_Shop_Electronic_TD.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System;

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
		public async Task<KhachHang?> DeleteAsync(string MaKh)
		{
			var model = await db.KhachHangs.FirstOrDefaultAsync(x => x.MaKh == MaKh);
			if (model == null)
			{
				throw new KeyNotFoundException($"Không tìm thấy khách hàng với mã {MaKh}");
			}
			db.KhachHangs.Remove(model);
			await db.SaveChangesAsync();
			return model;
		}



		public async Task<KhachHang?> GetByIdAsync(string MaKh)
		{
			return await db.KhachHangs.FindAsync(MaKh);

		}

		public async Task<KhachHang?> UpdateAsync(string MaKh, UpdateKH model)
		{
			var khachHangModel = await db.KhachHangs.FirstOrDefaultAsync(x => x.MaKh == MaKh);
			if (khachHangModel == null)
			{
				throw new KeyNotFoundException($"Không tìm thấy khách hàng với mã {MaKh}");
			}

			// Cập nhật thông tin của đối tượng KhachHang từ đối tượng UpdateKH
			khachHangModel = model.UpdateKhachHangInfo(khachHangModel);

			// Chỉ cập nhật các trường đã thay đổi
			db.Entry(khachHangModel).State = EntityState.Modified;
			await db.SaveChangesAsync();
			return khachHangModel;
		}
		public async Task<KhachHang?> GetByEmailAsync(string Username)
		{
			return await db.KhachHangs.FirstOrDefaultAsync(kh => kh.MaKh == Username);
		}
		public bool VerifyPassword(string inputPassword, string storedPassword, string randomKey)
		{
			string hashedInputPassword = inputPassword.ToMd5Hash(randomKey);
			return hashedInputPassword == storedPassword;
		}
		public async Task<KhachHang?> UpdatePasswordAsync(string codeSend, string newPassword, string email)
		{
			// Find the customer by email
			var khachHang = await db.KhachHangs.FirstOrDefaultAsync(kh => kh.Email == email);
			if (khachHang == null)
			{
				return null; // No customer found with this email
			}

			// Verify the RandomKey matches the codeSend provided by the customer
			if (khachHang.RandomKey != codeSend)
			{
				return null; // RandomKey does not match, returning null to indicate failure
			}

			// Update the password, hashing it with the RandomKey
			khachHang.MatKhau = newPassword.ToMd5Hash(khachHang.RandomKey);

			// Save the changes to the database
			await db.SaveChangesAsync();

			return khachHang; // Return the updated customer object
		}


	}
}
