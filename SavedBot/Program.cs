using SavedBot;
using SavedBot.Loggers;
using SavedBot.Model;
using System.Text;

string token = "Your token";

ILogger logger = new ConsoleLogger();
IModelContext context = new MockModelContext(logger);
Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = Encoding.UTF8;

Bot bot = new Bot(token, context, logger);
bot.Start();

Console.ReadLine();