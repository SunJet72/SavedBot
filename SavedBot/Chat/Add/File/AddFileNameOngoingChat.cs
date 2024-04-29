namespace SavedBot.Chat.Add.File
{
    internal class AddFileNameOngoingChat(long userId, string name) : OngoingChat(userId)
    {
        public string Name { get; private set; } = name;
    }
}
