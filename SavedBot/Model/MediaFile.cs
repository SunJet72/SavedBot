using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SavedBot.Model
{
    public class MediaFile
    {
        public int Id { get; set; }
        public long UserId { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public string FileId { get; set; }

        public virtual TelegramUser User { get; set; }
    }
}


