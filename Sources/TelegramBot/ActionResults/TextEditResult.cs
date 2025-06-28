using Telegram.Bot;
using System.Threading.Tasks;
using TelegramBot.Abstractions;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot.ActionResults
{
    /// <summary>
    /// Text edit result.
    /// /// </summary>
    public class TextEditResult : IActionResult
    {
        /// <summary>
        /// Text message.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Message identifier to edit.
        /// </summary>
        public int MessageId { get; }

        /// <summary>
        /// Inline keyboard.
        /// </summary>
        public InlineKeyboardMarkup? Keyboard { get; }

        /// <summary>
        /// Creates a new instance of <see cref="TextEditResult"/>.
        /// </summary>
        /// <param name="text">Text message.</param>
        /// <param name="messageId">Message identifier to edit.</param>
        /// <param name="keyboard">Inline keyboard markup (optional).</param>
        public TextEditResult(string text, int messageId, InlineKeyboardMarkup? keyboard = null)
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
            return context.Bot.EditMessageText(context.ChatId, messageId: MessageId, text: Text, replyMarkup: Keyboard);
        }
    }
}
