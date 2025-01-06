using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TDProjectMVC.Data;
using TDProjectMVC.Helpers;
using TDProjectMVC.Models.MoMo;
using TDProjectMVC.Services;
using TDProjectMVC.Services.Mail;
using TDProjectMVC.Services.Momo;
using TDProjectMVC.ViewModels;
using System.Linq;
using TDProjectMVC.Services.VnPay;
namespace TDProjectMVC.Controllers
{
    public class CartController : Controller
    {
        private readonly Hshop2023Context db;
        private readonly IVnPayService _vnPayservice;
        private readonly IMailSender _mailSender;
        private readonly IMomoService _momoService;
        private readonly ILogger<CartController> _logger;

        public CartController(IMailSender mailSender, Hshop2023Context context, IVnPayService vnPayService, IMomoService momoService, ILogger<CartController> logger)
        {
            db = context;
            _vnPayservice = vnPayService;
            _mailSender = mailSender;
            _momoService = momoService;
            _logger = logger;
        }
        public List<CartItem> Cart => HttpContext.Session.Get<List<CartItem>>(MySetting.CART_KEY) ?? new List<CartItem>();
        public IActionResult Index()
        {
            return View(Cart);
        }
        #region ---- CART ----
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
            var gioHang = HttpContext.Session.Get<List<CartItem>>(MySetting.CART_KEY) ?? new List<CartItem>();
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

        [HttpPost]
        public IActionResult ApplyCoupon([FromBody] CouponVM model)
        {
            if (model == null || string.IsNullOrEmpty(model.Description))
            {
                return Json(new { success = false, message = "No coupon code provided." });
            }

            var coupon = db.Coupons.FirstOrDefault(c => c.Description.Equals(model.Description));
            if (coupon == null)
            {
                return Json(new { success = false, message = "Invalid coupon code." });
            }

            if (coupon.DateEnd < DateTime.Now)
            {
                return Json(new { success = false, message = "Expired coupon code." });
            }

            // Calculate discount and total
            decimal discount = (decimal)coupon.Price;
            HttpContext.Session.SetString("CouponCode", model.Description);
            HttpContext.Session.SetString("CouponDiscount", discount.ToString());
            return Json(new { success = true, discount });
        }


        public IActionResult IncreaseQuantity(int id)
        {
            var gioHang = Cart;
            var item = gioHang.SingleOrDefault(p => p.MaHH == id);
            if (item != null)
            {
                item.SoLuong++; 
                HttpContext.Session.Set(MySetting.CART_KEY, gioHang); // Update session
            }
            return RedirectToAction("Index"); 
        }

        public IActionResult MinusQuantity(int id)
        {
            var gioHang = Cart;
            var item = gioHang.SingleOrDefault(p => p.MaHH == id);
            if (item != null && item.SoLuong > 1) 
            {
                item.SoLuong--;
                HttpContext.Session.Set(MySetting.CART_KEY, gioHang); // Update session
            }
            return RedirectToAction("Index");
        }


        #endregion -----
        #region ----- Thanh Toan (checkOut) -----
        [Authorize]
        [HttpGet]
        public IActionResult Checkout()
        {
            if (Cart == null || !Cart.Any())
            {
                return Redirect("/");
            }

            string couponCode = HttpContext.Session.GetString("CouponCode");
            decimal discount = 0;
            if (!string.IsNullOrEmpty(couponCode))
            {
                discount = decimal.Parse(HttpContext.Session.GetString("CouponDiscount"));
            }

            decimal subtotal = (decimal)Cart.Sum(p => p.SoLuong * p.DonGia);
            decimal total = subtotal * (100 - discount) / 100;

            ViewBag.Discount = discount;
            ViewBag.Total = total;

            return View(Cart);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Checkout(CheckOutVM model, string paymentMethod)
        {
            //if (!ModelState.IsValid)
            //{
            //    return View(Cart);
            //}

            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(customerId))
            {
                return RedirectToAction("Login", "Account");
            }

            string couponCode = HttpContext.Session.GetString("CouponCode");
            decimal discount = 0;
            if (!string.IsNullOrEmpty(couponCode))
            {
                discount = decimal.Parse(HttpContext.Session.GetString("CouponDiscount"));
            }

            decimal subtotal = (decimal)Cart.Sum(p => p.SoLuong * p.DonGia);
            decimal total = subtotal * (100 - discount) / 100;

            ViewBag.Discount = discount;
            ViewBag.Total = total;

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
                HoTen = model.HoTen,
                DiaChi = model.DiaChi,
                DienThoai = model.DienThoai,
                NgayDat = DateTime.Now,
                CachThanhToan = paymentMethod,
                CachVanChuyen = "GRAB",
                MaTrangThai = 0,
                GhiChu = model.GhiChu,
            };

