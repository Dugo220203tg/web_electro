using API_Web_Shop_Electronic_TD.Data;
using API_Web_Shop_Electronic_TD.Interfaces;
using API_Web_Shop_Electronic_TD.Models;
using Microsoft.EntityFrameworkCore;

namespace API_Web_Shop_Electronic_TD.Repository
{
	public class ThongKeRepository : IThongKeRepository
	{
		private readonly Hshop2023Context db;

		public ThongKeRepository(Hshop2023Context db)
		{
			this.db = db;
		}

		public async Task<List<DataSellProductVMD>> GetTopFavoriteProduct()
		{
			// First, get the base products with their ratings
			var products = await db.HangHoas
				.Include(hh => hh.DanhGiaSps)
				.Include(hh => hh.MaLoaiNavigation)
					.ThenInclude(ml => ml.DanhMuc)
				.Select(hh => new
				{
					hh.MaHh,
					hh.TenHh,
					hh.Hinh,
					hh.DonGia,
					hh.SoLuong,
					CategoryName = hh.MaLoaiNavigation.DanhMuc.TenDanhMuc ?? "Unknown",
					Ratings = hh.DanhGiaSps.Where(dg => dg.Sao.HasValue).Select(dg => dg.Sao.Value)
				})
				.ToListAsync();

			// Then perform the calculations in memory
			var topRatedProducts = products
				.Select(p => new
				{
					ProductId = p.MaHh,
					ProductName = p.TenHh ?? string.Empty,
					Image = p.Hinh ?? string.Empty,
					Price = p.DonGia ?? 0,
					CategoryName = p.CategoryName,
					Number = p.SoLuong ?? 0,
					AverageRating = p.Ratings.Any()
						? (int)Math.Round(p.Ratings.Average())
						: 0
				})
				.OrderByDescending(p => p.AverageRating)
				.ThenBy(p => p.ProductName)
				.Take(4);

			// Convert to final DTO
			return topRatedProducts
				.Select(p => new DataSellProductVMD
				{
					MaHH = p.ProductId,
					TenHH = p.ProductName,
					TrungBinhSao = p.AverageRating,
					Hinh = p.Image,
					DonGia = Convert.ToDouble(p.Price),
					SoLuong = p.Number,
					TenDanhMuc = p.CategoryName
				})
				.ToList();
		}



		public async Task<List<DataSellProductVMD>> GetTopSellProduct()
		{
			var productSell = await db.ChiTietHds
				.Include(ct => ct.MaHhNavigation)
				 .ThenInclude(hh => hh.DanhGiaSps)
				.GroupBy(ct => new
				{
					ProductId = ct.MaHhNavigation.MaHh,
					ProductName = ct.MaHhNavigation.TenHh,
					ProductPrice = ct.MaHhNavigation.DonGia,
					Image = ct.MaHhNavigation.Hinh,
					CategoryName = ct.MaHhNavigation.MaLoaiNavigation.DanhMuc.TenDanhMuc,
					AverageRating = ct.MaHhNavigation.DanhGiaSps.Any()
						? (int)Math.Round(ct.MaHhNavigation.DanhGiaSps.Average(dg => dg.Sao ?? 0))
						: 0
				})
				.Select(group => new
				{
					group.Key.ProductId,
					group.Key.ProductName,
					group.Key.ProductPrice,
					group.Key.Image,
					group.Key.CategoryName,
					group.Key.AverageRating,
					TotalQuantitySold = group.Sum(ct => ct.SoLuong)
				})
				.OrderByDescending(p => p.TotalQuantitySold)
				.ThenBy(p => p.ProductName)
				.Take(4)
				.ToListAsync();

			return productSell.Select(p => new DataSellProductVMD
			{
				TenDanhMuc = p.CategoryName,
				MaHH = p.ProductId,
				TenHH = p.ProductName,
				SoLuong = p.TotalQuantitySold,
				DonGia = (double)p.ProductPrice,
				Hinh = p.Image,
				TrungBinhSao = p.AverageRating
			}).ToList();
		}
	}
}
