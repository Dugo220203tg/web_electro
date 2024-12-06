using System.ComponentModel.DataAnnotations;

namespace TDProjectMVC.ViewModels
{
    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string PhoneNumber { get; set; }

        [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán")]
        public string PaymentMethod { get; set; }
    }
    public enum OrderStatus
    {
        Pending,
        Confirmed,
        Paid,
        Shipped,
        Delivered,
        Cancelled,
        PaymentFailed
    }

    public enum PaymentStatus
    {
        Success,
        Failed,
        Pending
    }
}
