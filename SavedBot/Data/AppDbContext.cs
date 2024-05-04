using Microsoft.EntityFrameworkCore;
using MySql.EntityFrameworkCore.Extensions;
using Microsoft.Extensions.Logging;
using SavedBot.Configuration;
using SavedBot.Model;

namespace SavedBot.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<TelegramUser> TelegramUsers { get; set; }
        public DbSet<SavedItem> SavedItems { get; set; }

        private string dbProvider, connString;
        public DbSet<SavedFile> SavedFiles { get; set; }

        public DbSet<SavedText> SavedTexts { get; set; }

        public AppDbContext(string _dbProvider,string _connString)
        { 
            dbProvider = _dbProvider;
            connString = _connString;
            Database.EnsureDeleted();
            Database.EnsureCreated();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            switch(dbProvider)
            {  
                case UserSecretValue.MySQL: optionsBuilder.UseMySQL(connString);
                break;
                case UserSecretValue.SqlServer: optionsBuilder.UseSqlServer(connString);
                break;
                default: throw new NotImplementedException();
            }
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
