using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;

namespace SavedBot.Model
{
    public class SavedFile : SavedItem
    {
        public long FileId { get; set; }

        public string FileName { get; set; }
        public MessageType FileType { get; set; }
        public SavedFile(string name, long id, MessageType fileType)
        {
            FileName = name;
            FileId = id;
            FileType = fileType;
        }
    }
}
