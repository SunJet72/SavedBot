namespace SavedBot.Chat
{
    public abstract class OngoingChat
    {
        public long ChatId { get; private set; }
        public OngoingChat(long chatId)
        {
            ChatId = chatId;
        }
    }
}
