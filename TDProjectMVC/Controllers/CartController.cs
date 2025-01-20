using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TDProjectMVC.Data;
using TDProjectMVC.Helpers;
using TDProjectMVC.Models.MoMo;
using TDProjectMVC.Services.Mail;
using TDProjectMVC.Services.Map;
using TDProjectMVC.Services.Momo;
using TDProjectMVC.Services.PayPal;
using TDProjectMVC.Services.VnPay;
using TDProjectMVC.ViewModels;
namespace TDProjectMVC.Controllers
{
    public class CartController : Controller
    {
        private readonly Hshop2023Context db;
        private readonly IVnPayService _vnPayservice;
        private readonly IMailSender _mailSender;
        private readonly IMomoService _momoService;
        private readonly ILogger<CartController> _logger;
        private readonly OpenStreetMapService _osmService;
        private readonly IPayPalService _payPalService;
        public CartController(IMailSender mailSender,
            OpenStreetMapService osmService, Hshop2023Context context, IVnPayService vnPayService, IMomoService momoService, ILogger<CartController> logger, IPayPalService payPalService)
        {
            db = context;
            _vnPayservice = vnPayService;
            _mailSender = mailSender;
            _momoService = momoService;
            _logger = logger;
            _osmService = osmService;
            _payPalService = payPalService;

        }
        public List<CartItem> Cart => HttpContext.Session.Get<List<CartItem>>(MySetting.CART_KEY) ?? new List<CartItem>();

        #region ---- CART ----
        public IActionResult Index()
        {
            return View(Cart);
        }
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
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["error"] = "Vui lòng kiểm tra lại thông tin đặt hàng";
                    return View(Cart);
                }

