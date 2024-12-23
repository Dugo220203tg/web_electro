namespace API_Web_Shop_Electronic_TD.Models
{
	public class HangHoaMD
	{
		public int MaHH { get; set; }
		public string TenHH { get; set; }
		public string Hinh { get; set; }
		public string MoTa { get; set; }
		public string MoTaDonVi { get; set; }
		public int MaLoai { get; set; }
		public DateOnly? NgaySX { get; set; }
		public double? GiamGia { get; set; }
		public string MaNCC { get; set; }
		public double DonGia { get; set; }
		public double SoLanXem { get; set; }
		public int? SoLuong { get; set; }
		public string TenNCC { get; set; }
		public string TenLoai { get; set; }
		public string TenDanhMuc { get; set; }
		public int MaDanhMuc { get; set; }


	}
	public class CreateHangHoaMD
	{
		public string TenHH { get; set; }
		public string Hinh { get; set; }
		public string MoTa { get; set; }
		public string MoTaDonVi { get; set; }
		public int? MaLoai { get; set; }
		public DateOnly? NgaySX { get; set; }
		public double? GiamGia { get; set; }
		public string MaNCC { get; set; }
		public double DonGia { get; set; }
		public double SoLanXem { get; set; }
		public int? SoLuong { get; set; }
	}
	public class AllHangHoaMD
	{
		public int MaHH { get; set; }
		public string TenHH { get; set; }
		public string Hinh { get; set; }
		public string MoTa { get; set; }
		public string MoTaDonVi { get; set; }
		public string? MaLoai { get; set; }
		public DateOnly? NgaySX { get; set; }
		public double? GiamGia { get; set; }
		public string? MaNCC { get; set; }
		public double DonGia { get; set; }
		public double SoLanXem { get; set; }
		public int? SoLuong { get; set; }

	}

	public class UpdateHangHoaMD
	{
		public string TenHH { get; set; }
		public string Hinh { get; set; }
		public string MoTa { get; set; }
		public string MoTaDonVi { get; set; }
		public int? MaLoai { get; set; }
		public DateOnly? NgaySX { get; set; }
		public double? GiamGia { get; set; }
		public string MaNCC { get; set; }
		public double DonGia { get; set; }
		public int? SoLuong { get; set; }
		public double SoLanXem { get; set; }

	}

}
