using API_Web_Shop_Electronic_TD.Data;
using API_Web_Shop_Electronic_TD.Interfaces;
using API_Web_Shop_Electronic_TD.Mappers;
using API_Web_Shop_Electronic_TD.Models;
using Microsoft.EntityFrameworkCore;

namespace API_Web_Shop_Electronic_TD.Repository
{
	public class CouponRepository : ICouponRepository
	{
		private readonly Hshop2023Context _context;
		public CouponRepository(Hshop2023Context context)
		{
			_context = context;
		}
		public async Task<Coupon> CreateAsync(CouponsMD coupon)
		{
			if (string.IsNullOrEmpty(coupon.Name))
			{
				throw new ArgumentException("Chưa nhập đủ thông tin: Tên coupon không được để trống");
			}
			if (coupon.price == null)
			{
				throw new ArgumentException("Chưa nhập đủ thông tin: Giá coupon không được để trống");
			}
			if (coupon.Quantity == null)
			{
				throw new ArgumentException("Chưa nhập đủ thông tin: số lương coupon không được để trống");
			}
			if (coupon.Status == null)
			{
				throw new ArgumentException("Chưa nhập đủ thông tin: Trạng thái coupon không được để trống");
			}
			var data = coupon.ToCouponDTO(); // Sử dụng mapper để chuyển đổi từ KhachHangsMD sang KhachHang

			await _context.Coupons.AddAsync(data);
			await _context.SaveChangesAsync();

			return data;
		}

		public async Task<Coupon> DeleteAsync(int id)
		{
			var model = await _context.Coupons.FirstOrDefaultAsync(x => x.Id == id);
			if (model == null)
			{
				throw new KeyNotFoundException($"Không tìm thấy loại sản phẩm với mã {id}");
			}
			_context.Coupons.Remove(model);
			await _context.SaveChangesAsync();
			return model;
		}

		public async Task<List<Coupon>> GetAllAsync()
		{
			return await _context.Coupons.ToListAsync();
		}

		public async Task<Coupon?> GetByIdAsync(int id)
		{
			return await _context.Coupons.FindAsync(id);
		}

		public async Task<Coupon> UpdateAsync(CouponsMD coupon, int id)
		{
			if (string.IsNullOrEmpty(coupon.Name))
			{
				throw new ArgumentException("Chưa nhập đủ thông tin: Tên loại không được để trống");
			}
			if (coupon.price == null)
			{
				throw new ArgumentException("Chưa nhập đủ thông tin: Giá coupon không được để trống");
			}
			if (coupon.Quantity == null)
			{
				throw new ArgumentException("Chưa nhập đủ thông tin: số lương coupon không được để trống");
			}
			if (coupon.Status == null)
			{
				throw new ArgumentException("Chưa nhập đủ thông tin: Trạng thái coupon không được để trống");
			}
			// Lấy đối tượng HangHoa từ cơ sở dữ liệu
			var Model = await _context.Coupons.FirstOrDefaultAsync(x => x.Id == id);

			// Kiểm tra xem HangHoaModel có null không
			if (Model == null)
			{
				throw new KeyNotFoundException($"Không tìm thấy Danh Muc sản phẩm với mã {id}");
			}

			// Cập nhật thông tin của HangHoaModel từ dữ liệu được gửi từ client
			Model.Name = coupon.Name;	
			Model.Status = coupon.Status;
			Model.Description = coupon.Description;
			Model.Price = coupon.price;
			Model.DateStart = coupon?.DateStart;
			Model.DateEnd = coupon?.DateEnd;
			Model.Quantity = coupon.Quantity;

			// Lưu thay đổi vào cơ sở dữ liệu
			await _context.SaveChangesAsync();

			// Trả về HangHoaModel đã được cập nhật
			return Model;
		}

		public async Task<UserCoupon> UseCoupone(string userId, string couponCode)
		{
			return await _context.UserCoupons
				.Include(x => x.Coupon)
				.FirstOrDefaultAsync(x => x.UserId == userId && x.Coupon.Description == couponCode);
		}

	}
}
