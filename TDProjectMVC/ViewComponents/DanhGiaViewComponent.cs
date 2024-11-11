using Microsoft.AspNetCore.Mvc;
using TDProjectMVC.Data;
using TDProjectMVC.ViewModels;

namespace TDProjectMVC.ViewComponents
{
    public class DanhGiaViewComponent : ViewComponent
    {
        private readonly Hshop2023Context db;
        public DanhGiaViewComponent(Hshop2023Context db)
        {
            this.db = db;
        }
        public IViewComponentResult Invoke(int maHH)
        {
            var data = db.DanhGiaSps
                .Where(dg => dg.MaHh == maHH)
                .Select(lo => new DanhGiaVM
                {
                    MaKH = lo.MaKh,
                    Ngay = (DateTime)lo.Ngay,
                    NoiDung = lo.NoiDung,
                    MaHH = lo.MaHh,
                    Sao = (int)lo.Sao,
                })
                .ToList();
            return View("Index", data);
        }
    }
}
