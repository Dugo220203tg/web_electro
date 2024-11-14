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
	public class DanhGiaSpController : Controller
	{

		private readonly Hshop2023Context db;
		private readonly IDanhGiaSp DanhGiaSpRepository;
		public DanhGiaSpController(Hshop2023Context db, IDanhGiaSp DanhGiaSpRepository)
		{
			this.db = db;
			this.DanhGiaSpRepository = DanhGiaSpRepository;
		}

		[HttpGet]
		public async Task<IActionResult> GetAll()
		{

			var danhgias = await DanhGiaSpRepository.GetAllAsync();
			var model = danhgias.Select(s => s.ToDanhGiaDo()).ToList();

			return Ok(model);
		}
		[HttpGet("{MaDg}")]
		public async Task<IActionResult> GetById([FromRoute] int MaDg)
		{
			try
			{
				if (!ModelState.IsValid)
					return BadRequest(ModelState);

				var danhgias = await DanhGiaSpRepository.GetByIdAsync(MaDg);

				if (danhgias == null)
				{
					return NotFound(new ErrorResponse
					{
						Message = "Không tìm thấy dữ liệu",
						Errors = new List<string> { "Không tìm thấy thông tin với mã đã cho" }
					});
				}

				return Ok(danhgias.ToDanhGiaDo());

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
		public async Task<IActionResult> Post(CreateDanhGiaSpMD model)
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

				if (!ModelState.IsValid)
					return BadRequest(ModelState);

				if (model.MaHH <= 0)
					validationErrors.Add("Mã hàng hóa không hợp lệ hoặc chưa được nhập");
				if (model.Sao < 0)
					validationErrors.Add("Định dạng không hợp lệ ");
				if (string.IsNullOrEmpty(model.MaKH))
					validationErrors.Add("Mã Khách hàng không hợp lệ hoặc chưa được nhập");
				if (string.IsNullOrEmpty(model.NoiDung))
					validationErrors.Add("Nội dung không hợp lệ hoặc chưa được nhập");
				if (model.TrangThai < 0)
					validationErrors.Add("Mã trạng thái không hợp lệ hoặc chưa được nhập");
				var createdModel = await DanhGiaSpRepository.CreateAsync(model);
				var resultModel = createdModel.ToCreateDanhGiaDo();

				return Ok(createdModel);
			}
			catch (Exception ex)
			{
				// Xử lý và thông báo lỗi tại đây
				return BadRequest("Đã xảy ra lỗi: " + ex.ToString());
			}
		}
		[HttpPut]
		[Route("{MaDg}")]
		public async Task<IActionResult> Update([FromRoute] int MaDg, [FromBody] CreateDanhGiaSpMD model)
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

				if (!ModelState.IsValid)
					return BadRequest(ModelState);

				if (model.MaHH <= 0)
					validationErrors.Add("Mã hàng hóa không hợp lệ hoặc chưa được nhập");
				if (model.Sao < 0)
					validationErrors.Add("Định dạng không hợp lệ ");
				if (string.IsNullOrEmpty(model.MaKH))
					validationErrors.Add("Mã Khách hàng không hợp lệ hoặc chưa được nhập");
				if (string.IsNullOrEmpty(model.NoiDung))
					validationErrors.Add("Nội dung không hợp lệ hoặc chưa được nhập");
				if (model.TrangThai < 0)
					validationErrors.Add("Mã trạng thái không hợp lệ hoặc chưa được nhập");

				if (!ModelState.IsValid)
					return BadRequest(ModelState);

				var Model = await DanhGiaSpRepository.UpdateAsync(MaDg, model);
				if (Model == null)
				{
					return NotFound();
				}
				return Ok(Model.ToCreateDanhGiaDo());
			}
			catch (Exception ex)
			{
				// Xử lý và thông báo lỗi tại đây
				return BadRequest("Đã xảy ra lỗi: " + ex.ToString());
			}
		}
		[HttpPost]
		public IActionResult UpdateTrangThai([FromBody] UpdateTrangThaiDanhGiaSpVM model)
		{
			try
			{
				var danhGia = db.DanhGiaSps.Find(model.MaDg);
				if (danhGia != null)
				{
					// Update the status based on the provided value
					danhGia.TrangThai = model.TrangThai;
					db.SaveChanges();

					var message = model.TrangThai == 1 ? "Đã hiển thị đánh giá" : "Đã ẩn đánh giá";
					return Ok(new { message });
				}
				return NotFound(new { message = "Lỗi chưa xác định." });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = ex.Message });
			}
		}
		[HttpDelete]
		[Route("{MaDg:int}")]
		public async Task<IActionResult> Delete([FromRoute] int MaDg)
		{
			// Kiểm tra tính hợp lệ của ModelState
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			// Xóa bản ghi từ bảng "HangHoas"
			var Model = await DanhGiaSpRepository.DeleteAsync(MaDg);

			// Nếu không tìm thấy bản ghi để xóa, trả về NotFound
			if (Model == null)
				return NotFound();

			// Trả về phản hồi NoContent nếu xóa thành công
			return Ok(new { message = "Xóa sản phẩm thành công." });
		}
	}
}
