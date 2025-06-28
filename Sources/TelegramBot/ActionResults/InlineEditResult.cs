using Telegram.Bot;
using System.Threading.Tasks;
using TelegramBot.Abstractions;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot.ActionResults
{
    /// <summary>
    /// Inline query result.
    /// </summary>
    public class InlineEditResult : IActionResult
    {
        /// <summary>
        /// Inline message (caption).
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Message identifier to edit.
        /// </summary>
        public int MessageId { get; }

        /// <summary>
        /// Inline keyboard.
        /// </summary>
        public InlineKeyboardMarkup Keyboard { get; }

        /// <summary>
        /// Creates a new instance of <see cref="InlineResult"/>.
        /// </summary>
        /// <param name="text">Inline message (caption).</param>
        /// <param name="keyboard">Inline keyboard.</param>
        /// <param name="messageId">Message identifier to edit.</param>
        public InlineEditResult(string text, InlineKeyboardMarkup keyboard, int messageId)
        {
            Text = text;
            Keyboard = keyboard;
            MessageId = messageId;
        }

        /// <summary>
        /// Executes the result asynchronously.
        /// </summary>
        /// <param name="context">Action context.</param>
        /// <returns>The task representing the result of the action.</returns>
        public Task ExecuteResultAsync(ActionContext context)
        {
            return context.Bot.EditMessageReplyMarkup(context.ChatId, messageId: MessageId, replyMarkup: Keyboard);
        }
    }
}
