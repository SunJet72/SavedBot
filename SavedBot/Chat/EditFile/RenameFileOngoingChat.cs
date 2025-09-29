using Telegram.Bot.Types;

namespace SavedBot.Chat.Edit
{
    internal class RenameFileOngoingChat(long userId, string name, int messageId) : OngoingChat(userId)
    {
        public string Name { get; private set; } = name;
        public int MessageId { get; private set; } = messageId;
    }
}