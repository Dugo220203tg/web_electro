﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using TrangQuanLy.Helpers;
using TrangQuanLy.Models;

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
        public async Task<IActionResult> Index(int? page, int? pagesize, int? categoryId, string sortOrder)
        {
            if (page == null) page = 1;
            if (pagesize == null) pagesize = 7;

            ViewBag.PageSize = pagesize;
            ViewBag.CurrentSort = sortOrder;

            // Sort parameters for view
            ViewBag.PriceSortParm = string.IsNullOrEmpty(sortOrder) ? "price_desc" : "";
            ViewBag.QuantitySortParm = sortOrder == "quantity" ? "quantity_desc" : "quantity";
            ViewBag.CategoryId = categoryId;

            // Get categories for dropdown
            List<DanhMucViewModel> categories = new List<DanhMucViewModel>();
            HttpResponseMessage catResponse = await _client.GetAsync(_client.BaseAddress + "/DanhMuc/GetAll");
            if (catResponse.IsSuccessStatusCode)
            {
                string catData = await catResponse.Content.ReadAsStringAsync();
                categories = JsonConvert.DeserializeObject<List<DanhMucViewModel>>(catData);
            }
            ViewBag.Categories = categories;

            // Get products
            List<HangHoaVM> hangHoa = new List<HangHoaVM>();
            string endpoint = categoryId.HasValue
                ? $"/HangHoa/GetByDanhMuc/{categoryId}"
                : "/HangHoa/GetAll";

            HttpResponseMessage response = await _client.GetAsync(_client.BaseAddress + endpoint);
            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                hangHoa = JsonConvert.DeserializeObject<List<HangHoaVM>>(data);
            }

            // Apply sorting
            switch (sortOrder)
            {
                case "price_desc":
                    hangHoa = hangHoa.OrderByDescending(h => h.DonGia).ToList();
                    break;
                case "quantity":
                    hangHoa = hangHoa.OrderBy(h => h.SoLuong).ToList();
                    break;
                case "quantity_desc":
                    hangHoa = hangHoa.OrderByDescending(h => h.SoLuong).ToList();
                    break;
                default: // Default sort by price ascending
                    hangHoa = hangHoa.OrderBy(h => h.DonGia).ToList();
                    break;
            }

            int totalItems = hangHoa.Count();
            var paginatedList = PaginatedList<HangHoaVM>.CreateAsync(hangHoa.AsQueryable(), page ?? 1, pagesize ?? 9);

            ViewBag.Page = page;
            ViewBag.TotalPages = paginatedList.TotalPages;

            return View(paginatedList);
        }
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Search(string? query, int? page, int? pagesize, int? categoryId, string sortOrder)
        {
            if (page == null) page = 1;
            if (pagesize == null) pagesize = 7;

            ViewBag.PageSize = pagesize;
            ViewBag.CurrentSort = sortOrder;
            ViewBag.Query = query;

            // Sort parameters for view
            ViewBag.PriceSortParm = string.IsNullOrEmpty(sortOrder) ? "price_desc" : "";
            ViewBag.QuantitySortParm = sortOrder == "quantity" ? "quantity_desc" : "quantity";
            ViewBag.CategoryId = categoryId;

            // Get categories for dropdown
            List<DanhMucViewModel> categories = new List<DanhMucViewModel>();
            HttpResponseMessage catResponse = await _client.GetAsync(_client.BaseAddress + "/DanhMuc/GetAll");
            if (catResponse.IsSuccessStatusCode)
            {
                string catData = await catResponse.Content.ReadAsStringAsync();
                categories = JsonConvert.DeserializeObject<List<DanhMucViewModel>>(catData);
            }
            ViewBag.Categories = categories;

            // Get products and apply search
            List<HangHoaVM> hangHoa = new List<HangHoaVM>();
            HttpResponseMessage response = await _client.GetAsync(_client.BaseAddress + "/HangHoa/GetAll");

            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                hangHoa = JsonConvert.DeserializeObject<List<HangHoaVM>>(data);

                // Apply search filter
                if (!string.IsNullOrEmpty(query))
                {
                    hangHoa = hangHoa.Where(h =>
                        MyUtil.RemoveDiacritics(h.TenHH).IndexOf(MyUtil.RemoveDiacritics(query), StringComparison.OrdinalIgnoreCase) >= 0 ||
                        MyUtil.RemoveDiacritics(h.TenLoai).IndexOf(MyUtil.RemoveDiacritics(query), StringComparison.OrdinalIgnoreCase) >= 0 ||
                        MyUtil.RemoveDiacritics(h.Hinh).IndexOf(MyUtil.RemoveDiacritics(query), StringComparison.OrdinalIgnoreCase) >= 0 ||
                        MyUtil.RemoveDiacritics(h.MoTa).IndexOf(MyUtil.RemoveDiacritics(query), StringComparison.OrdinalIgnoreCase) >= 0
                    ).ToList();
                }

                if (categoryId.HasValue)
                {
                    hangHoa = hangHoa.Where(h => h.maDanhMuc == categoryId.Value).ToList();
                }

                // Apply sorting
                switch (sortOrder)
                {
                    case "price_desc":
                        hangHoa = hangHoa.OrderByDescending(h => h.DonGia).ToList();
                        break;
                    case "quantity":
                        hangHoa = hangHoa.OrderBy(h => h.SoLuong).ToList();
                        break;
                    case "quantity_desc":
                        hangHoa = hangHoa.OrderByDescending(h => h.SoLuong).ToList();
                        break;
                    default: // Default sort by price ascending
                        hangHoa = hangHoa.OrderBy(h => h.DonGia).ToList();
                        break;
                }
            }
            else
            {
                return View("Error");
            }

            var paginatedList = PaginatedList<HangHoaVM>.CreateAsync(hangHoa.AsQueryable(), page.Value, pagesize.Value);
            ViewBag.Page = page;
            ViewBag.TotalPages = paginatedList.TotalPages;

            return View("Index", paginatedList);  // Use the Index view for search results
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
        [Route("HangHoa/Edit/{MaHH}")]
        public async Task<IActionResult> Edit(int MaHH, string returnUrl = null)
        {
            try
            {
                HangHoaVM hanghoa = new HangHoaVM();
                HttpResponseMessage response = await _client.GetAsync(_client.BaseAddress + "/HangHoa/GetById/" + MaHH);
                if (response.IsSuccessStatusCode)
                {
                    string data = response.Content.ReadAsStringAsync().Result;
                    hanghoa = JsonConvert.DeserializeObject<HangHoaVM>(data);
                }
                if (string.IsNullOrEmpty(returnUrl) && Request.Headers.ContainsKey("Referer"))
                {
                    returnUrl = Request.Headers["Referer"].ToString();
                }
                ViewBag.ReturnUrl = returnUrl;
                TempData["ReturnUrl"] = returnUrl;

                return View(hanghoa);
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        [Authorize]
        [HttpPost]
        [Route("HangHoa/Edit/{MaHH}")]
        public async Task<IActionResult> Edit(HangHoaVM model, IFormFile[] ImageFiles, int MaHH, string returnUrl = null)
        {
            try
            {
                if (model == null)
                {
                    TempData["error"] = "Dữ liệu không hợp lệ!";
                    return View(model);  // Return the model back to the view
                }
                string data = JsonConvert.SerializeObject(model);
                StringContent content = new StringContent(data, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _client.PutAsync(_client.BaseAddress + "/HangHoa/Update/" + MaHH, content);
                if (response.IsSuccessStatusCode)
                {
                    TempData["success"] = "Cập nhật thành công!";
                    if (string.IsNullOrEmpty(returnUrl) && TempData.ContainsKey("ReturnUrl"))
                    {
                        returnUrl = TempData["ReturnUrl"]?.ToString();
                    }
                    // Ensure we have a valid URL to return to
                    if (!string.IsNullOrEmpty(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    return RedirectToAction("Index");
                }
                string errorContent = await response.Content.ReadAsStringAsync();
                TempData["error"] = "Cập nhật thất bại: " + errorContent;
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                return View(model);
            }
        }
        [Authorize]
        [HttpPost]
        public IActionResult DeleteProduct(int MaHH, string returnUrl = null)
        {
            try
            {
                HttpResponseMessage response = _client.DeleteAsync(_client.BaseAddress + "/HangHoa/Delete/" + MaHH).Result;
                if (response.IsSuccessStatusCode)
                {
                    return Json(new { success = true, returnUrl = returnUrl });
                }
                return Json(new { success = false });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
