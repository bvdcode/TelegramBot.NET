using Telegram.Bot;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using TelegramBot.Abstractions;

namespace TelegramBot.ActionResults
{
    /// <summary>
    /// Markdown result.
    /// </summary>
    public class MarkdownResult : IActionResult
    {
        /// <summary>
        /// Markdown message.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Creates a new instance of <see cref="MarkdownResult"/>.
        /// </summary>
        /// <param name="text">Markdown message.</param>
        public MarkdownResult(string text)
        {
            Text = text;
        }

        /// <summary>
        /// Executes the result asynchronously.
        /// </summary>
        /// <param name="context">Action context.</param>
        /// <returns>The task representing the result of the action.</returns>
        public async Task ExecuteResultAsync(ActionContext context)
        {
            await context.Bot.SendTextMessageAsync(context.ChatId, Text, parseMode: ParseMode.MarkdownV2);
        }
    }
}
