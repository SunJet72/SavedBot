using SavedBot;
using SavedBot.Loggers;
using SavedBot.Model;
using System.Text;
using SavedBot.Database;
using SavedBot.DbModels;
string token = "Your token";

ILogger logger = new ConsoleLogger();
IModelContext context = new ModelContext(logger);
Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = Encoding.UTF8;
using (TelegramContext db = new TelegramContext())
{
    

}
Bot bot = new(token, context, logger);
bot.Start();

Console.ReadLine();