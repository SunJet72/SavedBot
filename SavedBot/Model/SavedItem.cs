using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SavedBot.Model
{
    public class SavedItem
    {
        public int Id { get; set; }
        public virtual TelegramUser User { get; set; }
    }
}


