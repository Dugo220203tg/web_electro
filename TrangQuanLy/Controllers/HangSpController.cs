using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PagedList;
using System.Text;
using TrangQuanLy.Helpers;
using TrangQuanLy.Models;

namespace TrangQuanLy.Controllers
{
    public class HangSpController : Controller
    {
        private readonly HttpClient _client;
        Uri baseAddress = new Uri("https://localhost:7109/api");
        public HangSpController()
        {
            _client = new HttpClient();
            _client.BaseAddress = baseAddress;
        }
        [HttpGet]
        [Authorize]
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
            List<HangSpViewMD> NhaCC = new List<HangSpViewMD>();
            HttpResponseMessage response = _client.GetAsync(_client.BaseAddress + "/HangSp/GetAll").Result;

            if (response.IsSuccessStatusCode)
            {
                string data = response.Content.ReadAsStringAsync().Result;
                NhaCC = JsonConvert.DeserializeObject<List<HangSpViewMD>>(data);
            }
            int totalItems = NhaCC.Count();
            var paginatedList = PaginatedList<HangSpViewMD>.CreateAsync(NhaCC.AsQueryable(), page ?? 1, pagesize ?? 5);
            ViewBag.Page = page;
            ViewBag.TotalPages = paginatedList.TotalPages;

            return View(paginatedList);
        }
        [HttpGet]
        public async Task<IActionResult> Search(string? query)
        {
            // Initialize HangHoaVM list to store search results
            List<HangSpViewMD> searchResult = new List<HangSpViewMD>();

            // Send a request to the API to get all HangHoa entities
            List<HangSpViewMD> LoaiSp = new List<HangSpViewMD>();
            HttpResponseMessage response = await _client.GetAsync(_client.BaseAddress + "/HangSp/GetAll");

            if (response.IsSuccessStatusCode)
            {
                string data = response.Content.ReadAsStringAsync().Result;
                LoaiSp = JsonConvert.DeserializeObject<List<HangSpViewMD>>(data);
            }
            else
            {
                return View("Error");
            }
            if (query != null)
            {
                searchResult = LoaiSp.Where(h => h.TenCongTy.Contains(query)).ToList();
                return View(searchResult);
            }
            if (query == null)
            {
                return View(LoaiSp);
            }
            return View();
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(HangSpViewMD model)
        {
            try
            {
                string data = JsonConvert.SerializeObject(model);
                StringContent content = new StringContent(data, Encoding.UTF8, "application/json");
                HttpResponseMessage response = _client.PostAsync(_client.BaseAddress + "/HangSp/Post", content).Result;

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
        public IActionResult Edit(string id)
        {
            try
            {
                HangSpViewMD LoaiSp = new HangSpViewMD();
                HttpResponseMessage respone = _client.GetAsync(_client.BaseAddress + "/HangSp/GetById/" + id).Result;
                if (respone.IsSuccessStatusCode)
                {
                    string data = respone.Content.ReadAsStringAsync().Result;
                    LoaiSp = JsonConvert.DeserializeObject<HangSpViewMD>(data);
                }
                return View(LoaiSp);

            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                return View();
            }
        }
        [HttpPost]
        public IActionResult Edit(HangSpViewMD model, string MaNcc)
        {
            try
            {
                string data = JsonConvert.SerializeObject(model);
                StringContent content = new StringContent(data, Encoding.UTF8, "application/json");
                HttpResponseMessage response = _client.PutAsync(_client.BaseAddress + "/HangSp/Update/" + MaNcc, content).Result;
                if (response.IsSuccessStatusCode)
                {
                    TempData["success"] = " Cập nhật thành công!";
                    return RedirectToAction("Index");
                }
                return View();
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                return View();
            }
        }

        [HttpPost]
        public IActionResult Delete(string MaNcc)
        {
            try
            {
                HttpResponseMessage response = _client.DeleteAsync(_client.BaseAddress + "/HangSp/Delete/" + MaNcc).Result;
                if (response.IsSuccessStatusCode)
                {
                    TempData["success"] = "Xóa thành công!";
                    return RedirectToAction("Index");
                }
                // Trả về danh sách dữ liệu nếu có
                return RedirectToAction("Index", "HangSp");
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                return View(); // Trả về View mặc định nếu có lỗi xảy ra
            }
        }


    }
}
