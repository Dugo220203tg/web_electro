using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
//using PagedList;
using System.Text;
using TrangQuanLy.Helpers;
using TrangQuanLy.Models;

namespace TrangQuanLy.Controllers
{
    public class DanhMucController : Controller
    {
        private readonly HttpClient _client;
        Uri baseAddress = new Uri("https://localhost:7109/api");
        public DanhMucController()
        {
            _client = new HttpClient();
            _client.BaseAddress = baseAddress;
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
            List<DanhMucViewModel> DanhMuc = new List<DanhMucViewModel>();
            HttpResponseMessage response = _client.GetAsync(_client.BaseAddress + "/DanhMuc/GetAll").Result;

            if (response.IsSuccessStatusCode)
            {
                string data = response.Content.ReadAsStringAsync().Result;
                DanhMuc = JsonConvert.DeserializeObject<List<DanhMucViewModel>>(data);
            }
            int totalItems = DanhMuc.Count();
            var paginatedList = PaginatedList<DanhMucViewModel>.CreateAsync(DanhMuc.AsQueryable(), page ?? 1, pagesize ?? 5);
            ViewBag.Page = page;
            ViewBag.TotalPages = paginatedList.TotalPages;

            return View(paginatedList);
        }
        [HttpGet]
        public async Task<IActionResult> Search(string? query)
        {
            // Initialize HangHoaVM list to store search results
            List<DanhMucViewModel> searchResult = new List<DanhMucViewModel>();

            // Send a request to the API to get all HangHoa entities
            List<DanhMucViewModel> DanhMuc = new List<DanhMucViewModel>();
            HttpResponseMessage response = _client.GetAsync(_client.BaseAddress + "/DanhMuc/GetAll").Result;

            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                DanhMuc = JsonConvert.DeserializeObject<List<DanhMucViewModel>>(data);
            }
            else
            {
                return View("Error");
            }
            if (query != null)
            {
                searchResult = DanhMuc.Where(h => MyUtil.RemoveDiacritics(h.TenDanhMuc)
                    .IndexOf(MyUtil.RemoveDiacritics(query), StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                return View(searchResult);
            }
            if (query == null)
            {
                return View(DanhMuc);
            }
            return View();
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(DanhMucViewModel model)
        {
            try
            {
                string data = JsonConvert.SerializeObject(model);
                StringContent content = new StringContent(data, Encoding.UTF8, "application/json");
                HttpResponseMessage response = _client.PostAsync(_client.BaseAddress + "/DanhMuc/Post", content).Result;

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
                DanhMucViewModel DanhMuc = new DanhMucViewModel();
                HttpResponseMessage response = _client.GetAsync(_client.BaseAddress + "/DanhMuc/GetById/" + id).Result;
                if (response.IsSuccessStatusCode)
                {
                    string data = response.Content.ReadAsStringAsync().Result;
                    DanhMuc = JsonConvert.DeserializeObject<DanhMucViewModel>(data);
                }
                return View(DanhMuc);

            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                return View();
            }
        }

        [HttpPost]
        public IActionResult Edit(DanhMucViewModel model, int Maloai)
        {
            try
            {
                string data = JsonConvert.SerializeObject(model);
                StringContent content = new StringContent(data, Encoding.UTF8, "application/json");
                HttpResponseMessage response = _client.PutAsync(_client.BaseAddress + "/DanhMuc/Update/" + Maloai, content).Result;
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
        public IActionResult DeleteConfirm(int Maloai)
        {
            try
            {
                HttpResponseMessage response = _client.DeleteAsync(_client.BaseAddress + "/DanhMuc/Delete/" + Maloai).Result;
                if (response.IsSuccessStatusCode)
                {
                    TempData["success"] = "Xóa thành công!";
                    return RedirectToAction("Index");
                }
                return View("Index", "DanhMuc");
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                return View();
            }
        }
    }
}
