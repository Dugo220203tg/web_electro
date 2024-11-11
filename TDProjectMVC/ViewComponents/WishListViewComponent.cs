using TDProjectMVC.Data;
using Microsoft.AspNetCore.Mvc;
using TDProjectMVC.ViewModels;
using System.Security.Claims;

namespace TDProjectMVC.ViewComponents
{
    public class WishListViewComponent : ViewComponent
    {
        public readonly Hshop2023Context db;
        public WishListViewComponent(Hshop2023Context db)
        {
            this.db = db;
        }
        public IViewComponentResult Invoke()
        {
            // Lấy ID của người dùng hiện tại, bạn cần thay đổi dòng này tùy theo cách bạn lấy thông tin người dùng đã đăng nhập trong ứng dụng của mình
            //var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userID = @User.Identity.Name;
            // Lấy danh sách yêu thích của người dùng hiện tại
            var data = db.YeuThiches
                            .Where(yt => yt.MaKh == userID)
                            .Select(lo => new WishListVM
                            {
                                MaYT = lo.MaYt,
                                MaKh = lo.MaKh,
                                MaHH = (int)lo.MaHh,
                                NgayChon = (DateTime)lo.NgayChon,
                                TenHH = lo.MaHhNavigation.TenHh,
                                Hinh = lo.MaHhNavigation.Hinh,
                                DonGia = (double)lo.MaHhNavigation.DonGia,
                                TenNCC = lo.MaHhNavigation.MaNccNavigation.TenCongTy,
                            })
                            .OrderBy(p => p.TenHH)
                            .ToList(); // Thêm ToList() để thực thi truy vấn LINQ và lấy danh sách kết quả

            return View("Index",data);
        }

    }
}
