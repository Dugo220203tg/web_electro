using API_Web_Shop_Electronic_TD.Interfaces;
using API_Web_Shop_Electronic_TD.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

[ApiController]
[Route("api/[controller]")]
public class CheckoutController : ControllerBase
{
	private readonly ICheckOutRepository _checkoutRepository;
	private readonly ILogger<CheckoutController> _logger;

	public CheckoutController(
		ICheckOutRepository checkoutRepository,
		ILogger<CheckoutController> logger)
	{
		_checkoutRepository = checkoutRepository;
		_logger = logger;
	}
	[Authorize]
	[HttpPost]
	public async Task<IActionResult> ProcessCheckout([FromBody] CheckOutMD request)
	{
		try
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(new { Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
			}

			var result = await _checkoutRepository.CheckOutAsync(request);
			return Ok(new {message = "Success"});
		}
		catch (ValidationException ex)
		{
			_logger.LogWarning(ex, "Validation error during checkout");
			return BadRequest(new { Message = ex.Message });
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error processing checkout");
			return StatusCode(500, new { Message = "An error occurred while processing your order" });
		}
	}
}