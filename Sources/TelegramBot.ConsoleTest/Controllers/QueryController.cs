using TelegramBot.Attributes;
using TelegramBot.Controllers;
using TelegramBot.Abstractions;

namespace TelegramBot.ConsoleTest.Controllers
{
    internal class QueryController : BotControllerBase
    {
        [TextRegex(".+hello.+")]
        public IActionResult HandleHelloAsync()
        {
            return Text("Hello!");
        }

        [Text("bye")]
        public IActionResult HandleByeAsync()
        {
            return Text("Goodbye!", removeReplyKeyboard: true);
        }
    }
}
