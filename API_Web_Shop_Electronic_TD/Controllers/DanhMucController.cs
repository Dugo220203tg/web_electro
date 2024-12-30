using API_Web_Shop_Electronic_TD.Data;
using API_Web_Shop_Electronic_TD.DTOs;
using API_Web_Shop_Electronic_TD.Interfaces;
using API_Web_Shop_Electronic_TD.Mappers;
using API_Web_Shop_Electronic_TD.Models;
using API_Web_Shop_Electronic_TD.Repository;
using Microsoft.AspNetCore.Mvc;
using ErrorResponse = API_Web_Shop_Electronic_TD.DTOs.ErrorResponse;

namespace API_Web_Shop_Electronic_TD.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	public class DanhMucController : Controller
	{
		private readonly Hshop2023Context db;
		private readonly IDanhMucRepository DanhMucRepository;
		public DanhMucController(Hshop2023Context db, IDanhMucRepository DanhMucRepository)
		{
			this.db = db;
			this.DanhMucRepository = DanhMucRepository;
		}
		[HttpGet]
		public async Task<IActionResult> GetAll()
		{

			var danhmucs = await DanhMucRepository.GetAllAsync();
			var model = danhmucs.Select(s => s.ToDanhMucDto()).ToList();

			return Ok(model);
		}
		[HttpGet("{MaDanhMuc}")]
		public async Task<IActionResult> GetById([FromRoute] int MaDanhMuc)
		{
			try
			{
				if (!ModelState.IsValid)
					return BadRequest(ModelState);

				var danhmucs = await DanhMucRepository.GetByIdAsync(MaDanhMuc);

				if (danhmucs == null)
				{
					return NotFound(new ErrorResponse
					{
						Message = "Không tìm thấy dữ liệu",
						Errors = new List<string> { "Không tìm thấy thông tin với mã đã cho" }
					});
				}

				return Ok(danhmucs.ToDanhMucDto());

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
		[HttpPost]
		public async Task<IActionResult> Post(DanhMucMD model)
		{
			try
			{
				// Kiểm tra tính hợp lệ của ModelState
				if (!ModelState.IsValid)
				{
					var errors = ModelState.SelectMany(m => m.Value.Errors)
										 .Select(e => new
										 {
											 Field = e.Exception?.Data["Field"]?.ToString() ?? "",
											 Message = e.ErrorMessage
										 })
										 .ToList();
					return BadRequest(new
					{
						message = "Dữ liệu không hợp lệ",
						errors = errors
					});
				}
				var validationErrors = new List<string>();
				if (string.IsNullOrEmpty(model.TenDanhMuc))
					validationErrors.Add("Tên danh mục không hợp lệ hoặc chưa được nhập");
				if (!ModelState.IsValid)
					return BadRequest(ModelState);

				var createdModel = await DanhMucRepository.CreateAsync(model);
				return Ok(createdModel);
			}
			catch (Exception ex)
			{
				// Xử lý và thông báo lỗi tại đây
				return BadRequest("Đã xảy ra lỗi: " + ex.ToString());
			}
		}
		[HttpPut]
		[Route("{MaDanhMuc}")]
		public async Task<IActionResult> Update([FromRoute] int MaDanhMuc, [FromBody] DanhMucMD model)
		{
			try
			{
				// Kiểm tra tính hợp lệ của ModelState
				if (!ModelState.IsValid)
				{
					var errors = ModelState.SelectMany(m => m.Value.Errors)
										 .Select(e => new
										 {
											 Field = e.Exception?.Data["Field"]?.ToString() ?? "",
											 Message = e.ErrorMessage
										 })
										 .ToList();
					return BadRequest(new
					{
						message = "Dữ liệu không hợp lệ",
						errors = errors
					});
				}
				var validationErrors = new List<string>();
				if (string.IsNullOrEmpty(model.TenDanhMuc))
					validationErrors.Add("Tên danh mục không hợp lệ hoặc chưa được nhập");
				if (!ModelState.IsValid)
					return BadRequest(ModelState);

				var Model = await DanhMucRepository.UpdateAsync(MaDanhMuc, model);
				if (Model == null)
				{
					return NotFound();
				}
				return Ok(Model.ToDanhMucDto());
			}
			catch (Exception ex)
			{
				// Xử lý và thông báo lỗi tại đây
				return BadRequest("Đã xảy ra lỗi: " + ex.ToString());
			}
		}
		[HttpDelete]
		[Route("{MaDanhMuc:int}")]
		public async Task<IActionResult> Delete([FromRoute] int MaDanhMuc)
		{
			// Kiểm tra tính hợp lệ của ModelState
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			// Xóa bản ghi từ bảng "HangHoas"
			var model = await DanhMucRepository.DeleteAsync(MaDanhMuc);

			// Nếu không tìm thấy bản ghi để xóa, trả về NotFound
			if (model == null)
				return NotFound();

			// Trả về phản hồi NoContent nếu xóa thành công
			return NoContent();
		}
	}
}