                var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(customerId))
                {
                    return RedirectToAction("DangNhap", "KhachHang");
                }

                // Kiểm tra giỏ hàng
                if (Cart == null || !Cart.Any())
                {
                    TempData["error"] = "Giỏ hàng trống!";
                    return RedirectToAction("Index", "Cart");
                }

                // Tính toán giá trị đơn hàng
                string couponCode = HttpContext.Session.GetString("CouponCode");
                decimal discount = 0;
                if (!string.IsNullOrEmpty(couponCode))
                {
                    var discountStr = HttpContext.Session.GetString("CouponDiscount");
                    decimal.TryParse(discountStr, out discount);
                }

                decimal subtotal = (decimal)Cart.Sum(p => p.SoLuong * p.DonGia);
                decimal total = subtotal * (100 - discount) / 100;
                HttpContext.Session.SetString("FullName", model.HoTen);
                HttpContext.Session.SetString("PhoneNumber", model.DienThoai);
                HttpContext.Session.SetString("Address", model.DiaChi);

                // Tạo đối tượng HoaDon
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
                    GhiChu = model.GhiChu
                };

                // Xử lý theo phương thức thanh toán
                switch (paymentMethod?.ToUpper())
                {
                    case "COD":
                        return await ProcessOrder(hoadon, model.Email);

                    case "MOMO":
                        var momoModel = new OrderInfoModel
                        {
                            FullName = model.HoTen,
                            Amount = total,
                            OrderId = Guid.NewGuid().ToString(),
                            OrderInfo = "Thanh toán MOMO"
                        };

                        CreatePaymentMomo(momoModel);
                        return await PaymentCallBack(model); ;

                    case "VNPAY":
                        var paymentModel = new PaymentInformationModel
                        {
                            FullName = model.HoTen,
                            Amount = (double)total * 100,
                            Description = "Thanh toán qua VNPAY",
                            OrderType = "other"
                        };
                        var paymentUrl = _vnPayservice.CreatePaymentUrl(paymentModel, HttpContext);

                        return Redirect(paymentUrl);
                    case "PAYPAL":
                        var paypalModel = new PaymentInformationModel
                        {
                            FullName = model.HoTen,
                            Amount = (double)total,
                            Description = "Thanh toán qua PayPal",
                            OrderType = "other"
                        };
                        var url = await _payPalService.CreatePaymentUrl(paypalModel, HttpContext);

                        return Redirect(url);

                    default:
                        TempData["error"] = "Vui lòng chọn phương thức thanh toán!";
                        return View(Cart);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Checkout Error: {ex}");
                TempData["error"] = "Đã xảy ra lỗi trong quá trình xử lý đơn hàng!";
                return View(Cart);
            }
        }
        private async Task<bool> SaveOrderAndPaymentData(
            string customerId,
            string fullName,
            string address,
            string phone,
            string paymentMethod,
            string orderInfo,
            decimal amount,
            string? couponCode,
            List<CartItem> cartItems,
            string? notes = null
        )
        {
            using (var transaction = await db.Database.BeginTransactionAsync())
            {
                try
                {
                    var payHistory = new PayHistory
                    {
                        FullName = fullName,
                        OrderInfo = $"Thanh toán qua {paymentMethod}",
                        Amount = (double)amount,
                        PayMethod = paymentMethod,
                        CouponCode = couponCode,
                        CreateDate = DateTime.UtcNow
                    };
                    db.Add(payHistory);
                    await db.SaveChangesAsync();

                    var hoaDon = new HoaDon
                    {
                        MaKh = customerId,
                        HoTen = fullName,
                        DiaChi = address,
                        DienThoai = phone,
                        NgayDat = DateTime.Now,
                        CachThanhToan = paymentMethod,
                        CachVanChuyen = "GRAB",
                        MaTrangThai = 0,
                        MaNv = null,
                        GhiChu = notes,
                        PayId = payHistory.Id
                    };

                    var customerExists = await db.KhachHangs.AnyAsync(k => k.MaKh == customerId);
                    if (!customerExists)
                    {
                        throw new Exception($"Customer with ID {customerId} not found");
                    }

                    db.Add(hoaDon);
                    await db.SaveChangesAsync();

                    var chiTietHds = cartItems.Select(item => new ChiTietHd
                    {
                        MaHd = hoaDon.MaHd,
                        MaHh = item.MaHH,
                        DonGia = item.DonGia,
                        SoLuong = item.SoLuong,
                        MaGiamGia = item.GiamGia
                    }).ToList();

                    db.AddRange(chiTietHds);
                    await db.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return true;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError($"Error saving order data: {ex.Message}");
                    throw;
                }
            }
        }

        // Helper method to verify all required foreign keys exist
        private async Task ValidateForeignKeys(string customerId, List<string> productIds)
        {
            // Verify customer exists
            var customerExists = await db.KhachHangs.AnyAsync(k => k.MaKh == customerId);
            if (!customerExists)
            {
                throw new Exception($"Customer with ID {customerId} not found");
            }

            // Verify all products exist
            //var existingProducts = await db.KhachHangs
            //    .Where(h => productIds.Contains(h.MaHh))
            //    .Select(h => h.MaHh)
            //    .ToListAsync();

            //var missingProducts = productIds.Except(existingProducts).ToList();
            //if (missingProducts.Any())
            //{
            //    throw new Exception($"Products not found: {string.Join(", ", missingProducts)}");
            //}

            var statusExists = await db.TrangThais.AnyAsync(t => t.MaTrangThai == 0);
            if (!statusExists)
            {
                throw new Exception("Invalid order status");
            }
        }
        private void ClearOrderSessionData()
        {
            // Clear cart
            HttpContext.Session.Remove(MySetting.CART_KEY);

            // Clear coupon related data
            HttpContext.Session.Remove("CouponCode");
            HttpContext.Session.Remove("CouponDiscount");
            HttpContext.Session.Remove("FullName");
            HttpContext.Session.Remove("PhoneNumber");
            HttpContext.Session.Remove("Address");
            
        }
        // Refactored Method to Process Order
        private async Task<IActionResult> ProcessOrder(HoaDon hoadon, string email)
        {
            try
            {
                string couponCode = HttpContext.Session.GetString("CouponCode");
                decimal amount = (decimal)Cart.Sum(item => item.SoLuong * item.DonGia);

                bool saved = await SaveOrderAndPaymentData(
                    customerId: hoadon.MaKh,
                    fullName: hoadon.HoTen,
                    address: hoadon.DiaChi,
                    phone: hoadon.DienThoai,
                    paymentMethod: "COD",
                    orderInfo: "Thanh toán khi nhận hàng",
                    amount: amount,
                    couponCode: couponCode,
                    cartItems: Cart,
                    notes: hoadon.GhiChu
                );

                if (saved)
                {
                    HttpContext.Session.Remove(MySetting.CART_KEY);
                    HttpContext.Session.Remove("CouponCode");
                    HttpContext.Session.Remove("CouponDiscount");

                    if (!string.IsNullOrEmpty(email))
                    {
                        await _mailSender.SendEmailAsync(email, "Đặt hàng thành công", "Đơn hàng đã được đặt thành công!");
                    }
                    var notification = new Notification
                    {
                        Name = "Đơn hàng mới",
                        Description = $"Tài khoản {hoadon.HoTen} đã tạo đơn hàng thành công.",
                        Status = false,
                        CreateAt = DateTime.Now
                    };
                    db.Notifications.Add(notification);
                    await db.SaveChangesAsync();
                    TempData["success"] = "Đặt hàng thành công";
                    return RedirectToAction("PaymentSuccess");
                }

                TempData["error"] = "Đã xảy ra lỗi khi lưu đơn hàng";
                return View("PaymentFail");
            }
            catch (Exception ex)
            {
                _logger.LogError($"ProcessOrder Error: {ex.Message}");
                TempData["error"] = "Đã xảy ra lỗi vui lòng liên hệ nhà phát triển!";
                return View("PaymentFail");
            }
        }


        public IActionResult PaymentFail()
        {
            return View();
        }
        public IActionResult PaymentSuccess()
        {
            ClearOrderSessionData();
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

        [HttpPost]
        public async Task<IActionResult> PaymentCallBack(CheckOutVM hoadon)
        {
            try
            {
                var response = _momoService.PaymentExecuteAsync(HttpContext.Request.Query);
                var requestQuery = HttpContext.Request.Query;

                if (requestQuery["resultCode"] == "0") // Successful payment
                {
                    var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (string.IsNullOrEmpty(customerId))
                    {
                        _logger.LogWarning("Customer ID not found in claims.");
                        TempData["error"] = "Không tìm thấy thông tin khách hàng. Vui lòng đăng nhập lại.";
                        return RedirectToAction("PaymentFail");
                    }

                    var amount = decimal.Parse(requestQuery["amount"]);
                    var orderInfo = requestQuery["orderInfo"].ToString();
                    var cart = HttpContext.Session.Get<List<CartItem>>(MySetting.CART_KEY);

                    if (cart == null || !cart.Any())
                    {
                        TempData["error"] = "Giỏ hàng trống.";
                        return RedirectToAction("PaymentFail");
                    }
                    string fullName = HttpContext.Session.GetString("FullName");
                    string phoneNumber = HttpContext.Session.GetString("PhoneNumber");
                    string address = HttpContext.Session.GetString("Address");
                    bool saved = await SaveOrderAndPaymentData(
                        customerId: customerId,
                        fullName: fullName,
                        address: address,
                        phone: phoneNumber,
                        paymentMethod: "MOMO",
                        orderInfo: orderInfo,
                        amount: amount,
                        couponCode: HttpContext.Session.GetString("CouponCode"),
                        cartItems: cart,
                        notes: hoadon.GhiChu
                    );

                    if (saved)
                    {
                        var notification = new Notification
                        {
                            Name = "Đơn hàng mới",
                            Description = $"Tài khoản {hoadon.HoTen} đã tạo đơn hàng thành công.",
                            Status = false,
                            CreateAt = DateTime.Now
                        };
                        db.Notifications.Add(notification);
                        await db.SaveChangesAsync();

                        TempData["success"] = "Giao dịch thành công!";
                        return RedirectToAction("PaymentSuccess");
                    }

                    TempData["error"] = "Đơn hàng đã được tạo nhưng có lỗi khi lưu.";
                    return RedirectToAction("PaymentFail");
                }
                else // Failed or canceled payment
                {
                    _logger.LogWarning($"MOMO Payment Failed. ResponseCode: {requestQuery["resultCode"]}");
                    TempData["error"] = "Giao dịch Momo đã bị hủy.";
                    return RedirectToAction("Index", "Cart");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"MOMO PaymentCallBack Error: {ex}");
                TempData["error"] = "Có lỗi xảy ra khi xử lý đơn hàng: " + ex.Message;
                return RedirectToAction("PaymentFail");
            }
        }
        #endregion
        #region --- VNPAY ---
        public IActionResult CreatePaymentVnPay(PaymentInformationModel paymentModel)
        {
            // Validate the input model
            if (paymentModel == null)
            {
                TempData["error"] = "Thông tin thanh toán không hợp lệ.";
                return RedirectToAction("PaymentFail");
            }

            // Validate the amount
            if (paymentModel.Amount <= 0)
            {
                TempData["error"] = "Số tiền thanh toán không hợp lệ.";
                return RedirectToAction("PaymentFail");
            }

            try
            {
                // Call the service to create a payment
                var paymentUrl = _vnPayservice.CreatePaymentUrl(paymentModel, HttpContext);

                // Redirect to VNPAY payment gateway
                return Redirect(paymentUrl);
            }
            catch (Exception ex)
            {
                // Log the exception
                _logger.LogError(ex, "An error occurred while creating VNPAY payment.");

                // Show a user-friendly error message
                TempData["error"] = "Đã xảy ra lỗi trong quá trình xử lý thanh toán. Vui lòng thử lại.";
                return RedirectToAction("PaymentFail");
            }
        }
        [HttpGet]
        public async Task<IActionResult> VNPayCallBack(CheckOutVM hoadon)
        {
            try
            {
                var vnpayResponse = _vnPayservice.PaymentExecute(Request.Query);

                if (!vnpayResponse.Success)
                {
                    _logger.LogWarning("VNPay payment validation failed");
                    TempData["error"] = "Không thể xác thực giao dịch";
                    return RedirectToAction("PaymentFail");
                }

                if (vnpayResponse.VnPayResponseCode != "00")
                {
                    _logger.LogWarning($"VNPay payment failed. Response Code: {vnpayResponse.VnPayResponseCode}");
                    TempData["error"] = "Thanh toán không thành công";
                    return RedirectToAction("PaymentFail");
                }

                var customerId = HttpContext.User.Claims.SingleOrDefault(p => p.Type == MySetting.CLAIM_CUSTOMERID)?.Value;
                if (string.IsNullOrEmpty(customerId))
                {
                    _logger.LogWarning("Customer ID not found in claims");
                    TempData["error"] = "Không tìm thấy thông tin khách hàng";
                    return RedirectToAction("PaymentFail");
                }

                var cart = Cart;
                if (cart == null || !cart.Any())
                {
                    TempData["error"] = "Giỏ hàng trống";
                    return RedirectToAction("PaymentFail");
                }

                decimal amount = (decimal)cart.Sum(item => item.SoLuong * item.DonGia);
                string fullName = HttpContext.Session.GetString("FullName");
                string phoneNumber = HttpContext.Session.GetString("PhoneNumber");
                string address = HttpContext.Session.GetString("Address");
                bool saved = await SaveOrderAndPaymentData(
                    customerId: customerId,
                    fullName: fullName,
                    address: address,
                    phone: phoneNumber,
                    paymentMethod: "VNPAY",
                    orderInfo: vnpayResponse.OrderDescription,
                    amount: amount,
                    couponCode: HttpContext.Session.GetString("CouponCode"),
                    cartItems: cart,
                    notes: $"VNPAY Transaction ID: {vnpayResponse.TransactionId}"
                );

                if (saved)
                {
                    var notification = new Notification
                    {
                        Name = "Đơn hàng mới",
                        Description = $"Tài khoản {hoadon.HoTen} đã tạo đơn hàng thành công.",
                        Status = false,
                        CreateAt = DateTime.Now
                    };
                    db.Notifications.Add(notification);
                    await db.SaveChangesAsync();

                    TempData["success"] = "Thanh toán thành công!";
                    return RedirectToAction("PaymentSuccess");
                }

                TempData["error"] = "Lỗi khi lưu thông tin đơn hàng";
                return RedirectToAction("PaymentFail");
            }
            catch (Exception ex)
            {
                _logger.LogError($"VNPayCallBack error: {ex}");
                TempData["error"] = "Đã xảy ra lỗi trong quá trình xử lý";
                return RedirectToAction("PaymentFail");
            }
        }
        #endregion

        #region --- PAYPAL ---

        [HttpGet] // Changed from HttpPost to HttpGet
        public async Task<IActionResult> PayPalCallBack([FromQuery] string? paymentId, [FromQuery] string? PayerID)
        {
            try
            {
                var response = _payPalService.PaymentExecute(HttpContext.Request.Query);

                if (response.Success) // Successful payment
                {
                    var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (string.IsNullOrEmpty(customerId))
                    {
                        _logger.LogWarning("Customer ID not found in claims.");
                        TempData["error"] = "Không tìm thấy thông tin khách hàng. Vui lòng đăng nhập lại.";
                        return RedirectToAction("PaymentFail");
                    }

                    var cart = HttpContext.Session.Get<List<CartItem>>(MySetting.CART_KEY);
                    if (cart == null || !cart.Any())
                    {
                        TempData["error"] = "Giỏ hàng trống.";
                        return RedirectToAction("PaymentFail");
                    }

                    // Lấy thông tin từ session
                    string fullName = HttpContext.Session.GetString("FullName");
                    string phoneNumber = HttpContext.Session.GetString("PhoneNumber");
                    string address = HttpContext.Session.GetString("Address");

                    // Tính tổng tiền
                    decimal amount = (decimal)cart.Sum(item => item.SoLuong * item.DonGia);

                    // Kiểm tra và áp dụng mã giảm giá nếu có
                    string couponCode = HttpContext.Session.GetString("CouponCode");
                    if (!string.IsNullOrEmpty(couponCode))
                    {
                        var discountStr = HttpContext.Session.GetString("CouponDiscount");
                        if (decimal.TryParse(discountStr, out decimal discount))
                        {
                            amount = amount * (100 - discount) / 100;
                        }
                    }

                    // Lưu thông tin đơn hàng và thanh toán
                    bool saved = await SaveOrderAndPaymentData(
                        customerId: customerId,
                        fullName: fullName,
                        address: address,
                        phone: phoneNumber,
                        paymentMethod: "PAYPAL",
                        orderInfo: $"Thanh toán PayPal - Payment ID: {paymentId}",
                        amount: amount,
                        couponCode: couponCode,
                        cartItems: cart
                    );

                    if (saved)
                    {
                        // Tạo thông báo
                        var notification = new Notification
                        {
                            Name = "Đơn hàng mới",
                            Description = $"Tài khoản {fullName} đã tạo đơn hàng thành công.",
                            Status = false,
                            CreateAt = DateTime.Now
                        };
                        db.Notifications.Add(notification);
                        await db.SaveChangesAsync();

                        TempData["success"] = "Giao dịch PayPal thành công!";
                        return RedirectToAction("PaymentSuccess");
                    }

                    TempData["error"] = "Đơn hàng đã được tạo nhưng có lỗi khi lưu.";
                    return RedirectToAction("PaymentFail");
                }
                else // Failed or canceled payment
                {
                    _logger.LogWarning($"PayPal Payment Failed. Payment ID: {paymentId}");
                    TempData["error"] = "Giao dịch PayPal đã bị hủy hoặc thất bại.";
                    return RedirectToAction("Index", "Cart");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"PayPal PaymentCallBack Error: {ex}");
                TempData["error"] = "Có lỗi xảy ra khi xử lý đơn hàng PayPal: " + ex.Message;
                return RedirectToAction("PaymentFail");
            }
        }

        #endregion
        #region ---OpenStreetMap---
        public async Task<IActionResult> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest(new { message = "Query cannot be empty." });
            }
            try
            {
                // Log the incoming request
                _logger.LogInformation($"Received search request with query: {query}");

                var suggestions = await _osmService.GetAddressSuggestionsAsync(query);

                // Log the OpenStreetMap URL
                var osmUrl = $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(query)}&format=json&addressdetails=1&limit=5";
                _logger.LogInformation($"OpenStreetMap URL: {osmUrl}");

                if (suggestions == null || !suggestions.Any())
                {
                    _logger.LogWarning("No suggestions found for query: {Query}", query);
                    return NotFound(new { message = "No suggestions found." });
                }

                // Log the number of suggestions
                _logger.LogInformation($"Found {suggestions.Count} suggestions for query: {query}");

                return Ok(suggestions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching for address with query: {Query}", query);
                return StatusCode(500, new { message = "Internal server error.", details = ex.Message });
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