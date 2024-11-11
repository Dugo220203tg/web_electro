using Microsoft.AspNetCore.Mvc;
using TDProjectMVC.Data;
using TDProjectMVC.ViewModels;

namespace TDProjectMVC.ViewComponents
{

    public class Product_in_DetailViewComponent : ViewComponent
    {
        private readonly Hshop2023Context db;

        public Product_in_DetailViewComponent(Hshop2023Context context) => db = context;

        public IViewComponentResult Invoke()
        {
                var data = db.HangHoas.Select(lo => new HangHoaVM
                {
                    MaHH = lo.MaHh,
                    TenHH = lo.TenHh,
                    DonGia = (double)lo.DonGia,
                    TenLoai = lo.MaLoaiNavigation.TenLoai,
                    Hinh = lo.Hinh ?? ""
                }).OrderBy(p => p.DonGia);
                return View("Index", data);

        }
    }
}
