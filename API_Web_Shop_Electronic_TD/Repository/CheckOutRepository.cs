using API_Web_Shop_Electronic_TD.Data;
using API_Web_Shop_Electronic_TD.Interfaces;
using API_Web_Shop_Electronic_TD.Models;
using API_Web_Shop_Electronic_TD.Services;
using Azure.Core;
using Microsoft.AspNetCore.Mvc;
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

	//public async Task<PayHistory> InitiateCheckoutAsync(CheckOutMD model)
	//{
	//	using var transaction = await _db.Database.BeginTransactionAsync();
	//	try
	//	{
	//		var payHistory = new PayHistory
	//		{
	//			FullName = model.FullName,
	//			OrderInfo = model.OrderInfo,
	//			Amount = model.Amount,
	//			PayMethod = "VNPay",
	//			CouponCode = model.CouponCode,
	//			CreateDate = DateTime.UtcNow,
	//		};

	//		_db.PayHistorys.Add(payHistory);
	//		await _db.SaveChangesAsync();
	//		await transaction.CommitAsync();

	//		return payHistory;
	//	}
	//	catch (Exception ex)
	//	{
	//		await transaction.RollbackAsync();
	//		_logger.LogError(ex, "Error initiating checkout");
	//		throw;
	//	}
	//}

	public async Task ProcessVnPayPaymentAsync(CheckOutMD paymentResponse)
	{
		using var transaction = await _db.Database.BeginTransactionAsync();
		try
		{
			var payHistory = new PayHistory
			{
				FullName = paymentResponse.FullName,
				OrderInfo = paymentResponse.OrderInfo,
				Amount = paymentResponse.Amount,
				PayMethod = "VNPay",
				CouponCode = paymentResponse.CouponCode,
				CreateDate = DateTime.UtcNow,
			};

			_db.PayHistorys.Add(payHistory);
			await _db.SaveChangesAsync();

			// Create order
			var hoaDon = new HoaDon
			{
				MaKh = paymentResponse.MaKh, // Ensure this field exists in PayHistory
				NgayDat = DateTime.UtcNow,
				NgayGiao = DateTime.UtcNow.AddDays(3), // Configurable delivery time
				HoTen = paymentResponse.FullName,
				CachThanhToan = "VNPay",
				MaTrangThai = 1, // Order status: Paid
				PayId = payHistory.Id,
			};

			_db.HoaDons.Add(hoaDon);
			await _db.SaveChangesAsync();

			// Process order details from temporary storage or session
			var orderDetails = await _db.ChiTietHds
				.Where(od => od.MaCt == payHistory.Id)
				.ToListAsync();

			foreach (var detail in orderDetails)
			{
				var product = await _db.HangHoas.FindAsync(detail.MaHh);
				if (product == null || product.SoLuong < detail.SoLuong)
					throw new InvalidOperationException($"Insufficient stock for product {detail.MaHh}");

				var chiTietHd = new ChiTietHd
				{
					MaHd = hoaDon.MaHd,
					MaHh = detail.MaHh,
					DonGia = detail.DonGia,
					SoLuong = detail.SoLuong,
					MaGiamGia = detail.MaGiamGia
				};

				product.SoLuong -= detail.SoLuong;
				_db.ChiTietHds.Add(chiTietHd);
			}

			// Clear cart
			var cartItems = await _db.Carts
				.Where(c => c.UserId == paymentResponse.MaKh)
				.ToListAsync();
			_db.Carts.RemoveRange(cartItems);

			await _db.SaveChangesAsync();
			await transaction.CommitAsync();
		}
		catch (Exception ex)
		{
			await transaction.RollbackAsync();
			_logger.LogError(ex, "Error processing VNPay payment");
			throw;
		}
	}
}