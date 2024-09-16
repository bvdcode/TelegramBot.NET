using System;

namespace TelegramBot.Attributes
{
    /// <summary>
    /// Attribute to mark a method as requiring authorization.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeAttribute : Attribute { }
}
