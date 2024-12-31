using API_Web_Shop_Electronic_TD.Data;
using API_Web_Shop_Electronic_TD.Interfaces;
using API_Web_Shop_Electronic_TD.Mappers;
using Microsoft.AspNetCore.Mvc;
using API_Web_Shop_Electronic_TD.Models;
using ErrorResponse = API_Web_Shop_Electronic_TD.DTOs.ErrorResponse;
using System.Security.Claims;

namespace API_Web_Shop_Electronic_TD.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	public class CouponController : Controller
	{
		private readonly Hshop2023Context db;
		private readonly ICouponRepository CouponRepository;
		public CouponController(Hshop2023Context db, ICouponRepository CouponRepository)
		{
			this.db = db;
			this.CouponRepository = CouponRepository;
		}
		[HttpGet]
		public async Task<IActionResult> GetAll()
		{

			var coupons = await CouponRepository.GetAllAsync();
			var model = coupons.Select(s => s.ToCouponDo()).ToList();

			return Ok(model);
		}
		[HttpGet("{id}")]
		public async Task<IActionResult> GetById([FromRoute] int id)
		{
			try
			{
				if (!ModelState.IsValid)
					return BadRequest(ModelState);

				var coupons = await CouponRepository.GetByIdAsync(id);

				if (coupons == null)
				{
					return NotFound(new ErrorResponse
					{
						Message = "Không tìm thấy dữ liệu",
						Errors = new List<string> { "Không tìm thấy thông tin với mã đã cho" }
					});
				}

				return Ok(coupons.ToCouponDo());

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
		public async Task<IActionResult> Post(CouponsMD model)
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
				if (string.IsNullOrEmpty(model.Name))
					validationErrors.Add("Tên danh mục không hợp lệ hoặc chưa được nhập");
				if (model.price == null)
					validationErrors.Add("Đơn giá không hợp lệ hoặc chưa được nhập");
				if (model.Status == null)
					validationErrors.Add("Trạng thái coupon không hợp lệ hoặc chưa được nhập");
				if (string.IsNullOrEmpty(model.Description))
					validationErrors.Add("Thông tin coupon không hợp lệ hoặc chưa được nhập");
				if (!ModelState.IsValid)
					return BadRequest(ModelState);

				var createdModel = await CouponRepository.CreateAsync(model);
				return Ok(createdModel);
			}
			catch (Exception ex)
			{
				// Xử lý và thông báo lỗi tại đây
				return BadRequest("Đã xảy ra lỗi: " + ex.ToString());
			}
		}
		[HttpPut]
		[Route("{id}")]
		public async Task<IActionResult> Update([FromRoute] int id, [FromBody] CouponsMD model)
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
				if (string.IsNullOrEmpty(model.Name))
					validationErrors.Add("Tên danh mục không hợp lệ hoặc chưa được nhập");
				if (model.price == null)
					validationErrors.Add("Đơn giá không hợp lệ hoặc chưa được nhập");
				if (model.Status == null)
					validationErrors.Add("Trạng thái coupon không hợp lệ hoặc chưa được nhập");
				if (string.IsNullOrEmpty(model.Description))
					validationErrors.Add("Thông tin coupon không hợp lệ hoặc chưa được nhập");
				if (!ModelState.IsValid)
					return BadRequest(ModelState);

				var Model = await CouponRepository.UpdateAsync(model, id);
				if (Model == null)
				{
					return NotFound();
				}
				return Ok(Model.ToCouponDo());
			}
			catch (Exception ex)
			{
				// Xử lý và thông báo lỗi tại đây
				return BadRequest("Đã xảy ra lỗi: " + ex.ToString());
			}
		}
		[HttpDelete]
		[Route("{id:int}")]
		public async Task<IActionResult> Delete([FromRoute] int id)
		{
			// Kiểm tra tính hợp lệ của ModelState
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			// Xóa bản ghi từ bảng "HangHoas"
			var model = await CouponRepository.DeleteAsync(id);

			// Nếu không tìm thấy bản ghi để xóa, trả về NotFound
			if (model == null)
				return NotFound();

			// Trả về phản hồi NoContent nếu xóa thành công
			return NoContent();
		}
		[HttpGet("{couponCode}")]
		public async Task<IActionResult> UseCoupon(string couponCode)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			// Xóa bản ghi từ bảng "HangHoas"
			var model = await CouponRepository.UseCoupone(userId, couponCode);

			// Nếu không tìm thấy bản ghi để xóa, trả về NotFound
			if (model == null)
				return NotFound();
			if (model.Quantity < 1)
				return StatusCode(400, new { message = " số lượng mã giảm giá này của bạn đã hết " });
			couponReport coupon = new couponReport
			{
				price = (int)model.Coupon.Price,
				Name = model.Coupon.Name,
				DateEnd = (DateTime)model.Coupon.DateEnd,
				status = (int)model.Coupon.Status,
			};	
			// Trả về phản hồi NoContent nếu xóa thành công
			return Ok(coupon);
		}
	}
}
