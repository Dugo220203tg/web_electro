using System.ComponentModel.DataAnnotations;

namespace TDProjectMVC.ViewModels
{
    public class LoginVM
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập hoặc email")]
        [Display(Name = "Tên đăng nhập/Email")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [Display(Name = "Mật khẩu")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Ghi nhớ đăng nhập")]
        public bool RememberMe { get; set; }
    }
    public class ResetPassword
    {
        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        public string newpassword { get; set; }

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
        [Compare("newpassword", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string Passwordcheck { get; set; }
    }
}
