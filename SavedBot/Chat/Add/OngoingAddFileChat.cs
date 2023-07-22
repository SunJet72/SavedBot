using SavedBot.Model;

namespace SavedBot.Chat.Add
{
    public class OngoingAddFileChat : OngoingChat
    {
        public SavedFile File { get; private set; }
        public OngoingAddFileChat(long chatId, SavedFile file) : base(chatId)
        {
            File = file;
        }
    }
}
