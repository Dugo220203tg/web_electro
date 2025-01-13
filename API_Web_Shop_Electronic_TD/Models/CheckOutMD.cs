// PayHistory
using System.ComponentModel.DataAnnotations;
namespace API_Web_Shop_Electronic_TD.Models
{
	//public class PayHistory
	//{
	//	public int Id { get; set; }
	//	[Required]
	//	public string FullName { get; set; }
	//	public string? OrderInfo { get; set; }
	//	[Required]
	//	[Range(0, double.MaxValue)]
	//	public double Amount { get; set; }
	//	[Required]
	//	public string PayMethod { get; set; }
	//	public string? CouponCode { get; set; }
	//	public DateTime CreateDate { get; set; }

	//	// Navigation property
	//	public HoaDon HoaDon { get; set; }
	//}

	//// HoaDon
	//public class HoaDonResponseMD
	//{
	//	[Required]
	//	public string MaKh { get; set; }
	//	public DateTime NgayDat { get; set; }
	//	[Required]
	//	public string HoTen { get; set; }
	//	[Required]
	//	public string DiaChi { get; set; }
	//	[Required]
	//	public string CachThanhToan { get; set; }
	//	[Required]
	//	public string CachVanChuyen { get; set; }
	//	[Range(0, double.MaxValue)]
	//	public double ShippingFee { get; set; } // Đổi tên theo C# convention
	//	public int MaTrangThai { get; set; }
	//	public string? GhiChu { get; set; }
	//	[Required]
	//	[Phone]
	//	public string DienThoai { get; set; }
	//	public int PayId { get; set; }

	//	// Navigation properties
	//	public PayHistory PayHistory { get; set; }
	//	public ICollection<ChiTietHoaDonResponseMD> ChiTietHoaDons { get; set; } =
	//		new List<ChiTietHoaDonResponseMD>();

	//	// Tính tổng tiền
	//	public double TotalAmount =>
	//		ChiTietHoaDons.Sum(ct => ct.ThanhTien) + ShippingFee;
	//}

	public class CheckOutMD
	{
		// PayHistory info
		[Required(ErrorMessage = "Full name is required")]
		public string FullName { get; set; }
		public string? OrderInfo { get; set; }
		[Required]
		[Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
		public double Amount { get; set; }
		[Required(ErrorMessage = "Payment method is required")]
		public string PayMethod { get; set; }
		public string? CouponCode { get; set; }

		// HoaDon info
		[Required(ErrorMessage = "Customer ID is required")]
		public string MaKh { get; set; }
		[Required(ErrorMessage = "Name is required")]
		public string DiaChi { get; set; }

		[Range(0, double.MaxValue, ErrorMessage = "Shipping fee cannot be negative")]
		public double ShippingFee { get; set; }
		public string? GhiChu { get; set; }
		[Required(ErrorMessage = "Phone number is required")]
		[Phone(ErrorMessage = "Invalid phone number format")]
		public string DienThoai { get; set; }
		
		// Chi tiết hóa đơn
		[Required(ErrorMessage = "Order details are required")]
		[MinLength(1, ErrorMessage = "Order must contain at least one item")]
		public List<ChiTietHoaDon1MD> ChiTietHoaDons { get; set; } = new List<ChiTietHoaDon1MD>();
	}

	public class ChiTietHoaDon1MD
	{
		[Required(ErrorMessage = "Product ID is required")]
		public int MaHh { get; set; }

		[Required]
		[Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
		public int SoLuong { get; set; }

		[Required]
		[Range(0, int.MaxValue, ErrorMessage = "Price cannot be negative")]
		public int DonGia { get; set; }

		public int? MaGiamGia { get; set; }
	}
}