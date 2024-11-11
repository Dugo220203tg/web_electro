using Microsoft.AspNetCore.Mvc;
using TDProjectMVC.Helpers;
using TDProjectMVC.ViewModels;

namespace TDProjectMVC.ViewComponents
{
    public class CartProductViewComponent : ViewComponent
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CartProductViewComponent(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public IViewComponentResult Invoke()
        {
            var cart = _httpContextAccessor.HttpContext.Session.Get<List<CartItem>>(MySetting.CART_KEY);
            var totalQuantity = cart?.Sum(p => p.SoLuong) ?? 0;
            var totalAmount = cart?.Sum(p => p.SoLuong * p.DonGia) ?? 0;

            var model = new
            {
                CardProducts = cart,
                TotalQuantity = totalQuantity,
                TotalAmount = totalAmount
            };

            return View(model);
        }
    }
}
