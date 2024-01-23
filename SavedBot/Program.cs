using Microsoft.Extensions.Configuration;
using SavedBot;
using SavedBot.Loggers;
using SavedBot.Model;
using System.Text;

var config = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

string token = config["TG_BOT_KEY"];

ILogger logger = new ConsoleLogger();
IModelContext context = new MockModelContext(logger);
Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = Encoding.UTF8;

Bot bot = new(token, context, logger);
bot.Start();

Console.ReadLine();