using Microsoft.AspNetCore.Mvc;
using SavedBot.Bot;

namespace SavedBotWebhook.Controllers;

public class BotController(Bot bot) : ControllerBase
{
    private readonly Bot _bot = bot;

    [HttpPost("")]
    public async Task<IActionResult> Post([FromBody] Telegram.Bot.Types.Update update)
    {
        Console.WriteLine("Got update: " + update.Id);
        await _bot.HandleUpdateAsync(update, CancellationToken.None);
        return Ok();
    }
}

