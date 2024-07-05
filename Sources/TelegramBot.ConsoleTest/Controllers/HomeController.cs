using TelegramBot.Controllers;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;

namespace TelegramBot.ConsoleTest.Controllers
{
    public class HomeController(ILogger<HomeController> _logger) : BotControllerBase
    {
        [BotCommand("/start")]
        public async Task StartAsync(Update update)
        {
            await _logger.LogInformation("Start command received.");
        }
    }
}
