using Microsoft.Extensions.Logging;
using SavedBot.Chat;
using SavedBot.Chat.Add;
using SavedBot.Exceptions;
using SavedBot.Model;
using Telegram.Bot.Types;

namespace SavedBot.Handlers
{
    internal class AddCommandHandler(IModelContext modelContext, ILogger logger) : CommandHandler(modelContext, logger)
    {
        private readonly List<OngoingChat> _chats = [];

        public async override void Handle(OngoingChat chat)
        {
            switch (chat)
            {
                case OngoingAddFileChat addFileChat:
                    {
                        Console.WriteLine($"ongoingAddFileChat handler: {addFileChat.UserId }");

                        if (_chats.Find((c) => c.UserId == addFileChat.UserId) is OngoingNameChat addChat)
                        {
                            addFileChat.File.FileName = addChat.Name;
                            addFileChat.File.User = new TelegramUser() { Id = addFileChat.UserId };


                             await _modelContext.AddItemAsync(addFileChat.File);
                            _chats.Remove(addChat);
                        }
                        else throw new NotFoundOngoingAddChatException();
                    }
                    break;
                case OngoingAddTextChat addTextChat:
                    {
                        Console.WriteLine($"ongoingAddTextChat handler: {addTextChat.UserId }");
                        if (_chats.Find((c) => c.UserId == addTextChat.UserId) is OngoingNameChat nameChat)
                        {
                            //TODO: SavedText
                            //_modelContext.AddText(addTextChat.UserId, nameChat.Name, addTextChat.Text);
                            _chats.Remove(nameChat);
                        }
                        else throw new NotFoundOngoingAddChatException();
                    }
                    break;
                case OngoingNameChat nameChat:
                    {
                        if (_chats.Find((c) => c.UserId == nameChat.UserId) is OngoingAddChat addChat)
                        {
                            _chats.Remove(addChat);
                            _chats.Add(nameChat);
                            _logger.LogDebug("ongoingNameChat handler: {UserId}", chat.UserId);   
                        }
                        return;
                    }
                case OngoingAddChat addChat:
                    {
                        _chats.Add(addChat);
                        _logger.LogDebug("ongoingAddChat handler: {UserId}", chat.UserId);
                        return;
                    }
                default:
                    {
                        _logger.LogDebug("ongoingChat handler: {UserId}", chat.UserId);
                    }
                    break;                  
            }
        }

        public override bool IsNamed(long chatId)
        {
             bool res = _chats.First((c) => c.UserId == chatId) switch
             {
                OngoingAddFileChat or OngoingAddTextChat => true,
                _ => false
             };
            return res;
        }
    }
}
