using SavedBot.Exceptions;
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
    public class ModelContext(AppDbContext telegramContext) : IModelContext
    {
        public readonly AppDbContext _dbContext = telegramContext;

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
            _dbContext.TelegramUsers.Add(user);
            _dbContext.SaveChanges();
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
