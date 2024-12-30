using API_Web_Shop_Electronic_TD.Models;

namespace API_Web_Shop_Electronic_TD.Interfaces
{
	public interface IThongKeRepository
	{
		Task<List<DataSellProductVMD>> GetTopSellProduct();
		Task<List<DataSellProductVMD>> GetTopFavoriteProduct();
	}
}
