using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TDProjectMVC.Data;
using TDProjectMVC.ViewModels;

namespace TDProjectMVC.ViewComponents
{
    public class DanhGiaViewComponent : ViewComponent
    {
        private readonly Hshop2023Context db;
        private const int PageSize = 3;

        public DanhGiaViewComponent(Hshop2023Context db)
        {
            this.db = db;
        }

        public int GetReviewCountAsync(int maHH)
        {
            return db.DanhGiaSps.Count(d => d.MaHh == maHH && d.TrangThai == 1);
        }

        public IViewComponentResult Invoke(int maHH, int page = 1)
        {
            try
            {
                var reviewCount = GetReviewCountAsync(maHH);
                var totalPages = Math.Max(1, (int)Math.Ceiling(reviewCount / (double)PageSize));
                page = Math.Min(Math.Max(1, page), totalPages); // Ensure page is within valid range

                var data = db.DanhGiaSps
                    .Where(dg => dg.MaHh == maHH && dg.TrangThai == 1)
                    .OrderByDescending(dg => dg.Ngay)
                    .Skip((page - 1) * PageSize)
                    .Take(PageSize)
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
                    DanhGias = data ?? new List<DanhGiaVM>(), // Ensure DanhGias is never null
                    CurrentPage = page,
                    TotalPages = totalPages,
                    MaHH = maHH
                };

                return View("Default", model); // Change to match your actual view name
            }
            catch (Exception ex)
            {
                // Return empty model instead of null
                return View("Default", new DanhGiaListViewModel
                {
                    ReviewCount = 0,
                    DanhGias = new List<DanhGiaVM>(),
                    CurrentPage = 1,
                    TotalPages = 1,
                    MaHH = maHH
                });
            }
        }
    }
}
