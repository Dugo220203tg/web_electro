//using System.Security.Cryptography;
//using System.Text;
//using System.Net.Http.Json;
//using TDProjectMVC.Models;
//using TDProjectMVC.Models.MoMo;
//using Microsoft.Extensions.Options;
//using Microsoft.DotNet.Scaffolding.Shared.CodeModifier.CodeChange;
//using Newtonsoft.Json;
//using System.Reflection.Metadata.Ecma335;

//namespace TDProjectMVC.Services.Momo
//{
//    public class MomoService : IMomoService
//    {
//        private readonly IOptions<MomoOptionModel> _options;
//        private readonly HttpClient _httpClient;

//        public MomoService(IOptions<MomoOptionModel> options, HttpClient httpClient)
//        {
//            _options = options;
//            _httpClient = httpClient;
//        }

//        public async Task<MomoCreatePaymentResponseModel> CreatePaymentMomo(OrderInfo model)
//        {
//            // Generate unique order ID and format order information
//            model.OrderID = DateTime.UtcNow.Ticks.ToString();
//            model.OrderInfomation = "Khách hàng: " + model.FullName + ". Nội dung: " + model.OrderInfomation;

//            // Generate raw data for the request signature
//            var rawData =
//                $"partnerCode={_options.Value.PartnerCode}" +
//                $"&accessKey={_options.Value.AccessKey}" +
//                $"&requestId={model.OrderID}" +
//                $"&amount={model.Amount}" +
//                $"&orderId={model.OrderID}" +
//                $"&orderInfo={model.OrderInfomation}" +
//                $"&returnUrl={_options.Value.ReturnUrl}" +
//                $"&notifyUrl={_options.Value.NotifyUrl}" +
//                $"&extraData="; // Optional, can include additional information

//            // Generate the signature
//            var signature = GenerateSignature(rawData, _options.Value.SecretKey);
//            var client = new RestClient(_options.Value.MomoApiUrl);
//            var request = new RestRequest() { Method = Method.Post };
//            request.AddHeader("Content-Type", "application/json, charset=UTF-8");

//            // Prepare the payment request payload
//            var paymentRequest = new
//            {
//                partnerCode = _options.Value.PartnerCode,
//                accessKey = _options.Value.AccessKey,
//                requestId = model.OrderID,
//                amount = model.Amount,
//                orderId = model.OrderID,
//                orderInfo = model.OrderInfomation,
//                returnUrl = _options.Value.ReturnUrl,
//                notifyUrl = _options.Value.NotifyUrl,
//                extraData = "", // Optional
//                requestType = "captureMoMoWallet",
//                signature
//            };

//            // Send the request to Momo's API
//            request.AddParameter("application/json", JsonConvert.SerializeObject(requestData), ParameterType.RequestBody);

//            var response = await client.ExecuteAsync(request);
//            return JsonConvert.DeserializeObject<MomoCreatePaymentResponseModel>(response.Content);
//        }
//        public MomoExecuteResponseModel PaymentExecuteAsync(IQueryCollection collection)
//        {
//            var amount = collection.First(s => s.Key == "amount").Value;
//            var orderInfo = collection.First(s => s.Key == "orderInfo").Value;
//            var orderId = collection.First(s => s.Key == "orderId").Value;
//            return new MomoExecuteResponseModel()
//            {
//                Amount = amount,
//                OrderId = orderId,
//                OrderInfo = orderInfo
//            };
//        }
//        private static string GenerateSignature(string rawData, string secretKey)
//        {
//            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
//            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawData));
//            return Convert.ToHexString(hash).ToLower();
//        }
//    }
//}
