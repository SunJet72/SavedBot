using Microsoft.Extensions.Configuration;
using SavedBot.Bot;
using SavedBot.Configuration;
using SavedBot.Data;
using SavedBot.Exceptions;

var config = new ConfigurationBuilder()
    .AddUserSecrets<Bot>().Build();

string dbProvider = config[UserSecretKey.DbProvider]
?? throw new UserSecretNotFoundException(UserSecretKey.DbProvider);

string connString = config[UserSecretKey.ConnectionString]
?? throw new UserSecretNotFoundException(UserSecretKey.ConnectionString);


AppDbContext dbContext = new AppDbContext(dbProvider,connString);

Bot bot = Bot.BuildBot(config[UserSecretKey.BotKey] ?? throw new UserSecretNotFoundException(UserSecretKey.BotKey), dbContext);
bot.StartPolling();

Console.ReadLine();
