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
			var data = db.Loais.Select(lo => new HomeMenuVM
			{
				MaLoai = lo.MaLoai,
				TenLoai = lo.TenLoai,
			}).OrderBy(p => p.TenLoai);
			return View("homeMenu", data);
		}

	}
}
