using Microsoft.AspNetCore.Mvc;
using TDProjectMVC.Data;
using TDProjectMVC.ViewModels;

namespace TDProjectMVC.ViewComponents
{

	public class HomeMenuViewComponent : ViewComponent
	{
		private readonly Hshop2023Context db;

		public HomeMenuViewComponent(Hshop2023Context context) => db = context;

		public IViewComponentResult Invoke()
		{
            var currentDanhMuc = HttpContext.Request.Query["danhmuc"].ToString();

            var data = db.DanhMucSps.Select(lo => new DanhMucVM
            {
                ID = lo.MaDanhMuc,
                TenDanhMuc = lo.TenDanhMuc,
                IsSelected = !string.IsNullOrEmpty(currentDanhMuc) &&
                            lo.MaDanhMuc.ToString() == currentDanhMuc
            }).OrderBy(p => p.TenDanhMuc);
            return View("homeMenu", data);
		}

	}
}
