using System.ComponentModel.DataAnnotations;

namespace API_Web_Shop_Electronic_TD.Models
{
	public class RoleMD
	{
		[Required]
		public int id { get; set; }
		[Required]
		public string roleName { get; set; }
		public string description { get; set; }
	}
}
