using API_Web_Shop_Electronic_TD.Data;
using API_Web_Shop_Electronic_TD.Interfaces;
using API_Web_Shop_Electronic_TD.Models;
using Microsoft.EntityFrameworkCore;

namespace API_Web_Shop_Electronic_TD.Repository
{
	public class DanhGiaSpRepository : IDanhGiaSp
	{
		private readonly Hshop2023Context db;
		public DanhGiaSpRepository(Hshop2023Context db)
		{
			this.db = db;
		}
		public async Task<DanhGiaSp> CreateAsync(CreateDanhGiaSpMD model)
		{
			// Ensure MaKH and MaHH are valid before proceeding
			if (string.IsNullOrEmpty(model.MaKH) || model.MaHH <= 0)
			{
				throw new ArgumentException("MaKH and MaHH are required.");
			}

			var danhgia = new DanhGiaSp
			{
				MaKh = model.MaKH,
				NoiDung = model.NoiDung,
				Sao = model.Sao,
				Ngay = model.Ngay,
				MaHh = model.MaHH,
				TrangThai = model.TrangThai
			};

			// Add the new review to the database
			await db.DanhGiaSps.AddAsync(danhgia);
			await db.SaveChangesAsync();

			return danhgia;
		}


		public async Task<DanhGiaSp?> DeleteAsync(int MaDg)
		{
			var Model = await db.DanhGiaSps.FirstOrDefaultAsync(x => x.MaDg == MaDg);
			if (Model == null)
			{
				throw new KeyNotFoundException($"Không tìm đánh giá với mã {MaDg}");
			}
			db.DanhGiaSps.Remove(Model);
			await db.SaveChangesAsync();
			return Model;
		}

		public async Task<List<DanhGiaSp>> GetAllAsync()
		{
			return await db.DanhGiaSps
				.Include(h => h.MaKhNavigation)
				.Include(h => h.MaHhNavigation)
				.ToListAsync();
		}

		public async Task<DanhGiaSp?> GetByIdAsync(int MaDg)
		{
			return await db.DanhGiaSps
				.Include(h => h.MaKhNavigation)
				.Include(h => h.MaHhNavigation)
				.FirstOrDefaultAsync(h => h.MaDg == MaDg);
		}

		public async Task<DanhGiaSp?> UpdateAsync(int MaDg, CreateDanhGiaSpMD model)
		{
			// Lấy đối tượng HangHoa từ cơ sở dữ liệu
			var DanhGiaModel = await db.DanhGiaSps
				.Include(d => d.MaKhNavigation) 
				.Include(d => d.MaHhNavigation)
				.FirstOrDefaultAsync(x => x.MaDg == MaDg);

			// Kiểm tra xem HangHoaModel có null không
			if (DanhGiaModel == null)
			{
				throw new KeyNotFoundException($"Không tìm thấy loại sản phẩm với mã {MaDg}");
			}

			DanhGiaModel.Sao = model.Sao;
			DanhGiaModel.NoiDung = model.NoiDung;
			DanhGiaModel.TrangThai = model.TrangThai;

			// Lưu thay đổi vào cơ sở dữ liệu
			await db.SaveChangesAsync();

			// Trả về HangHoaModel đã được cập nhật
			return DanhGiaModel;
		}
	}
}
