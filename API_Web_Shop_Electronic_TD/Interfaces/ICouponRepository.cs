using API_Web_Shop_Electronic_TD.Data;
using API_Web_Shop_Electronic_TD.Models;

namespace API_Web_Shop_Electronic_TD.Interfaces
{
	public interface ICouponRepository
	{
		Task<List<Coupon>> GetAllAsync();
		Task<Coupon> GetByIdAsync(int id);
		Task<Coupon> CreateAsync(CouponsMD coupon);
		Task<Coupon> UpdateAsync(CouponsMD coupon,int id);
		Task<Coupon> DeleteAsync(int id);
		Task<UserCoupon> UseCoupone(string userId, string couponCode);
	}
}