            try
            {
                switch (paymentMethod.ToUpperInvariant())
                {
                    case "VNPAY":
                        var paymentModel = new PaymentInformationModel
                        {
                            FullName = model.HoTen,
                            Amount = (double)total * 100,
                            Description = "Thanh toán qua VNPAY",
                            OrderType = "other" // Cập nhật loại đơn hàng tùy thuộc vào cấu hình của bạn

                        };

                        var paymentUrl = _vnPayservice.CreatePaymentUrl(paymentModel, HttpContext);
                        return Redirect(paymentUrl);
                    case "MOMO":
                        var momoModel = new OrderInfoModel
                        {
                            FullName = model.HoTen,
                            Amount = total,
                            OrderId = Guid.NewGuid().ToString(),
                            OrderInfo = "Thanh toán MOMO"
                        };
                        return await CreatePaymentMomo(momoModel);
                    default:
                        return await ProcessOrder(hoadon, model.Email);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Checkout Error: {ex}");
                TempData["Message"] = "Đã xảy ra lỗi khi xử lý thanh toán. Vui lòng thử lại.";
                return RedirectToAction("PaymentFail");
            }
        }

        // Refactored Method to Process Order
        private async Task<IActionResult> ProcessOrder(HoaDon hoadon, string email)
        {
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

                    // Clear cart
                    HttpContext.Session.Remove(MySetting.CART_KEY);

                    if (!string.IsNullOrEmpty(email))
                    {
                        await _mailSender.SendEmailAsync(email, "Đặt hàng thành công", "Đơn hàng đã được đặt thành công!");
                    }

                    TempData["success"] = "Đặt hàng thành công";
                    return View("Success");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    TempData["error"] = "Đã xảy ra lỗi vui lòng liên hệ nhà phát triển!";
                    throw;
                }
            }
        }


        public IActionResult PaymentFail()
        {
            return View();
        }
        public IActionResult PaymentSuccess()
        {
            return View("Success");

        }
        #endregion -----
        #region---MOMO---
        [HttpPost]
        public async Task<IActionResult> CreatePaymentMomo(OrderInfoModel model)
        {
            // Validate the input model
            if (model == null)
            {
                TempData["error"] = "Thông tin thanh toán không hợp lệ.";
                return RedirectToAction("PaymentFail");
            }

            // Validate the amount
            if (model.Amount <= 0)
            {
                TempData["error"] = "Số tiền thanh toán không hợp lệ.";
                return RedirectToAction("PaymentFail");
            }

            // Assign default values if necessary
            model.FullName ??= User.Identity?.Name ?? "Unknown User";
            model.OrderInfo ??= "Thanh toán đơn hàng";

            // Log payment information for debugging
            _logger.LogInformation($"Initiating MoMo payment with details: Amount={model.Amount}, FullName={model.FullName}, OrderInfo={model.OrderInfo}");

            try
            {
                // Call the service to create a payment
                var response = await _momoService.CreatePaymentAsync(model);

                // Validate the response
                if (response == null || string.IsNullOrEmpty(response.PayUrl))
                {
                    TempData["error"] = "Không thể tạo liên kết thanh toán. Vui lòng thử lại.";
                    _logger.LogError("Failed to create MoMo payment. Response is null or missing PayUrl.");
                    return RedirectToAction("PaymentFail");
                }

                // Log the payment URL for debugging
                _logger.LogInformation($"MoMo Payment URL created: {response.PayUrl}");

                // Redirect to the payment URL
                return Redirect(response.PayUrl);
            }
            catch (Exception ex)
            {
                // Log the exception
                _logger.LogError(ex, "An error occurred while creating MoMo payment.");

                // Show a user-friendly error message
                TempData["error"] = "Đã xảy ra lỗi trong quá trình xử lý thanh toán. Vui lòng thử lại.";
                return RedirectToAction("PaymentFail");
            }
        }

        [HttpGet]
        public async Task<IActionResult> PaymentCallBack(OrderInfoModel model)
        {
            var response = _momoService.PaymentExecuteAsync(HttpContext.Request.Query);
            var requestQuery = HttpContext.Request.Query;
            if (requestQuery["resultCode"] == "0") // Successful payment
            {
                var orderId = requestQuery["orderId"];
                var amount = decimal.Parse(requestQuery["amount"]);
                var orderInfo = requestQuery["orderInfo"];
                var datePaid = DateTime.Now;
                try
                {
                    var result = await SaveOrderToDatabase(orderId, amount, orderInfo, datePaid, "Momo");
                    if (!result)
                    {
                        TempData["error"] = "Đơn hàng đã được tạo nhưng có lỗi khi lưu.";
                        return RedirectToAction("PaymentFail");
                    }

                    TempData["success"] = "Giao dịch thành công!";
                    return RedirectToAction("PaymentSuccess");
                }
                catch (Exception ex)
                {
                    // Log error
                    TempData["error"] = "Có lỗi xảy ra khi xử lý đơn hàng: " + ex.Message;
                    return RedirectToAction("PaymentFail");
                }
            }
            else // Failed or canceled payment
            {
                var newMomoInsert = new OrderInfoModel
                {
                    OrderId = requestQuery["orderId"],
                    FullName = User.FindFirstValue(ClaimTypes.Email),
                    Amount = decimal.Parse(requestQuery["amount"]),
                    OrderInfo = requestQuery["orderInfo"],
                    DatePaid = DateTime.Now
                };

                db.Add(newMomoInsert);
                await db.SaveChangesAsync();

                TempData["error"] = "Giao dịch Momo đã bị hủy.";
                return RedirectToAction("Index", "Cart");
            }
        }

        // Refactored Method for Saving Orders
        private async Task<bool> SaveOrderToDatabase(string orderId, decimal amount, string orderInfo, DateTime datePaid, string paymentMethod)
        {
            using (var transaction = await db.Database.BeginTransactionAsync())
            {
                try
                {
                    // Create the order
                    var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (string.IsNullOrEmpty(customerId))
                    {
                        throw new InvalidOperationException("Customer ID is missing.");
                    }

                    var hoadon = new HoaDon
                    {
                        MaKh = customerId,
                        HoTen = User.FindFirstValue(ClaimTypes.Name) ?? "N/A",
                        DiaChi = "Default Address", // Customize as needed
                        DienThoai = "Default Phone", // Customize as needed
                        NgayDat = DateTime.Now,
                        CachThanhToan = paymentMethod,
                        CachVanChuyen = "Default Shipping", // Customize as needed
                        MaTrangThai = 0,
                        GhiChu = orderInfo
                    };

                    db.Add(hoadon);
                    await db.SaveChangesAsync();

                    // Create order details
                    var cart = HttpContext.Session.Get<List<CartItem>>(MySetting.CART_KEY);
                    if (cart == null || !cart.Any())
                    {
                        throw new InvalidOperationException("Cart is empty.");
                    }

                    var cthds = cart.Select(item => new ChiTietHd
                    {
                        MaHd = hoadon.MaHd,
                        MaHh = item.MaHH,
                        DonGia = item.DonGia,
                        SoLuong = item.SoLuong,
                        MaGiamGia = item.GiamGia,
                    }).ToList();

                    db.AddRange(cthds);
                    await db.SaveChangesAsync();

                    // Commit transaction
                    await transaction.CommitAsync();

                    // Clear cart
                    HttpContext.Session.Remove(MySetting.CART_KEY);
                    HttpContext.Session.Remove("CouponCode");
                    HttpContext.Session.Remove("CouponDiscount");
                    return true;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }
        #endregion
        #region--- VNPAY ---
        public async Task<IActionResult> VNPayCallBack(CheckOutVM model)
        {
            try
            {
                var response = _vnPayservice.PaymentExecute(Request.Query);

                // Check if payment failed
                if (response == null || response.VnPayResponseCode != "00")
                {
                    _logger.LogWarning($"VNPay Payment Failed. ResponseCode: {response?.VnPayResponseCode ?? "null"}");
                    TempData["Message"] = "Thanh toán không thành công. Vui lòng thử lại.";
                    return RedirectToAction("PaymentFail");
                }

                // Get customer details
                var customerId = HttpContext.User.Claims.SingleOrDefault(p => p.Type == MySetting.CLAIM_CUSTOMERID)?.Value;
                if (string.IsNullOrEmpty(customerId))
                {
                    _logger.LogWarning("Customer ID not found in claims.");
                    TempData["Message"] = "Không tìm thấy thông tin khách hàng. Vui lòng đăng nhập lại.";
                    return RedirectToAction("PaymentFail");
                }

                // Map data from response to the order
                var hoadon = new HoaDon
                {
                    MaKh = customerId,
                    HoTen = model.HoTen ?? "Unknown Customer",
                    DiaChi = model.DiaChi ?? "N/A",
                    DienThoai = model.DienThoai ?? "N/A",
                    NgayDat = DateTime.Now,
                    CachThanhToan = "VNPay",
                    CachVanChuyen = "VnExpress",
                    MaTrangThai = 0,
                    GhiChu = model.GhiChu ?? "No additional notes",
                };

                // Save order and details in a database transaction
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

                        // Commit the transaction
                        await transaction.CommitAsync();

                        // Clear the cart
                        HttpContext.Session.Set<List<CartItem>>(MySetting.CART_KEY, new List<CartItem>());
                        HttpContext.Session.Remove("CouponCode");
                        HttpContext.Session.Remove("CouponDiscount");
                        TempData["Message"] = "Thanh toán VNPay thành công!";
                        return View("Success");
                    }
                    catch (Exception dbEx)
                    {
                        // Rollback the transaction in case of failure
                        await transaction.RollbackAsync();
                        _logger.LogError($"Error saving order details: {dbEx}");
                        TempData["Message"] = "Đã xảy ra lỗi khi xử lý đơn hàng. Vui lòng thử lại.";
                        return RedirectToAction("PaymentFail");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"VNPayCallBack encountered an error: {ex}");
                TempData["Message"] = "Đã xảy ra lỗi khi xử lý thanh toán. Vui lòng thử lại.";
                return RedirectToAction("PaymentFail");
            }
        }

        #endregion
    }
}

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