namespace SavedBot.Chat.Add
{
    internal class OngoingNameChat(long chatId, string name) : OngoingChat(chatId)
    {
        public string Name { get; private set; } = name;
    }
}
