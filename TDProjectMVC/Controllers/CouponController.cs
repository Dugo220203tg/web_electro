using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using TDProjectMVC.Data;
using TDProjectMVC.Helpers;
using TDProjectMVC.ViewModels;

namespace TDProjectMVC.Controllers
{
    public class CouponController : Controller
    {
        private readonly HttpClient _client;
        Uri baseAddress = new Uri("https://localhost:7109/api");
        private readonly Hshop2023Context db;

        public CouponController(Hshop2023Context context)
        {
            _client = new HttpClient();
            _client.BaseAddress = baseAddress;
            db = context;

        }
        [Authorize]
        [HttpGet]
        public IActionResult Index(int? page, int? pagesize)
        {
            if (page == null)
            {
                page = 1;
            }
            if (pagesize == null)
            {
                pagesize = 5;
            }
            ViewBag.PageSize = pagesize;
            //List<CouponVM> Coupon = new List<CouponVM>();
            //HttpResponseMessage response = _client.GetAsync(_client.BaseAddress + "/Coupon/GetAll").Result;
            //if (response.IsSuccessStatusCode)
            //{
            //    string data = response.Content.ReadAsStringAsync().Result;
            //    Coupon = JsonConvert.DeserializeObject<List<CouponVM>>(data);
            //}
            var database = db.Coupons.Select(c => new CouponVM
            {
                id = c.Id,
                Name = c.Name,
                Description = c.Description,
                price = (decimal)c.Price,
                DateEnd = (DateTime)c.DateEnd,
                DateStart = (DateTime)c.DateStart,
                Status = (int)c.Status,
                Quantity = (int)c.Quantity,
            });
            int totalItems = database.Count();
            var paginatedList = PaginatedList<CouponVM>.CreateAsync(database.AsQueryable(), page ?? 1, pagesize ?? 5);
            ViewBag.Page = page;
            ViewBag.TotalPages = paginatedList.TotalPages;

            return View(paginatedList);
        }
        [HttpGet]
        public async Task<IActionResult> Search(string? query)
        {
            // Initialize HangHoaVM list to store search results
            List<CouponVM> searchResult = new List<CouponVM>();

            // Send a request to the API to get all HangHoa entities
            List<CouponVM> Coupon = new List<CouponVM>();
            HttpResponseMessage response = _client.GetAsync(_client.BaseAddress + "/Coupon/GetAll").Result;

            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                Coupon = JsonConvert.DeserializeObject<List<CouponVM>>(data);
            }
            else
            {
                return View("Error");
            }
            if (query != null)
            {
                searchResult = Coupon.Where(h => MyUtil.RemoveDiacritics(h.Name)
                    .IndexOf(MyUtil.RemoveDiacritics(query), StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                return View(searchResult);
            }
            if (query == null)
            {
                return View(Coupon);
            }
            return View();
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(CouponVM model)
        {
            try
            {
                string data = JsonConvert.SerializeObject(model);
                StringContent content = new StringContent(data, Encoding.UTF8, "application/json");
                HttpResponseMessage response = _client.PostAsync(_client.BaseAddress + "/Coupon/Post", content).Result;

                if (response.IsSuccessStatusCode)
                {
                    TempData["success"] = "Thêm danh mục mới thành công ";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                return View();

            }
            return View();
        }
        [HttpGet]
        public IActionResult Edit(int id)
        {
            try
            {
                CouponVM Coupon = new CouponVM();
                HttpResponseMessage response = _client.GetAsync(_client.BaseAddress + "/Coupon/GetById/" + id).Result;
                if (response.IsSuccessStatusCode)
                {
                    string data = response.Content.ReadAsStringAsync().Result;
                    Coupon = JsonConvert.DeserializeObject<CouponVM>(data);
                }
                return View(Coupon);

            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                return View();
            }
        }

        [HttpPost]
        public IActionResult Edit(CouponVM model, int id)
        {
            try
            {
                string data = JsonConvert.SerializeObject(model);
                StringContent content = new StringContent(data, Encoding.UTF8, "application/json");
                HttpResponseMessage response = _client.PutAsync(_client.BaseAddress + "/Coupon/Update/" + id, content).Result;
                if (response.IsSuccessStatusCode)
                {
                    TempData["success"] = "Cập nhật thành công!";
                    return RedirectToAction("Index");
                }
                // Nếu có lỗi, trả về view và truyền model vào view
                return View();
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                // Nếu có lỗi, trả về view và truyền model vào view
                return View();
            }
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirm(int id)
        {
            try
            {
                HttpResponseMessage response = _client.DeleteAsync(_client.BaseAddress + "/Coupon/Delete/" + id).Result;
                if (response.IsSuccessStatusCode)
                {
                    TempData["success"] = "Xóa thành công!";
                    return RedirectToAction("Index");
                }
                return View("Index");
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                return View();
            }
        }
    }
}
