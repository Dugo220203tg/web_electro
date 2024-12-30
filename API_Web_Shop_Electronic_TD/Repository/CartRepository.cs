//using API_Web_Shop_Electronic_TD.Data;
//using API_Web_Shop_Electronic_TD.Helpers;
//using API_Web_Shop_Electronic_TD.Interfaces;
//using API_Web_Shop_Electronic_TD.Models;
//using Microsoft.AspNetCore.Http;
//using Microsoft.EntityFrameworkCore;

//namespace API_Web_Shop_Electronic_TD.Repository
//{
//	public class CartRepository : ICartRepository
//	{
//		private readonly Hshop2023Context _db;

//		public CartRepository(Hshop2023Context db)
//		{
//			_db = db;
//		}

//		public List<CartItem> Cart => HttpContext.Session.Get<List<CartItem>>(MySetting.CART_KEY) ?? new List<CartItem>();


//		private void SaveCart(List<CartItem> cart)
//		{
//			return HttpContext?.Session.Set(MySetting.CART_KEY, cart);
//		}

//		public async Task<CartResponse> AddToCartAsync(int id, int quantity = 1)
//		{
//			try
//			{
//				var cart = GetCart();
//				var item = cart.SingleOrDefault(p => p.MaHH == id);

//				var product = await _db.HangHoas.SingleOrDefaultAsync(p => p.MaHh == id);
//				if (product == null)
//				{
//					return new CartResponse
//					{
//						Success = false,
//						Message = $"Không tìm thấy hàng hóa có mã {id}"
//					};
//				}

//				if (item == null)
//				{
//					item = new CartItem
//					{
//						MaHH = product.MaHh,
//						TenHH = product.TenHh,
//						DonGia = product.DonGia ?? 0,
//						Hinh = product.Hinh ?? string.Empty,
//						SoLuong = quantity
//					};
//					cart.Add(item);
//				}
//				else
//				{
//					item.SoLuong += quantity;
//				}

//				SaveCart(cart);

//				return new CartResponse
//				{
//					Success = true,
//					CartCount = cart.Sum(i => i.SoLuong),
//					Message = "Thêm vào giỏ hàng thành công"
//				};
//			}
//			catch (Exception ex)
//			{
//				return new CartResponse
//				{
//					Success = false,
//					Message = "Lỗi khi thêm vào giỏ hàng: " + ex.Message
//				};
//			}
//		}

//		public async Task<CartData> GetCartDataAsync()
//		{
//			var cart = GetCart();

//			return new CartData
//			{
//				CartItems = cart.Select(p => new CartItemData
//				{
//					MaHH = p.MaHH,
//					TenHH = p.TenHH,
//					SoLuong = p.SoLuong,
//					DonGia = (decimal)p.DonGia,
//					Hinh = p.Hinh?.Split(',').FirstOrDefault()?.Trim() ?? ""
//				}).ToList(),
//				TotalQuantity = cart.Sum(p => p.SoLuong),
//				TotalAmount = (decimal)cart.Sum(p => p.SoLuong * p.DonGia)
//			};
//		}

//		public async Task<CartResponse> RemoveFromCartAsync(int id)
//		{
//			try
//			{
//				var cart = GetCart();
//				var item = cart.SingleOrDefault(p => p.MaHH == id);

//				if (item == null)
//				{
//					return new CartResponse
//					{
//						Success = false,
//						Message = "Sản phẩm không tồn tại trong giỏ hàng"
//					};
//				}

//				cart.Remove(item);
//				SaveCart(cart);

//				return new CartResponse
//				{
//					Success = true,
//					CartCount = cart.Sum(i => i.SoLuong),
//					Message = "Đã xóa sản phẩm khỏi giỏ hàng"
//				};
//			}
//			catch (Exception ex)
//			{
//				return new CartResponse
//				{
//					Success = false,
//					Message = "Lỗi khi xóa sản phẩm: " + ex.Message
//				};
//			}
//		}
//	}
//}
