using API_Web_Shop_Electronic_TD.Data;
using API_Web_Shop_Electronic_TD.DTOs;
using API_Web_Shop_Electronic_TD.Interfaces;
using API_Web_Shop_Electronic_TD.Mappers;
using API_Web_Shop_Electronic_TD.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace API_Web_Shop_Electronic_TD.Repository
{
	public class DanhMucRepository :IDanhMuc
	{
		private readonly Hshop2023Context db;

		public DanhMucRepository(Hshop2023Context db)
		{
			this.db = db;
		}
		public async Task<DanhMucSp> CreateAsync(DanhMucMD model)
		{
			if (string.IsNullOrEmpty(model.TenDanhMuc))
			{
				throw new ArgumentException("Chưa nhập đủ thông tin: Tên Loại không được để trống");
			}
			var danhmuc = model.ToDanhMucDTO(); // Sử dụng mapper để chuyển đổi từ KhachHangsMD sang KhachHang

			await db.DanhMucSps.AddAsync(danhmuc);
			await db.SaveChangesAsync();

			return danhmuc;
		}
		public async Task<DanhMucSp?> DeleteAsync(int MaDanhMuc)
		{
			var danhmucModel = await db.DanhMucSps.FirstOrDefaultAsync(x => x.MaDanhMuc == MaDanhMuc);
			if (danhmucModel == null)
			{
				throw new KeyNotFoundException($"Không tìm thấy loại sản phẩm với mã {MaDanhMuc}");
			}
			db.DanhMucSps.Remove(danhmucModel);
			await db.SaveChangesAsync();
			return danhmucModel;
		}

		public async Task<List<DanhMucSp>> GetAllAsync()
		{
			return await db.DanhMucSps.ToListAsync();
		}

		public async Task<DanhMucSp?> GetByIdAsync(int MaDanhMuc)
		{
			return await db.DanhMucSps.FindAsync(MaDanhMuc);
		}

		public async Task<DanhMucSp?> UpdateAsync(int MaDanhMuc, DanhMucMD model)
		{
			if (string.IsNullOrEmpty(model.TenDanhMuc))
			{
				throw new ArgumentException("Chưa nhập đủ thông tin: Tên loại không được để trống");
			}
			// Lấy đối tượng HangHoa từ cơ sở dữ liệu
			var danhmucModel = await db.DanhMucSps.FirstOrDefaultAsync(x => x.MaDanhMuc == MaDanhMuc);

			// Kiểm tra xem HangHoaModel có null không
			if (danhmucModel == null)
			{
				throw new KeyNotFoundException($"Không tìm thấy Danh Muc sản phẩm với mã {MaDanhMuc}");
			}

			// Cập nhật thông tin của HangHoaModel từ dữ liệu được gửi từ client
			danhmucModel.TenDanhMuc = model.TenDanhMuc;

			// Lưu thay đổi vào cơ sở dữ liệu
			await db.SaveChangesAsync();

			// Trả về HangHoaModel đã được cập nhật
			return danhmucModel;
		}
	}
}
