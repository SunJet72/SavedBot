using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;

namespace SavedBot.Model
{
    [Table("SavedFiles")]
    public class SavedFile : SavedItem
    {
        public long FileId { get; set; }

        public string FileName { get; set; }
        public MessageType FileType { get; set; }
        public SavedFile(string name, long id, MessageType fileType)
        {
            this.FileName = name;
            this.FileId = id;
            this.FileType = fileType;
        }
    }
}
