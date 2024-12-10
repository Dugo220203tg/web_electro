using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using TrangQuanLy.Models;
using Microsoft.AspNetCore.Authorization;
using TrangQuanLy.Helpers;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Drawing.Printing;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;

namespace TrangQuanLy.Controllers
{
    public class HangHoaController : Controller
    {
        private readonly HttpClient _client;
        Uri baseAddress = new Uri("https://localhost:7109/api");
        public HangHoaController()
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
                pagesize = 9;
            }
            ViewBag.PageSize = pagesize;

            List<HangHoaVM> Hanghoa = new List<HangHoaVM>();
            HttpResponseMessage response = await _client.GetAsync(_client.BaseAddress + "/HangHoa/GetAll");

            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                Hanghoa = JsonConvert.DeserializeObject<List<HangHoaVM>>(data);
            }

            int totalItems = Hanghoa.Count();

            // Creating PaginatedList
            var paginatedList = PaginatedList<HangHoaVM>.CreateAsync(Hanghoa.AsQueryable(), page ?? 1, pagesize ?? 9);

            ViewBag.Page = page;
            ViewBag.TotalPages = paginatedList.TotalPages;

            return View(paginatedList);
        }
        [HttpGet]
        [Authorize]

        public async Task<IActionResult> Search(string? query, int? page, int? pagesize)
        {
            if (page == null)
            {
                page = 1;
            }
            if (pagesize == null)
            {
                pagesize = 9;
            }
            // Initialize HangHoaVM list to store search results
            List<HangHoaVM> searchResult = new List<HangHoaVM>();

            // Send a request to the API to get all HangHoa entities
            HttpResponseMessage response = await _client.GetAsync(_client.BaseAddress + "/HangHoa/GetAll");

            if (response.IsSuccessStatusCode)
            {
                string data = response.Content.ReadAsStringAsync().Result;
                List<HangHoaVM> Hanghoa = JsonConvert.DeserializeObject<List<HangHoaVM>>(data);

                if (!string.IsNullOrEmpty(query))
                {
                    searchResult = Hanghoa.Where(h =>
                    MyUtil.RemoveDiacritics(h.TenHH).IndexOf(MyUtil.RemoveDiacritics(query), StringComparison.OrdinalIgnoreCase) >= 0 ||
                                MyUtil.RemoveDiacritics(h.TenLoai).IndexOf(MyUtil.RemoveDiacritics(query), StringComparison.OrdinalIgnoreCase) >= 0 ||
                                MyUtil.RemoveDiacritics(h.Hinh).IndexOf(MyUtil.RemoveDiacritics(query), StringComparison.OrdinalIgnoreCase) >= 0 ||
                                MyUtil.RemoveDiacritics(h.MoTa).IndexOf(MyUtil.RemoveDiacritics(query), StringComparison.OrdinalIgnoreCase) >= 0
                            ).ToList();
                }
                else
                {
                    searchResult = Hanghoa;
                }
            }
            else
            {
                return View("Error");
            }
            var paginatedList = PaginatedList<HangHoaVM>.CreateAsync(searchResult.AsQueryable(), page.Value, pagesize.Value);
            // Pass the current page and page size as ViewBag for rendering pagination controls in the view
            ViewBag.Page = page;
            ViewBag.TotalPages = paginatedList.TotalPages;
            ViewBag.PageSize = pagesize;
            ViewBag.Query = query;  // Truyền từ khóa tìm kiếm để giữ nguyên khi phân trang

            return View(paginatedList);
        }
        [Authorize]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [Authorize]
        [HttpPost]
        public IActionResult Create(CreateHangHoaVM model)
        {
            try
            {
                // Serialize the model
                string data = JsonConvert.SerializeObject(model);

                System.Diagnostics.Debug.WriteLine("Serialized Data: " + data);

                StringContent content = new StringContent(data, Encoding.UTF8, "application/json");

                // Post the data to the API
                HttpResponseMessage response = _client.PostAsync(_client.BaseAddress + "/HangHoa/Post", content).Result;
                // Serial hóa dữ liệu từ model thành JSON

                // Ghi dữ liệu đã serial hóa ra cửa sổ Output trong Visual Studio
                System.Diagnostics.Debug.WriteLine("Response status: " + response.StatusCode);
                System.Diagnostics.Debug.WriteLine("Response content: " + response.Content.ReadAsStringAsync().Result);

                // Check if the response is successful
                if (response.IsSuccessStatusCode)
                {
                    TempData["success"] = "Tạo đơn hàng thành công";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["error"] = "Không thể thêm sản phẩm. Vui lòng kiểm tra lại.";
                    return View(model); // Return the model to retain form data on error
                }
            }
            catch (Exception ex)
            {
                // Log the error message and return the model
                TempData["error"] = $"Có lỗi xảy ra: {ex.Message}";
                return View(model); // Return the model to keep form data
            }
        }
        [Authorize]
        [HttpGet]
        [ActionName("Edit")]
        public IActionResult Edit_Get(int id)
        {
            try
            {
                HangHoaVM hanghoa = new HangHoaVM();
                HttpResponseMessage response = _client.GetAsync(_client.BaseAddress + "/HangHoa/GetById/" + id).Result;
                if (response.IsSuccessStatusCode)
                {
                    string data = response.Content.ReadAsStringAsync().Result;
                    hanghoa = JsonConvert.DeserializeObject<HangHoaVM>(data);
                }
                return View(hanghoa);
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                return View();
            }
        }

        [Authorize]
        [HttpPost]
        [ActionName("Edit")]
        public IActionResult Edit_Post(AllHangHoaVM model, int MaHH)
        {
            try
            {
                if (model == null)
                {
                    TempData["error"] = "Dữ liệu không hợp lệ!";
                    return View();
                }

                HttpResponseMessage response = _client.GetAsync(_client.BaseAddress + "/HangHoa/GetById/" + MaHH).Result;

                if (!response.IsSuccessStatusCode)
                {
                    TempData["error"] = "Không tìm thấy sản phẩm!";
                    return RedirectToAction("Index");
                }

                var existingProduct = JsonConvert.DeserializeObject<AllHangHoaVM>(response.Content.ReadAsStringAsync().Result);

                if (existingProduct == null)
                {
                    TempData["error"] = "Sản phẩm không hợp lệ!";
                    return RedirectToAction("Index");
                }

                if (string.IsNullOrEmpty(model.Hinh))
                {
                    model.Hinh = existingProduct.Hinh;
                }

                string data = JsonConvert.SerializeObject(model);
                StringContent content = new StringContent(data, Encoding.UTF8, "application/json");

                response = _client.PutAsync(_client.BaseAddress + "/HangHoa/Update/" + MaHH, content).Result;

                if (response.IsSuccessStatusCode)
                {
                    TempData["success"] = "Cập nhật thành công!";
                    return RedirectToAction("Index");
                }

                TempData["error"] = "Cập nhật không thành công!";
                return View();
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                return View();
            }
        }

        [Authorize]
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirm(int MaHH)
        {
            try
            {
                HttpResponseMessage response = _client.DeleteAsync(_client.BaseAddress + "/HangHoa/Delete/" + MaHH).Result;
                if (response.IsSuccessStatusCode)
                {
                    TempData["success"] = "Xóa thành công!";
                    return RedirectToAction("Index");
                }
                return View("Index", "HangHoa");
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                return View();
            }
        }
    }
}
