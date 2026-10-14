using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.JSInterop;
using RathodWebScraping.Data;
using RathodWebScraping.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RathodWebScraping.Components.Pages
{
    public partial class Home : ComponentBase
    {
        [Inject] private IChatClient ChatClient { get; set; } = default!;
        [Inject] private IDbContextFactory<ChatDbContext> DbFactory { get; set; } = default!;
        [Inject] private VoiceSpeechService VoiceService { get; set; } = default!;
        [Inject] private IJSRuntime JSRuntime { get; set; } = default!;

        protected List<ChatSession> sessions = new();
        protected List<DisplayMessage> displayMessages = new();

        protected ChatSession? activeSession;
        protected string userInput = string.Empty;
        protected bool isTyping = false;
        protected bool isListeningToVoice = false;
        protected string? voiceErrorMessage = null;
        private string currentUserTrackingId = string.Empty;
        private readonly List<ChatMessage> chatHistory = new();
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await InitializeUserSessionContext();
            }
        }
        private async Task InitializeUserSessionContext()
        {
            try
            {
                var storedId = await JSRuntime.InvokeAsync<string?>("localStorage.getItem", "rathod_hub_user_token");

                if (string.IsNullOrWhiteSpace(storedId))
                {
                    storedId = Guid.NewGuid().ToString("N");
                    await JSRuntime.InvokeVoidAsync("localStorage.setItem", "rathod_hub_user_token", storedId);
                }

                currentUserTrackingId = storedId;

                await LoadSessionsFromDatabase();

                if (sessions.Count == 0)
                {
                    await CreateNewSession();
                }
                else
                {
                    await SwitchSession(sessions.First().Id);
                }
            }
            catch (Exception ex)
            {
                voiceErrorMessage = $"Session Routing Initialization Error: {ex.Message}";
            }
            finally
            {
                StateHasChanged();
            }
        }
        protected async Task LoadSessionsFromDatabase()
        {
            if (string.IsNullOrEmpty(currentUserTrackingId)) return;

            using var db = await DbFactory.CreateDbContextAsync();
            sessions = await db.Sessions
                                .Where(s => s.UserId == currentUserTrackingId)
                                .OrderByDescending(s => s.CreatedAt)
                                .ToListAsync();
        }
        protected async Task CreateNewSession()
        {
            if (string.IsNullOrEmpty(currentUserTrackingId)) return;

            using var db = await DbFactory.CreateDbContextAsync();

            var newSession = new ChatSession
            {
                UserId = currentUserTrackingId,
                Title = $"Chat Section {sessions.Count + 1}"
            };

            db.Sessions.Add(newSession);
            await db.SaveChangesAsync();

            await LoadSessionsFromDatabase();
            await SwitchSession(newSession.Id);
        }
        protected async Task SwitchSession(int sessionId)
        {
            using var db = await DbFactory.CreateDbContextAsync();
            activeSession = await db.Sessions
                                     .Include(s => s.Messages)
                                     .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == currentUserTrackingId);

            if (activeSession == null) return;

            chatHistory.Clear();
            displayMessages.Clear();

            chatHistory.Add(new ChatMessage(
                ChatRole.System,
                "You are a helpful assistant. Wrap code responses in standard markdown codeblocks."
            ));

            foreach (var msg in activeSession.Messages.OrderBy(m => m.Timestamp))
            {
                var role = msg.Role == "User" ? ChatRole.User : ChatRole.Assistant;
                chatHistory.Add(new ChatMessage(role, msg.Content));

                displayMessages.Add(new DisplayMessage
                {
                    Role = msg.Role,
                    Content = msg.Content,
                    HtmlContent = msg.Role == "AI" ? Markdig.Markdown.ToHtml(msg.Content) : string.Empty
                });
            }

            StateHasChanged();
        }
        protected async Task SendMessage()
        {
            if (string.IsNullOrWhiteSpace(userInput) || isTyping || activeSession == null)
                return;

            string userMessageText = userInput;
            userInput = string.Empty;
            isTyping = true;

            displayMessages.Add(new DisplayMessage { Role = "User", Content = userMessageText });
            chatHistory.Add(new ChatMessage(ChatRole.User, userMessageText));

            using (var db = await DbFactory.CreateDbContextAsync())
            {
                db.Messages.Add(new ChatMessageRecord
                {
                    ChatSessionId = activeSession.Id,
                    Role = "User",
                    Content = userMessageText
                });

                var sessionToUpdate = await db.Sessions.FirstOrDefaultAsync(s => s.Id == activeSession.Id && s.UserId == currentUserTrackingId);
                if (sessionToUpdate != null && sessionToUpdate.Title.StartsWith("Chat Section"))
                {
                    sessionToUpdate.Title = userMessageText.Length > 20 ? userMessageText[..20] + "..." : userMessageText;
                    activeSession.Title = sessionToUpdate.Title;
                }
                await db.SaveChangesAsync();
            }

            var assistantMsg = new DisplayMessage { Role = "AI", Content = "" };
            displayMessages.Add(assistantMsg);
            StateHasChanged();

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

                using (var db = await DbFactory.CreateDbContextAsync())
                {
                    db.Messages.Add(new ChatMessageRecord
                    {
                        ChatSessionId = activeSession.Id,
                        Role = "AI",
                        Content = assistantMsg.Content
                    });
                    await db.SaveChangesAsync();
                }
                await VoiceService.PlayTextToSpeechAsync(assistantMsg.Content);

                await LoadSessionsFromDatabase();
            }
            catch (System.Exception ex)
            {
                assistantMsg.Content = $"Error: {ex.Message}";
                assistantMsg.HtmlContent = assistantMsg.Content;
            }
            finally
            {
                isTyping = false;
                StateHasChanged();
            }
        }
        protected async Task StartVoiceCapture()
        {
            voiceErrorMessage = null;
            isListeningToVoice = true;
            StateHasChanged();

            await VoiceService.StartSpeechToTextAsync(
                async (transcript) =>
                {
                    isListeningToVoice = false;
                    if (!string.IsNullOrWhiteSpace(transcript))
                    {
                        userInput = transcript;
                        await InvokeAsync(StateHasChanged);
                        await InvokeAsync(SendMessage);
                    }
                    else
                    {
                        await InvokeAsync(StateHasChanged);
                    }
                },
                async (error) =>
                {
                    isListeningToVoice = false;
                    voiceErrorMessage = error switch
                    {
                        "not-allowed" => "Microphone access denied. Check your browser browser permission configuration settings.",
                        "no-speech" => "No speech was detected. Please try tapping the button and speaking again.",
                        "network" => "Network connection error interrupted transcription engine sync.",
                        _ => $"Voice Service Error: {error}"
                    };
                    await InvokeAsync(StateHasChanged);
                }
            );
        }
        protected async Task StopVoiceCapture()
        {
            isListeningToVoice = false;
            await VoiceService.StopSpeechToTextAsync();
            StateHasChanged();
        }
        protected async Task DeleteSessionAsync(int sessionId)
        {
            if (string.IsNullOrEmpty(currentUserTrackingId)) return;

            using (var db = await DbFactory.CreateDbContextAsync())
            {
                var targetSession = await db.Sessions
                                            .Include(s => s.Messages)
                                            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == currentUserTrackingId);

                if (targetSession != null)
                {
                    db.Messages.RemoveRange(targetSession.Messages);
                    db.Sessions.Remove(targetSession);
                    await db.SaveChangesAsync();
                }
            }
            await LoadSessionsFromDatabase();
            if (activeSession?.Id == sessionId)
            {
                if (sessions.Count == 0)
                {
                    activeSession = null;
                    chatHistory.Clear();
                    displayMessages.Clear();
                    await CreateNewSession();
                }
                else
                {
                    await SwitchSession(sessions.First().Id);
                }
            }
            else
            {
                StateHasChanged();
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