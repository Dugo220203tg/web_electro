using API_Web_Shop_Electronic_TD.Data;
using API_Web_Shop_Electronic_TD.Models;

namespace API_Web_Shop_Electronic_TD.Interfaces
{
	public interface ICheckOutRepository
	{
		Task<PayHistory> CheckOutAsync(CheckOutMD model);
	}
}
