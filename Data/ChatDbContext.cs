using Microsoft.EntityFrameworkCore;

namespace RathodWebScraping.Data
{
    public class ChatSession
    {
        public int Id { get; set; }
        public string Title { get; set; } = "New Chat";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public List<ChatMessageRecord> Messages { get; set; } = new();
    }

    public class ChatMessageRecord
    {
        public int Id { get; set; }
        public int ChatSessionId { get; set; }
        public string Role { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
    public class ChatDbContext : DbContext
    {
        public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options) { }
        public DbSet<ChatSession> Sessions => Set<ChatSession>();
        public DbSet<ChatMessageRecord> Messages => Set<ChatMessageRecord>();
    }
}