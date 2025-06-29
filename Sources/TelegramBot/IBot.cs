using System.Threading;
using Microsoft.Extensions.Hosting;

namespace TelegramBot
{
    /// <summary>
    /// Interface for bot.
    /// </summary>
    public interface IBot : IHost
    {
        /// <summary>
        /// Runs the bot.
        /// </summary>
        /// <param name="token">Cancellation token (optional).</param>
        void Run(CancellationToken token = default);
    }
}