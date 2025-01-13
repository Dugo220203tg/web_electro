using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TDProjectMVC.Data;
using TDProjectMVC.ViewModels;

namespace TDProjectMVC.Controllers
{
    public class WishListController : Controller
    {
        private readonly Hshop2023Context db;
        public WishListController(Hshop2023Context context)
        {
            db = context;
        }

        [HttpGet]
        public async Task<IActionResult> updateWishList()
        {
           var MaKh = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
           
            var YeuThichs = db.YeuThiches.AsQueryable()
                                         .Where(p => p.MaKh == MaKh);
            var result = await YeuThichs
                .Select(p => new WishListVM
                {
                    MaYT = p.MaYt,
                    MaHH = (int)p.MaHh,
                    TenHH = p.MaHhNavigation.TenHh,
                    DonGia = (double)p.MaHhNavigation.DonGia,
                    Hinh = p.MaHhNavigation.Hinh
                })
                .ToListAsync();

            // Trả về dữ liệu dưới dạng JSON
            return Json(result);
        }
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var MaKh = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var YeuThichs = db.YeuThiches.AsQueryable()
                                         .Where(p => p.MaKh == MaKh);
            var result = await YeuThichs
                .Select(p => new WishListVM
                {
                    MaYT = p.MaYt,
                    MaHH = (int)p.MaHh,
                    TenHH = p.MaHhNavigation.TenHh,
                    DonGia = (double)p.MaHhNavigation.DonGia,
                    Hinh = p.MaHhNavigation.Hinh
                })
                .ToListAsync();

            // Trả về dữ liệu dưới dạng JSON
            return View(result);
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddToWishList(int MaHH)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var existingItem = await db.YeuThiches.FirstOrDefaultAsync(y => y.MaHh == MaHH && y.MaKh == userId);

            if (existingItem != null)
            {
                return Json(new { success = false, message = "Product is already in wishlist" });
            }

            var yeuthich = new YeuThich
            {
                MaHh = MaHH,
                MaKh = userId,
                NgayChon = DateTime.Now,
            };

            db.Add(yeuthich);
            await db.SaveChangesAsync();
            return Json(new { success = true, message = "Product added to wishlist" });
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> RemoveWishList(int id)
        {
            try
            {
                var yeuthichremove = await db.YeuThiches.FirstOrDefaultAsync(p => p.MaYt == id);

                if (yeuthichremove == null)
                {
                    // Trường hợp sản phẩm không còn tồn tại
                    return Json(new { success = false, message = "Product not found or has already been removed." });
                }

                db.YeuThiches.Remove(yeuthichremove);
                await db.SaveChangesAsync();

                return Json(new { success = true, message = "Product removed from wishlist" });
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // Xử lý lỗi concurrency
                return StatusCode(409, new { success = false, message = "A concurrency error occurred.", details = ex.Message });
            }
            catch (Exception ex)
            {
                // Xử lý lỗi chung
                return StatusCode(500, new { success = false, message = "An error occurred while removing the product from the wishlist.", details = ex.Message });
            }
        }
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetWishListCount()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var count = await db.YeuThiches.CountAsync(p => p.MaKh == userId);
            return Json(new { count });
        }
    }
}