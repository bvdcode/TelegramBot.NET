using System;
using TelegramBot.Abstractions;
using TelegramBot.ActionResults;

namespace TelegramBot.Controllers
{
    /// <summary>
    /// Base class for bot controllers.
    /// </summary>
    public abstract class BotControllerBase
    {
        /// <summary>
        /// Sends a text message to the sender.
        /// </summary>
        /// <param name="text">Text message.</param>
        /// <returns>Result of the <see cref="IActionResult"/>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="text"/> is null or empty.</exception>
        public IActionResult Text(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentNullException(nameof(text));
            }
            return new TextResult(text);
        }
    }
}
