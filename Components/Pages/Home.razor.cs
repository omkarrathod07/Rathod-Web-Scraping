using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.AI;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace RathodWebScraping.Components.Pages
{
    public partial class Home
    {
        [Inject]
        private IConfiguration configuration { get; set; } = default!;
        private string userPrompt = "";
        private string buttonText = "Ask";
        private string? responseText;
        private string? apiKey;
        private string? gptModel;

        protected override void OnInitialized()
        {
            apiKey = configuration["OpenAI:ApiKey"];
            gptModel = configuration["OpenAI:GptModel"];
        }
        private async Task SubmitPrompt()
        {
            buttonText = "Sending...";
            string? url = "https://api.openai.com/v1/chat/completions";
            var requestData = new
            {
                models = gptModel,
                messages = new[]
                {
                    new {role="system",content="Yor a helpful assistant."},
                    new {role="user",content=userPrompt}
                },
                temperature = 0.7
            };
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
            requestMessage.Headers.Add("Authorization",$"bearer {apiKey}");
            requestMessage.Content = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json");
            try
            {
                var response = await Http.SendAsync(requestMessage);
            }
            catch(Exception ex)
            {
                responseText = $"Error: {ex.Message}";
            }
            finally
            {
                buttonText = "Ask";
            }
        }
    }
}