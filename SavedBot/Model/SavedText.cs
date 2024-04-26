using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SavedBot.Model
{
    [Table("SavedTexts")]
    public class SavedText : SavedItem
    {
        public string Text { get; set; }
    }
}
