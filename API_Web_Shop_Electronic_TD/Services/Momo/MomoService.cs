using System.Data;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using API_Web_Shop_Electronic_TD.Models;

namespace API_Web_Shop_Electronic_TD.Services.Momo
{
	public class MomoService : IMomoService
	{
		private readonly IOptions<MomoOptionModel> _options;
		private readonly HttpClient _httpClient;
		private readonly ILogger<MomoService> _logger;

		public MomoService(
			IOptions<MomoOptionModel> options,
			HttpClient httpClient,
			ILogger<MomoService> logger)
		{
			_options = options;
			_httpClient = httpClient;
			_logger = logger;
		}

		public MomoExecuteResponseModel PaymentExecuteAsync(IQueryCollection collection)
		{
			try
			{
				_logger.LogInformation("Processing MOMO callback with query parameters: {@QueryParams}",
					collection.ToDictionary(x => x.Key, x => x.Value.ToString()));

				// Extract and validate required parameters
				var amount = collection["amount"].ToString();
				var orderId = collection["orderId"].ToString();
				var orderInfo = collection["orderInfo"].ToString();
				var resultCode = int.Parse(collection["resultCode"].ToString());
				var message = collection["message"].ToString();
				var payType = collection["payType"].ToString();
				var transId = long.Parse(collection["transId"].ToString());

				if (string.IsNullOrEmpty(amount) || string.IsNullOrEmpty(orderId))
				{
					throw new ArgumentException("Missing required MOMO callback parameters");
				}

				return new MomoExecuteResponseModel
				{
					Amount = amount,
					OrderId = orderId,
					OrderInfo = orderInfo,
					ResponseCode = resultCode,
					Message = message,
					PayType = payType,
					TransId = transId
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error processing MOMO callback");
				throw;
			}
		}


		public async Task<MomoCreatePaymentResponseModel> CreatePaymentAsync(OrderInfoModel model)
		{
			model.OrderId = DateTime.UtcNow.Ticks.ToString();
			model.OrderInfo = "Khách hàng: " + model.FullName + ". Nội dung: " + model.OrderInfo;
			var rawData = $"partnerCode={_options.Value.PartnerCode}&accessKey={_options.Value.AccessKey}&requestId={model.OrderId}&amount={model.Amount}&orderId={model.OrderId}&orderInfo={model.OrderInfo}&returnUrl={_options.Value.ReturnUrl}&notifyUrl={_options.Value.NotifyUrl}&extraData=";

			var signature = ComputeHmacSha256(rawData, _options.Value.SecretKey);

			var client = new RestClient(_options.Value.MomoApiUrl);
			var request = new RestRequest(_options.Value.MomoApiUrl, Method.Post);
			request.AddHeader("Content-Type", "application/json");
			// Create an object representing the request data
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

			request.AddParameter("application/json", JsonConvert.SerializeObject(requestData), ParameterType.RequestBody);

			var response = await client.ExecuteAsync(request);

			return JsonConvert.DeserializeObject<MomoCreatePaymentResponseModel>(response.Content);
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