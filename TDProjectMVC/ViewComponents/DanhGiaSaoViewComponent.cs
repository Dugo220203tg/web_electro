using Microsoft.AspNetCore.Mvc;
using TDProjectMVC.Data;
using TDProjectMVC.ViewModels;
using System.Linq;

namespace TDProjectMVC.ViewComponents
{
    public class DanhGiaSaoViewComponent : ViewComponent
    {
        private readonly Hshop2023Context db;
        public DanhGiaSaoViewComponent(Hshop2023Context db)
        {
            this.db = db;
        }
        public IViewComponentResult Invoke(int maHH)
        {
            var data = db.DanhGiaSps
                .Where(dg => dg.MaHh == maHH)
                .ToList(); // Chuyển dữ liệu thành List để tránh thực hiện truy vấn đa lần

            var tongSao = data.Sum(dg => dg.Sao);
            var trungBinhSao = data.Count > 0 ? (double)tongSao / data.Count : 0;

            var motSao = data.Count(dg => dg.Sao == 1);
            var haiSao = data.Count(dg => dg.Sao == 2);
            var baSao = data.Count(dg => dg.Sao == 3);
            var bonSao = data.Count(dg => dg.Sao == 4);
            var namSao = data.Count(dg => dg.Sao == 5);

            var danhGiaVM = new DanhGiaVM
            {
                TrungBinhSao = trungBinhSao,
                MotSao = motSao,
                HaiSao = haiSao,
                BaSao = baSao,
                BonSao = bonSao,
                NamSao = namSao
            };

            return View("Index", new List<DanhGiaVM> { danhGiaVM });
        }
    }
}
