namespace SavedBot.Chat.Edit
{
    internal class RenameFileOngoingChat(long userId, string name) : OngoingChat(userId)
    {
        public string Name { get; private set; } = name;
    }
}