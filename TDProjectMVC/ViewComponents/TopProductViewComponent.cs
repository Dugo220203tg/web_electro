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
            var data = db.HangHoas.Select(lo => new HangHoaVM
            {
                MaHH = lo.MaHh,
                TenHH = lo.TenHh,
                DonGia = (double)lo.DonGia,
                TenLoai = lo.MaLoaiNavigation.TenLoai,
                Hinh = lo.Hinh ?? "",
                DiemDanhGia = lo.DanhGiaSps.Any() ? (int)Math.Round(lo.DanhGiaSps.Average(dg => dg.Sao ?? 0)) : 0

            }).OrderBy(p => p.DonGia);
            return View("Index", data);
        }
    }
}
