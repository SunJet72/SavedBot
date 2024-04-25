using SavedBot.Bot;
using SavedBot.Configuration;
using SavedBot.Data;
using SavedBot.Exceptions;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddUserSecrets<Bot>();

AppDbContext dbContext = new AppDbContext(new Microsoft.EntityFrameworkCore.DbContextOptions<AppDbContext>(),
    builder.Configuration[UserSecretKey.ConnectionString] ?? throw new UserSecretNotFoundException(UserSecretKey.ConnectionString));

Bot bot = Bot.BuildBot(builder.Configuration[UserSecretKey.BotKey] ?? throw new UserSecretNotFoundException(UserSecretKey.BotKey), dbContext);

bot.SetWebhook(builder.Configuration[UserSecretKey.WebhookUrl] ?? throw new UserSecretNotFoundException(UserSecretKey.WebhookUrl));

builder.Services.AddSingleton(bot);
builder.Services.AddControllers().AddNewtonsoftJson();

var app = builder.Build();
app.MapControllers();

app.Run();