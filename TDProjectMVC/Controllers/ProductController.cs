using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PagedList;
using System;
using System.Diagnostics.Metrics;
using System.Drawing.Printing;
using TDProjectMVC.Data;
using TDProjectMVC.ViewModels;
using TDProjectMVC.Helpers;

namespace TDProjectMVC.Controllers
{
    public class ProductController : Controller
    {
        private readonly Hshop2023Context db;

        public ProductController(Hshop2023Context context)
        {
            db = context;
        }

        public IActionResult Index(int? danhmuc, string? hang, int? loai, decimal? minPrice, decimal? maxPrice, int? page, int? pageSize)
        {
            int pageIndex = page ?? 1;
            int pageSizeValue = pageSize ?? 9;
            ViewBag.PageSize = pageSizeValue;

            var hangHoas = db.HangHoas.AsQueryable();

            if (loai.HasValue)
            {
                hangHoas = hangHoas.Where(p => p.MaLoai == loai.Value);
            }
            if (hang != null)
            {
                hangHoas = hangHoas.Where(p => p.MaNcc == hang);
            }
            if (danhmuc != null)
            {
                hangHoas = hangHoas.Where(p => p.MaLoaiNavigation.DanhMucId == danhmuc);
            }

            // Lọc sản phẩm theo khoảng giá
            if (minPrice.HasValue)
            {
                hangHoas = hangHoas.Where(p => (decimal?)p.DonGia >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                hangHoas = hangHoas.Where(p => (decimal?)p.DonGia <= maxPrice.Value);
            }
            var result = hangHoas.Select(p => new HangHoaVM
            {
                MaHH = p.MaHh,
                TenHH = p.TenHh,
                DonGia = p.DonGia ?? 0,
                Hinh = p.Hinh ?? "",
                MoTaNgan = p.MoTaDonVi ?? "",
                TenLoai = p.MaLoaiNavigation.TenLoai,
                GiamGia = p.GiamGia,
                MaNCC = p.MaNccNavigation.TenCongTy,
                ML = p.MaLoai,
                NCC = p.MaNcc,
                DiemDanhGia = p.DanhGiaSps.Any() ? (int)Math.Round(p.DanhGiaSps.Average(dg => dg.Sao ?? 0)) : 0
            }).ToList(); // Thực hiện ToList() tại đây

            int totalItems = result.Count();
            var paginatedList = PaginatedList<HangHoaVM>.CreateAsync(result, pageIndex, pageSizeValue);
            ViewBag.TotalPages = paginatedList.TotalPages;
            ViewBag.Page = pageIndex;
            ViewBag.Loai = loai;
            ViewBag.hang = hang;

            return View(paginatedList);
        }



        [HttpGet]
        public async Task<IActionResult> Search(string? query)
        {
            var hangHoas = db.HangHoas.AsQueryable();
            if (!string.IsNullOrEmpty(query))
            {
                hangHoas = hangHoas.Where(p => p.TenHh.Contains(query) || p.MaLoaiNavigation.TenLoai.Contains(query));
            }

            var result = await hangHoas.Select(p => new HangHoaVM
            {
                MaHH = p.MaHh,
                TenHH = p.TenHh,
                DonGia = p.DonGia ?? 0,
                Hinh = p.Hinh ?? "",
                MoTaNgan = p.MoTaDonVi ?? "",
                TenLoai = p.MaLoaiNavigation.TenLoai
            }).ToListAsync();

            return View(result);
        }

        public IActionResult Detail(int id)
        {
            var data = db.HangHoas
                .Include(p => p.MaLoaiNavigation)
                .Include(p => p.MaNccNavigation)
                .Include(p => p.DanhGiaSps)
                .SingleOrDefault(p => p.MaHh == id);

            if (data == null)
            {
                TempData["Message"] = $"Không tìm thấy sản phẩm có mã {id}";
                return Redirect("/404");
            }

            // Increment view count
            data.SoLanXem += 1;
            db.SaveChanges();

            // Get related products
            var relatedProducts = db.HangHoas
                .Where(p => p.MaLoai == data.MaLoai && p.MaHh != data.MaHh)
                .Take(4)
                .Select(p => new HangHoaVM
                {
                    MaHH = p.MaHh,
                    TenHH = p.TenHh,
                    DonGia = p.DonGia ?? 0,
                    Hinh = p.Hinh ?? "",
                    MoTaNgan = p.MoTaDonVi ?? "",
                    TenLoai = p.MaLoaiNavigation.TenLoai,
                    GiamGia = p.GiamGia,
                    DiemDanhGia = p.DanhGiaSps.Any() ? (int)Math.Round(p.DanhGiaSps.Average(dg => dg.Sao ?? 0)) : 0
                })
                .ToList();
            ViewBag.RelatedProducts = relatedProducts;
            // Get image URLs
            var imageUrls = new List<string>();
            if (!string.IsNullOrEmpty(data.Hinh))
            {
                imageUrls = data.Hinh.Split(',').ToList();
            }

            // Calculate average rating
            double diemDanhGia = data.DanhGiaSps.Any() ? data.DanhGiaSps.Average(dg => dg.Sao ?? 0) : 0;
            int countdg = db.DanhGiaSps.Count(d => d.MaHh == id);
            var result = new HangHoaVM
            {
                MaHH = data.MaHh,
                TenHH = data.TenHh,
                MoTa = data.MoTa,
                MaNCC = data.MaNccNavigation != null ? data.MaNccNavigation.TenCongTy : "",
                DonGia = data.DonGia ?? 0,
                Hinh = data.Hinh ?? "",
                MoTaNgan = data.MoTaDonVi ?? "",
                TenLoai = data.MaLoaiNavigation.TenLoai,
                ML = data.MaLoai,
                NCC = data.MaNcc,
                SoLuong = 10,
                DiemDanhGia = (int)diemDanhGia,  // Assign calculated rating
                ImageUrls = imageUrls,
                CountDg = countdg
            };

            return View(result);
        }
        [HttpGet]
        public IActionResult LoadReviews(int maHH, int page = 1)
        {
            return ViewComponent("DanhGia", new { maHH = maHH, page = page });
        }
    }
}
