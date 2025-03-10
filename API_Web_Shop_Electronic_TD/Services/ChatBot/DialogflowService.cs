using Google.Apis.Auth.OAuth2;
using Google.Cloud.Dialogflow.V2;
using Microsoft.Extensions.Configuration;

namespace API_Web_Shop_Electronic_TD.Services.ChatBot
{
	public class DialogflowService
	{
		private readonly SessionsClient _sessionsClient;
		private readonly string _projectId;
		private readonly string _sessionId;

		public DialogflowService(IConfiguration configuration)
		{
			_projectId = configuration["Dialogflow:ProjectId"];
			var credentialsPath = configuration["Dialogflow:JsonCredentialsPath"];
			var credential = GoogleCredential.FromFile(credentialsPath)
				.CreateScoped(SessionsClient.DefaultScopes);

			_sessionsClient = new SessionsClientBuilder
			{
				Credential = credential
			}.Build();

			_sessionId = Guid.NewGuid().ToString();
		}

		public async Task<string> DetectIntent(string text)
		{
			var session = SessionName.FromProjectSession(_projectId, _sessionId);
			var queryInput = new QueryInput
			{
				Text = new TextInput
				{
					Text = text,
					LanguageCode = "en-US"
				}
			};

			var response = await _sessionsClient.DetectIntentAsync(session, queryInput);
			return response.QueryResult.FulfillmentText;
		}

		public string GetIntentName(string text)
		{
			var session = SessionName.FromProjectSession(_projectId, _sessionId);
			var queryInput = new QueryInput
			{
				Text = new TextInput
				{
					Text = text,
					LanguageCode = "en-US"
				}
			};

			var response = _sessionsClient.DetectIntentAsync(session, queryInput).Result;
			return response.QueryResult.Intent.DisplayName;
		}
	}
}