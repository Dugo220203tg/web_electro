using AutoMapper;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TDProjectMVC.Data;
using TDProjectMVC.Helpers;
using TDProjectMVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using TDProjectMVC.Models;
using TDProjectMVC.Services.Mail;


namespace TDProjectMVC.Controllers
{
    public class KhachHangController : Controller
    {
        private readonly Hshop2023Context db;

        private readonly IMapper _mapper;
        private readonly IMailSender _mailSender;


        public KhachHangController(IMailSender mailSender, Hshop2023Context context, IMapper mapper)
        {
            db = context;
            _mapper = mapper;
            _mailSender = mailSender;
        }
        #region ---Login---
        [HttpGet]
        public IActionResult DangKy()
        {
            return View(new RegisterVM());
        }

        [HttpPost]
        public async Task<IActionResult> DangKyAsync(RegisterVM model)
        {
            // Nếu có lỗi trong ModelState thì trả về view cùng với thông báo lỗi
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var khachHangExit = await db.KhachHangs.SingleOrDefaultAsync(kh => kh.MaKh == model.MaKh);

                if (khachHangExit != null)
                {
                    ModelState.AddModelError("loi", "Username đã được sử dụng");
                    TempData["error"] = "Username đã được sử dụng";
                }
                else
                {
                    var khachHang = _mapper.Map<KhachHang>(model);
                    khachHang.RandomKey = MyUtil.GenerateRamdomKey();
                    khachHang.MatKhau = model.MatKhau.ToMd5Hash(khachHang.RandomKey);
                    khachHang.HieuLuc = false; // Account is inactive until email verification
                    khachHang.VaiTro = 0;

                    db.Add(khachHang);
                    await db.SaveChangesAsync();

                    if (!string.IsNullOrEmpty(model.Email))
                    {
                        var receiver = model.Email;
                        var subject = "Mã xác nhận của bạn";
                        var message = khachHang.RandomKey;

                        bool emailSent = await _mailSender.SendEmailAsync(receiver, subject, message);
                        if (!emailSent)
                        {
                            ModelState.AddModelError("", "Tài khoản đã được tạo, nhưng không thể gửi email xác nhận.");
                        }
                        else
                        {
                            TempData["success"] = "Đăng ký tài khoản thành công! Vui lòng kiểm tra email để xác nhận.";
                            return RedirectToAction("NhapMaXacNhan", new { khachHangId = khachHang.MaKh });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Loi", ex.Message);
                TempData["error"] = ex.Message;
            }

            return View(model);
        }
        [HttpGet]
        public IActionResult NhapMaXacNhan(string khachHangId, string type = "Register")
        {
            return View(new ConfirmCodeVM
            {
                KhachHangId = khachHangId,
                Type = type
            });
        }
        [HttpPost]
        public async Task<IActionResult> NhapMaXacNhan(ConfirmCodeVM model)
        {
            var khachHang = await db.KhachHangs.FindAsync(model.KhachHangId);

            if (khachHang == null)
            {
                ModelState.AddModelError("", "Không tìm thấy thông tin khách hàng.");
                return View(model);
            }

            if (khachHang.RandomKey != model.ConfirmationCode)
            {
                ModelState.AddModelError("", "Mã xác nhận không hợp lệ.");
                return View(model);
            }

            try
            {
                switch (model.Type)
                {
                    case "Register":
                        // Xử lý xác nhận đăng ký
                        khachHang.HieuLuc = true;
                        await db.SaveChangesAsync();
                        TempData["success"] = "Tài khoản của bạn đã được kích hoạt!";
                        return RedirectToAction("DangNhap");

                    case "ResetPassword":
                        // Xử lý xác nhận reset password
                        khachHang.HieuLuc = true; // Đảm bảo tài khoản được kích hoạt
                        khachHang.RandomKey = null; // Xóa mã xác nhận sau khi đã sử dụng
                        await db.SaveChangesAsync();
                        TempData["success"] = "Mật khẩu đã được đặt lại thành công!";
                        return RedirectToAction("DangNhap");

                    default:
                        ModelState.AddModelError("", "Loại xác nhận không hợp lệ.");
                        return View(model);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Có lỗi xảy ra: {ex.Message}");
                return View(model);
            }
        }
        #endregion
        #region ---RESET PASSWORD---
        [HttpGet]
        public IActionResult ResetPassword()
        {
            return View(new ResetPassword());
        }
        [HttpPost]
        [AllowAnonymous] // Cho phép truy cập không cần đăng nhập
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPassword model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Kiểm tra email tồn tại
                var khachHang = await db.KhachHangs
                    .SingleOrDefaultAsync(kh => kh.Email == model.email);

                if (khachHang == null)
                {
                    ModelState.AddModelError("", "Email không tồn tại trong hệ thống");
                    TempData["error"] = "Email không tồn tại trong hệ thống";
                    return View(model);
                }

                // Kiểm tra mật khẩu xác nhận
                if (model.Passwordcheck != model.newpassword)
                {
                    ModelState.AddModelError("", "Mật khẩu xác nhận không khớp");
                    TempData["error"] = "Mật khẩu xác nhận không khớp";
                    return View(model);
                }

                // Tạo random key mới và cập nhật mật khẩu
                khachHang.RandomKey = MyUtil.GenerateRamdomKey();
                khachHang.MatKhau = model.newpassword.ToMd5Hash(khachHang.RandomKey);

                // Gửi email xác nhận
                var emailSent = await _mailSender.SendEmailAsync(
                    model.email,
                    "Mã xác nhận đổi mật khẩu",
                    khachHang.RandomKey
                );

                if (!emailSent)
                {
                    ModelState.AddModelError("", "Không thể gửi email xác nhận");
                    TempData["error"] = "Không thể gửi email xác nhận";
                    return View(model);
                }

                // Lưu thay đổi vào database
                await db.SaveChangesAsync();

                TempData["success"] = "Vui lòng kiểm tra email và nhập mã xác nhận";
                return RedirectToAction("NhapMaXacNhan", new { khachHangId = khachHang.MaKh, type = "ResetPassword" });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
                TempData["error"] = "Có lỗi xảy ra: " + ex.Message;
                return View(model);
            }
        }

        #endregion
        #region ---DangNhap---
        [HttpGet]
        public IActionResult DangNhap(string? ReturnUrl)
        {
            ViewBag.ReturnUrl = ReturnUrl;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> DangNhap(LoginVM model, string? ReturnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Tìm khách hàng theo username hoặc email
                var khachHang = await db.KhachHangs
                    .SingleOrDefaultAsync(kh => kh.MaKh == model.UserName || kh.Email == model.UserName);

                // Kiểm tra tài khoản tồn tại
                if (khachHang == null)
                {
                    ModelState.AddModelError("", "Thông tin đăng nhập không chính xác");
                    TempData["error"] = "Thông tin đăng nhập không chính xác";
                    return View(model);
                }

                // Kiểm tra vai trò
                if (khachHang.VaiTro == 1)
                {
                    ModelState.AddModelError("", "Tài khoản không có quyền truy cập");
                    TempData["error"] = "Tài khoản không có quyền truy cập";
                    return View(model);
                }

                // Kiểm tra tài khoản bị khóa
                if (!khachHang.HieuLuc)
                {
                    ModelState.AddModelError("", "Tài khoản đã bị khóa. Vui lòng liên hệ Admin.");
                    TempData["error"] = "Tài khoản đã bị khóa. Vui lòng liên hệ Admin.";
                    return View(model);
                }

                // Kiểm tra mật khẩu
                var hashedPassword = model.Password.ToMd5Hash(khachHang.RandomKey);
                if (khachHang.MatKhau != hashedPassword)
                {
                    ModelState.AddModelError("", "Thông tin đăng nhập không chính xác");
                    TempData["error"] = "Thông tin đăng nhập không chính xác";
                    return View(model);
                }

                // Tạo claims cho đăng nhập
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, khachHang.MaKh),
            new Claim(ClaimTypes.Email, khachHang.Email),
            new Claim(ClaimTypes.Name, khachHang.HoTen),
            new Claim(ClaimTypes.Role, "Customer"),
            new Claim("CustomerID", khachHang.MaKh)
        };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                // Đăng nhập
                await HttpContext.SignInAsync(claimsPrincipal);

                // Chuyển hướng
                if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
                {
                    return Redirect(ReturnUrl);
                }
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi xảy ra trong quá trình đăng nhập");
                TempData["error"] = "Có lỗi xảy ra trong quá trình đăng nhập";
                // Log error
                return View(model);
            }
        }
        #endregion

