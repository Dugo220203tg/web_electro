using API_Web_Shop_Electronic_TD.Data;
using API_Web_Shop_Electronic_TD.Models;
using Microsoft.AspNetCore.Mvc;

namespace API_Web_Shop_Electronic_TD.Interfaces
{
	public interface ICheckOutRepository
	{
		//Task<PayHistory> InitiateCheckoutAsync(CheckOutMD model);
		Task ProcessVnPayPaymentAsync(CheckOutMD paymentResponse);
	}
}
