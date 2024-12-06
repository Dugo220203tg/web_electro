using System.ComponentModel.DataAnnotations;

namespace TDProjectMVC.Models.MoMo
{

    public class OrderInfoModel
    {
        public int Id { get; set; }
        public string OrderId { get; set; }
        public string OrderInfo { get; set; }
        public string FullName { get; set; }
        public decimal Amount { get; set; }
        public DateTime DatePaid { get; set; }
    }
}
