using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SavedBot.Model;

namespace SavedBot.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<TelegramUser> TelegramUsers { get; set; }
        public DbSet<SavedItem> SavedItems { get; set; }

        private string connString;
        public DbSet<SavedFile> SavedFiles { get; set; }

        public DbSet<SavedText> SavedTexts { get; set; }

        public AppDbContext(string _connString)
        { 
            connString = _connString;
            Database.EnsureDeleted();
            Database.EnsureCreated();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(connString);
            optionsBuilder.UseLoggerFactory(LoggerFactory.Create(builder =>
            {
                builder.AddSimpleConsole();    // указываем наш провайдер логгирования
            }));
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}
