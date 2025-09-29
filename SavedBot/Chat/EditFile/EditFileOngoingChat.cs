namespace SavedBot.Chat.Edit
{
    internal class EditFileOngoingChat(long userId, string fileId, string fileUniqueId, int messageId): OngoingChat(userId)
    {
        public string FileId { get; private set; } = fileId;
        public string FileUniqueId { get; private set; } = fileUniqueId;
        public int MessageId { get; private set; } = messageId;
    }
}