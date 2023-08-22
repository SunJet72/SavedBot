using SavedBot.Exceptions;
using SavedBot.Loggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Telegram.Bot.Types;
using SavedBot.Database;
using SavedBot.DbModels;
using static System.Net.Mime.MediaTypeNames;

namespace SavedBot.Model
{
    public class ModelContext : IModelContext
    {
        public readonly TelegramContext _telegramContext;
        public ModelContext(TelegramContext telegramContext)
        {
            _telegramContext = telegramContext;
        }
       
#pragma warning disable IDE0052 // Remove unread private members
        private readonly ILogger _logger;
#pragma warning restore IDE0052 // Remove unread private members
    
        private bool CheckNameAvailability(long chatId, string name)
        {
            bool isAvailable = !_telegramContext.SavedMessages.Any(message => message.name == name);
            return isAvailable;
        }
        public  void AddFile(long chatId, string name, SavedFile file)
        {
            
        }

        public void AddText(long chatId, string name, string text)
        {
           
        }

        /* public SavedFile FindFile(long chatId, string name)
        {

           
        }
        public string FindText(long chatId, string name)
        {
   
        }

        public IEnumerable<string> Search(long chatId, string partial, int limit)
        {
            
        } */

        public void AddUser(DbUser user)
        {
            _telegramContext.Users.Add(user);
            _telegramContext.SaveChanges();
        }

      
        public DbUser? GetUserById(long userId)
        {
            return _telegramContext.Users.FirstOrDefault(u => u.id == userId);
            
        }
    }
}
