using Telegram.Bot;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using TelegramBot.Abstractions;

namespace TelegramBot.ActionResults
{
    internal class MarkdownResult : IActionResult
    {
        public string Text { get; }

        public MarkdownResult(string text)
        {
            Text = text;
        }

        public async Task ExecuteResultAsync(ActionContext context)
        {
            await context.Bot.SendTextMessageAsync(context.ChatId, Text, parseMode: ParseMode.MarkdownV2);
        }
    }
}
