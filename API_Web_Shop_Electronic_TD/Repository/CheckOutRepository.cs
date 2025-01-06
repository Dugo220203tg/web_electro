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

	public async Task<int> ProcessPaymentAsync(CheckOutMD request, string paymentMethod)
	{
		using var transaction = await _db.Database.BeginTransactionAsync();
		try
		{
			// Create payment history
			var payHistory = await CreatePaymentHistory(request, paymentMethod);

			// Create order
			var hoaDon = await CreateOrder(request, payHistory.Id);

			// Process order details
			await ProcessOrderDetails(request.ChiTietHoaDons, hoaDon.MaHd);

			// Clear cart
			await ClearUserCart(request.MaKh);

			await _db.SaveChangesAsync();
			await transaction.CommitAsync();

			return hoaDon.MaHd;
		}
		catch (Exception ex)
		{
			await transaction.RollbackAsync();
			_logger.LogError(ex, $"Error processing {paymentMethod} payment");
			throw;
		}
	}

	private async Task<PayHistory> CreatePaymentHistory(CheckOutMD request, string paymentMethod)
	{
		var payHistory = new PayHistory
		{
			FullName = request.FullName,
			OrderInfo = request.OrderInfo,
			Amount = request.Amount,
			PayMethod = paymentMethod,
			CouponCode = request.CouponCode,
			CreateDate = DateTime.UtcNow,
			//user = request.MaKh,
		};

		_db.PayHistorys.Add(payHistory);
		await _db.SaveChangesAsync();

		return payHistory;
	}

	private async Task<HoaDon> CreateOrder(CheckOutMD request, int payHistoryId)
	{
		// Validate customer exists again (in case of concurrent deletion)
		if (!await _db.KhachHangs.AnyAsync(k => k.MaKh == request.MaKh))
		{
			throw new InvalidOperationException($"Customer not found with ID: {request.MaKh}");
		}
		var hoaDon = new HoaDon
		{
			MaKh = request.MaKh,
			NgayDat = DateTime.UtcNow,
			NgayGiao = DateTime.UtcNow.AddDays(3),
			HoTen = request.FullName,
			DiaChi = request.DiaChi,
			DienThoai = request.DienThoai,
			GhiChu = request.GhiChu,
			CachThanhToan = request.PayMethod,
			PhiVanChuyen = request.ShippingFee,
			MaTrangThai = DetermineOrderStatus(request.PayMethod),
			PayId = payHistoryId,
		};

		_db.HoaDons.Add(hoaDon);
		await _db.SaveChangesAsync();

		return hoaDon;
	}

	private int DetermineOrderStatus(string paymentMethod)
	{
		// You can customize these status codes based on your system
		return paymentMethod.ToLower() switch
		{
			"directcheck" => 0, // Pending payment
			"vnpay" => 1,      // Paid
			"momo" => 1,       // Paid
			_ => 0             // Default to pending
		};
	}

	private async Task ProcessOrderDetails(IEnumerable<ChiTietHoaDon1MD> orderDetails, int orderId)
	{
		foreach (var detail in orderDetails)
		{
			var product = await _db.HangHoas.FindAsync(detail.MaHh);
			if (product == null)
			{
				throw new InvalidOperationException($"Product not found: {detail.MaHh}");
			}

			if (product.SoLuong < detail.SoLuong)
			{
				throw new InvalidOperationException($"Insufficient stock for product {detail.MaHh}");
			}

			var chiTietHd = new ChiTietHd
			{
				MaHd = orderId,
				MaHh = detail.MaHh,
				DonGia = detail.DonGia,
				SoLuong = detail.SoLuong,
				MaGiamGia = (double)detail.MaGiamGia,
			};

			// Update product stock
			product.SoLuong -= detail.SoLuong;

			_db.ChiTietHds.Add(chiTietHd);
		}

		await _db.SaveChangesAsync();
	}

	private async Task ClearUserCart(string userId)
	{
		var cartItems = await _db.Carts
			.Where(c => c.UserId == userId)
			.ToListAsync();

		_db.Carts.RemoveRange(cartItems);
		await _db.SaveChangesAsync();
	}

	public async Task UpdatePaymentStatus(int orderId, bool isSuccessful)
	{
		var order = await _db.HoaDons.FindAsync(orderId);
		if (order == null)
		{
			throw new InvalidOperationException($"Order not found: {orderId}");
		}

		order.MaTrangThai = isSuccessful ? 1 : 2; // 1: Paid, 2: Failed
		await _db.SaveChangesAsync();
	}
}