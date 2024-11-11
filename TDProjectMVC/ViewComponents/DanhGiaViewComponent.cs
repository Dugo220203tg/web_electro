using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        public int GetReviewCountAsync(int maHH)
        {
            return  db.DanhGiaSps.Count(d => d.MaHh == maHH);
        }
        public IViewComponentResult Invoke(int maHH)
        {
            var reviewCount = GetReviewCountAsync(maHH);

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
            var model = new DanhGiaListViewModel
            {
                ReviewCount = reviewCount,
                DanhGias = data
            };
            return View("Index", data);
        }
    }
}
