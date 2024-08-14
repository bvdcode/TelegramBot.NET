using TelegramBot.Attributes;
using TelegramBot.Controllers;
using TelegramBot.Abstractions;

namespace TelegramBot.ConsoleTest.Controllers
{
    internal class QueryController : BotControllerBase
    {
        [TextQuery(pattern: ".+hello.+")]
        public IActionResult HandleHelloAsync()
        {
            return Text("Hello!");
        }

        [TextQuery(pattern: "bye")]
        public IActionResult HandleByeAsync()
        {
            return Text("Goodbye!");
        }
    }
}
