using API_Web_Shop_Electronic_TD.Data;
using API_Web_Shop_Electronic_TD.Interfaces;
using API_Web_Shop_Electronic_TD.Models;
using Microsoft.EntityFrameworkCore;

namespace API_Web_Shop_Electronic_TD.Repository
{
	public class WishListRepository : IWishListRepository
	{
		private readonly Hshop2023Context db;

		public WishListRepository(Hshop2023Context db)
		{
			this.db = db;
		}
		public async Task<List<YeuThich>> GetWishListByAccountId(string accountId)
		{
			// Check for null or empty account ID
			if (string.IsNullOrEmpty(accountId))
			{
				throw new ArgumentException("Account ID cannot be null or empty.", nameof(accountId));
			}

			// Fetch wishlist items filtered by account ID
			return await db.YeuThiches
				.Include(yt => yt.MaHhNavigation)
					.ThenInclude(hh => hh.MaNccNavigation)
				.Where(yt => yt.MaKh == accountId)
				.ToListAsync();
		}

		public async Task RemoveFromWishListAsync(string userId, int productId)
		{
			var wishItem = await db.YeuThiches
				.SingleOrDefaultAsync(y => y.MaKh == userId && y.MaHh == productId);

			if (wishItem == null)
			{
				throw new ArgumentException("Không tìm thấy sản phẩm trong danh sách yêu thích");
			}

			db.YeuThiches.Remove(wishItem);
			await db.SaveChangesAsync();
		}

		public async Task<YeuThich> AddWishListAsync(WishListRequest model)
		{
			var wishProduct = await db.YeuThiches
				.SingleOrDefaultAsync(yt => yt.MaKh == model.maKh && yt.MaHh == model.maHh);

			if (wishProduct != null)
			{
				throw new ArgumentException("Sản phẩm đã có trong danh sách yêu thích");
			}

			YeuThich newYeuThich = new YeuThich
			{
				MaHh = model.maHh,
				MaKh = model.maKh,
				NgayChon = DateTime.Now
			};

			await db.YeuThiches.AddAsync(newYeuThich);
			await db.SaveChangesAsync();
			return newYeuThich;
		}
	}
}
