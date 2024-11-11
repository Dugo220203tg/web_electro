using API_Web_Shop_Electronic_TD.Data;
using API_Web_Shop_Electronic_TD.Interfaces;
using API_Web_Shop_Electronic_TD.Mappers;
using API_Web_Shop_Electronic_TD.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace API_Web_Shop_Electronic_TD.Repository
{
	public class HoaDonRepository : IHoaDon
	{
		private readonly Hshop2023Context db;
		public HoaDonRepository(Hshop2023Context db)
		{
			this.db = db;
		}
		public async Task<HoaDon?> DeleteAsync(int MaHd)
		{
			var Model = await db.HoaDons
				.Include(h => h.ChiTietHds) // Load ChiTietHoaDon records
				.FirstOrDefaultAsync(x => x.MaHd == MaHd);

			if (Model == null)
			{
				throw new KeyNotFoundException($"Không tìm thấy Hóa đơn với mã {MaHd}");
			}

			db.HoaDons.Remove(Model);
			await db.SaveChangesAsync(); // Cascade delete will handle ChiTietHoaDon records

			return Model;
		}


		public async Task<List<HoaDon>> GetAllAsync()
		{
			return await db.HoaDons
				.Include(h => h.MaTrangThaiNavigation)
				.Include(h => h.ChiTietHds)
				.ThenInclude(ct => ct.MaHhNavigation) // Include navigation property
				.ToListAsync();
		}

		public async Task<HoaDon?> GetByIdAsync(int MaHd)
		{
			return await db.HoaDons
				.Include(h => h.MaTrangThaiNavigation)
				.Include(h => h.ChiTietHds)
				//.Include(h => h.MaKhNavigation)
				.ThenInclude(ct => ct.MaHhNavigation)
				.FirstOrDefaultAsync(h => h.MaHd == MaHd);
		}

		public async Task<HoaDon?> UpdateAsync(int MaHd, HoaDonMD model)
		{
			// Kiểm tra các điều kiện của HoaDon
			if (string.IsNullOrEmpty(model.MaKH))
			{
				throw new ArgumentException("Mã khách hàng không được để trống");
			}
			if (string.IsNullOrEmpty(model.HoTen))
			{
				throw new ArgumentException("Tên khách hàng không được để trống");
			}
			if (string.IsNullOrEmpty(model.DiaChi))
			{
				throw new ArgumentException("Địa chỉ không được để trống");
			}
			if (string.IsNullOrEmpty(model.DienThoai))
			{
				throw new ArgumentException("Địa chỉ không được để trống");
			}
			if (!model.MaTrangThai.HasValue || model.MaTrangThai < 0)
			{
				throw new ArgumentException("Mã loại không hợp lệ hoặc chưa được nhập");
			}
			if (!model.PhiVanChuyen.HasValue || model.PhiVanChuyen < 0)
			{
				throw new ArgumentException("Phí vận chuyển không hợp lệ hoặc chưa được nhập");
			}
			if (string.IsNullOrEmpty(model.CachVanChuyen))
			{
				throw new ArgumentException("Cách vận chuyển không được để trống");
			}
			if (string.IsNullOrEmpty(model.CachThanhToan))
			{
				throw new ArgumentException("Cách thanh toán không được để trống");
			}
			var existingKh = await db.KhachHangs.FirstOrDefaultAsync(l => l.MaKh == model.MaKH);
			if (existingKh == null)
			{
				throw new ArgumentException($"Khách hàng {model.MaKH} không tồn tại trong hệ thống");
			}
			if (string.IsNullOrEmpty(model.HoTen))
			{
				throw new ArgumentException("Tên khách hàng chưa được nhập");
			}

			// Kiểm tra điều kiện cho ChiTietHds
			if (model.ChiTietHds == null || !model.ChiTietHds.Any())
			{
				throw new ArgumentException("Chi tiết hóa đơn không được để trống");
			}

			foreach (var chiTiet in model.ChiTietHds)
			{
				// Kiểm tra MaHH
				if (chiTiet.MaHH < 0)
				{
					throw new ArgumentException($"Mã hàng hóa không hợp lệ hoặc chưa được nhập tại chi tiết hóa đơn");
				}

				// Kiểm tra sự tồn tại của hàng hóa
				var existingHangHoa = await db.HangHoas.FirstOrDefaultAsync(h => h.MaHh == chiTiet.MaHH);
				if (existingHangHoa == null)
				{
					throw new ArgumentException($"Hàng hóa với mã {chiTiet.MaHH} không tồn tại trong hệ thống");
				}

				// Kiểm tra SoLuong
				if (chiTiet.SoLuong < 0)
				{
					throw new ArgumentException($"Số lượng không hợp lệ hoặc chưa được nhập tại chi tiết hóa đơn có mã hàng hóa {chiTiet.MaHH}");
				}

				// Kiểm tra DonGia
				if (chiTiet.DonGia < 0)
				{
					throw new ArgumentException($"Đơn giá không hợp lệ hoặc chưa được nhập tại chi tiết hóa đơn có mã hàng hóa {chiTiet.MaHH}");
				}

				// Kiểm tra MaGiamGia nếu có
				//if (chiTiet.MaGiamGia > 0)
				//{
				//	var existingGiamGia = await db.GiamGias.FirstOrDefaultAsync(g => g.MaGiamGia == chiTiet.MaGiamGia);
				//	if (existingGiamGia == null)
				//	{
				//		throw new ArgumentException($"Mã giảm giá {chiTiet.MaGiamGia} không tồn tại trong hệ thống");
				//	}
				//}
			}
			// Lấy đối tượng HoaDon từ cơ sở dữ liệu
			var hoaDon = await db.HoaDons
				.Include(h => h.ChiTietHds) // Load related ChiTietHoaDons
				.FirstOrDefaultAsync(x => x.MaHd == MaHd);

			// Kiểm tra xem HoaDon có tồn tại hay không
			if (hoaDon == null)
			{
				throw new KeyNotFoundException($"Không tìm thấy Hóa đơn với mã {MaHd}");
			}

			// Cập nhật thông tin của HoaDon
			hoaDon.MaKh = model.MaKH;
			hoaDon.NgayDat = model.NgayDat;
			hoaDon.HoTen = model.HoTen;
			hoaDon.DiaChi = model.DiaChi;
			hoaDon.CachVanChuyen = model.CachVanChuyen;
			hoaDon.PhiVanChuyen = (double)(double?)model.PhiVanChuyen;
			hoaDon.MaTrangThai = (int)model.MaTrangThai;
			hoaDon.DienThoai = model.DienThoai;

			// Cập nhật thông tin trong bảng ChiTietHoaDon
			// Xóa những chi tiết không còn trong model
			var chiTietToRemove = hoaDon.ChiTietHds
				.Where(ct => !model.ChiTietHds.Any(m => m.MaCT == ct.MaCt))
				.ToList();
			db.ChiTietHds.RemoveRange(chiTietToRemove);

			// Cập nhật hoặc thêm chi tiết hóa đơn
			foreach (var chiTietModel in model.ChiTietHds)
			{
				var chiTiet = hoaDon.ChiTietHds.FirstOrDefault(ct => ct.MaCt == chiTietModel.MaCT);

				if (chiTiet != null) // Nếu chi tiết đã tồn tại, cập nhật
				{
					chiTiet.MaHh = chiTietModel.MaHH;
					chiTiet.SoLuong = chiTietModel.SoLuong;
					chiTiet.DonGia = chiTietModel.DonGia;
					chiTiet.MaGiamGia = chiTietModel.MaGiamGia;
				}
				else // Nếu chi tiết chưa tồn tại, thêm mới
				{
					hoaDon.ChiTietHds.Add(new ChiTietHd
					{
						MaHd = hoaDon.MaHd,
						MaHh = chiTietModel.MaHH,
						SoLuong = chiTietModel.SoLuong,
						DonGia = chiTietModel.DonGia,
						MaGiamGia = chiTietModel.MaGiamGia,
					});
				}
			}

			// Lưu các thay đổi vào cơ sở dữ liệu
			await db.SaveChangesAsync();

			// Trả về hóa đơn đã được cập nhật
			return hoaDon;
		}


		public async Task<HoaDon> CreateAsync(HoaDonMD model)
		{
			// Kiểm tra các điều kiện của HoaDon
			if (string.IsNullOrEmpty(model.MaKH))
			{
				throw new ArgumentException("Mã khách hàng không được để trống");
			}
			if (string.IsNullOrEmpty(model.HoTen))
			{
				throw new ArgumentException("Tên khách hàng không được để trống");
			}
			if (string.IsNullOrEmpty(model.DiaChi))
			{
				throw new ArgumentException("Địa chỉ không được để trống");
			}
			if (string.IsNullOrEmpty(model.DienThoai))
			{
				throw new ArgumentException("Địa chỉ không được để trống");
			}
			if (!model.MaTrangThai.HasValue || model.MaTrangThai < 0)
			{
				throw new ArgumentException("Mã loại không hợp lệ hoặc chưa được nhập");
			}
			if (!model.PhiVanChuyen.HasValue || model.PhiVanChuyen < 0)
			{
				throw new ArgumentException("Phí vận chuyển không hợp lệ hoặc chưa được nhập");
			}
			if (string.IsNullOrEmpty(model.CachVanChuyen))
			{
				throw new ArgumentException("Cách vận chuyển không được để trống");
			}
			if (string.IsNullOrEmpty(model.CachThanhToan))
			{
				throw new ArgumentException("Cách thanh toán không được để trống");
			}
			var existingKh = await db.KhachHangs.FirstOrDefaultAsync(l => l.MaKh == model.MaKH);
			if (existingKh == null)
			{
				throw new ArgumentException($"Khách hàng {model.MaKH} không tồn tại trong hệ thống");
			}
			if (string.IsNullOrEmpty(model.HoTen))
			{
				throw new ArgumentException("Tên khách hàng chưa được nhập");
			}

			// Kiểm tra điều kiện cho ChiTietHds
			if (model.ChiTietHds == null || !model.ChiTietHds.Any())
			{
				throw new ArgumentException("Chi tiết hóa đơn không được để trống");
			}

			foreach (var chiTiet in model.ChiTietHds)
			{
				// Kiểm tra MaHH
				if (chiTiet.MaHH < 0)
				{
					throw new ArgumentException($"Mã hàng hóa không hợp lệ hoặc chưa được nhập tại chi tiết hóa đơn");
				}

				// Kiểm tra sự tồn tại của hàng hóa
				var existingHangHoa = await db.HangHoas.FirstOrDefaultAsync(h => h.MaHh == chiTiet.MaHH);
				if (existingHangHoa == null)
				{
					throw new ArgumentException($"Hàng hóa với mã {chiTiet.MaHH} không tồn tại trong hệ thống");
				}

				// Kiểm tra SoLuong
				if (chiTiet.SoLuong < 0)
				{
					throw new ArgumentException($"Số lượng không hợp lệ hoặc chưa được nhập tại chi tiết hóa đơn có mã hàng hóa {chiTiet.MaHH}");
				}

				// Kiểm tra DonGia
				if (chiTiet.DonGia < 0)
				{
					throw new ArgumentException($"Đơn giá không hợp lệ hoặc chưa được nhập tại chi tiết hóa đơn có mã hàng hóa {chiTiet.MaHH}");
				}

				// Kiểm tra MaGiamGia nếu có
				//if (chiTiet.MaGiamGia > 0)
				//{
				//	var existingGiamGia = await db.GiamGias.FirstOrDefaultAsync(g => g.MaGiamGia == chiTiet.MaGiamGia);
				//	if (existingGiamGia == null)
				//	{
				//		throw new ArgumentException($"Mã giảm giá {chiTiet.MaGiamGia} không tồn tại trong hệ thống");
				//	}
				//}
			}

			var hoadon = model.ToHoaDonDTO();
			await db.HoaDons.AddAsync(hoadon);
			await db.SaveChangesAsync();
			return hoadon;
		}
	}
}
