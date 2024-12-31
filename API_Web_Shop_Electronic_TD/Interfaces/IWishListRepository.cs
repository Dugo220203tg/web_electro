using API_Web_Shop_Electronic_TD.Data;
using API_Web_Shop_Electronic_TD.Models;

namespace API_Web_Shop_Electronic_TD.Interfaces
{
	public interface IWishListRepository
	{
		Task<List<YeuThich>> GetWishListByAccountId(string userDetailId);
		Task RemoveFromWishListAsync(string userId, int productId);
		//Task RemoveFromWishListAsync(string userId, int productId);
		//Task RemoveFromWishListAsync(string userId, int productId);

		Task<YeuThich> AddWishListAsync(WishListRequest model);
	}
}
