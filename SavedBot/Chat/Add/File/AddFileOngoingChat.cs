using Telegram.Bot.Types.Enums;

namespace SavedBot.Chat.Add.File
{
    internal class AddFileOngoingChat(long userId, string fileId, MessageType messageType) : OngoingChat(userId)
    {
        public string FileId { get; private set; } = fileId;
        public MessageType FileType { get; private set; } = messageType;
    }
}
