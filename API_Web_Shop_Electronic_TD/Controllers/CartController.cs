using API_Web_Shop_Electronic_TD.Data;
using API_Web_Shop_Electronic_TD.Helpers;
using API_Web_Shop_Electronic_TD.Models;
using Microsoft.AspNetCore.Mvc;

namespace API_Web_Shop_Electronic_TD.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class CartController : ControllerBase
	{
		private readonly Hshop2023Context db;

		public CartController(Hshop2023Context context)
		{
			this.db = context;
		}

		public List<CartItem> Cart => HttpContext.Session.Get<List<CartItem>>(MySetting.CART_KEY) ?? new List<CartItem>();

		[HttpPost]
		public IActionResult AddToCart(int id, int quantity = 1, string type = "Normal")
		{
			var gioHang = Cart;
			var item = gioHang.SingleOrDefault(p => p.MaHH == id);

			if (item == null)
			{
				var hanghoa = db.HangHoas.SingleOrDefault(p => p.MaHh == id);
				if (hanghoa == null)
				{
					return new JsonResult(new { success = false, message = $"Không tìm thấy hàng hóa có mã {id}" });
				}

				item = new CartItem
				{
					MaHH = hanghoa.MaHh,
					TenHH = hanghoa.TenHh,
					DonGia = hanghoa.DonGia ?? 0,
					Hinh = hanghoa.Hinh ?? string.Empty,
					SoLuong = quantity
				};
				gioHang.Add(item);
			}
			else
			{
				item.SoLuong += quantity;
			}

			HttpContext.Session.Set(MySetting.CART_KEY, gioHang);
			return new JsonResult(new { success = true, cartCount = gioHang.Sum(i => i.SoLuong) });
		}

		[HttpGet("GetCartData")]
		public IActionResult GetCartData()
		{
			var cartData = new
			{
				CardProducts = Cart.Select(p => new
				{
					p.MaHH,
					p.TenHH,
					p.SoLuong,
					p.DonGia,
					Hinh = p.Hinh?.Split(',').FirstOrDefault()?.Trim() ?? ""
				}),
				TotalQuantity = Cart.Sum(p => p.SoLuong),
				TotalAmount = Cart.Sum(p => p.SoLuong * p.DonGia)
			};
			return new JsonResult(cartData);
		}

		[HttpPost("RemoveCart")]
		public JsonResult RemoveCart(int id)
		{
			var gioHang = HttpContext.Session.Get<List<CartItem>>(MySetting.CART_KEY) ?? new List<CartItem>();
			var item = gioHang.SingleOrDefault(p => p.MaHH == id);

			if (item != null)
			{
				gioHang.Remove(item);
				HttpContext.Session.Set(MySetting.CART_KEY, gioHang);
				return new JsonResult(new { success = true });
			}
			else
			{
				return new JsonResult(new { success = false, message = "Sản phẩm không tồn tại trong giỏ hàng" });
			}
		}
	}
}
