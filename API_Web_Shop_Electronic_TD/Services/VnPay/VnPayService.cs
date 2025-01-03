using API_Web_Shop_Electronic_TD.Helpers;
using API_Web_Shop_Electronic_TD.Models;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace API_Web_Shop_Electronic_TD.Services
{
    public class VnPayService : IVnPayService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<VnPayService> _logger;

        public VnPayService(IConfiguration config, ILogger<VnPayService> logger)
        {
            _configuration = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public string CreatePaymentUrl(PaymentInformationModel model, HttpContext context)
        {
            ArgumentNullException.ThrowIfNull(model, nameof(model));
            ArgumentNullException.ThrowIfNull(context, nameof(context));

            if (model.Amount <= 0)
                throw new ArgumentException("Amount must be greater than zero");

            var pay = new VnPayLibrary();
            pay.AddRequestData("vnp_Version", _configuration["Vnpay:Version"]);
            pay.AddRequestData("vnp_Command", _configuration["Vnpay:Command"]);
            pay.AddRequestData("vnp_TmnCode", _configuration["Vnpay:TmnCode"]);
            pay.AddRequestData("vnp_Amount", ((model.Amount)).ToString());
            pay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            pay.AddRequestData("vnp_CurrCode", "VND");
            pay.AddRequestData("vnp_IpAddr", context.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1");
            pay.AddRequestData("vnp_Locale", "vn");
            pay.AddRequestData("vnp_OrderInfo", $"{model.FullName} - {model.Description}");
            pay.AddRequestData("vnp_OrderType", model.OrderType ?? "other");
            pay.AddRequestData("vnp_ReturnUrl", _configuration["PaymentCallBack:ReturnUrl"]);
            pay.AddRequestData("vnp_TxnRef", DateTime.Now.Ticks.ToString());

            var paymentUrl = pay.CreateRequestUrl(_configuration["Vnpay:BaseUrl"], _configuration["Vnpay:HashSecret"]);

            _logger.LogInformation("Payment URL: " + paymentUrl);
            return paymentUrl;
        }

        public PaymentResponseModel PaymentExecute(IQueryCollection collections)
        {
            ArgumentNullException.ThrowIfNull(collections, nameof(collections));

            try
            {
                var pay = new VnPayLibrary();
                var hashSecret = _configuration["Vnpay:HashSecret"]
                    ?? throw new InvalidOperationException("VnPay Hash Secret not configured");

                var response = GetFullResponseData(collections, hashSecret);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing payment");
                throw;
            }
        }

        public PaymentResponseModel GetFullResponseData(IQueryCollection collection, string hashSecret)
        {
            ArgumentNullException.ThrowIfNull(collection, nameof(collection));
            ArgumentNullException.ThrowIfNull(hashSecret, nameof(hashSecret));

            var vnPay = new VnPayLibrary();

            // Filter and add only VnPay-related parameters
            foreach (var (key, value) in collection)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    vnPay.AddResponseData(key, value);
                }
            }

            try
            {
                var orderId = Convert.ToInt64(vnPay.GetResponseData("vnp_TxnRef"));
                var vnPayTranId = Convert.ToInt64(vnPay.GetResponseData("vnp_TransactionNo"));
                var vnpResponseCode = vnPay.GetResponseData("vnp_ResponseCode");
                var vnpSecureHash = collection.FirstOrDefault(k => k.Key == "vnp_SecureHash").Value;

                // Sửa ValidateSignature
                var checkSignature = vnPay.ValidateSignature(vnpSecureHash, hashSecret);
                if (!checkSignature)
                {
                    _logger.LogWarning("Invalid payment signature");
                    return new PaymentResponseModel { Success = false };
                }

                return new PaymentResponseModel
                {
                    Success = true,
                    PaymentMethod = "VnPay",
                    OrderDescription = vnPay.GetResponseData("vnp_OrderInfo"),
                    OrderId = orderId.ToString(),
                    PaymentId = vnPayTranId.ToString(),
                    TransactionId = vnPayTranId.ToString(),
                    Token = vnpSecureHash,
                    VnPayResponseCode = vnpResponseCode
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment response");
                return new PaymentResponseModel { Success = false };
            }
        }

        private string ComputeHmacSha256(string message, string secretKey)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secretKey);
            var messageBytes = Encoding.UTF8.GetBytes(message);

            using var hmac = new HMACSHA256(keyBytes);
            var hashBytes = hmac.ComputeHash(messageBytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }
}
