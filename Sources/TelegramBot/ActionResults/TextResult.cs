using Telegram.Bot;
using System.Threading.Tasks;
using TelegramBot.Abstractions;
using Telegram.Bot.Types.ReplyMarkups;

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
        /// Indicates whether to clear the inline keyboard after sending the message.
        /// </summary>
        public bool RemoveReplyKeyboard { get; set; }

        /// <summary>
        /// Creates a new instance of <see cref="TextResult"/>.
        /// </summary>
        /// <param name="text">Text message.</param>
        /// <param name="removeReplyKeyboard">Indicates whether to clear the reply keyboard after sending the message.</param>
        public TextResult(string text, bool removeReplyKeyboard = false)
        {
            Text = text;
            RemoveReplyKeyboard = removeReplyKeyboard;
        }

        /// <summary>
        /// Executes the result asynchronously.
        /// </summary>
        /// <param name="context">Action context.</param>
        /// <returns>The task representing the result of the action.</returns>
        public Task ExecuteResultAsync(ActionContext context)
        {
            return context.Bot.SendMessage(context.ChatId, Text, 
                replyMarkup: RemoveReplyKeyboard ? new ReplyKeyboardRemove() : null);
        }
    }
}
