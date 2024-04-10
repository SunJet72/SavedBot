using SavedBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace SavedBot.Handlers
{
    public class MessageHandler
    {
        private readonly IDbService _dbService;
        private readonly TelegramBotClient _botClient;

        public MessageHandler(IDbService dbService, TelegramBotClient botClient)
        {
            _dbService = dbService;
            _botClient = botClient;
        }

        public async Task HandleMessageAsync(Message message)
        {
            var telegramUser = message.From;

            await _dbService.CreateNewUser(telegramUser);
        }
    }
}
