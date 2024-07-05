using System.Threading;
using TelegramBot.Controllers;

namespace TelegramBot
{
    /// <summary>
    /// Telegram bot application.
    /// </summary>
    public class BotApp : IBot
    {
        /// <summary>
        /// Maps controllers inherited from <see cref="BotControllerBase"/>.
        /// </summary>
        public void MapControllers()
        {

        }

        /// <summary>
        /// Runs the bot.
        /// </summary>
        /// <param name="token">Cancellation token (optional).</param>
        public void Run(CancellationToken token = default)
        {

        }
    }
}