using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_Web_Shop_Electronic_TD.Models
{
	public class DanhMucMD
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int MaDanhMuc { get; set; }
		[Required(ErrorMessage = "Tên danh mục không được để trống")]
		public string TenDanhMuc { get; set;}
	}
	public class LoaiSpMD
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		[Required(ErrorMessage = "Tên loại không được để trống")]
		public int MaLoai { get; set; }
		public string TenLoai { get; set; }
		public string? Hinh { get; set; }
		public string Mota { get; set; }
		public int? DanhMuc_id { get; set; }
	}
	public class CreateLoaiSpMD
	{
		public string TenLoai { get; set; }
		public string? Hinh { get; set; }
		public string Mota { get; set; }
		public int? DanhMuc_id { get; set; }
	}
	public class HangSpMD
	{
		[Key]
		[Required(ErrorMessage = "Mã Nhà cung cấp không được để trống")]
		public string MaNCC { get; set; }
		[Required(ErrorMessage = "Tên Nhà cung cấp không được để trống")]
		public string? TenCongTy { get; set; }
		public string Mota { get; set; }
		public string DiaChi { get; set; }
		public string Email { get; set; }
		public string NguoiLienLac { get; set; }
		[Required(ErrorMessage = "Số điện thoại không được để trống")]
		public string DienThoai { get; set; }
		public string Logo { get; set; }

	}
	public class TrangThaiHoaDonMD
	{
		public int MaTrangThai { get; set; }
		public string TenTrangThai { get; set; }
	}
}