        #region ---DangXuat---
        [Authorize]
        public async Task<IActionResult> DangXuat()
        {
            await HttpContext.SignOutAsync();
            return Redirect("/");
        }
        #endregion

        #region ---Xem Thông Tin --- Cập nhật thông tin --- Bình luận sản phẩm ---

        [Authorize]
        public IActionResult Profile()
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                // Lấy mã khách hàng từ claim 'CustomerID'
                // var customerId = User.Identity.Name;
                var customerId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "CustomerID")?.Value;
                if (customerId != null)
                {
                    // Thực hiện truy vấn để lấy thông tin khách hàng từ cơ sở dữ liệu
                    var khachHang = db.KhachHangs.FirstOrDefault(kh => kh.MaKh == customerId);

                    if (khachHang != null)
                    {
                        // Đã tìm thấy thông tin khách hàng, bạn có thể sử dụng nó để hiển thị trên trang web
                        ViewBag.CustomerName = khachHang.HoTen;
                        ViewBag.CustomerDienThoai = khachHang.DienThoai;
                        ViewBag.CustomerEmail = khachHang.Email;
                        ViewBag.CustomerAddress = khachHang.DiaChi;

                        // Return the Profile view (not a redirect to Profile to avoid looping)
                        return View();
                    }
                }

                ViewBag.ErrorMessage = "Không tìm thấy thông tin khách hàng.";
                return View("Error");
            }

            // User is not authenticated
            ViewBag.ErrorMessage = "Người dùng chưa đăng nhập.";
            return View("Error");
        }

        [Authorize]
        [HttpPost]
        public IActionResult UpdateProfile(string customerName, string customerEmail, string customerAddress)
        {
            // Lấy mã khách hàng từ claim 'CustomerID'
            var customerId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "CustomerID")?.Value;

            if (customerId != null)
            {
                // Thực hiện truy vấn để lấy thông tin khách hàng từ cơ sở dữ liệu
                var khachHang = db.KhachHangs.FirstOrDefault(kh => kh.MaKh == customerId);

                if (khachHang != null)
                {
                    // Cập nhật thông tin khách hàng
                    khachHang.HoTen = customerName;
                    khachHang.Email = customerEmail;
                    khachHang.DiaChi = customerAddress;

                    // Lưu thay đổi vào cơ sở dữ liệu
                    db.SaveChanges();

                    // Chuyển hướng người dùng đến trang Profile sau khi cập nhật thành công
                    return RedirectToAction("Profile");
                }
            }

            // Nếu không tìm thấy thông tin khách hàng, bạn có thể xử lý theo ý của mình, ví dụ: hiển thị thông báo lỗi.
            ViewBag.ErrorMessage = "Không tìm thấy thông tin khách hàng.";
            return View("Error"); // Thay thế "Error" bằng tên View chứa trang thông báo lỗi của bạn.
        }


        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddComment(int MaHH, string noiDung, int sao)
        {
            if (sao < 1 || sao > 5)
            {
                return Json(new { success = false, message = "Invalid rating value." });
            }

            if (string.IsNullOrWhiteSpace(noiDung))
            {
                return Json(new { success = false, message = "Comment content cannot be empty." });
            }

            try
            {
                var userId = User.Identity.Name; // Assuming the user is authenticated and Name is used as the user ID

                if (userId == null)
                {
                    return Json(new { success = false, message = "User not authenticated." });
                }

                var danhgia = new DanhGiaSp
                {
                    MaHh = MaHH,
                    MaKh = userId,
                    Ngay = DateTime.Now,
                    NoiDung = noiDung,
                    Sao = sao,
                    TrangThai = 1 // Assuming this means active
                };

                db.Add(danhgia);
                await db.SaveChangesAsync();

                return Json(new { success = true, message = "Comment submitted successfully!" });
            }
            catch (Exception ex)
            {
                // Log the exception here using your logging mechanism (like Serilog, NLog, etc.)
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        #endregion
    }
}
