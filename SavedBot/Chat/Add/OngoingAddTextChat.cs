namespace SavedBot.Chat.Add
{
    internal class OngoingAddTextChat(long userId, string text) : OngoingChat(userId)
    {
        public string Text { get; private set; } = text;
    }
}
