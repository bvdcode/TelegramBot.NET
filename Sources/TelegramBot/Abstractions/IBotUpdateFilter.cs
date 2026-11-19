using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace TelegramBot.Abstractions
{
    /// <summary>
    /// Inspects incoming updates before routing or controller authorization.
    /// </summary>
    public interface IBotUpdateFilter
    {
        /// <summary>
        /// Returns a result to stop processing, or null to continue to the next filter and route.
        /// </summary>
        /// <param name="update">The incoming update.</param>
        /// <param name="cancellationToken">The update cancellation token.</param>
        /// <returns>The response that stops processing, or null.</returns>
        Task<IActionResult?> FilterAsync(Update update, CancellationToken cancellationToken);
    }
}
