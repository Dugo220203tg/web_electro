using API_Web_Shop_Electronic_TD.Data;
using API_Web_Shop_Electronic_TD.Models;

namespace API_Web_Shop_Electronic_TD.Mappers
{
	public static class HangHoaMappers
	{
		public static HangHoaMD ToHangHoaDo(this HangHoa Model)
		{
			if (Model == null)
			{
				return null; // Hoặc ném lỗi nếu cần
			}

			string TenLoai = Model.MaLoaiNavigation != null ? Model.MaLoaiNavigation.TenLoai : null;
			string TenNCC = Model.MaNccNavigation != null ? Model.MaNccNavigation.TenCongTy : null;
			string TenDanhMuc = Model.MaLoaiNavigation?.DanhMuc != null ? Model.MaLoaiNavigation.DanhMuc.TenDanhMuc : null;
			int MaDanhMuc = Model.MaLoaiNavigation?.DanhMuc != null ? Model.MaLoaiNavigation.DanhMuc.MaDanhMuc : 0;

			// Tính toán đánh giá trung bình và số lượng đánh giá
			double trungBinhSao = 0;
			int soLuongDanhGia = 0;
			var danhGiaHopLe = Model.DanhGiaSps?.Where(d => d.TrangThai == 1).ToList();

			if (danhGiaHopLe != null && danhGiaHopLe.Any())
			{
				trungBinhSao = danhGiaHopLe.Average(d => d.Sao ?? 0);
				soLuongDanhGia = danhGiaHopLe.Count;
			}

			var hangHoaMD = new HangHoaMD
			{
				MaHH = Model.MaHh,
				TenHH = Model.TenHh,
				Hinh = Model.Hinh,
				MoTa = Model.MoTa,
				MoTaDonVi = Model.MoTaDonVi,
				MaLoai = Model.MaLoai,
				NgaySX = Model.NgaySx,
				GiamGia = Model.GiamGia,
				MaNCC = Model.MaNcc,
				DonGia = (double)Model.DonGia,
				SoLanXem = (double)Model.SoLanXem,
				SoLuong = Model.SoLuong.HasValue ? (int)Model.SoLuong.Value : 0,
				TenLoai = TenLoai,
				TenNCC = TenNCC,
				TenDanhMuc = TenDanhMuc,
				MaDanhMuc = MaDanhMuc,
				TrungBinhSao = (decimal)trungBinhSao,
				SoLuongDanhGia = soLuongDanhGia
			};

			// Thêm danh sách đánh giá chi tiết nếu có
			if (danhGiaHopLe != null && danhGiaHopLe.Any())
			{
				hangHoaMD.danhGiaSpMDs = danhGiaHopLe.Select(d => new DanhGiaSpResponseMD
				{
					MaDg = d.MaDg,
					MaKH = d.MaKh,
					Sao = d.Sao ?? 0,
					NoiDung = d.NoiDung,
					Ngay = (DateTime)d.Ngay
				}).ToList();
			}
			return hangHoaMD;
		}


		public static AllHangHoaMD ToAllHangHoaDo(this HangHoa Model)
		{
			if (Model == null)
			{
				// Handle the case where Model is null
				return null; // Or throw an exception, depending on your requirements
			}

			string maLoai = Model.MaLoaiNavigation != null ? Model.MaLoaiNavigation.TenLoai : null;
			string maNcc = Model.MaNccNavigation != null ? Model.MaNccNavigation.TenCongTy : null;

			return new AllHangHoaMD
			{
				MaHH = Model.MaHh,
				TenHH = Model.TenHh,
				Hinh = Model.Hinh,
				MoTa = Model.MoTa,
				MoTaDonVi = Model.MoTaDonVi,
				MaLoai = maLoai,
				NgaySX = Model.NgaySx,
				GiamGia = Model.GiamGia,
				MaNCC = maNcc,
				DonGia = (double)Model.DonGia,
				SoLanXem = (double)Model.SoLanXem,
				SoLuong = (int)Model.SoLuong
			};
		}

		public static HangHoa ToHangHoaCreateDTO(this CreateHangHoaMD Model)
		{

			return new HangHoa
			{
				TenHh = Model.TenHH,
				Hinh = Model.Hinh,
				MoTa = Model.MoTa,
				MoTaDonVi = Model.MoTaDonVi,
				MaLoai = (int)Model.MaLoai,
				NgaySx = (DateOnly)Model.NgaySX,
				GiamGia = (double)Model.GiamGia,
				MaNcc = Model.MaNCC,
				DonGia = Model.DonGia,
				SoLuong = Model.SoLuong
			};
		}
		public static HangHoaMD ToTrangThaiDo(this HangHoa Model)
		{
			return new HangHoaMD
			{
				MaHH = Model.MaHh,
				TenHH = Model.TenHh,
				Hinh = Model.Hinh,
			};
		}
	}
}
