using Telegram.Bot;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using TelegramBot.Abstractions;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot.ActionResults
{
    internal class InlineResult : IActionResult
    {
        public string Text { get; }
        public bool UseMarkdown { get; }
        public InlineKeyboardMarkup Keyboard { get; }

        public InlineResult(string text, InlineKeyboardMarkup keyboard, bool useMarkdown)
        {
            Text = text;
            Keyboard = keyboard;
            UseMarkdown = useMarkdown;
        }

        public async Task ExecuteResultAsync(ActionContext context)
        {
            ParseMode? parseMode = null;
            if (UseMarkdown)
            {
                parseMode = ParseMode.MarkdownV2;
            }
            await context.Bot.SendTextMessageAsync(context.ChatId, Text, replyMarkup: Keyboard, parseMode: parseMode);
        }
    }
}
