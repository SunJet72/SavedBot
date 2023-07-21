using SavedBot.Model;

namespace SavedBot.Chat
{
    public class OngoingAddFileChat : OngoingChat
    {
        public SavedFile File { get; private set; }
        public OngoingAddFileChat(long chatId, SavedFile file) : base(chatId)
        {
            this.File = file;
        }
    }
}
