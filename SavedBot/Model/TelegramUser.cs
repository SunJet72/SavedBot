using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SavedBot.Model
{
    public class TelegramUser : IComparable<TelegramUser>
    {
        public TelegramUser() { }
        public TelegramUser(long id, long chatId, string languageCode)
        {
            Id = id;
            ChatId = chatId;
            LanguageCode = languageCode;
        }
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }
        public long? ChatId { get; set; }

        public IEnumerable<SavedItem>? SavedItems { get; set; }
        
        public string LanguageCode { get; set; } = "en";

        public int CompareTo(TelegramUser? other)
        {
            if (other is null) return 1;
            return Id.CompareTo(other.Id);
        }
    }
}
