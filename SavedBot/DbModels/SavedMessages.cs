using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SavedBot.DbModels
{
    public class SavedMessages
    {
        public int id { get; set; }
        public string name { get; set; }
        public long chatId { get; set; }
        public SavedMessageType messageType { get; set; }

        public enum SavedMessageType
        {
            Text,
            Audio,
            Video,
            Photo, 
            Animation,
            Voice,
            VideoNode,
            Document,
            Sticker,

        }
    }
}
