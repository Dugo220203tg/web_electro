using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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
                HttpResponseMessage response = await _client.GetAsync(_client.BaseAddress + "/KhachHangs/GetById/" + model.UserName);
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
                            new Claim(ClaimTypes.Name, khachHang.UserName),
                            new Claim("CustomerID", khachHang.UserName),
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
                HttpResponseMessage response = _client.PutAsync(_client.BaseAddress + "/KhachHangs/Update/" + model.UserName, content).Result;

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
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirm(int Username)
        {
            try
            {
                HttpResponseMessage response = _client.DeleteAsync(_client.BaseAddress + "/KhachHangs/Delete/" + Username).Result;
                if (response.IsSuccessStatusCode)
                {
                    TempData["success"] = "Xóa thành công!";
                    return RedirectToAction("Index");
                }
                return View("Index", "Admin");
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                return View();
            }
        }
    }
}
