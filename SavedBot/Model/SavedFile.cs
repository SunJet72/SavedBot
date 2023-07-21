using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;

namespace SavedBot.Model
{
    public class SavedFile
    {
        public string Id { get; set; }
        //TODO: Our own FileType enum
        public MessageType FileType { get; set; }
        public SavedFile(/*string name,*/ string id, MessageType fileType)// : base(name)
        {
            Id = id;
            FileType = fileType;
        }
    }
}
