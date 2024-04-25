using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;

namespace SavedBot.Model
{
    internal class SavedFile(/*string name,*/ string id, MessageType fileType)
    {
        public string Id { get; set; } = id;
        //TODO: Our own FileType enum
        public MessageType FileType { get; set; } = fileType;
    }
}
