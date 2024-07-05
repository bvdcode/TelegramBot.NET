using System.Threading;
using TelegramBot.Controllers;

namespace TelegramBot
{
    /// <summary>
    /// Interface for bot.
    /// </summary>
    public interface IBot
    {
        /// <summary>
        /// Maps controllers inherited from <see cref="BotControllerBase"/>.
        /// </summary>
        void MapControllers();

        /// <summary>
        /// Runs the bot.
        /// </summary>
        /// <param name="token">Cancellation token (optional).</param>
        void Run(CancellationToken token = default);
    }
}