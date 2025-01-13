using API_Web_Shop_Electronic_TD.Data;
using API_Web_Shop_Electronic_TD.Helpers;
using API_Web_Shop_Electronic_TD.Interfaces;
using API_Web_Shop_Electronic_TD.Models;
using API_Web_Shop_Electronic_TD.Services;
using API_Web_Shop_Electronic_TD.Services.Map;
using API_Web_Shop_Electronic_TD.Services.Momo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

[ApiController]
[Route("api/[controller]")]
public class CheckoutController : ControllerBase
{
	private readonly ICheckOutRepository _checkoutRepository;
	private readonly IVnPayService _vnPayService;
	private readonly IMomoService _momoService;
	private readonly ILogger<CheckoutController> _logger;
	private readonly OpenStreetMapService _osmService;

	public CheckoutController(
		ICheckOutRepository checkoutRepository,
		IVnPayService vnPayService,
		IMomoService momoService,
		ILogger<CheckoutController> logger,
		OpenStreetMapService osmService)
	{
		_checkoutRepository = checkoutRepository;
		_vnPayService = vnPayService;
		_momoService = momoService;
		_logger = logger;
		_osmService = osmService;

	}

	[HttpPost]
	public async Task<IActionResult> ProcessCheckout([FromBody] CheckOutMD request)
	{
		try
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(new
				{
					Errors = ModelState.Values
						.SelectMany(v => v.Errors)
						.Select(e => e.ErrorMessage)
				});
			}

			// Process initial payment in repository
			int orderId = await _checkoutRepository.ProcessPaymentAsync(request, request.PayMethod);

			// Handle different payment methods
			switch (request.PayMethod.ToLower())
			{
				case "vnpay":
					var vnpayModel = new PaymentInformationModel
					{
						FullName = request.FullName,
						Amount = request.Amount * 100,
						Description = "Thanh toán qua VNPay",
						OrderType = request.PayMethod,
						OrderId = orderId
					};
					var vnpayUrl = _vnPayService.CreatePaymentUrl(vnpayModel, HttpContext);
					return Ok(new { PayUrl = vnpayUrl, OrderId = orderId });

				case "momo":
					var momoModel = new OrderInfoModel
					{
						FullName = request.FullName,
						Amount = (decimal)request.Amount,
						OrderId = orderId.ToString(),
						OrderInfo = "Thanh toán MOMO"
					};
					return await CreatePaymentMomo(momoModel);

				case "directcheck":
					return Ok(new { Id = orderId });

				default:
					return BadRequest(new { Message = "Invalid payment method" });
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error processing checkout");
			return StatusCode(500, new { Message = "An error occurred while processing your order" });
		}
	}
	[HttpPost("momo/payment")]
	public async Task<IActionResult> CreatePaymentMomo(OrderInfoModel model)
	{
		// Validate the input model
		if (model == null)
		{
			return BadRequest(new { Message = "Invalid payment information." });
		}

		if (model.Amount <= 0)
		{
			return BadRequest(new { Message = "Invalid payment amount." });
		}

		model.FullName ??= User.Identity?.Name ?? "Unknown User";
		model.OrderInfo ??= "Payment for order";

		_logger.LogInformation($"Initiating MoMo payment with details: Amount={model.Amount}, FullName={model.FullName}, OrderInfo={model.OrderInfo}");

		try
		{
			var response = await _momoService.CreatePaymentAsync(model);

			if (response == null || string.IsNullOrEmpty(response.PayUrl))
			{
				_logger.LogError("Failed to create MoMo payment. Response is null or missing PayUrl.");
				return BadRequest(new { Message = "Failed to create MoMo payment. Please try again." });
			}

			_logger.LogInformation($"MoMo Payment URL created: {response.PayUrl}");
			return Ok(new { PayUrl = response.PayUrl });
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "An error occurred while creating MoMo payment.");
			return StatusCode(500, new { Message = "An error occurred during payment processing. Please try again." });
		}
	}

	[HttpGet("vnpay/callback")]
	public async Task<IActionResult> VnPayCallback([FromQuery] string orderId)
	{
		try
		{
			var paymentResponse = _vnPayService.PaymentExecute(Request.Query);
			if (!paymentResponse.Success)
			{
				_logger.LogWarning("VNPay payment validation failed: {OrderId}", orderId);
				await _checkoutRepository.UpdatePaymentStatus(int.Parse(orderId), false);
				return Redirect("https://localhost:4200/checkout/failure");
			}

			await _checkoutRepository.UpdatePaymentStatus(int.Parse(orderId), true);
			return Redirect("https://localhost:4200/checkout/success");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error processing VNPay callback");
			return Redirect("https://localhost:4200/checkout/failure");
		}
	}

	[HttpGet("momo/callback")]
	public async Task<IActionResult> MomoCallback()
	{
		try
		{
			_logger.LogInformation("Received MOMO callback");

			var response = _momoService.PaymentExecuteAsync(HttpContext.Request.Query);

			if (!int.TryParse(response.OrderId, out int orderId))
			{
				_logger.LogError("Invalid orderId in MOMO callback: {OrderId}", response.OrderId);
				return Redirect("https://localhost:4200/checkout/failure");
			}

			var isSuccessful = response.ResponseCode == 0;

			_logger.LogInformation(
				"MOMO payment {Status} for order {OrderId}. TransactionId: {TransId}",
				isSuccessful ? "succeeded" : "failed",
				orderId,
				response.TransId
			);

			await _checkoutRepository.UpdatePaymentStatus(orderId, isSuccessful);

			return Redirect(isSuccessful ? "https://localhost:4200/checkout/success" : "https://localhost:4200/checkout/failure");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error processing MOMO callback");
			return Redirect("https://localhost:4200/checkout/failure");
		}
	}
	[HttpGet("search")]
	public async Task<IActionResult> Search(string query)
	{
		if (string.IsNullOrWhiteSpace(query))
		{
			return BadRequest(new { message = "Query cannot be empty." });
		}

		try
		{
			var suggestions = await _osmService.GetAddressSuggestionsAsync(query);
			if (suggestions == null || !suggestions.Any())
			{
				return NotFound(new { message = "No suggestions found." });
			}

			// Tính phí ship cho từng địa chỉ
			foreach (var suggestion in suggestions)
			{
				suggestion.ShippingFee = _osmService.CalculateShippingFee(new AddressSuggestion
				{
					Lat = suggestion.Lat,
					Lon = suggestion.Lon,
					display_name = suggestion.display_name
				});
			}

			return Ok(suggestions);
		}
		catch (Exception ex)
		{
			return StatusCode(500, new { message = "Internal server error.", details = ex.Message });
		}
	}


}