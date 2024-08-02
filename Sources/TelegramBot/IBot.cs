using System.Threading;
using TelegramBot.Controllers;
using Microsoft.Extensions.Hosting;

namespace TelegramBot
{
    /// <summary>
    /// Interface for bot.
    /// </summary>
    public interface IBot : IHost
    {
        /// <summary>
        /// Maps controllers inherited from <see cref="BotControllerBase"/>.
        /// </summary>
        IBot MapControllers();

        /// <summary>
        /// Runs the bot.
        /// </summary>
        /// <param name="token">Cancellation token (optional).</param>
        void Run(CancellationToken token = default);
    }
}