using System.ComponentModel.DataAnnotations;

namespace API_Web_Shop_Electronic_TD.Models
{
	public class DanhGiaSpMD
	{
		[Key]
		public int MaDg { get; set; }
		public string MaKH { get; set; }
		public string NoiDung { get; set; }
		public int Sao {  get; set; }
		public DateTime Ngay { get; set; }
		[Key]
		public int MaHH { get; set; }
		public int TrangThai { get; set; }
		public string TenHangHoa { get; set; }
		public string TenKhachHang {  get; set; }
		public string HinhAnh { get; set; }
	}
	public class DanhGiaSpResponseMD
	{
		[Key]
		public int MaDg { get; set; }
		[Key]
		public string MaKH { get; set; }
		public string NoiDung { get; set; }
		public int Sao { get; set; }
		public DateTime Ngay { get; set; }
		[Key]
		public int MaHH { get; set; }
		public int TrangThai { get; set; }
	}
	public class CreateDanhGiaSpMD
	{
		public string MaKH { get; set; }
		public int Sao { get; set; }
		public DateTime Ngay { get; set; }
		public string NoiDung { get; set; }
		public int MaHH { get; set; }
		public int TrangThai { get; set; }
	}
	public class UpdateTrangThaiDanhGiaSpVM
	{
		public int MaDg { get; set; }
		public int TrangThai { get; set; }

	}
}
