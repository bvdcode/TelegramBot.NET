using Telegram.Bot.Types;
using TelegramBot.ActionResults;

namespace TelegramBot.Abstractions
{
    /// <summary>
    /// Defines the bot authorization handler.
    /// </summary>
    public interface IBotAuthorizationHandler
    {
        /// <summary>
        /// Authorize the user.
        /// </summary>
        /// <param name="authorizingUser">The user who is trying to perform the action.</param>
        /// <returns>True if the user is authorized, otherwise false.</returns>
        bool Authorize(User authorizingUser);

        /// <summary>
        /// Handle the unauthorized user.
        /// </summary>
        /// <param name="authorizingUser">The user who is trying to perform the action.</param>
        /// <returns>Result of the <see cref="IActionResult"/> action, ex. <see cref="TextResult"/> or <see cref="EmptyResult"/>.</returns>
        IActionResult HandleUnauthorized(User authorizingUser);
    }
}
