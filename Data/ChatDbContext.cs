using Microsoft.EntityFrameworkCore;

namespace RathodWebScraping.Data
{
    public class ChatDbContext : DbContext
    {
        public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options) { }
        public DbSet<ChatSession> Session => Set<ChatSession>();
        public DbSet<ChatMessageRecord> Messages => Set<ChatMessageRecord>();
    }
}