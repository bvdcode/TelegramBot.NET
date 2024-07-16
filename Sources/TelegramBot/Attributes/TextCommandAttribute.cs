using System;

namespace TelegramBot.Attributes
{
    /// <summary>
    /// Text command attribute for bot controllers.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class TextCommandAttribute : Attribute
    {
        /// <summary>
        /// Command name.
        /// </summary>
        public string Command { get; }

        /// <summary>
        /// Creates a new instance of <see cref="TextCommandAttribute"/>.
        /// </summary>
        /// <param name="command">The command name.</param>
        public TextCommandAttribute(string command)
        {
            Command = command;
        }
    }
}