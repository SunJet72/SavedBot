using SavedBot.Model;

namespace SavedBot.Chat.Add
{
    internal class OngoingAddFileChat(long chatId, SavedFile file) : OngoingChat(chatId)
    {
        public SavedFile File { get; private set; } = file;
    }
}
