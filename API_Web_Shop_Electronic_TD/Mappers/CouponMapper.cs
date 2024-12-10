using API_Web_Shop_Electronic_TD.Data;
using API_Web_Shop_Electronic_TD.Models;
using NuGet.Protocol;

namespace API_Web_Shop_Electronic_TD.Mappers
{
	public static class CouponMapper
	{
		public static CouponsMD ToCouponDo(this Coupon model)
		{
			return new CouponsMD
			{
				id = model.Id,
				Name = model.Name,
				price = (decimal)model.Price,
				DateStart = (DateTime)model.DateStart,
				DateEnd = (DateTime)model.DateEnd,
				Description = model.Description,
				Quantity = (int)model.Quantity,
				Status = (int)model.Status,
			};
		}
		public static Coupon ToCouponDTO(this CouponsMD model)
		{
			return new Coupon
			{
				Name = model.Name,
				Price = model.price,
				DateStart = model.DateStart,
				DateEnd = model.DateEnd,
				Description = model.Description,
				Quantity = model.Quantity,
				Status = model.Status,
			};
		}
	}
}
