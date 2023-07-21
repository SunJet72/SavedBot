using SavedBot;
using SavedBot.Loggers;
using SavedBot.Model;
using System.Text;

string token = "6378630596:AAFxOZrAGeg8I-OzwAh03_XcESf6komVD6U";

ILogger logger = new ConsoleLogger();
IModelContext context = new MockModelContext(logger);
Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = Encoding.UTF8;

Bot bot = new Bot(token, context, logger);
bot.Start();

Console.ReadLine();