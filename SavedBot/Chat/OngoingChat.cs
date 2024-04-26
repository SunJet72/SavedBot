namespace SavedBot.Chat
{
    internal abstract class OngoingChat(long userId)
    {
        public long UserId { get; private set; } = userId;
    }
}
