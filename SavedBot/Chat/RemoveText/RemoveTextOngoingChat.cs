namespace SavedBot.Chat.RemoveText
{
    internal class RemoveTextOngoingChat(long userId, string text) : OngoingChat(userId)
    {
        public string Text { get; private set; } = text;
    }
}