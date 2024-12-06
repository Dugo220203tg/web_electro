using Microsoft.AspNetCore.Mvc;

namespace TDProjectMVC.Controllers
{
    public class CouponController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
