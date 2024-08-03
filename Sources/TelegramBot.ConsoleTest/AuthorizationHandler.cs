using Telegram.Bot.Types;
using TelegramBot.Abstractions;
using TelegramBot.ActionResults;

namespace TelegramBot.ConsoleTest
{
    internal class AuthorizationHandler : IBotAuthorizationHandler
    {
        public bool Authorize(User authorizingUser)
        {
            return authorizingUser.Id == 1234567890;
        }

        public IActionResult HandleUnauthorized(User authorizingUser)
        {
            return new TextResult("You are not authorized to use this bot.");
        }
    }
}