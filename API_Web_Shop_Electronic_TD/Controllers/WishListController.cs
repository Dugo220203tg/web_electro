using API_Web_Shop_Electronic_TD.Interfaces;
using API_Web_Shop_Electronic_TD.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API_Web_Shop_Electronic_TD.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	public class WishListController : Controller
	{
		private readonly IWishListRepository wishListRepository;
		private readonly IConfiguration configuration1;

		public WishListController(IWishListRepository wishListRepository, IConfiguration configuration)
		{
			this.wishListRepository = wishListRepository;
			configuration1 = configuration;
		}
		#region ----- WISH LIST -----
		[Authorize]
		[HttpGet]
		public async Task<IActionResult> GetWishListByAccountId()
		{
			// Retrieve the user detail ID from the claims
			var userDetailId = User.FindFirstValue(ClaimTypes.NameIdentifier);

			if (string.IsNullOrEmpty(userDetailId))
			{
				return Unauthorized("User is not logged in or invalid user detail ID.");
			}

			// Fetch the wishlist by account ID
			var wishlist = await wishListRepository.GetWishListByAccountId(userDetailId);

			if (wishlist == null || !wishlist.Any())
			{
				return NotFound("No wishlist items found for this account.");
			}

			// Map the wishlist items to a list of WishListReport
			var wishlistReport = wishlist.Select(item => new WishListReport
			{
				maHH = (int)item.MaHh,
				maYt = item.MaYt,
				tenHH = item.MaHhNavigation.TenHh,
				donGia = (double)item.MaHhNavigation.DonGia,
				hinh = item.MaHhNavigation.Hinh,
				tenNCC = item.MaHhNavigation.MaNccNavigation.TenCongTy,
			}).ToList();

			return Ok(wishlistReport);
		}
		[Authorize]
		[HttpPost]
		public async Task<IActionResult> AddToWishList([FromBody] WishListRequest model)
		{
			if (model == null)
			{
				return BadRequest("Invalid request data.");
			}

			try
			{
				var addedItem = await wishListRepository.AddWishListAsync(model);
				return Ok(new { message = "Sản phẩm đã được thêm vào danh sách yêu thích.", data = addedItem });
			}
			catch (ArgumentException ex)
			{
				return Conflict(new { message = ex.Message });
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Internal server error: {ex.Message}");
			}
		}

		[HttpDelete]
		[Route("Remove/{userId}/{productId}")]
		public async Task<IActionResult> RemoveFromWishList(string userId, int productId)
		{
			try
			{
				await wishListRepository.RemoveFromWishListAsync(userId, productId);
				return Ok(new { message = "Đã xóa sản phẩm khỏi danh sách yêu thích" });
			}
			catch (ArgumentException ex)
			{
				return BadRequest(new { message = ex.Message });
			}
			catch (Exception ex)
			{
				// Log error here
				return StatusCode(500, new { message = "Đã xảy ra lỗi khi xóa khỏi danh sách yêu thích" });
			}
		}
		#endregion
	}
}
