using System;
using System.Text.RegularExpressions;

namespace TelegramBot.Attributes
{
    /// <summary>
    /// Regex text route attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class TextRegexAttribute : Attribute
    {
        /// <summary>
        /// Default priority for regex text routes.
        /// </summary>
        public const int DefaultPriority = 0;

        /// <summary>
        /// Regex pattern.
        /// </summary>
        public string Pattern { get; }

        /// <summary>
        /// Regex options.
        /// </summary>
        public RegexOptions Options { get; }

        /// <summary>
        /// Regex route matcher.
        /// </summary>
        public Regex Regex { get; }

        /// <summary>
        /// Route priority. Higher values win before route type is considered.
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// Determines whether the regex must match the entire message text.
        /// </summary>
        public bool MatchEntireText { get; set; }

        /// <summary>
        /// Creates a new instance of <see cref="TextRegexAttribute"/>.
        /// </summary>
        /// <param name="pattern">Regex pattern.</param>
        public TextRegexAttribute(string pattern)
            : this(pattern, RegexOptions.None)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="TextRegexAttribute"/>.
        /// </summary>
        /// <param name="pattern">Regex pattern.</param>
        /// <param name="options">Regex options.</param>
        public TextRegexAttribute(string pattern, RegexOptions options)
        {
            if (string.IsNullOrWhiteSpace(pattern))
            {
                throw new ArgumentException("Text regex pattern cannot be empty.", nameof(pattern));
            }

            Pattern = pattern;
            Options = options;
            Regex = new Regex(pattern, options);
            Priority = DefaultPriority;
            MatchEntireText = true;
        }

        internal bool IsMatch(string text)
        {
            Match match = Regex.Match(text);
            if (!match.Success)
            {
                return false;
            }

            return !MatchEntireText || match.Index == 0 && match.Length == text.Length;
        }
    }
}
