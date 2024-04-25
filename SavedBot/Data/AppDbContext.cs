using Microsoft.EntityFrameworkCore;
using SavedBot.Model;

namespace SavedBot.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<TelegramUser> TelegramUsers { get; set; }
        public DbSet<SavedItem> SavedItems { get; set; }

        private string connString;

        public AppDbContext(DbContextOptions<AppDbContext> options, string _connString) : base(options) 
        { 
            connString = _connString;
            Database.EnsureDeleted();
            Database.EnsureCreated();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSqlServer(connString);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}
