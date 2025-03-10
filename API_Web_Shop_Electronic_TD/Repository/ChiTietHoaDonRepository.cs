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
		

		async Task<ChiTietHd?> ICtHoaDon.UpdateAsync(int MaCt, PostChiTietHoaDonMD model)
		{
			try
			{
				var Model = await db.ChiTietHds.FirstOrDefaultAsync(x => x.MaCt == MaCt);

				if (Model == null)
				{
					throw new KeyNotFoundException($"Không tìm thấy chi tiết hóa đơn với mã {MaCt}");
				}
				if (model.MaHH != Model.MaHh)
				{
					throw new KeyNotFoundException($"Không tìm thấy hàng hóa trong chi tiết hóa đơn với mã {MaCt}");
				}
				if (model.MaHD != Model.MaHd)
				{
					throw new KeyNotFoundException($"Không tìm thấy hóa đơn tương thích trong chi tiết hóa đơn với mã {MaCt}");
				}
				var hanghoa = await db.HangHoas.FirstOrDefaultAsync(x => x.MaHh == Model.MaHh);
				if (hanghoa == null)
				{
					throw new KeyNotFoundException($"Hàng hóa với mã {Model.MaHh}");
				}
				var HoaDon = await db.HoaDons.FirstOrDefaultAsync(x => x.MaHd == Model.MaHd);
				if (HoaDon == null)
				{
					throw new KeyNotFoundException($"Không tồn tại hóa đơn với mã {Model.MaHd}");

				}
				Model.MaHd = model.MaHD;
				Model.MaHh = model.MaHH;
				Model.DonGia = model.DonGia;
				Model.SoLuong = model.SoLuong;

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
