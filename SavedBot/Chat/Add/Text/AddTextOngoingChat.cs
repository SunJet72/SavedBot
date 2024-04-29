namespace SavedBot.Chat.Add.Text
{
    internal class AddTextOngoingChat(long userId, string text) : OngoingChat(userId)
    {
        public string Text { get; private set; } = text;
    }
}
