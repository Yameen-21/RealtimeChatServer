using Google.Apis.Auth.OAuth2;
using System.Text;
using System.Text.Json;

namespace RealtimeChatServer.Services
{
    public class DialogflowService
    {
        // Reuse HttpClient (best practice)
        private static readonly HttpClient _httpClient = new HttpClient();

        // Dialogflow configuration
        private const string ProjectId = "chatbottest-hocg";
        private const string SessionId = "123456";

        public async Task<string> GetResponseAsync(string userMessage)
        {
            try
            {
                // Load Google Service Account credentials
                var credential = GoogleCredential
                    .FromFile("dialogflow.json")
                    .CreateScoped("https://www.googleapis.com/auth/cloud-platform");

                // Generate access token dynamically
                var accessToken = await credential.UnderlyingCredential
                    .GetAccessTokenForRequestAsync();

                // Dialogflow detectIntent endpoint
                var url = $"https://dialogflow.googleapis.com/v2/projects/{ProjectId}/agent/sessions/{SessionId}:detectIntent";

                // Request payload sent to Dialogflow
                var requestBody = new
                {
                    queryInput = new
                    {
                        text = new
                        {
                            text = userMessage,
                            languageCode = "en"
                        }
                    }
                };

                var request = new HttpRequestMessage(HttpMethod.Post, url);

                // Attach authorization token
                request.Headers.Add("Authorization", $"Bearer {accessToken}");

                // Attach JSON body
                request.Content = new StringContent(
                    JsonSerializer.Serialize(requestBody),
                    Encoding.UTF8,
                    "application/json"
                );

                // Send request to Dialogflow
                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                    return "Dialogflow API error.";

                var responseJson = await response.Content.ReadAsStringAsync();

                // Extract bot reply from JSON response
                using var document = JsonDocument.Parse(responseJson);

                return document
                    .RootElement
                    .GetProperty("queryResult")
                    .GetProperty("fulfillmentText")
                    .GetString() ?? "No response from bot.";
            }
            catch (Exception)
            {
                // Fallback response on failure
                return "Dialogflow unavailable.";
            }
        }
    }
}
