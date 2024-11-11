using System.ComponentModel.DataAnnotations;

namespace TrangQuanLy.Models
{
	public class AdminViewModel
	{

        [Display(Name = "User Name")]
		[Required(ErrorMessage = "*")]
		[MaxLength(20, ErrorMessage = "Max 20 keys")]
		public string UserName { get; set; }
		[Display(Name = "Password")]
		[Required(ErrorMessage = "*")]
		[DataType(DataType.Password)]
		public string Password { get; set; }
		public int Vaitro { get; set; }
		public string Email { get; set; }
		public string Hoten { get; set; }
        public string RandomKey { get; set; }
        public bool HieuLuc { get; set; }
	}
    public class AdminDkMD
    {
        [Display(Name = "User Name")]
        [Required(ErrorMessage = "*")]
        [MaxLength(20, ErrorMessage = "Max 20 keys")]
        public string UserName { get; set; }
        [Display(Name = "Password")]
        [Required(ErrorMessage = "*")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public string Email { get; set; }

    }
}
