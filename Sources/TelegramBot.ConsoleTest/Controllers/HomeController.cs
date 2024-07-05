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
        public async Task<IActionResult> StartAsync(Update update)
        {
            _logger.LogInformation("Start command received.");
            return Text("Hello!");
        }
    }
}
