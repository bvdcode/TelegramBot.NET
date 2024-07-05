using System;

namespace TelegramBot.Attributes
{
    /// <summary>
    /// Command attribute for bot controllers.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class BotCommandAttribute : Attribute
    {
        /// <summary>
        /// Command name.
        /// </summary>
        public string Command { get; }

        /// <summary>
        /// Creates a new instance of <see cref="BotCommandAttribute"/>.
        /// </summary>
        /// <param name="command">The command name.</param>
        public BotCommandAttribute(string command)
        {
            Command = command;
        }
    }
}