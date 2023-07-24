using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Threading.Tasks;
using SavedBot.DbModels;

namespace SavedBot.Database
{
    public partial class TelegramContext : DbContext
    {
        public TelegramContext()
        {
            Database.EnsureCreated();
        }
        public TelegramContext(DbContextOptions<TelegramContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=TelegramDB;Integrated Security=True;Encrypt=False;");

        public DbSet<DbUser> Users { get; set; }
        public DbSet<SavedMessages> SavedMessages { get; set; }
    }
}
