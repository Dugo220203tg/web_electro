using System.ComponentModel.DataAnnotations;

namespace TDProjectMVC.ViewModels
{
    public class LoginVM
    {
        [Required(ErrorMessage = "Vui lòng nhập Username.")]
        [Display(Name = "User Name")]
        [MaxLength(20, ErrorMessage = "Max 20 characters allowed.")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Mật khẩu.")]
        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
