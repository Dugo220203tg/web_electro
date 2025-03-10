using System.Text.Json.Serialization;

namespace API_Web_Shop_Electronic_TD.Models
{
	public class ChatBotMD
	{
	}
	//public class OpenAISettings
	//{
	//	public string ApiKey { get; set; }
	//	public string Model { get; set; } = "gpt-3.5-turbo";
	//}
	public class OpenAISettings
	{
		public string ApiKey { get; set; }
		public string OrganizationId { get; set; }
	}
	public class ChatCompletionRequest
	{
		[JsonPropertyName("model")]
		public string Model { get; set; }
		[JsonPropertyName("messages")]
		public List<Message> Messages { get; set; }
		[JsonPropertyName("max_tokens")]
		public int MaxTokens { get; set; }

	}
	public class WebhookRequest
	{
		public string SessionId { get; set; }
		public string QueryText { get; set; }
	}
	public class WebhookResponse
	{
		public string FulfillmentText { get; set; }
	}

	public class ChatRequest
	{
		public string SessionId { get; set; }
		public string Message { get; set; }
		public string LanguageCode { get; set; } = "en";
	}

	public class ChatResponse
	{
		public string Message { get; set; }
	}
	public class OpenAIResponse
	{
		public List<Choice> choices { get; set; }
	}

	public class Choice
	{
		public Message message { get; set; }
	}

	public class Message
	{
		public string content { get; set; }
	}


	public class ChatCompletionResponse
	{
		[JsonPropertyName("id")]
		public string Id { get; set; }

		[JsonPropertyName("object")]
		public string Object { get; set; }

		[JsonPropertyName("created")]
		public int Created { get; set; }

		[JsonPropertyName("model")]
		public string Model { get; set; }

		[JsonPropertyName("choices")]
		public List<Choice> Choices { get; set; }

		[JsonPropertyName("usage")]
		public Usage Usage { get; set; }

		[JsonPropertyName("system_fingerprint")]
		public object SystemFingerprint { get; set; }
	}

	public class Usage
	{
		[JsonPropertyName("prompt_tokens")]
		public int PromptTokens { get; set; }

		[JsonPropertyName("completion_tokens")]
		public int CompletionTokens { get; set; }

		[JsonPropertyName("total_tokens")]
		public int TotalTokens { get; set; }
	}
	public class DialogflowSettings
	{
		public string ProjectId { get; set; }
		public string CredentialsPath { get; set; }
	}
}
