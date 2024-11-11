using Microsoft.AspNetCore.Mvc;
using TDProjectMVC.Data;
using TDProjectMVC.ViewModels;

namespace TDProjectMVC.ViewComponents
{
	public class TopProductViewComponent : ViewComponent
	{
		private readonly Hshop2023Context db;

		public TopProductViewComponent(Hshop2023Context context) => db = context;

		public IViewComponentResult Invoke(string type)
		{
			if (type == "type1")
			{
				var data = db.HangHoas.Select(lo => new HangHoaVM
				{
					TenHH = lo.TenHh,
					DonGia = (double)lo.DonGia,
					TenLoai = lo.MaLoaiNavigation.TenLoai,
					Hinh = lo.Hinh ?? ""
				}).OrderBy(p => p.DonGia);
				return View("Index", data);
			}
			else if (type == "type2")
			{
				var data = db.HangHoas.Select(lo => new HangHoaVM
				{
					TenHH = lo.TenHh,
					DonGia = (double)lo.DonGia,
					TenLoai = lo.MaLoaiNavigation.TenLoai,
					Hinh = lo.Hinh ?? ""
				}).OrderBy(p => p.DonGia);
				return View("Index2", data);
			}
			else
			{
				// Xử lý cho các trường hợp khác (nếu cần)
				// Trả về một giá trị mặc định hoặc throw một ngoại lệ
				return Content("Invalid type"); // Ví dụ trả về một chuỗi thông báo
			}
		}

	}
}
