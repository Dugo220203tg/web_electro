using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace API_Web_Shop_Electronic_TD.Services.ChatBot
{
	public class OpenAIService
	{
		private readonly HttpClient _httpClient;
		private readonly string _apiKey;

		public OpenAIService(IConfiguration configuration)
		{
			_httpClient = new HttpClient();
			_apiKey = configuration["OpenAI:ApiKey"];
			_httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);
		}

		public async Task<string> GetChatResponse(string message)
		{
			var requestBody = new
			{
				model = "gpt-3.5-turbo",
				messages = new[] { new { role = "user", content = message } },
				max_tokens = 100,
				temperature = 0.7
			};

			var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
			int maxRetries = 5; // More retries
			int delaySeconds = 2; // Start with shorter delay

			for (int attempt = 0; attempt < maxRetries; attempt++)
			{
				var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);

				if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
				{
					var retryAfter = response.Headers.TryGetValues("Retry-After", out var values)
						? int.Parse(values.FirstOrDefault() ?? "5")
						: delaySeconds;

					Console.WriteLine($"Rate limit hit. Attempt {attempt + 1}/{maxRetries}. Waiting {retryAfter}s...");
					await Task.Delay(retryAfter * 1000);
					delaySeconds = Math.Min(delaySeconds * 2, 60); // Cap at 60s
					continue;
				}

				response.EnsureSuccessStatusCode();
				var jsonResponse = await response.Content.ReadAsStringAsync();
				var result = JsonSerializer.Deserialize<OpenAIResponse>(jsonResponse);
				return result.choices[0].message.content.Trim();
			}

			throw new HttpRequestException("Exceeded retry attempts due to rate limiting (429).");
		}
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
}