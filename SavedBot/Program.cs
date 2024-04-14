using SavedBot;
using SavedBot.Loggers;
using SavedBot.Model;
using System.Text;
string token = "Your token";

ILogger logger = new ConsoleLogger();
IModelContext context = new ModelContext(logger);
Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = Encoding.UTF8;
Bot bot = new(token, context, logger);
bot.Start();

Console.ReadLine();