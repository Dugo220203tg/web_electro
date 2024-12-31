using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_Web_Shop_Electronic_TD.Models
{
	public class CouponsMD
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int id { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public int Quantity { get; set; }
		public DateTime DateEnd { get; set; }
		public DateTime DateStart { get; set; }
		public decimal price { get; set; }
		public int Status { get; set; }
	}
	public class couponReport
	{
		public string Name { get; set; }
		public int price {  get; set; }
		public int status { get; set; }
		public DateTime DateEnd { get; set; }
	}

}
