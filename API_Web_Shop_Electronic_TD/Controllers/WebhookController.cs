using Microsoft.AspNetCore.Mvc;
using API_Web_Shop_Electronic_TD.Services.ChatBot;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace API_Web_Shop_Electronic_TD.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ChatController : ControllerBase
	{
		private readonly DialogflowService _dialogflowService;
		private readonly OpenAIService _openAiService;

		public ChatController(DialogflowService dialogflowService, OpenAIService openAiService)
		{
			_dialogflowService = dialogflowService;
			_openAiService = openAiService;
		}

		[HttpPost]
		public async Task<IActionResult> Webhook([FromBody] DialogflowWebhookRequest request)
		{
			try
			{
				// Log request để debug
				Console.WriteLine($"Webhook request: {JsonSerializer.Serialize(request)}");

				// Lấy nội dung người dùng nhập
				string userMessage = request.QueryResult.QueryText;
				Console.WriteLine($"User message: {userMessage}");

				// Xác định intent và phản hồi từ Dialogflow
				var intentName = request.QueryResult.Intent.DisplayName;
				var dialogflowResponse = request.QueryResult.FulfillmentText;

				// Logic xử lý: Nếu không có phản hồi từ Dialogflow hoặc là Fallback, dùng OpenAI
				string responseText = string.IsNullOrEmpty(dialogflowResponse) || intentName == "Default Fallback Intent"
					? await _openAiService.GetChatResponse(userMessage)
					: dialogflowResponse;

				// Tạo phản hồi webhook cho Dialogflow
				var webhookResponse = new DialogflowWebhookResponse
				{
					FulfillmentText = responseText
				};

				Console.WriteLine($"Webhook response: {responseText}");
				return Ok(webhookResponse);
			}
			catch (HttpRequestException ex) when (ex.Message.Contains("429"))
			{
				Console.WriteLine("Rate limit exceeded for OpenAI.");
				return StatusCode(429, new DialogflowWebhookResponse
				{
					FulfillmentText = "Sorry, I'm too busy right now. Please try again later."
				});
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error: {ex.Message}");
				return StatusCode(500, new DialogflowWebhookResponse
				{
					FulfillmentText = "Something went wrong. Please try again."
				});
			}
		}
	}

	// Model cho Webhook Request (đơn giản hóa)
	public class DialogflowWebhookRequest
	{
		public QueryResultData QueryResult { get; set; }
	}

	public class QueryResultData
	{
		public string QueryText { get; set; }
		public IntentData Intent { get; set; }
		public string FulfillmentText { get; set; }
	}

	public class IntentData
	{
		public string DisplayName { get; set; }
	}

	// Model cho Webhook Response
	public class DialogflowWebhookResponse
	{
		[JsonPropertyName("fulfillmentText")]
		public string FulfillmentText { get; set; }
	}
}