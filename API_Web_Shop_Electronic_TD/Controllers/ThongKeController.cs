using API_Web_Shop_Electronic_TD.Interfaces;
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
	}
}
