using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SavedBot.Model
{
    public class User : IComparable<User>
    {
        public long Id { get; private set; }
        public long ChatId { get; private set; }

        public User(long id, long chatId)
        {
            Id = id;
            ChatId = chatId;
        }

        public int CompareTo(User? other)
        {
            if (other is null) return 1;
            return Id.CompareTo(other.Id);
        }
    }
}
