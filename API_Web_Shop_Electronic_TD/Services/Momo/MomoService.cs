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

		public MomoService(IOptions<MomoOptionModel> options, HttpClient httpClient)
		{
			_options = options;
			_httpClient = httpClient;
		}

		public MomoExecuteResponseModel PaymentExecuteAsync(IQueryCollection collection)
		{
			if (!collection.TryGetValue("amount", out var amount) ||
				!collection.TryGetValue("orderInfo", out var orderInfo) ||
				!collection.TryGetValue("orderId", out var orderId))
			{
				throw new ArgumentException("Missing required query parameters: amount, orderInfo, or orderId.");
			}

			return new MomoExecuteResponseModel
			{
				Amount = amount,
				OrderId = orderId,
				OrderInfo = orderInfo
			};
		}


		public async Task<MomoCreatePaymentResponseModel> CreatePaymentAsync(OrderInfoModel model)
		{
			model.OrderId = DateTime.UtcNow.Ticks.ToString();
			model.OrderInfo = "Khách hàng: " + model.FullName + ". Nội dung: " + model.OrderInfo;

			var rawData =
				$"partnerCode={_options.Value.PartnerCode}&accessKey={_options.Value.AccessKey}&requestId={model.OrderId}&amount={model.Amount}&orderId={model.OrderId}&orderInfo={model.OrderInfo}&returnUrl={_options.Value.ReturnUrl}¬ifyUrl={_options.Value.NotifyUrl}&extraData=";

			if (string.IsNullOrEmpty(rawData))
			{
				throw new ArgumentException("Raw data for signature cannot be null or empty.");
			}

			if (string.IsNullOrEmpty(_options.Value.SecretKey))
			{
				throw new ArgumentException("Secret key cannot be null or empty.");
			}

			var signature = ComputeHmacSha256(rawData, _options.Value.SecretKey);

			var client = new RestClient(_options.Value.MomoApiUrl);
			var request = new RestRequest() { Method = Method.Post };
			request.AddHeader("Content-Type", "application/json; charset=UTF-8");

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

			if (response == null || string.IsNullOrEmpty(response.Content))
			{
				throw new Exception("Failed to receive a valid response from the Momo API.");
			}

			return JsonConvert.DeserializeObject<MomoCreatePaymentResponseModel>(response.Content);
		}

		private string ComputeHmacSha256(string message, string secretKey)
		{
			if (string.IsNullOrEmpty(message))
			{
				throw new ArgumentNullException(nameof(message), "Message cannot be null or empty.");
			}

			if (string.IsNullOrEmpty(secretKey))
			{
				throw new ArgumentNullException(nameof(secretKey), "Secret key cannot be null or empty.");
			}

			var keyBytes = Encoding.UTF8.GetBytes(secretKey);
			var messageBytes = Encoding.UTF8.GetBytes(message);

			using var hmac = new HMACSHA256(keyBytes);
			var hashBytes = hmac.ComputeHash(messageBytes);

			return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
		}
	}
}