using SavedBot.Bot;
using SavedBot.Configuration;
using SavedBot.Data;
using SavedBot.Exceptions;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddUserSecrets<Bot>();

string dbProvider = builder.Configuration[UserSecretKey.DbProvider]
?? throw new UserSecretNotFoundException(UserSecretKey.DbProvider);

string connString = builder.Configuration[UserSecretKey.ConnectionString]
?? throw new UserSecretNotFoundException(UserSecretKey.ConnectionString);

AppDbContext dbContext = new AppDbContext(dbProvider, connString);

Bot bot = Bot.BuildBot(builder.Configuration[UserSecretKey.BotKey] ?? throw new UserSecretNotFoundException(UserSecretKey.BotKey), dbContext);

bot.SetWebhook(builder.Configuration[UserSecretKey.WebhookUrl] ?? throw new UserSecretNotFoundException(UserSecretKey.WebhookUrl));

builder.Services.AddSingleton(bot);
builder.Services.AddControllers();

var app = builder.Build();
app.MapControllers();

app.Run();