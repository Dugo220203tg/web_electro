using API_Web_Shop_Electronic_TD.Interfaces;
using API_Web_Shop_Electronic_TD.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using System.Security.Claims;
using static API_Web_Shop_Electronic_TD.Helpers.MyUtil;

namespace API_Web_Shop_Electronic_TD.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class CartController : ControllerBase
	{
		private readonly ICartRepository cartRepository;

		public CartController(ICartRepository cartRepository)
		{
			this.cartRepository = cartRepository;
		}
		[HttpPost("AddToCart")]
		public async Task<IActionResult> AddToCartAsync(CartResponse model)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}

				var result = await cartRepository.AddToCartAsync(model);
				return Ok(result);
			}
			catch (ArgumentException ex)
			{
				return BadRequest(new { message = ex.Message });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = "Đã xảy ra lỗi khi thêm vào danh sách" });
			}
		}

		[HttpGet("GetCartData")]
		public async Task<IActionResult> GetCartDataAsync()
		{
			var userDetailId = User.FindFirstValue(ClaimTypes.NameIdentifier);

			if (string.IsNullOrEmpty(userDetailId))
			{
				return Unauthorized("User is not logged in or invalid user detail ID.");
			}

			// Fetch the wishlist by account ID
			var carts = await cartRepository.GetCartDataAsync(userDetailId);

			if (carts == null || !carts.Any())
			{
				return Ok(new List<CartRequests>()); // Trả về danh sách trống
			}

			var cartRequest = carts.Select(item => new CartRequests
			{
				id = item.Id,
				MaKh = item.UserId,
				MaHh = item.ProductId,
				TenHH = item.Product?.TenHh ?? "Unknown Product",
				DonGia = item.Product?.DonGia ?? 0.0,
				Hinh = ImageHelper.GetFirstImage(item.Product?.Hinh),
				TenNcc = item.Product?.MaNccNavigation?.TenCongTy ?? "Unknown Supplier",
				Quantity = (int)item.Quantity,
			}).ToList();

			return Ok(cartRequest);
		}



		[HttpPost("Remove/{userId}/{productId}")]
		public async Task<IActionResult> RemoveCart(string userId, int productId)
		{
			try
			{
				Console.WriteLine($"Attempting to remove cart item. UserId: {userId}, ProductId: {productId}");
				await cartRepository.RemoveFromCartAsync(userId, productId);
				return Ok(new { message = "Đã xóa sản phẩm khỏi danh sách yêu thích" });
			}
			catch (ArgumentException ex)
			{
				return BadRequest(new { message = ex.Message });
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Unexpected error: {ex.Message}");
				return StatusCode(500, new { message = "Đã xảy ra lỗi khi xóa khỏi danh sách yêu thích" });
			}
		}

		[HttpPut("increase-quantity/{userId}/{productId}")]
		public async Task<IActionResult> IncreaseQuantityCartItem(string userId, int productId)
		{
			try
			{
				await cartRepository.IncreaseQuantity(userId, productId);
				return Ok(new { message = "Thêm thành công" });
			}
			catch (ArgumentException ex)
			{
				return BadRequest(new { message = ex.Message });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = "Đã xảy ra lỗi" });
			}
		}

		[HttpPut("minus-quantity/{userId}/{productId}")]
		public async Task<IActionResult> MinusQuantityCartItem(string userId, int productId)
		{
			try
			{
				await cartRepository.MinusQuantity(userId, productId);
				return Ok(new { message = "Xóa thành công" });
			}
			catch (ArgumentException ex)
			{
				return BadRequest(new { message = ex.Message });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = "Đã xảy ra lỗi" });
			}
		}

	}
}
