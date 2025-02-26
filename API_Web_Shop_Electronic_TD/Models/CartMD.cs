﻿using System.ComponentModel.DataAnnotations;

namespace API_Web_Shop_Electronic_TD.Models
{
	public class CartMD
	{
	}
	public class CartItem
	{
		public int MaHH { get; set; }
		public string TenHH { get; set; }
		public string Hinh { get; set; }
		public double DonGia { get; set; }
		public double GiamGia { get; set; }
		public double MaH { get; set; }
		public int SoLuong { get; set; }
		public double ThanhTien => SoLuong * DonGia;
		public List<string> ImageUrls { get; set; }

	}
	public class CartRequest
	{
		public bool Success { get; set; }
		public string Message { get; set; }
		public int CartCount { get; set; }
	}
	public class CartRequests
	{
		[Key]
		public int id {  get; set; }
		public string MaKh { get; set; }
		public int MaHh { get; set; }
		public string TenHH { get; set; }
		public string Hinh { get; set; }
		public double DonGia { get; set; }
		public string TenNcc { get; set; }
		public int Quantity { get; set; }
		public double Total => Quantity * DonGia;
	}
	public class CartResponse
	{
		public string MaKh { get; set; }
		public int MaHh { get; set; }
		public int Quantity { get; set; }
		public DateTime Ngay { get; set; }
	}
	public class CartData
	{
		public List<CartItemData> CartItems { get; set; }
		public int TotalQuantity { get; set; }
		public decimal TotalAmount { get; set; }
	}

	public class CartItemData
	{
		public int MaHH { get; set; }
		public string TenHH { get; set; }
		public int SoLuong { get; set; }
		public decimal DonGia { get; set; }
		public string Hinh { get; set; }
	}

	public class AddToCartRequest
	{
		public int ProductId { get; set; }
		public int Quantity { get; set; } = 1;
	}
}
