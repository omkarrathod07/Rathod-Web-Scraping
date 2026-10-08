namespace RathodWebScraping.Data
{
    public class ChatSession
    {
        public int Id { get; set; }
        public string Title { get; set; } = "New Chat";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public List<ChatMessageRecord> Messages { get; set; } = new();
    }
}