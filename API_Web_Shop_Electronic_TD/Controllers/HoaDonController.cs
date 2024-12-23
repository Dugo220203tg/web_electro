using API_Web_Shop_Electronic_TD.Data;
using API_Web_Shop_Electronic_TD.DTOs;
using API_Web_Shop_Electronic_TD.Interfaces;
using API_Web_Shop_Electronic_TD.Mappers;
using API_Web_Shop_Electronic_TD.Models;
using API_Web_Shop_Electronic_TD.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ErrorResponse = API_Web_Shop_Electronic_TD.DTOs.ErrorResponse;

namespace API_Web_Shop_Electronic_TD.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	public class HoaDonController : Controller
	{
		private readonly Hshop2023Context db;
		private readonly IHoaDon HoaDonRespository;
		public HoaDonController(Hshop2023Context db, IHoaDon HoaDonRespository)
		{
			this.db = db;
			this.HoaDonRespository = HoaDonRespository;
		}
		[HttpGet]
		public async Task<IActionResult> GetAll()
		{

			var HoaDon = await HoaDonRespository.GetAllAsync();
			var model = HoaDon.Select(s => s.ToHoaDonDo()).ToList();

			return Ok(model);
		}
		[HttpGet("{MaHd}")]
		public async Task<IActionResult> GetById([FromRoute] int MaHd)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var HoaDons = await HoaDonRespository.GetByIdAsync(MaHd);

			if (HoaDons == null)
			{
				return NotFound(new ErrorResponse
				{
					Message = "Không tìm thấy dữ liệu",
					Errors = new List<string> { "Không tìm thấy thông tin với mã đã cho" }
				});
			}

			return Ok(HoaDons.ToHoaDonDo());
		}
		[HttpPost]
		public async Task<IActionResult> Post(HoaDonMD model)
		{
			{
				try
				{
					if (!ModelState.IsValid)
						return BadRequest(ModelState);

					var createdModel = await HoaDonRespository.CreateAsync(model);
					return Ok(createdModel);
				}
				catch (Exception ex)
				{
					return BadRequest("Đã xảy ra lỗi khi thêm mới: " + ex.ToString());
				}
			}
		}
		[HttpPut]
		[Route("{MaHd}")]
		public async Task<IActionResult> Update([FromRoute] int MaHd, [FromBody] HoaDonMD model)
		{
			try
			{
				if (!ModelState.IsValid)
					return BadRequest(ModelState);

				var Model = await HoaDonRespository.UpdateAsync(MaHd, model);
				if (Model == null)
				{
					return NotFound();
				}
				return Ok(Model.ToHoaDonDo());
			}
			catch (Exception ex)
			{
				return BadRequest("Đã xảy ra lỗi: " + ex.ToString());
			}
		}
		[HttpPost]
		public IActionResult UpdateTrangThai([FromBody] HoaDonUpdateStatusModel model)
		{
			try
			{
				var hoaDon = db.HoaDons.Find(model.MaHD);
				if (hoaDon != null)
				{
					hoaDon.MaTrangThai = model.MaTrangThai;
					var exitTrangThai = db.TrangThais.FirstOrDefaultAsync(h => h.MaTrangThai == hoaDon.MaTrangThai);
					if (exitTrangThai == null)
					{
						throw new ArgumentException($"Trạng thái hóa đơn với mã {hoaDon.MaTrangThai} không tồn tại trong hệ thống");
					}
					db.SaveChanges();
					return Ok(new { message = "Trạng thái đã được cập nhật." });
				}
				return NotFound(new { message = "Đơn hàng không tìm thấy." });
			}
			catch (Exception ex)
			{
				return StatusCode(500, ex.Message);
			}
		}
		[HttpDelete]
		[Route("{MaHd:int}")]
		public async Task<IActionResult> Delete([FromRoute] int MaHd)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var HoaDon = await HoaDonRespository.DeleteAsync(MaHd);

			if (HoaDon == null)
				return NotFound();

			return NoContent();
		}
	}
}
