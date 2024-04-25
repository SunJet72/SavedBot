using Microsoft.Extensions.Configuration;
using SavedBot.Bot;
using SavedBot.Exceptions;

var config = new ConfigurationBuilder()
    .AddUserSecrets<Bot>().Build();

Bot bot = Bot.BuildBot(config["TG_BOT_KEY"] ?? throw new TelegramBotTokenNotFoundException());
bot.StartPolling();

Console.ReadLine();
