using Google.Cloud.Dialogflow.V2;

namespace API_Web_Shop_Electronic_TD.Services.ChatBot
{
	public interface IDialogflowService
	{
		Task<DetectIntentResponse> DetectIntentAsync(string sessionId, string text, string languageCode = "en");
		string GetResponseText(DetectIntentResponse response);
	}
}