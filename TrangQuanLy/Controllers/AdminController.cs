using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using TrangQuanLy.Helpers;
using TrangQuanLy.Models;

namespace TrangQuanLy.Controllers
{
    public class AdminController : Controller
    {
        private readonly HttpClient _client;
        private readonly IHttpContextAccessor _httpContextAccessor;
        Uri baseAddress = new Uri("https://localhost:7109/api");

        public AdminController(HttpClient client, IHttpContextAccessor httpContextAccessor)
        {
            _client = client;
            _client.BaseAddress = baseAddress;
            _httpContextAccessor = httpContextAccessor;
        }
        [HttpGet]
        public async Task<IActionResult> AllNotifications()
        {
            List<NotificationMD> notifications = new List<NotificationMD>();

            try
            {
                HttpResponseMessage response = _client.GetAsync(_client.BaseAddress + "/Notification/GetAllNotifications").Result;

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    notifications = JsonConvert.DeserializeObject<List<NotificationMD>>(data).ToList();
                }
            }
            catch (Exception ex)
            {
                ViewData["Error"] = "Không thể tải thông báo: " + ex.Message;
            }

            return View(notifications);
        }
        #region -- ĐĂNG XUẤT --
        [Authorize]
        public async Task<IActionResult> DangXuat()
        {
            await HttpContext.SignOutAsync();
            return Redirect("/");
        }
        #endregion
        #region --- DANG NHAP ---
        [HttpGet]
        public IActionResult DangNhap(string? ReturnUrl)
        {
            ViewBag.ReturnUrl = ReturnUrl;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> DangNhap(AdminViewModel model, string? ReturnUrl)
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync(_client.BaseAddress + "/KhachHangs/GetById/" + model.maKh);
                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    var khachHang = JsonConvert.DeserializeObject<AdminViewModel>(data);
                    if (khachHang != null)
                    {
                        if (khachHang.Vaitro != 1)
                        {
                            ModelState.AddModelError("", "Sai quyền đăng nhập");
                            TempData["error"] = "Sai quyền đăng nhập";

                        }
                        else
                        {
                            //Hash the input password with the stored RandomKey
                            string hashedInputPassword = model.Password.ToMd5Hash(khachHang.RandomKey);

                            if (khachHang.Password != hashedInputPassword)
                            {
                                ModelState.AddModelError("", "Sai thông tin đăng nhập");
                                TempData["error"] = "Sai thông tin đăng nhập";
                            }
                            else
                            {
                                var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Email, khachHang.Email),
                            new Claim(ClaimTypes.Name, khachHang.maKh),
                            new Claim("CustomerID", khachHang.maKh),
                            new Claim(ClaimTypes.Role, "Admin")
                        };
                                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                                await HttpContext.SignInAsync(claimsPrincipal);

                                if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
                                {
                                    return Redirect(ReturnUrl);
                                }
                                else
                                {
                                    TempData["success"] = "Đăng nhập thành công";
                                    return RedirectToAction("Index", "Home");
                                }
                            }
                        }
                    }
                    else
                    {
                        TempData["error"] = "Tài khoản không tồn tại hoặc không hoạt động";
                        ModelState.AddModelError("", "Tài khoản không tồn tại hoặc không hoạt động");

                    }
                }
                else
                {
                    ModelState.AddModelError("", "Lỗi khi gửi yêu cầu đăng nhập đến API");
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = "Lỗi: " + ex.Message;
            }
            // If authentication fails, return to the login view
            return View(model);
        }

        #endregion
        #region --- DANG KY ---

        [HttpGet]
        public IActionResult DangKy()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> DangKy(AdminViewModel model)
        {
            try
            {
                string data = JsonConvert.SerializeObject(model);
                StringContent content = new StringContent(data, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _client.PostAsync(_client.BaseAddress + "/KhachHangs/Post", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["success"] = "Tạo tài khoản admin thành công!";
                    return RedirectToAction("DangNhap", "Admin");
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }

            return View();
        }


        #endregion
        #region --- QUAN LY TAI KHOAN ---
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
            List<AdminViewModel> khachhang = new List<AdminViewModel>();
            HttpResponseMessage response = _client.GetAsync(_client.BaseAddress + "/KhachHangs/GetAll").Result;

            if (response.IsSuccessStatusCode)
            {
                string data = response.Content.ReadAsStringAsync().Result;
                khachhang = JsonConvert.DeserializeObject<List<AdminViewModel>>(data);
                khachhang = khachhang.Where(k => k.Vaitro == 0).ToList();

            }

            int totalItems = khachhang.Count();
            var paginatedList = PaginatedList<AdminViewModel>.CreateAsync(khachhang.AsQueryable(), page ?? 1, pagesize ?? 5);
            ViewBag.Page = page;
            ViewBag.TotalPages = paginatedList.TotalPages;
            ViewBag.PageSize = pagesize;

            return View(paginatedList);
        }
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Search(string? query, int? page, int? pagesize)
        {
            // Set default values for pagination if not provided
            page ??= 1;
            pagesize ??= 5;

            // Initialize list to store search results
            List<AdminViewModel> searchResult = new List<AdminViewModel>();

            // Send a request to the API to get all KhachHang entities
            HttpResponseMessage response = await _client.GetAsync(_client.BaseAddress + "/KhachHangs/GetAll");

            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                List<AdminViewModel> nguoidung = JsonConvert.DeserializeObject<List<AdminViewModel>>(data);

                // If there's a search query, filter the results
                if (!string.IsNullOrEmpty(query))
                {
                    searchResult = nguoidung.Where(h =>
                        MyUtil.RemoveDiacritics(h.maKh).IndexOf(MyUtil.RemoveDiacritics(query), StringComparison.OrdinalIgnoreCase) >= 0 ||
                        MyUtil.RemoveDiacritics(h.Hoten).IndexOf(MyUtil.RemoveDiacritics(query), StringComparison.OrdinalIgnoreCase) >= 0 ||
                        MyUtil.RemoveDiacritics(h.DienThoai).IndexOf(MyUtil.RemoveDiacritics(query), StringComparison.OrdinalIgnoreCase) >= 0 ||
                        MyUtil.RemoveDiacritics(h.Email).IndexOf(MyUtil.RemoveDiacritics(query), StringComparison.OrdinalIgnoreCase) >= 0
                    ).ToList();
                }
                else
                {
                    searchResult = nguoidung; // No query, so show all results
                }
            }
            else
            {
                return View("Error"); // Handle API error
            }
            var paginatedList = PaginatedList<AdminViewModel>.CreateAsync(searchResult.AsQueryable(), page.Value, pagesize.Value);

            // Pass the current page and page size as ViewBag for rendering pagination controls in the view
            ViewBag.Page = page;
            ViewBag.TotalPages = paginatedList.TotalPages;
            ViewBag.PageSize = pagesize;
            ViewBag.Query = query;  // Truyền từ khóa tìm kiếm để giữ nguyên khi phân trang

            return View(paginatedList);
        }

        [Authorize]
        [HttpGet]
        public IActionResult Edit(string id)
        {
            try
            {
                AdminViewModel hanghoa = new AdminViewModel();
                HttpResponseMessage respone = _client.GetAsync(_client.BaseAddress + "/KhachHangs/GetById/" + id).Result;
                if (respone.IsSuccessStatusCode)
                {
                    string data = respone.Content.ReadAsStringAsync().Result;
                    hanghoa = JsonConvert.DeserializeObject<AdminViewModel>(data);
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
        public IActionResult Edit(AdminViewModel model)
        {
            try
            {
                string data = JsonConvert.SerializeObject(model);
                StringContent content = new StringContent(data, Encoding.UTF8, "application/json");
                HttpResponseMessage response = _client
                    .PutAsync(_client.BaseAddress + "/KhachHangs/Update/" + model.maKh, content).Result;

                if (response.IsSuccessStatusCode)
                {
                    TempData["success"] = "Cập nhật thành công!";
                    return RedirectToAction("Index");
                }

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
        public IActionResult DeleteAccount(string maKh)
        {
            if (string.IsNullOrWhiteSpace(maKh))
            {
                return Json(new { success = false, message = "Invalid customer ID (maKh)." });
            }

            try
            {
                // Adjusted API URL to match the correct route
                var apiUrl = $"{_client.BaseAddress}/KhachHangs/Delete/Delete/{maKh}";
                HttpResponseMessage response = _client.DeleteAsync(apiUrl).Result;

                if (response.IsSuccessStatusCode)
                {
                    return Json(new { success = true });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        statusCode = (int)response.StatusCode,
                        reason = response.ReasonPhrase
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


    }
    #endregion ---

}
