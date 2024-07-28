using System.Reflection;
using Telegram.Bot.Types;

namespace TelegramBot.Handlers
{
    internal interface ITelegramUpdateHandler
    {
        MethodInfo GetMethodInfo();
        object[]? GetArguments();
    }
}