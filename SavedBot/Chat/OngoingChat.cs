namespace SavedBot.Chat
{
    internal abstract class OngoingChat(long chatId)
    {
        public long ChatId { get; private set; } = chatId;
    }
}
