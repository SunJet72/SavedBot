using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SavedBot.Model
{
    public class SavedItem
    {
        public SavedItem() { }
        public SavedItem(TelegramUser user) 
        {
            User = user;
        }
        public int Id { get; set; }
        public TelegramUser User { get; set; }
    }
}


