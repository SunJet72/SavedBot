using SavedBot.Exceptions;
using SavedBot.Loggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Telegram.Bot.Types;
using static System.Net.Mime.MediaTypeNames;

namespace SavedBot.Model
{
    public class MockModelContext : ModelContext
    {
        private Dictionary<long, Dictionary<string, SavedFile>> _files;
        private Dictionary<long, Dictionary<string, string>> _textes;
        private List<User> _users;
        private ILogger _logger;
        public MockModelContext(ILogger logger)
        {
            _files = new Dictionary<long, Dictionary<string, SavedFile>>();
            _textes = new Dictionary<long, Dictionary<string, string>>();
            _users = new List<User>();
            _logger = logger;
        }
        private bool CheckNameAvailability(long chatId, string name)
        {
            Dictionary<string, string> savedText;

            if (_textes.TryGetValue(chatId, out savedText))
                if (savedText.ContainsKey(name)) return false;

            Dictionary<string, SavedFile> savedFile;

            if (_files.TryGetValue(chatId, out savedFile))
                if (savedFile.ContainsKey(name)) return false;

            return true;
        }
        public override void AddFile(long chatId, string name, SavedFile file)
        {
            if (!CheckNameAvailability(chatId, name)) throw new NameAlreadyExistsException(name);

            Dictionary<string, SavedFile> saved;
            if (_files.TryGetValue(chatId, out saved))
                saved.Add(name, file);
            else
                _files.Add(chatId, new Dictionary<string, SavedFile>() { { name, file } });
        }

        public override void AddText(long chatId, string name, string text)
        {
            if (!CheckNameAvailability(chatId, name)) throw new NameAlreadyExistsException(name);

            Dictionary<string, string> saved;
            if (_textes.TryGetValue(chatId, out saved))
                saved.Add(name, text);
            else
                _textes.Add(chatId, new Dictionary<string, string>() { { name, text } });
        }

        public override SavedFile FindFile(long chatId, string name)
        {
            Dictionary<string, SavedFile> saved;

            if (_files.TryGetValue(chatId, out saved))
            {
                SavedFile file;
                if(saved.TryGetValue(name, out file))
                    return file;  
            }

            throw new SavedMessageNotFoundException(name);
        }
        public override string FindText(long chatId, string name)
        {
            Dictionary<string, string> saved;

            if (_textes.TryGetValue(chatId, out saved))
            {
                string text;
                if (saved.TryGetValue(name, out text))
                    return text;
            }

            throw new SavedMessageNotFoundException(name);
        }

        public override IEnumerable<string> Search(long chatId, string partial, int limit)
        {
            Dictionary<string, SavedFile> savedFiles;
            Dictionary<string, string> savedTextes;

            IEnumerable<string> result = null; 

            if (_files.TryGetValue(chatId, out savedFiles))
            {
                result = savedFiles.Keys.Where((key) => key.Contains(partial));
                if(result.Count() >= limit)
                    return result.Take(limit);
            }

            if (_textes.TryGetValue(chatId, out savedTextes)) 
            {
                if (result is null) result = savedTextes.Keys.Where((key) => key.Contains(partial));
                else result.Concat(savedTextes.Keys.Where((key) => key.Contains(partial)));
            }

            if(result.Count() > limit) result = result.Take(limit);

            return result;
        }

        public override void AddUser(User user)
        {
            if(!_users.Contains(user))
                _users.Add(user);
        }

        public override User? GetUserByChatId(long chatId)
        {
            return _users.Find((u)  => u.ChatId == chatId);
        }

        public override User? GetUserById(long userId)
        {
            return _users.Find((u) => u.Id == userId);
        }
    }
}
