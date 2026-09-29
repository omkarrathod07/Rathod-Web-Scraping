using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.AI;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RathodWebScraping.Pages
{
    // Inheriting from ComponentBase satisfies OnInitialized and StateHasChanged
    public partial class Home : ComponentBase
    {
        [Inject]
        private IChatClient ChatClient { get; set; } = default!;

        // Explicitly use the Microsoft Extensions AI ChatMessage structure
        private readonly List<Microsoft.Extensions.AI.ChatMessage> _chatHistory = new();
        private readonly List<DisplayMessage> _displayMessages = new();
        private string _userInput = string.Empty;
        private bool _isTyping = false;

        protected override void OnInitialized()
        {
            _chatHistory.Add(new Microsoft.Extensions.AI.ChatMessage(
                Microsoft.Extensions.AI.ChatRole.System,
                "You are a helpful assistant."
            ));
        }

        private async Task SendMessage()
        {
            if (string.IsNullOrWhiteSpace(_userInput) || _isTyping)
                return;

            string userMessageText = _userInput;
            _userInput = string.Empty;
            _isTyping = true;

            _chatHistory.Add(new Microsoft.Extensions.AI.ChatMessage(Microsoft.Extensions.AI.ChatRole.User, userMessageText));
            _displayMessages.Add(new DisplayMessage { Role = "User", Content = userMessageText });

            var assistantMsg = new DisplayMessage { Role = "AI", Content = "" };
            _displayMessages.Add(assistantMsg);

            try
            {
                // Request the async streaming response pipeline
                var responseUpdates = ChatClient.GetStreamingResponseAsync(_chatHistory);

                // FIX 2: Using 'var' handles the correct ChatResponseUpdate type automatically
                await foreach (var update in responseUpdates)
                {
                    if (!string.IsNullOrEmpty(update.Text))
                    {
                        assistantMsg.Content += update.Text;
                        StateHasChanged(); // Forces Blazor layout re-render for real-time text streaming
                    }
                }

                _chatHistory.Add(new Microsoft.Extensions.AI.ChatMessage(Microsoft.Extensions.AI.ChatRole.Assistant, assistantMsg.Content));
            }
            catch (System.Exception ex)
            {
                assistantMsg.Content = $"Error: {ex.Message}";
            }
            finally
            {
                _isTyping = false;
            }
        }

        private class DisplayMessage
        {
            public string Role { get; set; } = string.Empty;
            public string Content { get; set; } = string.Empty;
        }
    }
}
