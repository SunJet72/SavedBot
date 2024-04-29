namespace SavedBot.Chat.Edit
{
    internal class EditFileOngoingChat(long userId, string fileId) : OngoingChat(userId)
    {
        public string FileId { get; private set; } = fileId;
    }
}