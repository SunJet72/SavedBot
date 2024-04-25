using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using SavedBot.Model;

namespace SavedBot.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<TelegramUser> TelegramUsers { get; set; }
        public DbSet<SavedItem> SavedItems { get; set; }

        public DbSet<SavedFile> SavedFiles { get; set; }

        public DbSet<SavedText> SavedTexts { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) 
        { 
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=TelegramDB;Integrated Security=True;Encrypt=False;");
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SavedItem>()
                .HasOne(mf => mf.User)
                .WithMany()
                .HasForeignKey(mf => mf.Id);
        }
    }
}
