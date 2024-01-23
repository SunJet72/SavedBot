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
    public class MockModelContext : IModelContext
    {
        private readonly Dictionary<long, Dictionary<string, SavedFile>> _files;
        private readonly Dictionary<long, Dictionary<string, string>> _textes;
        private readonly List<User> _users;
#pragma warning disable IDE0052 // Remove unread private members
        private readonly ILogger _logger;
#pragma warning restore IDE0052 // Remove unread private members
        public MockModelContext(ILogger logger)
        {
            _files = new Dictionary<long, Dictionary<string, SavedFile>>();
            _textes = new Dictionary<long, Dictionary<string, string>>();
            _users = new List<User>();
            _logger = logger;
        }
        private bool CheckNameAvailability(long chatId, string name)
        {
            if (_textes.TryGetValue(chatId, out Dictionary<string, string>? savedText))
                if (savedText.ContainsKey(name)) return false;

            if (_files.TryGetValue(chatId, out Dictionary<string, SavedFile>? savedFile))
                if (savedFile.ContainsKey(name)) return false;

            return true;
        }
        public void AddFile(long chatId, string name, SavedFile file)
        {
            if (!CheckNameAvailability(chatId, name)) throw new NameAlreadyExistsException(name);

            if (_files.TryGetValue(chatId, out Dictionary<string, SavedFile>? saved))
                saved.Add(name, file);
            else
                _files.Add(chatId, new Dictionary<string, SavedFile>() { { name, file } });
        }

        public void AddText(long chatId, string name, string text)
        {
            if (!CheckNameAvailability(chatId, name)) throw new NameAlreadyExistsException(name);

            if (_textes.TryGetValue(chatId, out Dictionary<string, string>? saved))
                saved.Add(name, text);
            else
                _textes.Add(chatId, new Dictionary<string, string>() { { name, text } });
        }

        public SavedFile GetFile(long chatId, string name)
        {

            if (_files.TryGetValue(chatId, out Dictionary<string, SavedFile>? saved))
            {
                if (saved.TryGetValue(name, out SavedFile? file))
                    return file;
            }

            throw new SavedMessageNotFoundException(name);
        }
        public string GetText(long chatId, string name)
        {

            if (_textes.TryGetValue(chatId, out Dictionary<string, string>? saved))
            {
                if (saved.TryGetValue(name, out string? text))
                    return text;
            }

            throw new SavedMessageNotFoundException(name);
        }

        public IEnumerable<string> Search(long chatId, string partial, int limit)
        {
            IEnumerable<string> result = Array.Empty<string>();

            if (_files.TryGetValue(chatId, out Dictionary<string, SavedFile>? savedFiles))
            {
                result = savedFiles.Keys.Where((key) => key.Contains(partial,StringComparison.CurrentCultureIgnoreCase));
                if(result.Count() >= limit)
                    return result.Take(limit);
            }

            if (_textes.TryGetValue(chatId, out Dictionary<string, string>? savedTextes)) 
                result = result.Concat(savedTextes.Keys.Where((key) => key.Contains(partial, StringComparison.CurrentCultureIgnoreCase)));

            if(result.Count() > limit) result = result.Take(limit);

            return result;
        }

        public void AddUser(User user)
        {
            if(!_users.Contains(user))
                _users.Add(user);
        }

        public User? GetUserByChatId(long chatId)
        {
            return _users.Find((u)  => u.ChatId == chatId);
        }

        public User? GetUserById(long userId)
        {
            return _users.Find((u) => u.Id == userId);
        }
    }
}
