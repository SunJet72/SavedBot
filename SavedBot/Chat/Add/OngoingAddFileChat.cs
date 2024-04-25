using SavedBot.Model;

namespace SavedBot.Chat.Add
{
    internal class OngoingAddFileChat(long userId, SavedFile file) : OngoingChat(userId)
    {
        public SavedFile File { get; private set; } = file;
    }
}
