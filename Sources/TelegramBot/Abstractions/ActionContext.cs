using Telegram.Bot;

namespace TelegramBot.Abstractions
{
    /// <summary>
    /// Context for bot actions.
    /// </summary>
    public class ActionContext
    {
        /// <summary>
        /// Telegram bot client.
        /// </summary>
        public ITelegramBotClient Bot { get; }

        /// <summary>
        /// Chat identifier.
        /// </summary>
        public long ChatId { get; set; }

        /// <summary>
        /// Creates a new instance of <see cref="ActionContext"/>.
        /// </summary>
        /// <param name="bot">Telegram bot client.</param>
        /// <param name="chatId">Chat identifier.</param>
        public ActionContext(ITelegramBotClient bot, long chatId)
        {
            Bot = bot;
            ChatId = chatId;
        }
    }
}