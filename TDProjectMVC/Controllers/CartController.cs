using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TDProjectMVC.Data;
using TDProjectMVC.Helpers;
using TDProjectMVC.Models;
using TDProjectMVC.Services;
using TDProjectMVC.Services.Mail;
using TDProjectMVC.Services.Momo;
using TDProjectMVC.ViewModels;

namespace TDProjectMVC.Controllers
{
    public class CartController : Controller
    {
        private readonly Hshop2023Context db;
        private readonly IVnPayService _vnPayservice;
        private readonly IMailSender _mailSender;
        private readonly IMomoService _momoService;
        public CartController(IMailSender mailSender, Hshop2023Context context, IVnPayService vnPayservice, IMomoService momoService)
        {
            db = context;
            _vnPayservice = vnPayservice;
            _mailSender = mailSender;
            _momoService = momoService;

        }
        public List<CartItem> Cart => HttpContext.Session.Get<List<CartItem>>(MySetting.CART_KEY) ?? new List<CartItem>();
        public IActionResult Index()
        {
            return View(Cart);
        }
        #region Thêm cart
        [HttpPost]
        public IActionResult AddToCart(int id, int quantity = 1, string type = "Normal")
        {
            var gioHang = Cart;
            var item = gioHang.SingleOrDefault(p => p.MaHH == id);

            if (item == null)
            {
                var hanghoa = db.HangHoas.SingleOrDefault(p => p.MaHh == id);
                if (hanghoa == null)
                {
                    return Json(new { success = false, message = $"Không tìm thấy hàng hóa có mã {id}" });
                }

                item = new CartItem
                {
                    MaHH = hanghoa.MaHh,
                    TenHH = hanghoa.TenHh,
                    DonGia = hanghoa.DonGia ?? 0,
                    Hinh = hanghoa.Hinh ?? String.Empty,
                    SoLuong = quantity
                };
                gioHang.Add(item);
            }
            else
            {
                item.SoLuong += quantity;
            }

            HttpContext.Session.Set(MySetting.CART_KEY, gioHang);
            return Json(new { success = true, cartCount = gioHang.Sum(i => i.SoLuong) });
        }

        //Hiển thị dữ liệu trong giỏ hàng
        public IActionResult GetCartData()
        {
            var cartData = new
            {
                CardProducts = Cart.Select(p => new
                {
                    p.MaHH,
                    p.TenHH,
                    p.SoLuong,
                    p.DonGia,
                    Hinh = p.Hinh?.Split(',').FirstOrDefault()?.Trim() ?? ""
                }),
                TotalQuantity = Cart.Sum(p => p.SoLuong),
                TotalAmount = Cart.Sum(p => p.SoLuong * p.DonGia)
            };
            return Json(cartData);
        }

