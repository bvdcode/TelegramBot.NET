using Telegram.Bot;
using System.Threading.Tasks;
using TelegramBot.Abstractions;

namespace TelegramBot.ActionResults
{
    /// <summary>
    /// Represents a result for deleting a message in a Telegram bot.
    /// </summary>
    public class DeleteMessageResult : IActionResult
    {
        private readonly int _messageId;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteMessageResult"/> class.
        /// </summary>
        /// <param name="messageId">The identifier of the message to delete.</param>
        public DeleteMessageResult(int messageId)
        {
            _messageId = messageId;
        }

        /// <summary>
        /// Executes the result asynchronously, deleting the specified message.
        /// </summary>
        /// <param name="context">The action context containing the bot and chat information.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task ExecuteResultAsync(ActionContext context)
        {
            return context.Bot.DeleteMessage(context.ChatId, _messageId);
        }
    }
}
