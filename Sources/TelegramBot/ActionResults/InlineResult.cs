using Telegram.Bot;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using TelegramBot.Abstractions;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot.ActionResults
{
    /// <summary>
    /// Inline query result.
    /// </summary>
    public class InlineResult : IActionResult
    {
        /// <summary>
        /// Inline message (caption).
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Use markdown or just plain text.
        /// </summary>
        public bool UseMarkdown { get; }

        /// <summary>
        /// Inline keyboard.
        /// </summary>
        public InlineKeyboardMarkup Keyboard { get; }

        /// <summary>
        /// Creates a new instance of <see cref="InlineResult"/>.
        /// </summary>
        /// <param name="text">Inline message (caption).</param>
        /// <param name="keyboard">Inline keyboard.</param>
        /// <param name="useMarkdown">Use markdown or just plain text.</param>
        public InlineResult(string text, InlineKeyboardMarkup keyboard, bool useMarkdown)
        {
            Text = text;
            Keyboard = keyboard;
            UseMarkdown = useMarkdown;
        }

        /// <summary>
        /// Executes the result asynchronously.
        /// </summary>
        /// <param name="context">Action context.</param>
        /// <returns>The task representing the result of the action.</returns>
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
