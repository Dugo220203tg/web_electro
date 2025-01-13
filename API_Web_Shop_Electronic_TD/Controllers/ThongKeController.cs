using API_Web_Shop_Electronic_TD.Interfaces;
using API_Web_Shop_Electronic_TD.Models;
using API_Web_Shop_Electronic_TD.Repository;
using Microsoft.AspNetCore.Mvc;

namespace API_Web_Shop_Electronic_TD.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	public class ThongKeController : Controller
	{
		private readonly IThongKeRepository thongKeRepository;

		public ThongKeController(IThongKeRepository thongKeRepository)
		{
			this.thongKeRepository = thongKeRepository;
		}
		[HttpGet]
		public async Task<IActionResult> GetTopSellProduct()
		{
			var product = await thongKeRepository.GetTopSellProduct();
			return Ok(product);
		}

		[HttpGet]
		public async Task<IActionResult> GetTopFavoriteProduct()
		{
			var product = await thongKeRepository.GetTopFavoriteProduct();
			return Ok(product);
		}
		[HttpGet]
		public async Task<IActionResult> Statistics()
		{
			var statistics = await thongKeRepository.GetStatisticsAsync();
			return Ok(statistics);
		}
		[HttpGet]
		public async Task<IActionResult> DataSellProduct()
		{
			var statistics = await thongKeRepository.GetDataSellProduct();
			return Ok(statistics);
		}
	}
}
