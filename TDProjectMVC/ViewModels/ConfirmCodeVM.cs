using System.ComponentModel.DataAnnotations;

namespace TDProjectMVC.ViewModels
{
    public class ConfirmCodeVM
    {
        public string KhachHangId { get; set; } // Or use string if your IDs are strings
        [Required(ErrorMessage = "Mã xác nhận là bắt buộc.")]
        public string ConfirmationCode { get; set; }
    }
}
