using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var maKh = User.Identity.Name;
            var wishListItems = await GetWishListItems(maKh);
            return View(wishListItems);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetWishListContent()
        {
            var maKh = User.Identity.Name;
            var wishListItems = await GetWishListItems(maKh);
            return PartialView("_WishListItems", wishListItems);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetWishListCount()
        {
            var maKh = User.Identity.Name;
            var count = await db.YeuThiches.CountAsync(y => y.MaKh == maKh);
            return Json(count);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddToWishList(int maHH)
        {
            try
            {
                var userId = User.Identity.Name;

                var existingItem = await db.YeuThiches
                    .FirstOrDefaultAsync(y => y.MaHh == maHH && y.MaKh == userId);

                if (existingItem != null)
                {
                    return Json(new { success = false, message = "Sản phẩm đã có trong mục Yêu Thích" });
                }

                var yeuthich = new YeuThich
                {
                    MaHh = maHH,
                    MaKh = userId,
                    NgayChon = DateTime.Now,
                };

                db.Add(yeuthich);
                await db.SaveChangesAsync();

                // Get updated count
                var newCount = await db.YeuThiches.CountAsync(y => y.MaKh == userId);

                return Json(new
                {
                    success = true,
                    message = "Thêm sản phẩm thành công",
                    count = newCount
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Đã xảy ra lỗi khi thêm sản phẩm",
                    details = ex.Message
                });
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> RemoveWishList(int id)
        {
            try
            {
                var userId = User.Identity.Name;
                var yeuthichremove = await db.YeuThiches
                    .FirstOrDefaultAsync(p => p.MaYt == id && p.MaKh == userId);

                if (yeuthichremove == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy sản phẩm hoặc đã bị xóa" });
                }

                db.YeuThiches.Remove(yeuthichremove);
                await db.SaveChangesAsync();

                // Get updated count
                var newCount = await db.YeuThiches.CountAsync(y => y.MaKh == userId);

                return Json(new
                {
                    success = true,
                    message = "Đã xóa sản phẩm khỏi danh sách yêu thích",
                    count = newCount
                });
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return StatusCode(409, new
                {
                    success = false,
                    message = "Lỗi đồng thời khi cập nhật dữ liệu",
                    details = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Đã xảy ra lỗi khi xóa sản phẩm",
                    details = ex.Message
                });
            }
        }

        // Helper method to get wishlist items
        private async Task<List<WishListVM>> GetWishListItems(string maKh)
        {
            return await db.YeuThiches
                .Where(p => p.MaKh == maKh)
                .Select(p => new WishListVM
                {
                    MaYT = p.MaYt,
                    MaHH = (int)p.MaHh,
                    TenHH = p.MaHhNavigation.TenHh,
                    DonGia = (double)p.MaHhNavigation.DonGia,
                    Hinh = p.MaHhNavigation.Hinh
                })
                .ToListAsync();
        }
    }
}
