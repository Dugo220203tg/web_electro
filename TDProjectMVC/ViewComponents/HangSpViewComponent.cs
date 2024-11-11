using Microsoft.AspNetCore.Mvc;
using TDProjectMVC.Data;
using TDProjectMVC.ViewModels;

namespace TDProjectMVC.ViewComponents
{
	public class HangSpViewComponent : ViewComponent
	{
		private readonly Hshop2023Context db;

		public HangSpViewComponent(Hshop2023Context context) => db = context;

		public IViewComponentResult Invoke()
		{
			var data = db.NhaCungCaps.Select(lo => new HangSpVM
			{
				MaNCC = lo.MaNcc,
				TenCongTy = lo.TenCongTy,
				SoLuong = lo.HangHoas.Count,
			}).OrderBy(p => p.TenCongTy);
			return View("Index", data);
		}
	}
}
