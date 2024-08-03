using System.Reflection;

namespace TelegramBot.Handlers
{
    internal interface ITelegramUpdateHandler
    {
        MethodInfo? GetMethodInfo();
        object[]? GetArguments();
    }
}