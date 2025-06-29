using TelegramBot.Builders;
using TelegramBot.Attributes;
using TelegramBot.Controllers;
using TelegramBot.Abstractions;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot.AspNetCoreTest.Controllers
{
    public class PhoneController(ILogger<PhoneController> _logger) : BotControllerBase
    {
        private const string PhoneKeyPrefix = "Phone:";

        [TextCommand("/phone")]
        public IActionResult GetPhone()
        {
            long value = GetPhoneValue();
            _logger.LogInformation("Counter value requested: {Value}", value);
            return CreateCounterResponse(value, edit: false);
        }

        [InlineCommand("/phone/{digit}")]
        public IActionResult AddDigit(int digit)
        {
            if (digit < 0 || digit > 9)
            {
                _logger.LogWarning("Invalid digit: {Digit}", digit);
                return Text("Please enter a valid digit (0-9).");
            }
            long value = GetPhoneValue();
            if (value > 999_999_9999)
            {
                _logger.LogWarning("Phone number exceeds 11 digits.");
                return Void();
            }
            value = value * 10 + digit;
            SetPhoneValue(value);
            _logger.LogInformation("Phone changed: {Value}", value);
            return CreateCounterResponse(value);
        }

        [InlineCommand("/phone/delete")]
        public IActionResult DeleteLastDigit()
        {
            long value = GetPhoneValue();
            if (value == 0)
            {
                _logger.LogWarning("No digits to delete.");
                return Void();
            }
            value /= 10; // Remove the last digit
            SetPhoneValue(value);
            _logger.LogInformation("Last digit deleted, new value: {Value}", value);
            return CreateCounterResponse(value);
        }

        [InlineCommand("/phone/confirm")]
        public IActionResult ConfirmPhone()
        {
            long value = GetPhoneValue();
            if (value == 0)
            {
                _logger.LogWarning("No phone number entered.");
                return Text("Please enter a valid phone number.");
            }
            _logger.LogInformation("Phone number confirmed: {Value}", value);
            return Text($"Phone number confirmed: {FormatPhoneNumber(value)}");
        }

        private static string FormatPhoneNumber(long value)
        {
            if (value <= 0)
            {
                return "+_ (___) ___-____";
            }
            string phoneNumber = value.ToString().PadRight(11, '_');
            return $"+{phoneNumber[0]} ({phoneNumber[1..4]}) {phoneNumber.Substring(4, 3)}-{phoneNumber.Substring(7, 4)}";
        }

        private string GetPhoneKey() => $"{PhoneKeyPrefix}{User.Id}";

        private long GetPhoneValue() => GetValue<long>(GetPhoneKey());

        private void SetPhoneValue(long value) => SetValue(GetPhoneKey(), value);

        private static InlineKeyboardMarkup CreatePhoneKeyboard(bool showNumbers = true)
        {
            var builder = new KeyboardBuilder()
                .WithColumns(3);
            for (int i = 1; i <= 9 && showNumbers; i++)
            {
                builder.AddButton(i.ToString(), $"/phone/{i}");
            }
            builder.AddButton("⬅️", "/phone/delete");
            if (showNumbers)
            {
                builder.AddButton("0", "/phone/0");
            }
            builder.AddButton("📞", "/phone/confirm");
            return builder.Build();
        }

        private IActionResult CreateCounterResponse(long value, bool edit = true)
        {
            // Fix for CS8076, CS8361, CS8088, CS1002, CS0103, and CS0201
            // Parenthesize the conditional expression and ensure proper syntax
            string message = FormatPhoneNumber(value);
            return edit 
                ? TextEdit(message, CreatePhoneKeyboard(value < 999_999_9999)) 
                : Inline(message, CreatePhoneKeyboard(value < 999_999_9999));
        }
    }
}
