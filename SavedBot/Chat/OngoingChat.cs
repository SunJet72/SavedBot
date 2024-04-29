namespace SavedBot.Chat
{
    internal abstract class OngoingChat(long userId)
    {
        public long UserId { get; private set; } = userId;

        public override int GetHashCode()
        {
            return UserId.GetHashCode();
        }

        public override bool Equals(object? other)
        {
            if(other == null) return false;
            if(other is OngoingChat chat)
            {
                return chat.UserId == UserId;
            }
            else return false;
        }
    }
}
