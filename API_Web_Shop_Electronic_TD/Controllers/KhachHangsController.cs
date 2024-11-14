using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using API_Web_Shop_Electronic_TD.Data;
using API_Web_Shop_Electronic_TD.Interfaces;
using API_Web_Shop_Electronic_TD.Repository;
using API_Web_Shop_Electronic_TD.Mappers;
using API_Web_Shop_Electronic_TD.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using API_Web_Shop_Electronic_TD.DTOs;

namespace API_Web_Shop_Electronic_TD.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	public class KhachHangsController : Controller
	{
		private readonly IKhachHangRepository KhachHangsRepository;
		private readonly Hshop2023Context db;


		public KhachHangsController(IKhachHangRepository KhachHangsRepository, Hshop2023Context db)
		{
			this.KhachHangsRepository = KhachHangsRepository;
			this.db = db;
		}
		[HttpPost("login")]
		public async Task<IActionResult> Login([FromBody] LoginDTO model)
		{
			if (string.IsNullOrWhiteSpace(model.UserName))
			{
				return BadRequest(new { message = "UserName không được để trống" });
			}

			if (string.IsNullOrWhiteSpace(model.Password))
			{
				return BadRequest(new { message = "Mật khẩu không được để trống" });
			}

			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var khachHang = await KhachHangsRepository.GetByEmailAsync(model.UserName);

			if (khachHang == null || !KhachHangsRepository.VerifyPassword(model.Password, khachHang.MatKhau, khachHang.RandomKey))
			{
				return Unauthorized(new { message = "UserName hoặc mật khẩu không đúng" });
			}

			return Ok(new { message = "Đăng nhập thành công", khachHang = khachHang });
		}
		[HttpPost("UpdatePassword")]
		public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordDTO model)
		{
			if (string.IsNullOrWhiteSpace(model.CodeSend) || string.IsNullOrWhiteSpace(model.NewPassword))
			{
				return BadRequest(new { message = "Mật khẩu hiện tại và mật khẩu mới không được để trống" });
			}

			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var khachHang = await KhachHangsRepository.UpdatePasswordAsync(model.CodeSend, model.NewPassword, model.Email);
			if (khachHang == null)
			{
				return Unauthorized(new { message = "Email hoặc mã xác nhận không đúng" });
			}

			return Ok(new { message = "Cập nhật mật khẩu thành công" });
		}

		[HttpGet]
		public async Task<IActionResult> GetAll()
		{
			try
			{
				var khachHangsMD = await KhachHangsRepository.GetAllAsync();
				var khachhangs = khachHangsMD.Where(s => s.VaiTro == 0);
				var model = khachhangs.Select(k => k.ToKhachHangDo()).ToList();

				return Ok(model);
			}
			catch (Exception ex)
			{
				// Xử lý và thông báo lỗi
				return BadRequest("Đã xảy ra lỗi: " + ex.ToString());
			}
		}

		[HttpGet("{MaKh}")]
		public async Task<IActionResult> GetById([FromRoute] string MaKh)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var khachhang = await KhachHangsRepository.GetByIdAsync(MaKh);

			if (khachhang == null)
			{
				return NotFound(new ErrorResponse
				{
					Message = "Không tìm thấy dữ liệu",
					Errors = new List<string> { "Không tìm thấy thông tin với mã đã cho" }
				});
			}

			return Ok(khachhang.ToKhachHangDo());
		}
		[HttpPost]
		public async Task<IActionResult> Post(AdminDkMD model)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					var errors = ModelState.Values.SelectMany(v => v.Errors)
												   .Select(e => e.ErrorMessage)
												   .ToList();
					return BadRequest(new ErrorResponse
					{
						Message = "Dữ liệu không hợp lệ",
						Errors = errors
					});
				}

				var createdModel = await KhachHangsRepository.CreateAsync(model);
				return Ok(createdModel);
			}
			catch (ArgumentException ex)
			{
				return BadRequest(new ErrorResponse
				{
					Message = "Dữ liệu không hợp lệ",
					Errors = new List<string> { ex.Message }
				});
			}
			catch (Exception ex)
			{
				return BadRequest(new ErrorResponse
				{
					Message = "Đã xảy ra lỗi",
					Errors = new List<string> { ex.Message }
				});
			}
		}

		[HttpPut]
		[Route("{MaKh}")]
		public async Task<IActionResult> Update([FromRoute] string MaKh, [FromBody] UpdateKH model)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			try
			{
				var updatedKhachHang = await KhachHangsRepository.UpdateAsync(MaKh, model);

				// Chỉ trả về các trường đã được cập nhật
				var result = new
				{
					Email = updatedKhachHang.Email,
					NgaySinh = updatedKhachHang.NgaySinh,
					DiaChi = updatedKhachHang.DiaChi,
					DienThoai = updatedKhachHang.DienThoai
				};

				return Ok(new { message = "Cập nhật thông tin thành công", khachHang = result });
			}
			catch (KeyNotFoundException ex)
			{
				return NotFound(new { message = ex.Message });
			}
			catch (Exception ex)
			{
				// Log the exception here
				return StatusCode(500, new { message = "Đã xảy ra lỗi trong quá trình xử lý", error = ex.Message });
			}
		}

		[HttpPost]
		public IActionResult UpdateTrangThai([FromBody] UpdateHieuLucVMD model)
		{
			try
			{
				var khachhangs = db.KhachHangs.Find(model.UserName);
				if (khachhangs != null)
				{
					if (model.HieuLuc == false)
					{
						khachhangs.HieuLuc = false;
						db.SaveChanges();
						return Ok(new { message = "Đã khóa sử dụng tài khoản !" });
					}
					else if (model.HieuLuc == true)
					{
						khachhangs.HieuLuc = true;
						db.SaveChanges();
						return Ok(new { message = "Đã mở khóa tài khoản !" });
					}
				}
				return NotFound(new { message = "Lỗi chưa xác định." });
			}
			catch (Exception ex)
			{
				return StatusCode(500, ex.Message);
			}
		}

		[HttpDelete]
		[Route("{MaKh}")]
		public async Task<IActionResult> Delete([FromRoute] string MaKh)
		{
			// Kiểm tra tính hợp lệ của ModelState
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			// Xóa bản ghi từ bảng "HangHoas"
			var deleteKH = await KhachHangsRepository.DeleteAsync(MaKh);

			// Nếu không tìm thấy bản ghi để xóa, trả về NotFound
			if (deleteKH == null)
				return NotFound();

			// Trả về phản hồi NoContent nếu xóa thành công
			return NoContent();
		}

	}
}
