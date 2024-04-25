using SavedBot.Bot;
using SavedBot.Exceptions;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddUserSecrets<Bot>();

Bot bot = Bot.BuildBot(builder.Configuration["TG_BOT_KEY"] ?? throw new WebhookUrlNotFoundException());
bot.SetWebhook(builder.Configuration["WEBHOOK_URL"] ?? throw new WebhookUrlNotFoundException());

builder.Services.AddSingleton(bot);

builder.Services.AddControllers().AddNewtonsoftJson();

var app = builder.Build();
app.MapControllers();

app.Run();