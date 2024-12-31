using API_Web_Shop_Electronic_TD.Data;
using API_Web_Shop_Electronic_TD.Interfaces;
using API_Web_Shop_Electronic_TD.Models;
using Microsoft.EntityFrameworkCore;

namespace API_Web_Shop_Electronic_TD.Repository
{
	public class CartRepository : ICartRepository
	{
		private readonly Hshop2023Context db;

		public CartRepository(Hshop2023Context db)
		{
			this.db = db;
		}

		public async Task<Cart> AddToCartAsync(CartResponse model)
		{
			var cartItem = await db.Carts
				.SingleOrDefaultAsync(x => x.UserId == model.MaKh && x.ProductId == model.MaHh);
			if(cartItem != null)
			{
				throw new ArgumentException("Error");
			}
			Cart newCart = new Cart
			{
				UserId = model.MaKh,
				ProductId = model.MaHh,
				Quantity = model.Quantity,
				CreateAt = DateTime.UtcNow,
			};
			await db.Carts.AddAsync(newCart);
			await db.SaveChangesAsync();
			return newCart;
		}


		public async Task<List<Cart>> GetCartDataAsync(string userId)
		{
			if (string.IsNullOrEmpty(userId))
			{
				throw new ArgumentException("Account ID cannot be null or empty.", nameof(userId));
			}

			return await db.Carts
				.Include(x => x.Product)
					.ThenInclude(p => p.MaNccNavigation)
				.Include(x => x.User)
				.Where(x => x.UserId == userId)
				.ToListAsync();
		}

		public async Task IncreaseQuantity(string userId, int productId)
		{
				using var transaction = await db.Database.BeginTransactionAsync();
			try
			{
				var cartItem = await db.Carts
					.SingleOrDefaultAsync(x => x.UserId == userId && x.ProductId == productId);

				if (cartItem == null)
				{
					throw new ArgumentException($"No cart item found for UserId: {userId} and ProductId: {productId}");
				}

				var product = await db.HangHoas
					.SingleOrDefaultAsync(p => p.MaHh == productId);

				if (product == null)
				{
					throw new ArgumentException($"Product with ProductId: {productId} does not exist.");
				}

				if (cartItem.Quantity >= product.SoLuong)
				{
					throw new InvalidOperationException("Cannot increase quantity. Exceeds available stock.");
				}

				cartItem.Quantity += 1;	
				var result = await db.SaveChangesAsync();
				Console.WriteLine($"Number of affected rows: {result}");
				await db.SaveChangesAsync();
				await transaction.CommitAsync();
			}
			catch
			{
				await transaction.RollbackAsync();
				throw;
			}
		}


		public async Task MinusQuantity(string userId, int productId)
		{
			using var transaction = await db.Database.BeginTransactionAsync();
			try
			{
				var cartItem = await db.Carts
					.SingleOrDefaultAsync(x => x.UserId == userId && x.ProductId == productId);

				if (cartItem == null)
				{
					throw new ArgumentException($"No cart item found for UserId: {userId} and ProductId: {productId}");
				}

				if (cartItem.Quantity <= 1)
				{
					throw new InvalidOperationException("Cannot decrease quantity below 1. Use remove function instead.");
				}
				cartItem.Quantity --;
				await db.SaveChangesAsync();
				await transaction.CommitAsync();
			}
			catch
			{
				await transaction.RollbackAsync();
				throw;
			}
		}

		public async Task RemoveFromCartAsync(string userId, int productId)
		{
			var cartItem = await db.Carts
				.SingleOrDefaultAsync(x => x.UserId == userId && x.ProductId == productId);
			var user = await db.KhachHangs.SingleOrDefaultAsync(kh => kh.MaKh == userId);
			if (user == null)
			{
				throw new ArgumentException("User not exits");
			}
			if (cartItem == null)
			{
				throw new ArgumentException("Cart item không tồn tại.");
			}

			Console.WriteLine($"Removing cart item: UserId = {userId}, ProductId = {productId}");
			db.Carts.Remove(cartItem);
			var result = await db.SaveChangesAsync();

			if (result <= 0)
			{
				throw new Exception("Không thể xóa bản ghi khỏi cơ sở dữ liệu.");
			}
		}

	}
}
