using System.ComponentModel.DataAnnotations.Schema;
using Telegram.Bot.Types.Enums;

namespace SavedBot.Model
{
    [Table("SavedFiles")]
    public class SavedFile : SavedItem
    {
        public string FileId { get; set; }
        public string FileUniqueId { get; set; }

        public string FileName { get; set; }
        public MessageType FileType { get; set; }

        public SavedFile() { }
    }
}
