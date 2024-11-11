using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TDProjectMVC.ViewModels
{
	[Table("KhachHang")]
	public class RegisterVM
	{
		[Key]
		public string MaKh { get; set; }


		[Required(ErrorMessage = "*")]
		[DataType(DataType.Password)]
		public string MatKhau { get; set; }

		[Required(ErrorMessage = "*")]
		[MaxLength(50, ErrorMessage = "Tối đa 50 kí tự")]
		public string HoTen { get; set; }

		public bool GioiTinh { get; set; } = true;

		[DataType(DataType.Date)]
		public DateOnly? NgaySinh { get; set; }

		[MaxLength(60, ErrorMessage = "Tối đa 60 kí tự")]
		public string DiaChi { get; set; }

		[MaxLength(24, ErrorMessage = "Tối đa 24 kí tự")]
		[RegularExpression(@"0[9875]\d{8}", ErrorMessage = "Chưa đúng định dạng di động Việt Nam")]
		public string DienThoai { get; set; }

		[EmailAddress(ErrorMessage = "Chưa đúng định dạng email")]
		public string Email { get; set; }

		public string? Hinh { get; set; }
        
    }
}
