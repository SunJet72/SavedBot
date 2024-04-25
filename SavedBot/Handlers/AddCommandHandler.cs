using Microsoft.Extensions.Logging;
using SavedBot.Chat;
using SavedBot.Chat.Add;
using SavedBot.Exceptions;
using SavedBot.Model;

namespace SavedBot.Handlers
{
    internal class AddCommandHandler(IModelContext modelContext, ILogger logger) : CommandHandler(modelContext, logger)
    {
        private readonly List<OngoingChat> _chats = [];

        public override void Handle(OngoingChat chat)
        {
            switch (chat)
            {
                case OngoingAddFileChat addFileChat:
                    {
                        Console.WriteLine($"ongoingAddFileChat handler: {addFileChat.ChatId }");

                        if (_chats.Find((c) => c.ChatId == addFileChat.ChatId) is OngoingNameChat addChat)
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
                        if (_chats.Find((c) => c.ChatId == addTextChat.ChatId) is OngoingNameChat nameChat)
                        {
                            _modelContext.AddText(addTextChat.ChatId, nameChat.Name, addTextChat.Text);
                            _chats.Remove(nameChat);
                        }
                        else throw new NotFoundOngoingAddChatException();
                    }
                    break;
                case OngoingNameChat nameChat:
                    {
                        if (_chats.Find((c) => c.ChatId == nameChat.ChatId) is OngoingAddChat addChat)
                        {
                            _chats.Remove(addChat);
                            _chats.Add(nameChat);
                            _logger.LogDebug($"ongoingNameChat handler: {chat.ChatId}");   
                        }
                        return;
                    }
                case OngoingAddChat addChat:
                    {
                        _chats.Add(addChat);
                        _logger.LogDebug($"ongoingAddChat handler: {chat.ChatId}");
                        return;
                    }
                default:
                    {
                        _logger.LogDebug($"ongoingChat handler: {chat.ChatId}");
                    }
                    break;                  
            }
        }

        public override bool IsNamed(long chatId)
        {
             bool res = _chats.First((c) => c.ChatId == chatId) switch
             {
                OngoingAddFileChat or OngoingAddTextChat => true,
                _ => false
             };
            return res;
        }
    }
}
