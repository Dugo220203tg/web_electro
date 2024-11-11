using API_Web_Shop_Electronic_TD.Data;
using API_Web_Shop_Electronic_TD.DTOs;
using API_Web_Shop_Electronic_TD.Interfaces;
using API_Web_Shop_Electronic_TD.Mappers;
using API_Web_Shop_Electronic_TD.Models;
using API_Web_Shop_Electronic_TD.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace API_Web_Shop_Electronic_TD.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	public class HangHoaController : ControllerBase
	{
		private readonly Hshop2023Context db;
		private readonly IHangHoaRepository hangHoaRepository;
		public HangHoaController(Hshop2023Context db, IHangHoaRepository hangHoaRepository)
		{
			this.db = db;
			this.hangHoaRepository = hangHoaRepository;
		}
		[HttpGet]
		public async Task<IActionResult> GetAll()
		{

			var hanghoas = await hangHoaRepository.GetAllAsync();
			var model = hanghoas.Select(s => s.ToHangHoaDo()).ToList();

			return Ok(model);
		}
		[HttpGet("{MaHH}")]
		public async Task<IActionResult> GetById([FromRoute] int MaHH)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var hangHoa = await hangHoaRepository.GetByIdAsync(MaHH);

			if (hangHoa == null)
			{
				return NotFound(new ErrorResponse
				{
					Message = "Không tìm thấy dữ liệu",
					Errors = new List<string> { "Không tìm thấy thông tin với mã đã cho" }
				});
			}

			return Ok(hangHoa.ToHangHoaDo());
		}
		[HttpPost]
		public async Task<IActionResult> Post(CreateHangHoaMD model)
		{
			try
			{
				if (!ModelState.IsValid)
					return BadRequest(ModelState);

				var createdModel = await hangHoaRepository.CreateAsync(model);
				return Ok(createdModel);
			}
			catch (Exception ex)
			{
				// Xử lý và thông báo lỗi tại đây
				return BadRequest("Đã xảy ra lỗi: " + ex.ToString());
			}
		}
		[HttpPut]
		[Route("{MaHh}")]
		public async Task<IActionResult> Update([FromRoute] int MaHh, [FromBody] UpdateHangHoaMD model)
		{
			try
			{
				if (!ModelState.IsValid)
					return BadRequest(ModelState);

				var Model = await hangHoaRepository.UpdateAsync(MaHh, model);
				if (Model == null)
				{
					return NotFound();
				}

				return Ok(Model.ToHangHoaDo());
			}
			catch (Exception ex)
			{
				// Xử lý và thông báo lỗi tại đây
				return BadRequest("Đã xảy ra lỗi: " + ex.ToString());
			}
		}

		[HttpPut]
		public IActionResult Put(HangHoaMD Model)
		{
			if (Model == null || Model.MaHH == 0)
			{
				if (Model == null)
				{
					return BadRequest();
				}
				else if (Model.MaHH == 0)
				{
					return BadRequest($"employee id {Model.MaHH} is invalid");
				}
			}
			try
			{
				var hanghoas = db.HangHoas.Find(Model.MaHH);
				if (hanghoas == null)
				{
					return BadRequest($"employee not found with id {Model.MaHH}");
				}
				hanghoas.TenHh = Model.TenHH;
				hanghoas.Hinh = Model.Hinh;
				hanghoas.MoTa = Model.MoTa;
				hanghoas.MoTaDonVi = Model.MoTaDonVi;
				hanghoas.MaLoai = Model.MaLoai;
				hanghoas.NgaySx = (DateOnly)Model.NgaySX;
				hanghoas.GiamGia = (double)Model.GiamGia;
				hanghoas.MaNcc = Model.MaNCC;
				hanghoas.DonGia = Model.DonGia;
				hanghoas.SoLuong = Model.SoLuong;


				db.SaveChanges();
				return Ok("employee is update!");

			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}
		#region ---Xoa nhung phai khong ton tai trong chi tiet hoa don---
		[HttpDelete]
		[Route("{Mahh:int}")]
		public async Task<IActionResult> Delete([FromRoute] int Mahh)
		{
			// Kiểm tra tính hợp lệ của ModelState
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			// Xóa bản ghi từ bảng "HangHoas"
			var deletedHangHoa = await hangHoaRepository.DeleteAsync(Mahh);

			// Nếu không tìm thấy bản ghi để xóa, trả về NotFound
			if (deletedHangHoa == null)
				return NotFound();

			// Cập nhật các bản ghi trong bảng "ChiTietHD" liên quan
			await UpdateRelatedChiTietHD(deletedHangHoa);

			// Trả về phản hồi NoContent nếu xóa thành công
			return NoContent();
		}

		private async Task UpdateRelatedChiTietHD(HangHoa deletedHangHoa)
		{
			// Tìm và cập nhật các bản ghi trong bảng "ChiTietHD" liên quan đến bản ghi vừa xóa
			var relatedChiTietHDs = await db.ChiTietHds.Where(x => x.MaHh == deletedHangHoa.MaHh).ToListAsync();
			foreach (var chiTietHD in relatedChiTietHDs)
			{
				// Thực hiện cập nhật trường tham chiếu tương ứng (ví dụ: MaHH)
				chiTietHD.MaHh = 0; // hoặc gán cho một giá trị khác tùy thuộc vào yêu cầu của ứng dụng
			}
			// Lưu các thay đổi vào cơ sở dữ liệu
			await db.SaveChangesAsync();
		}
		#endregion
	}
}
