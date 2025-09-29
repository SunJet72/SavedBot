using Telegram.Bot.Types.Enums;

namespace SavedBot.Chat.Add.File
{
    internal class AddFileOngoingChat(long userId, string fileId, string fileUniqueId, MessageType messageType) : OngoingChat(userId)
    {
        public string FileId { get; private set; } = fileId;
        public string FileUniqueId { get; private set; } = fileUniqueId;
        public MessageType FileType { get; private set; } = messageType;
    }
}
