using SavedBot.Chat;
using SavedBot.Exceptions;
using SavedBot.Loggers;
using SavedBot.Model;

namespace SavedBot.Handlers
{
    public class AddCommandHandler : CommandHandler
    {
        private List<OngoingAddChat> _chats;
        public AddCommandHandler(IModelContext modelContext, ILogger logger) : base(modelContext, logger)
        {
            _chats = new List<OngoingAddChat>();
        }
        public override void Handle(OngoingChat chat)
        {
            switch (chat)
            {
                case OngoingAddFileChat addFileChat:
                    {
                        Console.WriteLine($"ongoingAddFileChat handler: {addFileChat.ChatId }");

                        if (_chats.Find((c) => c.ChatId == addFileChat.ChatId) is OngoingAddChat addChat)
                        {
                            _modelContext.AddFile(addFileChat.ChatId, addChat.Name, addFileChat.File);
                            _chats.Remove(addChat);
                        }
                        else throw new NotFoundOngoingAddChatException();
                    }
                    break;
                case OngoingAddTextChat addTextChat:
                    {
                        Console.WriteLine($"ongoingAddTextChat handler: {addTextChat.ChatId }");
                        if (_chats.Find((c) => c.ChatId == addTextChat.ChatId) is OngoingAddChat addChat)
                        {
                            _modelContext.AddText(addTextChat.ChatId, addChat.Name, addTextChat.Text);
                            _chats.Remove(addChat);
                        }
                        else throw new NotFoundOngoingAddChatException();
                    }
                    break;
                case OngoingAddChat addChat:
                    {
                        _chats.Add(addChat);
                        Console.WriteLine($"ongoingAddChat handler: {chat.ChatId}");
                        return;
                    }
                default:
                    {
                        Console.WriteLine($"ongoingChat handler: {chat.ChatId}");
                    }
                    break;                  
            }
        }

    }
}
