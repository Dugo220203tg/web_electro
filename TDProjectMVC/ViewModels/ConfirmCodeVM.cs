using System.ComponentModel.DataAnnotations;

namespace TDProjectMVC.ViewModels
{
    public class ConfirmCodeVM
    {
        public string KhachHangId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mã xác nhận")]
        public string ConfirmationCode { get; set; }

        public string Type { get; set; } // "Register" hoặc "ResetPassword"
    }
}
