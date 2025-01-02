using API_Web_Shop_Electronic_TD.Data;
using API_Web_Shop_Electronic_TD.Interfaces;
using API_Web_Shop_Electronic_TD.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Linq;
public class CheckOutRepository : ICheckOutRepository
{
	private readonly Hshop2023Context _db;
	private readonly ILogger<CheckOutRepository> _logger;

	public CheckOutRepository(Hshop2023Context db, ILogger<CheckOutRepository> logger)
	{
		_db = db;
		_logger = logger;
	}

	public async Task<PayHistory> CheckOutAsync(CheckOutMD model)
	{
		using var transaction = await _db.Database.BeginTransactionAsync();
		try
		{
			// 1. Lưu thông tin thanh toán
			var payHistory = new PayHistory
			{
				FullName = model.FullName,
				OrderInfo = model.OrderInfo,
				Amount = model.Amount,
				PayMethod = model.PayMethod,
				CouponCode = model.CouponCode,
				CreateDate = DateTime.UtcNow
			};
			await _db.PayHistorys.AddAsync(payHistory);
			await _db.SaveChangesAsync();

			// 2. Lưu hóa đơn
			var hoaDon = new HoaDon
			{
				MaKh = model.MaKh,
				NgayDat = DateTime.UtcNow,
				NgayGiao = DateTime.UtcNow,
				NgayCan = DateTime.UtcNow,
				HoTen = model.FullName,
				DiaChi = model.DiaChi,
				CachVanChuyen = model.CachVanChuyen,
				PhiVanChuyen = model.ShippingFee,
				CachThanhToan = model.PayMethod,
				MaTrangThai = 0,
				MaNv = null,
				GhiChu = model.GhiChu,
				DienThoai = model.DienThoai,
				PayId = payHistory.Id
			};
			await _db.HoaDons.AddAsync(hoaDon);
			await _db.SaveChangesAsync();

			// 3. Lưu chi tiết hóa đơn
			foreach (var chiTiet in model.ChiTietHoaDons)
			{
				var product = await _db.HangHoas.FindAsync(chiTiet.MaHh);
				if (product == null)
				{
					throw new ValidationException($"Product with ID {chiTiet.MaHh} not found");
				}
				if (product.SoLuong < chiTiet.SoLuong)
				{
					throw new ValidationException($"Insufficient stock for product {product.TenHh}");
				}

				var chiTietHd = new ChiTietHd
				{
					MaHd = hoaDon.MaHd,
					MaHh = chiTiet.MaHh,
					DonGia = chiTiet.DonGia,
					SoLuong = chiTiet.SoLuong,
					MaGiamGia = chiTiet.MaGiamGia ?? 0
				};
				await _db.ChiTietHds.AddAsync(chiTietHd);

				// Cập nhật số lượng sản phẩm
				product.SoLuong -= chiTiet.SoLuong;
			}

			await _db.SaveChangesAsync(); // Lưu các bản ghi ChiTietHd và cập nhật tồn kho
			await transaction.CommitAsync();

			// 4. Xóa các bản ghi trong bảng Carts
			var productIds = model.ChiTietHoaDons.Select(chiTiet => chiTiet.MaHh).ToList(); // Lấy danh sách ProductId từ model
			var cartsToDelete = await _db.Carts
				.Where(cart => cart.UserId == model.MaKh && productIds.Contains(cart.ProductId))
				.ToListAsync();

			_db.Carts.RemoveRange(cartsToDelete);
			await _db.SaveChangesAsync();

			return payHistory;
		}
		catch (Exception ex)
		{
			await transaction.RollbackAsync();
			_logger.LogError(ex, "Error during checkout process");
			throw;
		}
	}
}