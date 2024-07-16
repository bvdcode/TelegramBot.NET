using System;

namespace TelegramBot.Attributes
{
    /// <summary>
    /// Inline command attribute for bot controllers.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class InlineCommandAttribute : Attribute
    {
        /// <summary>
        /// Command name.
        /// </summary>
        public string Command { get; }

        /// <summary>
        /// Creates a new instance of <see cref="InlineCommandAttribute"/>.
        /// </summary>
        /// <param name="command">The command name.</param>
        public InlineCommandAttribute(string command)
        {
            Command = command;
        }
    }
}
