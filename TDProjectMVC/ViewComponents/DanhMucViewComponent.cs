using Microsoft.AspNetCore.Mvc;
using TDProjectMVC.Data;
using TDProjectMVC.ViewModels;

namespace TDProjectMVC.ViewComponents
{
	public class DanhMucViewComponent : ViewComponent
	{
		private readonly Hshop2023Context db;

		public DanhMucViewComponent(Hshop2023Context context) => db = context;

		public IViewComponentResult Invoke()
		{
			var data = db.DanhMucSps.Select(lo => new DanhMucVM
			{
				ID = lo.MaDanhMuc,
				TenDanhMuc = lo.TenDanhMuc,
			}).OrderBy(p => p.TenDanhMuc);
			return View("Index", data);
		}
	}
}
