using Telegram.Bot.Types;
using TelegramBot.Attributes;
using TelegramBot.Controllers;
using TelegramBot.Abstractions;
using Microsoft.Extensions.Logging;

namespace TelegramBot.ConsoleTest.Controllers
{
    public class HomeController(ILogger<HomeController> _logger) : BotControllerBase
    {
        [BotCommand("/start")]
        public async Task<IActionResult> HandleStartAsync()
        {
            _logger.LogInformation("Start command received.");
            await Task.Delay(1000);
            return Markdown("[inline mention of a user](tg://user?id=123)");
        }
    }
}
