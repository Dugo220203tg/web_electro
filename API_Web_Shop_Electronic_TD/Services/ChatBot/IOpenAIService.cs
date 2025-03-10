namespace API_Web_Shop_Electronic_TD.Services.ChatBot
{
	public interface IOpenAIService
	{
		Task<string> GetChatCompletionAsync(string question);

	}
}
