namespace SavedBot.Chat
{
    public class OngoingAddChat : OngoingChat
    {
        public string Name { get; private set; }
        public OngoingAddChat(long chatId, string name) : base(chatId) 
        {
             this.Name = name;
        }

    }
}
