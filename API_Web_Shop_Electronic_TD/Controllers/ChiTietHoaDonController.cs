using API_Web_Shop_Electronic_TD.Data;
using API_Web_Shop_Electronic_TD.Interfaces;
using API_Web_Shop_Electronic_TD.Mappers;
using API_Web_Shop_Electronic_TD.Models;
using Microsoft.AspNetCore.Mvc;
using ErrorResponse = API_Web_Shop_Electronic_TD.DTOs.ErrorResponse;

namespace API_Web_Shop_Electronic_TD.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	public class ChiTietHoaDonController : Controller
	{
		private readonly Hshop2023Context db;
		private readonly ICtHoaDon ChiTietHoaDonRepository;
		public ChiTietHoaDonController(Hshop2023Context db, ICtHoaDon ChiTietHoaDonRepository)
		{
			this.db = db;
			this.ChiTietHoaDonRepository = (ICtHoaDon?)ChiTietHoaDonRepository;
		}

		[HttpGet]
		public async Task<IActionResult> GetAll()
		{

			var ctHoaDon = await ChiTietHoaDonRepository.GetAllAsync();
			var model = ctHoaDon.Select(s => s.ToCtHoaDonDo()).ToList();

			return Ok(model);
		}
		[HttpGet]
		public async Task<IActionResult> Statistics()
		{
			var statistics = await ChiTietHoaDonRepository.GetStatisticsAsync();
			return Ok(statistics);
		}
		[HttpGet]
		public async Task<IActionResult> DataSellProduct()
		{
			var statistics = await ChiTietHoaDonRepository.GetDataSellProduct();
			return Ok(statistics);
		}
		[HttpGet("{MaCt}")]
		public async Task<IActionResult> GetById([FromRoute] int MaCt)
		{
			try
			{
				if (!ModelState.IsValid)
					return BadRequest(ModelState);

				var Model = await ChiTietHoaDonRepository.GetByIdAsync(MaCt);

				if (Model == null)
				{
					return NotFound(new ErrorResponse
					{
						Message = "Không tìm thấy dữ liệu",
						Errors = new List<string> { "Không tìm thấy thông tin với mã đã cho" }
					});
				}

				return Ok(Model.ToCtHoaDonDo());

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
		public async Task<IActionResult> Post(PostChiTietHoaDonMD model)
		{
			if (!ModelState.IsValid)
			{
				var errors = ModelState.Values
					.SelectMany(v => v.Errors)
					.Select(e => e.ErrorMessage)
					.ToList();

				return BadRequest(new { message = "Dữ liệu nhập không hợp lệ", errors });
			}
			try
			{
				if (!ModelState.IsValid)
					return BadRequest(ModelState);
				var createdModel = await ChiTietHoaDonRepository.CreateAsync(model);
				var resultModel = createdModel.ToChiTietHoaDonResult();

				return Ok(resultModel);
			}
			catch (Exception ex)
			{
				// Xử lý và thông báo lỗi tại đây
				return BadRequest("Đã xảy ra lỗi: " + ex.ToString());
			}
		}
		[HttpPut]
		[Route("{MaCt}")]
		public async Task<IActionResult> Update([FromRoute] int MaCt, [FromBody] PostChiTietHoaDonMD model)
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

				// Các kiểm tra logic khác
				var validationErrors = new List<string>();

				if (model.MaHH <= 0)
					validationErrors.Add("Mã hàng hóa không hợp lệ hoặc chưa được nhập");

				if (model.MaHD <= 0)
					validationErrors.Add("Mã hóa đơn không hợp lệ hoặc chưa được nhập");

				if (model.MaGiamGia <= 0)
					validationErrors.Add("Mã giảm giá không hợp lệ hoặc chưa được nhập");

				if (model.SoLuong <= 0)
					validationErrors.Add("Số lượng không hợp lệ hoặc chưa được nhập");

				if (model.DonGia <= 0)
					validationErrors.Add("Đơn giá không hợp lệ hoặc chưa được nhập");

				if (validationErrors.Any())
				{
					return BadRequest(new
					{
						message = "Dữ liệu không hợp lệ",
						errors = validationErrors
					});
				}

				// Thực hiện cập nhật
				var updatedModel = await ChiTietHoaDonRepository.UpdateAsync(MaCt, model);
				return Ok(updatedModel);
			}
			catch (KeyNotFoundException ex)
			{
				return NotFound(new { message = ex.Message });
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}
		[HttpDelete]
		[Route("{MaLoai:int}")]
		public async Task<IActionResult> Delete([FromRoute] int MaCt)
		{
			// Kiểm tra tính hợp lệ của ModelState
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			// Xóa bản ghi từ bảng "HangHoas"
			var Model = await ChiTietHoaDonRepository.DeleteAsync(MaCt);

			// Nếu không tìm thấy bản ghi để xóa, trả về NotFound
			if (Model == null)
				return NotFound();

			// Trả về phản hồi NoContent nếu xóa thành công
			return NoContent();
		}

	}
}
