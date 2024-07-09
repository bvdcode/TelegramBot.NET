using Telegram.Bot;
using System.Threading.Tasks;
using TelegramBot.Abstractions;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot.ActionResults
{
    internal class InlineResult : IActionResult
    {
        public string Text { get; }
        public InlineKeyboardMarkup Keyboard { get; }

        public InlineResult(string text, InlineKeyboardMarkup keyboard)
        {
            Text = text;
            Keyboard = keyboard;
        }

        public async Task ExecuteResultAsync(ActionContext context)
        {
            await context.Bot.SendTextMessageAsync(context.ChatId, Text, replyMarkup: Keyboard);
        }
    }
}
