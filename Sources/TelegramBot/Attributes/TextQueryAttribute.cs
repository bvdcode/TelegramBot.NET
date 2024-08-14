using System;
using System.Text.RegularExpressions;

namespace TelegramBot.Attributes
{
    /// <summary>
    /// Text query attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class TextQueryAttribute : Attribute
    {
        /// <summary>
        /// Pattern.
        /// </summary>
        public Regex? Regex { get; }

        /// <summary>
        /// Creates a new instance of <see cref="TextQueryAttribute"/>.
        /// </summary>
        /// <param name="pattern"><see cref="Regex"/> pattern.</param>
        public TextQueryAttribute(string? pattern = null)
        {
            if (pattern != null)
            {
                Regex = new Regex(pattern);
            }
        }
    }
}