using API_Web_Shop_Electronic_TD.Data;
using API_Web_Shop_Electronic_TD.Interfaces;
using API_Web_Shop_Electronic_TD.Mappers;
using API_Web_Shop_Electronic_TD.Models;
using Microsoft.EntityFrameworkCore;

namespace API_Web_Shop_Electronic_TD.Repository
{
	public class ChiTietHoaDonRepository : ICtHoaDon
	{
		private readonly Hshop2023Context db;
		public ChiTietHoaDonRepository(Hshop2023Context db)
		{
			this.db = db;
		}
		public async Task<ChiTietHd> CreateAsync(PostChiTietHoaDonMD model)
		{
			if (model.MaHH == 0)
			{
				throw new ArgumentException("Mã hàng hóa không hợp lệ hoặc chưa được nhập");
			}
			if (model.MaHD == 0 || model.MaHD <= 0)
			{
				throw new ArgumentException(" Mã hóa đơn không hợp lệ hoặc chưa được nhập");
			}
			if (model.MaGiamGia <= 0)
			{
				throw new ArgumentException("Mã giảm giá không hợp lệ hoặc chưa được nhập");
			}
			if (model.SoLuong <= 0)
			{
				throw new ArgumentException("Số lượng không hợp lệ hoặc chưa được nhập");
			}
			if (model.DonGia <= 0)
			{
				throw new ArgumentException("Đơn giá không hợp lệ hoặc chưa được nhập");
			}
			// Fetch the existing HangHoa from the database
			var hangHoa = await db.HangHoas.FirstOrDefaultAsync(h => h.MaHh == model.MaHH);
			if (hangHoa == null)
			{
				throw new KeyNotFoundException($"Không tìm thấy hàng hóa với mã {model.MaHH}");
			}

			// Map the ChiTietHoaDonMD to ChiTietHd and pass the existing HangHoa entity
			var chitiethoadon = model.ToCtHoaDonDTO();

			// Add the new ChiTietHd to the database
			await db.ChiTietHds.AddAsync(chitiethoadon);
			await db.SaveChangesAsync();

			return chitiethoadon;
		}


		async Task<ChiTietHd?> ICtHoaDon.DeleteAsync(int MaCt)
		{
			var Model = await db.ChiTietHds.FirstOrDefaultAsync(x => x.MaCt == MaCt);
			if (Model == null)
			{
				throw new KeyNotFoundException($"Không tìm thấy loại sản phẩm với mã {MaCt}");
			}
			db.ChiTietHds.Remove(Model);
			await db.SaveChangesAsync();
			return Model;
		}

		async Task<List<ChiTietHd>> ICtHoaDon.GetAllAsync()
		{
			return await db.ChiTietHds
				.Include(ct => ct.MaHdNavigation)
					.ThenInclude(hd => hd.MaTrangThaiNavigation)
				.Include(ct => ct.MaHhNavigation)
					.ThenInclude(h => h.MaLoaiNavigation)
				.ToListAsync();
		}

		async Task<ChiTietHd?> ICtHoaDon.GetByIdAsync(int MaCt)
		{
			return await db.ChiTietHds
				.Include(ct => ct.MaHhNavigation)
					.ThenInclude(h => h.MaLoaiNavigation)
				.FirstOrDefaultAsync(ct => ct.MaCt == MaCt);
		}
		async Task<List<CategorySalesStatistics>> ICtHoaDon.GetStatisticsAsync()
		{
			return await db.ChiTietHds
				.Include(ct => ct.MaHhNavigation)
					.ThenInclude(h => h.MaLoaiNavigation)
				.GroupBy(ct => new
				{
					CategoryId = ct.MaHhNavigation.MaLoaiNavigation.DanhMucId,
					Month = ct.MaHdNavigation.NgayDat.Month // Assuming NgayDat stores order date
				})
				.Select(group => new CategorySalesStatistics
				{
					DanhMucId = (int)group.Key.CategoryId,
					Month = group.Key.Month,
					TotalQuantitySold = group.Sum(ct => ct.SoLuong)
				})
				.OrderBy(stats => stats.DanhMucId)
				.ThenBy(stats => stats.Month)
				.ToListAsync();
		}


		async Task<ChiTietHd?> ICtHoaDon.UpdateAsync(int MaCt, PostChiTietHoaDonMD model)
		{
			try
			{
				var Model = await db.ChiTietHds.FirstOrDefaultAsync(x => x.MaCt == MaCt);
				
				if (Model == null)
				{
					throw new KeyNotFoundException($"Không tìm thấy chi tiết hóa đơn với mã {MaCt}");
				}
				var hanghoa = await db.HangHoas.FirstOrDefaultAsync(x => x.MaHh == Model.MaHh);
				if (hanghoa == null)
				{
					throw new KeyNotFoundException($"Hàng hóa với mã {Model.MaHh}");
				}
				Model.MaHd = model.MaHD;
				Model.MaHh = model.MaHH;
				Model.DonGia = model.DonGia;
				Model.SoLuong = model.SoLuong;
				Model.MaGiamGia = model.MaGiamGia;

				await db.SaveChangesAsync();
				return Model;
			}
			catch (Exception ex)
			{
				throw new Exception($"Lỗi khi cập nhật chi tiết hóa đơn: {ex.Message}");
			}
		}
	}
}
