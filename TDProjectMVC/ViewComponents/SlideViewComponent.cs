using Microsoft.AspNetCore.Mvc;
using TDProjectMVC.Data;
using TDProjectMVC.ViewModels;

namespace TDProjectMVC.ViewComponents
{
    public class SlideViewComponent : ViewComponent
    {
        private readonly Hshop2023Context context;

        public SlideViewComponent(Hshop2023Context context)
        {
            this.context = context;
        }
        public IViewComponentResult Invoke(string name)
        {
            var data = context.Slides.Select(s => new SildeVM
            {
                image = s.Image,
                name = s.SlideName,
                id = s.Id,
            }).Where(s => s.name == "slide");
            return View("Index",data);
        }
    }
}
