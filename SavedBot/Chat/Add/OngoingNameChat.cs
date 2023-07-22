namespace SavedBot.Chat.Add
{
    public class OngoingNameChat : OngoingChat
    {
        public string Name { get; private set; }
        public OngoingNameChat(long chatId, string name) : base(chatId)
        {
            Name = name;
        }

    }
}
