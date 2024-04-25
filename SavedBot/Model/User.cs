using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SavedBot.Model
{
    internal class User(long id, long chatId) : IComparable<User>
    {
        public long Id { get; private set; } = id;
        public long ChatId { get; private set; } = chatId;
        public string LanguageCode { get; set; } = "en";

        public int CompareTo(User? other)
        {
            if (other is null) return 1;
            return Id.CompareTo(other.Id);
        }
    }
}
