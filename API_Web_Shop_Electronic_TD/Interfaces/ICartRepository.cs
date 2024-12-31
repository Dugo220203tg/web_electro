
using API_Web_Shop_Electronic_TD.Data;
using API_Web_Shop_Electronic_TD.Models;

namespace API_Web_Shop_Electronic_TD.Interfaces
{
	public interface ICartRepository
	{
		Task<Cart> AddToCartAsync(CartResponse model);
		Task<List<Cart>> GetCartDataAsync(string userId);
		Task RemoveFromCartAsync(string userId, int productId);
		Task IncreaseQuantity(string userId, int productId);
		Task MinusQuantity(string userId, int productId);
	}
}
