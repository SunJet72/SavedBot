using Telegram.Bot.Types;

namespace SavedBot.Chat.RemoveText
{
    internal class RemoveTextOngoingChat(long userId, string text, int messageId) : OngoingChat(userId)
    {
        public string Text { get; private set; } = text;
        public int MessageId { get; private set; } = messageId;
    }
}