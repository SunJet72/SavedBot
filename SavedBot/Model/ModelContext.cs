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

        public async  Task AddText(string text)
        {

        }
        public async Task AddFile(SavedFile file)
        {
            await _dbContext.SavedItems.AddAsync(file);
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
            IQueryable<SavedItem> savedFilesQuery = _dbContext.SavedItems
                                                    .OfType<SavedFile>()
                                                    .Include(sf => sf.User)
                                                    .Where(sf => sf.User.Id == user.Id && sf.FileName.Contains(partial))
                                                    .Take(limit);

            List<SavedItem> savedFiles = await savedFilesQuery.ToListAsync();

            return savedFiles.AsQueryable();
        }

    }
}
