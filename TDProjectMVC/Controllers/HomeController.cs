using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TDProjectMVC.Data;
using TDProjectMVC.Models;
using TDProjectMVC.ViewModels;

namespace TDProjectMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
		private readonly Hshop2023Context db;

		public HomeController(ILogger<HomeController> logger, Hshop2023Context context)
        {
            _logger = logger;
			db = context;

		}

		public IActionResult Index(int? loai)
        {
			var hangHoas = db.HangHoas.AsQueryable();
			if (loai.HasValue)
			{
				hangHoas = hangHoas.Where(p => p.MaLoai == loai.Value);
			}
			var result = hangHoas.Select(p => new HangHoaVM
			{
				MaHH = p.MaHh,
				TenHH = p.TenHh,
				DonGia = p.DonGia ?? 0,
				Hinh = p.Hinh ?? "",
				MoTaNgan = p.MoTaDonVi ?? "",
				TenLoai = p.MaLoaiNavigation.TenLoai,
				GiamGia = p.GiamGia
			});
			return View(result);
		}
        [Route("/404")]
        public IActionResult PageNotFound()
        {
            return View();
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
