using Telegram.Bot.Types;

namespace SavedBot.Chat.Add.Text
{
    internal class AddTextOngoingChat(long userId, string text, int messageId) : OngoingChat(userId)
    {
        public string Text { get; private set; } = text;
        public int MessageId { get; private set; } = messageId;
    }
}
