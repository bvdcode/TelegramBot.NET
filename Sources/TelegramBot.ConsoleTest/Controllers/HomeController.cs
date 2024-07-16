using TelegramBot.Builders;
using TelegramBot.Attributes;
using TelegramBot.Controllers;
using TelegramBot.Abstractions;
using Microsoft.Extensions.Logging;

namespace TelegramBot.ConsoleTest.Controllers
{
    public class HomeController(ILogger<HomeController> _logger) : BotControllerBase
    {
        [TextCommand("/start")]
        public async Task<IActionResult> HandleStartAsync()
        {
            _logger.LogInformation("Start command received.");
            var keyboard = new KeyboardBuilder()
                .WithColumns(2)
                .AddButton("🇺🇸 English", "/language/en")
                .AddButton("🇷🇺 Русский", "/language/ru")
                .AddButton("🇪🇸 Español", "/language/es")
                .AddButton("🇺🇦 Українська", "/language/uk")
                .Build();
            return Inline("[Readme](https://github.com/bvdcode/TelegramBot.NET?tab=readme-ov-file)", keyboard, useMarkdown: true);
        }

        [InlineCommand("/language/{lang}")]
        public IActionResult HandleLanguageAsync(string lang)
        {
            _logger.LogInformation("Language command received.");
            return Text($"Language set to {lang}.");
        }
    }
}