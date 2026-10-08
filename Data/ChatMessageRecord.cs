namespace RathodWebScraping.Data
{
    public class ChatMessageRecord
    {
        public int Id { get; set; }
        public int ChatSessionId { get; set; }
        public string Role { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}