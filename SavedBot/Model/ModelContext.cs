using SavedBot.Exceptions;
using SavedBot.Loggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Telegram.Bot.Types;
using SavedBot.Data;
using SavedBot.Model;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.EntityFrameworkCore;

namespace SavedBot.Model
{
    public class ModelContext : IModelContext
    {
        public readonly AppDbContext _dbContext;
        public ModelContext(AppDbContext telegramContext)
        {
            _dbContext = telegramContext;
        }
       
#pragma warning disable IDE0052 // Remove unread private members
        private readonly ILogger _logger;
#pragma warning restore IDE0052 // Remove unread private members

        public async Task AddItem(SavedItem item)
        {
            if (item is SavedFile file)
            {
                await _dbContext.SavedFiles.AddAsync(file);
            }
            else if (item is SavedText text)
            {
                await _dbContext.SavedTexts.AddAsync(text);
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task AddUser(TelegramUser user)
        {
            await _dbContext.TelegramUsers.AddAsync(user);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<TelegramUser> GetUser(long userId)
        {
            return await _dbContext.TelegramUsers.FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<IQueryable<SavedItem>> Search(TelegramUser user, string partial, int limit)
        {
            var savedFiles = _dbContext.SavedFiles
                .Where(f => f.User == user)
                .Where(f => EF.Functions.Like(f.FileName, $"%{partial}%"))
                .OfType<SavedItem>()
                .Take(limit);

            var savedTexts = _dbContext.SavedTexts
                .Where(t => t.User == user)
                .Where(t => EF.Functions.Like(t.Text, $"%{partial}%"))
                .OfType<SavedItem>()
                .Take(limit);


            return savedFiles.Union(savedTexts).AsQueryable();
        }

    }
}
