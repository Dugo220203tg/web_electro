using API_Web_Shop_Electronic_TD.Data;
using API_Web_Shop_Electronic_TD.Models;
using Microsoft.AspNetCore.Mvc;

namespace API_Web_Shop_Electronic_TD.Interfaces
{
	public interface ICheckOutRepository
	{
		Task<int> ProcessPaymentAsync(CheckOutMD request, string paymentMethod);
		Task UpdatePaymentStatus(int orderId, bool isSuccessful);
	}
}
