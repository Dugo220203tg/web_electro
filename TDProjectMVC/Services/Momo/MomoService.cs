using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using TDProjectMVC.Models;
using TDProjectMVC.Models.MoMo;

namespace TDProjectMVC.Services.Momo
{
    public class MomoService : IMomoService
    {
        private readonly IOptions<MomoOptionModel> _options;
        private readonly HttpClient _httpClient;

        public MomoService(IOptions<MomoOptionModel> options, HttpClient httpClient)
        {
            _options = options;
            _httpClient = httpClient;
        }

        public MomoExecuteResponseModel PaymentExecuteAsync(IQueryCollection collection)
        {
            var amount = collection.FirstOrDefault(s => s.Key == "amount").Value.ToString() ?? "0";
            var orderInfo = collection.FirstOrDefault(s => s.Key == "orderInfo").Value.ToString() ?? string.Empty;
            var orderId = collection.FirstOrDefault(s => s.Key == "orderId").Value.ToString() ?? string.Empty;

            return new MomoExecuteResponseModel()
            {
                Amount = amount,
                OrderId = orderId,
                OrderInfo = orderInfo
            };
        }

        public async Task<MomoCreatePaymentResponseModel> CreatePaymentAsync(OrderInfoModel model)
        {
            try
            {
                model.OrderId = DateTime.UtcNow.Ticks.ToString();
                model.OrderInfo = "Khách hàng: " + model.FullName + ". Nội dung: " + model.OrderInfo;

                // Sửa lỗi URL format - thay "¬ifyUrl" bằng "&notifyUrl"
                var rawData =
                     $"partnerCode={_options.Value.PartnerCode}&accessKey={_options.Value.AccessKey}&requestId={model.OrderId}&amount={model.Amount}&orderId={model.OrderId}&orderInfo={model.OrderInfo}&returnUrl={_options.Value.ReturnUrl}&notifyUrl={_options.Value.NotifyUrl}&extraData=";

                var signature = ComputeHmacSha256(rawData, _options.Value.SecretKey);
                var client = new RestClient(_options.Value.MomoApiUrl);
                var request = new RestRequest() { Method = Method.Post };
                request.AddHeader("Content-Type", "application/json; charset=UTF-8");

                // Log thông tin request để debug
                Console.WriteLine($"MOMO API URL: {_options.Value.MomoApiUrl}");
                Console.WriteLine($"OrderId: {model.OrderId}");
                Console.WriteLine($"Amount: {model.Amount}");

                var requestData = new
                {
                    accessKey = _options.Value.AccessKey,
                    partnerCode = _options.Value.PartnerCode,
                    requestType = _options.Value.RequestType,
                    notifyUrl = _options.Value.NotifyUrl,
                    returnUrl = _options.Value.ReturnUrl,
                    orderId = model.OrderId,
                    amount = model.Amount.ToString(),
                    orderInfo = model.OrderInfo,
                    requestId = model.OrderId,
                    extraData = "",
                    signature = signature
                };

                var jsonRequest = JsonConvert.SerializeObject(requestData);
                Console.WriteLine($"Request JSON: {jsonRequest}");

                request.AddParameter("application/json", jsonRequest, ParameterType.RequestBody);
                var response = await client.ExecuteAsync(request);

                // Log response để debug
                Console.WriteLine($"MOMO Response Status: {response.StatusCode}");
                Console.WriteLine($"MOMO Response Content: {response.Content}");

                if (response.IsSuccessful)
                {
                    var result = JsonConvert.DeserializeObject<MomoCreatePaymentResponseModel>(response.Content);

                    // Log thông tin kết quả
                    Console.WriteLine($"MOMO PayUrl: {result?.PayUrl}");
                    Console.WriteLine($"MOMO ResultCode: {result?.ResultCode}");

                    return result;
                }
                else
                {
                    Console.WriteLine($"MOMO API call failed: {response.ErrorMessage}");
                    return new MomoCreatePaymentResponseModel
                    {
                        ResultCode = -1,
                        Message = $"API call failed: {response.ErrorMessage}"
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CreatePaymentAsync Exception: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");

                // Trả về đối tượng với thông báo lỗi thay vì ném ngoại lệ
                return new MomoCreatePaymentResponseModel
                {
                    ResultCode = -1,
                    Message = $"Exception: {ex.Message}"
                };
            }
        }

        private string ComputeHmacSha256(string message, string secretKey)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secretKey);
            var messageBytes = Encoding.UTF8.GetBytes(message);

            byte[] hashBytes;

            using (var hmac = new HMACSHA256(keyBytes))
            {
                hashBytes = hmac.ComputeHash(messageBytes);
            }

            var hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

            return hashString;
        }
    }
}