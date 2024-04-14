using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SavedBot.Model
{
    public class TelegramUser : IComparable<TelegramUser>
    {
        public TelegramUser(long id, long chatId)
        {
            Id = id;
            ChatId = chatId;
        }

        public long Id { get; set; }
        public long? ChatId { get; set; }

        public List<SavedFile>? SavedItems { get; set; }
        
        public string LanguageCode { get; set; } = "en";

        public int CompareTo(TelegramUser? other)
        {
            if (other is null) return 1;
            return Id.CompareTo(other.Id);
        }
    }
}
