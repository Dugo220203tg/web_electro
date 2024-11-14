using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PagedList;
using System.Text;
using TrangQuanLy.Helpers;
using TrangQuanLy.Models;

namespace TrangQuanLy.Controllers
{
    public class DanhGiaSpController : Controller
    {
        private readonly HttpClient _client;
        Uri baseAddress = new Uri("https://localhost:7109/api");
        public DanhGiaSpController()
        {
            _client = new HttpClient();
            _client.BaseAddress = baseAddress;
        }
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Index(int? page, int? pagesize)
        {
            if (page == null)
            {
                page = 1;
            }
            if (pagesize == null)
            {
                pagesize = 5;
            }
            List<DanhGiaSpMD> DanhGia = new List<DanhGiaSpMD>();
            HttpResponseMessage response = _client.GetAsync(_client.BaseAddress + "/DanhGiaSp/GetAll").Result;

            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                DanhGia = JsonConvert.DeserializeObject<List<DanhGiaSpMD>>(data);
            }
            int totalItems = DanhGia.Count();
            var paginatedList = PaginatedList<DanhGiaSpMD>.CreateAsync(DanhGia.AsQueryable(), page ?? 1, pagesize ?? 5);
            ViewBag.Page = page;
            ViewBag.TotalPages = paginatedList.TotalPages;
            ViewBag.PageSize = pagesize;

            return View(paginatedList);
        }
        [HttpGet]
        public async Task<IActionResult> Search(string? query)
        {
            // Initialize HangHoaVM list to store search results
            List<DanhGiaSpMD> searchResult = new List<DanhGiaSpMD>();

            // Send a request to the API to get all HangHoa entities
            List<DanhGiaSpMD> DanhGiaSp = new List<DanhGiaSpMD>();
            HttpResponseMessage response =  _client.GetAsync(_client.BaseAddress + "/DanhGiaSp/GetAll").Result;

            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                DanhGiaSp = JsonConvert.DeserializeObject<List<DanhGiaSpMD>>(data);
            }
            else
            {
                return View("Error");
            }
            if (query != null)
            {
                searchResult = DanhGiaSp.Where(h => MyUtil.RemoveDiacritics(h.TenHangHoa)
                    .IndexOf(MyUtil.RemoveDiacritics(query), StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                return View(searchResult);
            }
            if (query == null)
            {
                return View(DanhGiaSp);
            }
            return View();
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(DanhGiaSpMD model)
        {
            try
            {
                string data = JsonConvert.SerializeObject(model);
                StringContent content = new StringContent(data, Encoding.UTF8, "application/json");
                HttpResponseMessage response = _client.PostAsync(_client.BaseAddress + "/DanhGiaSp/Post", content).Result;

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
                DanhGiaSpMD DanhGiaSp = new DanhGiaSpMD();
                HttpResponseMessage response = _client.GetAsync(_client.BaseAddress + "/DanhGiaSp/GetById/" + id).Result;
                if (response.IsSuccessStatusCode)
                {
                    string data = response.Content.ReadAsStringAsync().Result;
                    DanhGiaSp = JsonConvert.DeserializeObject<DanhGiaSpMD>(data);
                }
                return View(DanhGiaSp);

            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                return View();
            }
        }

        [HttpPost]
        public IActionResult Edit(DanhGiaSpMD model, int MaDg)
        {
            try
            {
                string data = JsonConvert.SerializeObject(model);
                StringContent content = new StringContent(data, Encoding.UTF8, "application/json");
                HttpResponseMessage response = _client.PutAsync(_client.BaseAddress + "/DanhGiaSp/Update/" + MaDg, content).Result;
                if (response.IsSuccessStatusCode)
                {
                    TempData["success"] = "Employee Update!";
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


        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirm(int MaDg)
        {
            try
            {
                HttpResponseMessage response = _client.DeleteAsync(_client.BaseAddress + "/DanhGiaSp/Delete/" + MaDg).Result;
                if (response.IsSuccessStatusCode)
                {
                    TempData["success"] = "Xóa thành công!";
                    return RedirectToAction("Index");
                }
                return View("Index", "DanhGiaSp");
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                return View();
            }
        }
    }
}