        public JsonResult RemoveCart(int id)
        {
            var gioHang = Cart;
            var item = gioHang.SingleOrDefault(p => p.MaHH == id);
            if (item != null)
            {
                gioHang.Remove(item);
                HttpContext.Session.Set(MySetting.CART_KEY, gioHang);
                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false, message = "Sản phẩm không tồn tại trong giỏ hàng" });
            }
        }

        public IActionResult IncreaseQuantity(int id)
        {
            var gioHang = Cart;
            var item = gioHang.SingleOrDefault(p => p.MaHH == id);
            if (item != null)
            {
                item.SoLuong++; 
                HttpContext.Session.Set(MySetting.CART_KEY, gioHang);
            }
            return RedirectToAction("Index");
        }
        public IActionResult MinusQuantity(int id)
        {
            var gioHang = Cart; 
            var item = gioHang.SingleOrDefault(p => p.MaHH == id); 
            if (item != null)
            {
                item.SoLuong--; 
                HttpContext.Session.Set(MySetting.CART_KEY, gioHang); 
            }
            return RedirectToAction("Index"); 
        }

        #endregion
        #region Thanh Toan (checkOut)
        [Authorize]
        [HttpGet]
        public IActionResult Checkout()
        {
            if (Cart.Count == 0)
            {
                return Redirect("/");
            }

            return View(Cart);
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Checkout(CheckOutVM model, string payment = "COD")
        {
            if (!ModelState.IsValid)
            {
                return View(Cart);
            }

            if (payment == "Thanh toán VNPay")
            {
                var vnPayModel = new VnPaymentRequestModel
                {
                    Amount = Cart.Sum(p => p.ThanhTien),
                    CreatedDate = DateTime.Now,
                    Description = $"{model.HoTen ?? "N/A"} {model.DienThoai ?? "N/A"}",
                    FullName = model.HoTen ?? "N/A",
                    OrderId = new Random().Next(1, 100000)
                };
                return Redirect(_vnPayservice.CreatePaymentUrl(HttpContext, vnPayModel));
            }

            var customerId = User.Identity.Name;
            if (string.IsNullOrEmpty(customerId))
            {
                return RedirectToAction("Login", "Account");
            }

            KhachHang khachHang = null;
            if (model.GiongKhachHang)
            {
                khachHang = await db.KhachHangs.SingleOrDefaultAsync(kh => kh.MaKh == customerId);
                if (khachHang == null)
                {
                    ModelState.AddModelError("", "Không tìm thấy thông tin khách hàng");
                    return View(Cart);
                }
            }

            var hoadon = new HoaDon
            {
                MaKh = customerId,
                HoTen = model.HoTen ?? khachHang?.HoTen ?? "N/A",
                DiaChi = model.DiaChi ?? khachHang?.DiaChi ?? "N/A",
                DienThoai = model.DienThoai ?? khachHang?.DienThoai ?? "N/A",
                NgayDat = DateTime.Now,
                CachThanhToan = "COD",
                CachVanChuyen = "GRAB",
                MaTrangThai = 0,
                GhiChu = model.GhiChu ?? "N/A"
            };

            using (var transaction = await db.Database.BeginTransactionAsync())
            {
                try
                {
                    db.Add(hoadon);
                    await db.SaveChangesAsync();

                    var cthds = Cart.Select(item => new ChiTietHd
                    {
                        MaHd = hoadon.MaHd,
                        MaHh = item.MaHH,
                        DonGia = item.DonGia,
                        SoLuong = item.SoLuong,
                        MaGiamGia = item.GiamGia,
                    }).ToList();

                    db.AddRange(cthds);
                    await db.SaveChangesAsync();

                    await transaction.CommitAsync();

                    // Clear the cart only after successful commit
                    HttpContext.Session.Remove(MySetting.CART_KEY);
                    if (!string.IsNullOrEmpty(model.Email))
                    {
                        var receiver = model.Email;
                        var subject = "Đặt hàng thành công";
                        var message = "Đặt hàng thành công, theo dõi đơn hàng của bạn trên trang web !";

                        bool emailSent = await _mailSender.SendEmailAsync(receiver, subject, message);
                        if (!emailSent)
                        {
                            // You might want to implement a logging mechanism here
                            ModelState.AddModelError("", "Đơn hàng đã được tạo, nhưng không thể gửi email xác nhận.");
                        }
                    }
                    TempData["success"] = "Thêm sản phẩm mới thành công";
                    return View("Success");
                }
                catch (Exception ex)
                {
                    // Log the exception
                    TempData["error"] = "Đã xảy ra lỗi vui lòng liên hệ nhà phát triển!";
                    // You might want to implement a logging mechanism here
                    ModelState.AddModelError("", $"Lỗi khi lưu đơn hàng: {ex.Message}");
                    return View(Cart);
                }
            }
        }
        #endregion
        //#region Paypal payment
        //[Authorize]
        //[HttpPost("/Cart/create-paypal-order")]
        //public async Task<IActionResult> CreatePaypalOrder(CancellationToken cancellationToken)
        //{
        //	// Thông tin đơn hàng gửi qua Paypal
        //	var tongTien = Cart.Sum(p => p.ThanhTien).ToString();
        //	var donViTienTe = "USD";
        //	var maDonHangThamChieu = "DH" + DateTime.Now.Ticks.ToString();

        //	try
        //	{
        //		var response = await _paypalClient.CreateOrder(tongTien, donViTienTe, maDonHangThamChieu);

        //		return Ok(response);
        //	}
        //	catch (Exception ex)
        //	{
        //		var error = new { ex.GetBaseException().Message };
        //		return BadRequest(error);
        //	}
        //}

        //[Authorize]
        //[HttpPost("/Cart/capture-paypal-order")]
        //public async Task<IActionResult> CapturePaypalOrder(string orderID, CancellationToken cancellationToken)
        //{
        //	try
        //	{
        //		var response = await _paypalClient.CaptureOrder(orderID);

        //		// Lưu database đơn hàng của mình

        //		return Ok(response);
        //	}
        //	catch (Exception ex)
        //	{
        //		var error = new { ex.GetBaseException().Message };
        //		return BadRequest(error);
        //	}
        //}

        //#endregion
        public IActionResult PaymentFail()
        {
            return View();
        }
        public IActionResult PaymentSuccess()
        {
            return View("Success");

        }

        public IActionResult VNPayCallBack(CheckOutVM model)
        {
            var response = _vnPayservice.PaymentExecute(Request.Query);

            if (response == null || response.VnPayResponseCode != "00")
            {
                TempData["Message"] = $"Lỗi thanh toán VN Pay: {response.VnPayResponseCode}";
                return RedirectToAction("PaymentFail");
            }

            var khachHang = new KhachHang();
            // Lấy thông tin từ dữ liệu trả về của VNPay
            var orderId = response.OrderId;
            var customerId = HttpContext.User.Claims.SingleOrDefault(p => p.Type == MySetting.CLAIM_CUSTOMERID).Value;

            // Lưu đơn hàng vào cơ sở dữ liệu
            var hoadon = new HoaDon
            {
                MaKh = customerId,
                HoTen = model.HoTen ?? khachHang.HoTen,
                DiaChi = model.DiaChi ?? khachHang.DiaChi,
                DienThoai = model.DienThoai ?? khachHang.DienThoai,
                NgayDat = DateTime.Now,
                CachThanhToan = "VnPay",
                CachVanChuyen = "VnExpress",
                MaTrangThai = 0,
                GhiChu = model.GhiChu
            };

            // Tiến hành lưu đơn hàng và chi tiết đơn hàng vào cơ sở dữ liệu trong một giao dịch
            db.Database.BeginTransaction();
            try
            {
                db.Add(hoadon);
                db.SaveChanges(); // Lưu thông tin đơn hàng

                // Lưu thông tin chi tiết đơn hàng
                var cthds = new List<ChiTietHd>();
                foreach (var item in Cart)
                {
                    cthds.Add(new ChiTietHd
                    {
                        MaHd = hoadon.MaHd,
                        MaHh = item.MaHH,
                        DonGia = item.DonGia,
                        SoLuong = item.SoLuong,
                        MaGiamGia = item.GiamGia,
                    });
                }
                db.AddRange(cthds);
                db.SaveChanges(); // Lưu thông tin chi tiết đơn hàng

                db.Database.CommitTransaction(); // Xác nhận giao dịch

                // Xóa giỏ hàng trong session
                TempData["Message"] = $"Thanh toán VNPay thành công";
                HttpContext.Session.Set<List<CartItem>>(MySetting.CART_KEY, new List<CartItem>());
                return View("Success");
            }
            catch (Exception ex)
            {
                db.Database.RollbackTransaction(); // Hoàn tác giao dịch nếu có lỗi
                TempData["Message"] = $"Lỗi khi lưu đơn hàng: {ex.Message}";
                return RedirectToAction("PaymentFail");
            }
        }
        [HttpPost]
        public async Task<IActionResult> CreatePaymentMomo(OrderInfo model)
        {
                var response = await _momoService.CreatePaymentAsync(model);
                return Redirect(response.PayUrl);
        }
        [HttpGet]
        public IActionResult PaymentCallBack()
        {
            var response = _momoService.PaymentExecuteAsync(HttpContext.Request.Query);
            return View(response);
        }
    }
}

