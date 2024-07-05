using Telegram.Bot;
using System.Threading.Tasks;
using TelegramBot.Abstractions;

namespace TelegramBot.ActionResults
{
    /// <summary>
    /// Creates a text result.
    /// </summary>
    public class TextResult : IActionResult
    {
        /// <summary>
        /// Text message.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Creates a new instance of <see cref="TextResult"/>.
        /// </summary>
        /// <param name="text">Text message.</param>
        public TextResult(string text)
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
            await context.Bot.SendTextMessageAsync(context.ChatId, Text);
        }
    }
}
