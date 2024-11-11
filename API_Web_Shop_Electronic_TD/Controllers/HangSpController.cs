using API_Web_Shop_Electronic_TD.Data;
using API_Web_Shop_Electronic_TD.DTOs;
using API_Web_Shop_Electronic_TD.Interfaces;
using API_Web_Shop_Electronic_TD.Mappers;
using API_Web_Shop_Electronic_TD.Models;
using API_Web_Shop_Electronic_TD.Repository;
using Microsoft.AspNetCore.Mvc;

namespace API_Web_Shop_Electronic_TD.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	public class HangSpController : ControllerBase
	{
		private readonly Hshop2023Context db;
		private readonly IHangSp HangSpRepository;
		public HangSpController(Hshop2023Context db, IHangSp HangSpRepository)
		{
			this.db = db;
			this.HangSpRepository = HangSpRepository;
		}
		[HttpGet]
		public async Task<IActionResult> GetAll()
		{
			try
			{
				var hangsp = await HangSpRepository.GetAllAsync();
				var model = hangsp.Select(s => s.ToHangSpDo()).ToList();

				return Ok(model);
			}
			catch (Exception ex)
			{
				return BadRequest("Đã xảy ra lỗi khi lấy dữ liệu: " + ex.ToString());
			}
		}

		[HttpGet("{MaNcc}")]
		public async Task<IActionResult> GetById([FromRoute] string MaNcc)
		{
			try
			{
				if (!ModelState.IsValid)
					return BadRequest(ModelState);

				var model = await HangSpRepository.GetByIdAsync(MaNcc);

				if (model == null)
				{
					return NotFound(new ErrorResponse
					{
						Message = "Không tìm thấy dữ liệu",
						Errors = new List<string> { "Không tìm thấy thông tin với mã đã cho" }
					});
				}

				return Ok(model.ToHangSpDo());
			}
			catch (Exception ex)
			{
				return BadRequest("Đã xảy ra lỗi khi lấy dữ liệu: " + ex.ToString());
			}
		}

		[HttpPost]
		public async Task<IActionResult> Post(HangSpMD model)
		{
			try
			{
				if (!ModelState.IsValid)
					return BadRequest(ModelState);

				var createdModel = await HangSpRepository.CreateAsync(model);
				return Ok(createdModel);
			}
			catch (Exception ex)
			{
				return BadRequest("Đã xảy ra lỗi khi thêm mới: " + ex.ToString());
			}
		}

		[HttpPut]
		[Route("{MaNcc}")]
		public async Task<IActionResult> Update([FromRoute] string MaNcc, [FromBody] HangSpMD model)
		{
			try
			{
				if (!ModelState.IsValid)
					return BadRequest(ModelState);

				var Model = await HangSpRepository.UpdateAsync(MaNcc, model);
				if (Model == null)
				{
					return NotFound();
				}
				return Ok(Model.ToHangSpDo());
			}
			catch (Exception ex)
			{
				return BadRequest("Đã xảy ra lỗi khi cập nhật: " + ex.ToString());
			}
		}

		[HttpDelete]
		[Route("{MaNcc}")]
		public async Task<IActionResult> Delete([FromRoute] string MaNcc)
		{
			try
			{
				// Kiểm tra tính hợp lệ của ModelState
				if (!ModelState.IsValid)
					return BadRequest(ModelState);

				// Xóa bản ghi từ bảng "HangHoas"
				var deleted = await HangSpRepository.DeleteAsync(MaNcc);

				// Nếu không tìm thấy bản ghi để xóa, trả về NotFound
				if (deleted == null)
					return NotFound();

				// Trả về phản hồi NoContent nếu xóa thành công
				return NoContent();
			}
			catch (Exception ex)
			{
				return BadRequest("Đã xảy ra lỗi khi xóa: " + ex.ToString());
			}
		}

	}
}
