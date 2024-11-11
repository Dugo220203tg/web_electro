using API_Web_Shop_Electronic_TD.Data;
using API_Web_Shop_Electronic_TD.Interfaces;
using API_Web_Shop_Electronic_TD.Mappers;
using Microsoft.AspNetCore.Mvc;

namespace API_Web_Shop_Electronic_TD.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	public class TrangThaiHdController : Controller
	{
		private readonly Hshop2023Context db;
		private readonly ITrangThaiHd TrangThaiHdRepository;
		public TrangThaiHdController(Hshop2023Context db, ITrangThaiHd TrangThaiHdRepository)
		{
			this.db = db;
			this.TrangThaiHdRepository = TrangThaiHdRepository;
		}

		[HttpGet]
		public async Task<IActionResult> GetAll()
		{

			var trangthais = await TrangThaiHdRepository.GetAllAsync();
			var model = trangthais.Select(s => s.ToTrangThaiDo()).ToList();

			return Ok(model);
		}
	}
}
