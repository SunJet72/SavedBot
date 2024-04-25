namespace SavedBot.Chat.Add
{
    internal class OngoingAddTextChat(long chatId, string text) : OngoingChat(chatId)
    {
        public string Text { get; private set; } = text;
    }
}
