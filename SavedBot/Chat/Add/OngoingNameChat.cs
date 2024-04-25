namespace SavedBot.Chat.Add
{
    internal class OngoingNameChat(long userId, string name) : OngoingChat(userId)
    {
        public string Name { get; private set; } = name;
    }
}
