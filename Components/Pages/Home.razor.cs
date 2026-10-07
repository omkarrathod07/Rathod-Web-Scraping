using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.AI;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RathodWebScraping.Components.Pages
{
    public partial class Home : ComponentBase
    {
        [Inject]
        private IChatClient ChatClient { get; set; } = default!;

        protected readonly List<ChatMessage> chatHistory = new();
        protected readonly List<DisplayMessage> displayMessages = new();
        protected string userInput = string.Empty;
        protected bool isTyping = false;
        protected bool isDarkMode = false;

        protected override void OnInitialized()
        {
            chatHistory.Add(new ChatMessage(
                ChatRole.System,
                "You are a helpful assistant. Wrap code responses in standard markdown codeblocks."
            ));
        }
        protected void ToggleTheme()
        {
            isDarkMode = !isDarkMode;
        }
        protected async Task SendMessage()
        {
            if (string.IsNullOrWhiteSpace(userInput) || isTyping)
                return;

            string userMessageText = userInput;
            userInput = string.Empty;
            isTyping = true;

            chatHistory.Add(new ChatMessage(ChatRole.User, userMessageText));
            displayMessages.Add(new DisplayMessage { Role = "User", Content = userMessageText });

            var assistantMsg = new DisplayMessage { Role = "AI", Content = "" };
            displayMessages.Add(assistantMsg);

            try
            {
                var responseUpdates = ChatClient.GetStreamingResponseAsync(chatHistory);

                await foreach (var update in responseUpdates)
                {
                    if (!string.IsNullOrEmpty(update.Text))
                    {
                        assistantMsg.Content += update.Text;
                        assistantMsg.HtmlContent = Markdig.Markdown.ToHtml(assistantMsg.Content);
                        StateHasChanged();
                    }
                }

                chatHistory.Add(new ChatMessage(ChatRole.Assistant, assistantMsg.Content));
            }
            catch (System.Exception ex)
            {
                assistantMsg.Content = $"Error: {ex.Message}";
                assistantMsg.HtmlContent = assistantMsg.Content;
            }
            finally
            {
                isTyping = false;
            }
        }
    }
    public class DisplayMessage
    {
        public string Role { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string HtmlContent { get; set; } = string.Empty;
    }
}