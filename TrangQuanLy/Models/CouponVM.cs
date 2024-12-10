using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace TrangQuanLy.Models
{
    public class CouponVM
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id {  get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int quantity { get; set; }
        public DateTime DateEnd { get; set; }
        public DateTime DateStart { get; set; }
        public decimal price { get; set; }  
        public int Status { get; set; }
    }
}
