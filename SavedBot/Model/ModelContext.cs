using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SavedBot.Model
{
    public abstract class ModelContext : IModelContext
    {
        public abstract void AddFile(long chatId, string name, SavedFile file);
        public abstract void AddText(long chatId, string name, string text);
        public abstract void AddUser(User user);
        public abstract SavedFile FindFile(long chatId, string name);
        public abstract string FindText(long chatId, string name);
        public abstract User? GetUserByChatId(long chatId);
        public abstract User? GetUserById(long userId);
        public abstract IEnumerable<string> Search(long chatId, string partial, int limit);
    }
}
