using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SavedBot.Model
{
    public class TelegramUser : IComparable<TelegramUser>
    {
        public long Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Username { get; set; }

        public int CompareTo(TelegramUser? other)
        {
            if (other is null) return 1;
            return Id.CompareTo(other.Id);
        }
    }
}
