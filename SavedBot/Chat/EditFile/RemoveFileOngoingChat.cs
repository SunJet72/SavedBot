namespace SavedBot.Chat.Edit
{
    internal class RemoveFileOngoingChat(long userId, int messageId) : OngoingChat(userId)
    {
        public int MessageId { get; private set; } = messageId;
    }

}