using Telegram.Bot.Types;
using TelegramBot.Builders;
using TelegramBot.Attributes;
using TelegramBot.Controllers;
using TelegramBot.Abstractions;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot.AspNetCoreTest.Controllers
{
    public class CounterController(ILogger<CounterController> _logger) : BotControllerBase
    {
        private const string CounterKeyPrefix = "Counter:";
        private const string IncrementCommand = "/counter/increment";
        private const string DecrementCommand = "/counter/decrement";

        [TextCommand("/counter")]
        public IActionResult GetCounter()
        {
            int value = GetCounterValue();
            _logger.LogInformation("Counter value requested: {Value}", value);
            return CreateCounterResponse(value, edit: false);
        }

        [InlineCommand("/counter/increment")]
        public IActionResult IncrementCounter()
        {
            int value = GetCounterValue();
            value++;
            SetCounterValue(value);
            _logger.LogInformation("Counter incremented: {Value}", value);
            return CreateCounterResponse(value);
        }

        [InlineCommand("/counter/decrement")]
        public IActionResult DecrementCounter()
        {
            int value = GetCounterValue();
            value--;
            SetCounterValue(value);
            _logger.LogInformation("Counter decremented: {Value}", value);
            return CreateCounterResponse(value);
        }

        private string GetCounterKey() => $"{CounterKeyPrefix}{User.Id}";

        private int GetCounterValue() => GetValue<int>(GetCounterKey());

        private void SetCounterValue(int value) => SetValue(GetCounterKey(), value);

        private static InlineKeyboardMarkup CreateCounterKeyboard()
        {
            return new KeyboardBuilder()
                .WithColumns(2)
                .AddButton("➖", DecrementCommand)
                .AddButton("➕", IncrementCommand)
                .Build();
        }

        private IActionResult CreateCounterResponse(int value, bool edit = true)
        {
            string message = $"{CounterKeyPrefix} {value}";
            return edit ? TextEdit(message, CreateCounterKeyboard()) : Inline(message, CreateCounterKeyboard());
        }
    }
}