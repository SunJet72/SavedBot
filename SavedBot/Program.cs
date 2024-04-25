using Microsoft.Extensions.Configuration;
using SavedBot.Bot;
using SavedBot.Configuration;
using SavedBot.Data;
using SavedBot.Exceptions;

var config = new ConfigurationBuilder()
    .AddUserSecrets<Bot>().Build();

AppDbContext dbContext = new AppDbContext(config[UserSecretKey.ConnectionString] ?? throw new UserSecretNotFoundException(UserSecretKey.ConnectionString));

Bot bot = Bot.BuildBot(config[UserSecretKey.BotKey] ?? throw new UserSecretNotFoundException(UserSecretKey.BotKey), dbContext);
bot.StartPolling();

Console.ReadLine();
