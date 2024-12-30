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
	public class LoaiSpController : Controller
	{

		private readonly Hshop2023Context db;
		private readonly ILoaiSpRepository LoaiSpRepository;
		public LoaiSpController(Hshop2023Context db, ILoaiSpRepository LoaiSpRepository)
		{
			this.db = db;
			this.LoaiSpRepository = LoaiSpRepository;
		}

		[HttpGet]
		public async Task<IActionResult> GetAll()
		{

			var loais = await LoaiSpRepository.GetAllAsync();
			var model = loais.Select(s => s.ToLoaiDo()).ToList();

			return Ok(model);
		}
		[HttpGet("{MaLoai}")]
		public async Task<IActionResult> GetById([FromRoute] int MaLoai)
		{
			try
			{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var loais = await LoaiSpRepository.GetByIdAsync(MaLoai);

				if (loais == null)
				{
					return NotFound(new ErrorResponse
					{
						Message = "Không tìm thấy dữ liệu",
						Errors = new List<string> { "Không tìm thấy thông tin với mã đã cho" }
					});
				}

				return Ok(loais.ToLoaiDo());

			}

			catch (Exception ex)
			{
				return BadRequest(new ErrorResponse
				{
					Message = "Đã xảy ra lỗi",
					Errors = new List<string> { "Lỗi không xác định: " + ex.Message }
				});
			}
		}
		[HttpGet("{TenLoai}")]
		public async Task<IActionResult> GetByName([FromRoute] string TenLoai)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			try
			{
				var loais = await LoaiSpRepository.GetAllAsync();

				// Lọc danh sách các loại sản phẩm dựa trên tên loại được cung cấp
				if (!string.IsNullOrEmpty(TenLoai))
				{
					loais = loais.Where(l => l.TenLoai.Contains(TenLoai)).ToList();
				}

				// Kiểm tra xem danh sách lọc có rỗng không
				if (loais.Count == 0)
				{
					return NotFound(new ErrorResponse
					{
						Message = "Không tìm thấy dữ liệu",
						Errors = new List<string> { "Không tìm thấy thông tin với tên Loại đã cho" }
					});
				}

				// Trả về kết quả tìm kiếm
				return Ok(loais);
			}
			catch (Exception ex)
			{
				return BadRequest("Đã xảy ra lỗi: " + ex.ToString());
			}
		}

		[HttpPost]
		public async Task<IActionResult> Post(CreateLoaiSpMD model)
		{
			try
			{
				if (!ModelState.IsValid)
					return BadRequest(ModelState);

				var createdModel = await LoaiSpRepository.CreateAsync(model);
				return Ok(createdModel);
			}
			catch (Exception ex)
			{
				// Xử lý và thông báo lỗi tại đây
				return BadRequest("Đã xảy ra lỗi: " + ex.ToString());
			}
		}
		[HttpPut]
		[Route("{MaLoai}")]
		public async Task<IActionResult> Update([FromRoute] int MaLoai, [FromBody] CreateLoaiSpMD model)
		{
			try
			{
				if (!ModelState.IsValid)
					return BadRequest(ModelState);

				var Model = await LoaiSpRepository.UpdateAsync(MaLoai, model);
				if (Model == null)
				{
					return NotFound();
				}
				return Ok(Model.ToLoaiDo());
			}
			catch (Exception ex)
			{
				// Xử lý và thông báo lỗi tại đây
				return BadRequest("Đã xảy ra lỗi: " + ex.ToString());
			}
		}
		[HttpDelete]
		[Route("{MaLoai:int}")]
		public async Task<IActionResult> Delete([FromRoute] int MaLoai)
		{
			// Kiểm tra tính hợp lệ của ModelState
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			// Xóa bản ghi từ bảng "HangHoas"
			var loais = await LoaiSpRepository.DeleteAsync(MaLoai);

			// Nếu không tìm thấy bản ghi để xóa, trả về NotFound
			if (loais == null)
				return NotFound();

			// Trả về phản hồi NoContent nếu xóa thành công
			return NoContent();
		}
	}
}
