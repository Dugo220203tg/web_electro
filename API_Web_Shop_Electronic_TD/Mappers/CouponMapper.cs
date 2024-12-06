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
				price = model.Price,
				DateStart = (DateTime)model.DateStart,
				DateEnd = (DateTime)model.DateEnd,
				Description = model.Description,
				Quantility = model.Quantility,
				Status = model.Status,
			};
		}
		public static Coupon ToCouponDTO(this CouponsMD model)
		{
			return new Coupon
			{
				Id = model.id,
				Name = model.Name,
				Price = model.price,
				DateStart = model.DateStart,
				DateEnd = model.DateEnd,
				Description = model.Description,
				Quantility = model.Quantility,
				Status = model.Status,
			};
		}
	}
}
