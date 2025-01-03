using API_Web_Shop_Electronic_TD.Data;
using API_Web_Shop_Electronic_TD.Helpers;
using API_Web_Shop_Electronic_TD.Interfaces;
using API_Web_Shop_Electronic_TD.Models;
using API_Web_Shop_Electronic_TD.Services;
using API_Web_Shop_Electronic_TD.Services.Momo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

[ApiController]
[Route("api/[controller]")]
public class CheckoutController : ControllerBase
{
	private readonly ICheckOutRepository _checkoutRepository;
	private readonly ILogger<CheckoutController> _logger;
	private readonly IVnPayService _vnPayService;
	private readonly IMomoService _momoService;
	public CheckoutController(
		ICheckOutRepository checkoutRepository,
		ILogger<CheckoutController> logger,
		IVnPayService vnPayService,
		IMomoService momoService)
	{
		_checkoutRepository = checkoutRepository ?? throw new ArgumentNullException(nameof(checkoutRepository));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		_vnPayService = vnPayService ?? throw new ArgumentNullException(nameof(vnPayService));
		_momoService = momoService ?? throw new ArgumentNullException(nameof(momoService));
	}

	//[Authorize]
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
			//var result = await _checkoutRepository.InitiateCheckoutAsync(request);
		
			if (request.PayMethod == "vnpay")
			{
				var paymentModel = new PaymentInformationModel
				{
					FullName = request.FullName,
					Amount = request.Amount * 100,
					Description = "Thanh toán qua VNPay",
					OrderType = request.PayMethod,
				};

				var payUrl = _vnPayService.CreatePaymentUrl(paymentModel, HttpContext);
				return Ok(new { PayUrl = payUrl });
			}

			return RedirectToAction("VnPayCallback");
		}
		catch (ValidationException ex)
		{
			_logger.LogWarning(ex, "Validation error during checkout: {Message}", ex.Message);
			return BadRequest(new { Message = ex.Message });
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error processing checkout");
			return StatusCode(500, new { Message = "An error occurred while processing your order" });
		}
	}

	[HttpGet("vnpay/callback")]
	public async Task<IActionResult> VnPayCallback([FromBody] CheckOutMD request)
	{
		try
		{
			var paymentResponse = _vnPayService.PaymentExecute(Request.Query);
			if (!paymentResponse.Success)
			{
				_logger.LogWarning("VNPay payment validation failed: {OrderId}", paymentResponse.OrderId);
				return Ok( "ok" );
			}

			await _checkoutRepository.ProcessVnPayPaymentAsync(request);

			return NotFound("error");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error processing VNPay callback");
			return NotFound("error");
		}
	}
	[HttpPost("Momo/url")]
	public async Task<IActionResult> CreatePaymentUrl([FromBody] OrderInfoModel model)
	{
		var response = await _momoService.CreatePaymentAsync(model);
		if(response == null)
		{
			return NotFound("error");	
		}
		return Redirect(response.PayUrl);
	}


	[HttpGet]
	public IActionResult PaymentCallBack()
	{
		var response = _momoService.PaymentExecuteAsync(HttpContext.Request.Query);
		return Ok(response);
	}
}