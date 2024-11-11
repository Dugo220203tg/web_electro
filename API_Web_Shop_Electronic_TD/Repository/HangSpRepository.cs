using API_Web_Shop_Electronic_TD.Data;
using API_Web_Shop_Electronic_TD.Interfaces;
using API_Web_Shop_Electronic_TD.Mappers;
using API_Web_Shop_Electronic_TD.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace API_Web_Shop_Electronic_TD.Repository
{
	public class HangSpRepository : IHangSp
	{
		private readonly Hshop2023Context db;
		public HangSpRepository(Hshop2023Context db)
		{
			this.db = db;
		}

		public async Task<NhaCungCap> CreateAsync(HangSpMD model)
		{
			if (string.IsNullOrEmpty(model.MaNCC))
			{
				throw new ArgumentException("Chưa nhập đủ thông tin: Mã nhà cung cấp không được để trống");
			}
			if (string.IsNullOrEmpty(model.TenCongTy))
			{
				throw new ArgumentException("Chưa nhập đủ thông tin: Tên nhà cung cấp không được để trống");
			}
			if (string.IsNullOrEmpty(model.DienThoai))
			{
				throw new ArgumentException("Chưa nhập đủ thông tin: Điện thoại nhà cung cấp không được để trống");
			}
			if (string.IsNullOrEmpty(model.Email))
			{
				throw new ArgumentException("Chưa nhập đủ thông tin: Điện thoại nhà cung cấp không được để trống");
			}
			if (string.IsNullOrEmpty(model.Logo))
			{
				throw new ArgumentException("Chưa nhập đủ thông tin: Điện thoại nhà cung cấp không được để trống");
			}
			if (string.IsNullOrEmpty(model.Mota))
			{
				throw new ArgumentException("Chưa nhập đủ thông tin: Điện thoại nhà cung cấp không được để trống");
			}
			if (string.IsNullOrEmpty(model.DiaChi))
			{
				throw new ArgumentException("Chưa nhập đủ thông tin: Điện thoại nhà cung cấp không được để trống");
			}
			if (string.IsNullOrEmpty(model.NguoiLienLac))
			{
				throw new ArgumentException("Chưa nhập đủ thông tin: Điện thoại nhà cung cấp không được để trống");
			}
			var hangsp = model.TohangSpDTO(); 
			await db.NhaCungCaps.AddAsync(hangsp);
			await db.SaveChangesAsync();

			return hangsp; 
		}

		public async Task<NhaCungCap?> DeleteAsync(string MaNCC)
		{
			var NhaCC = await db.NhaCungCaps.FirstOrDefaultAsync(x => x.MaNcc == MaNCC);
			if (NhaCC == null)
			{
				throw new KeyNotFoundException($"Không tìm thấy Nhà cung cấp sản phẩm với mã {MaNCC}");
			}
			db.NhaCungCaps.Remove(NhaCC);
			await db.SaveChangesAsync();
			return NhaCC;
		}

		public async Task<List<NhaCungCap>> GetAllAsync()
		{
			return await db.NhaCungCaps.ToListAsync();

		}

		public async Task<NhaCungCap?> GetByIdAsync(string MaNCC)
		{
			return await db.NhaCungCaps.FindAsync(MaNCC);
		}

		public async Task<NhaCungCap?> UpdateAsync(string MaNCC, HangSpMD model)
		{
			if (string.IsNullOrEmpty(model.MaNCC))
			{
				throw new ArgumentException("Chưa nhập đủ thông tin: Mã nhà cung cấp không được để trống");
			}
			if (string.IsNullOrEmpty(model.TenCongTy))
			{
				throw new ArgumentException("Chưa nhập đủ thông tin: Tên nhà cung cấp không được để trống");
			}
			if (string.IsNullOrEmpty(model.DienThoai))
			{
				throw new ArgumentException("Chưa nhập đủ thông tin: Điện thoại nhà cung cấp không được để trống");
			}
			if (string.IsNullOrEmpty(model.Email))
			{
				throw new ArgumentException("Chưa nhập đủ thông tin: Điện thoại nhà cung cấp không được để trống");
			}
			if (string.IsNullOrEmpty(model.Logo))
			{
				throw new ArgumentException("Chưa nhập đủ thông tin: Điện thoại nhà cung cấp không được để trống");
			}
			if (string.IsNullOrEmpty(model.Mota))
			{
				throw new ArgumentException("Chưa nhập đủ thông tin: Điện thoại nhà cung cấp không được để trống");
			}
			if (string.IsNullOrEmpty(model.DiaChi))
			{
				throw new ArgumentException("Chưa nhập đủ thông tin: Điện thoại nhà cung cấp không được để trống");
			}
			if (string.IsNullOrEmpty(model.NguoiLienLac))
			{
				throw new ArgumentException("Chưa nhập đủ thông tin: Điện thoại nhà cung cấp không được để trống");
			}
			// Lấy đối tượng HangHoa từ cơ sở dữ liệu
			var NhaCcModel = await db.NhaCungCaps.FirstOrDefaultAsync(x => x.MaNcc == MaNCC);

			// Kiểm tra xem HangHoaModel có null không
			if (NhaCcModel == null)
			{
				throw new KeyNotFoundException($"Không tìm thấy Nhà cung cấp sản phẩm với mã {MaNCC}");
			}

			// Cập nhật thông tin của HangHoaModel từ dữ liệu được gửi từ client
			NhaCcModel.TenCongTy = model.TenCongTy;
			NhaCcModel.Logo = model.Logo;
			NhaCcModel.MoTa = model.Mota;
			NhaCcModel.Email = model.Email;
			NhaCcModel.DienThoai = model.DienThoai;
			NhaCcModel.DiaChi = model.DiaChi;
			NhaCcModel.NguoiLienLac = model.NguoiLienLac;
			// Lưu thay đổi vào cơ sở dữ liệu
			await db.SaveChangesAsync();

			// Trả về HangHoaModel đã được cập nhật
			return NhaCcModel;
		}
	}
}
