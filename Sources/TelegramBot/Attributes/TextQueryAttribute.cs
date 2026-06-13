using System;

namespace TelegramBot.Attributes
{
    /// <summary>
    /// Obsolete text query attribute. Use <see cref="TextAttribute"/>,
    /// <see cref="TextRegexAttribute"/>, or <see cref="TextFallbackAttribute"/> instead.
    /// </summary>
    [Obsolete("Use TextAttribute for exact text routes, TextRegexAttribute for regex routes, or TextFallbackAttribute for fallback text routes.")]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class TextQueryAttribute : Attribute
    {
        /// <summary>
        /// Exact text to match.
        /// </summary>
        public string? Text { get; }

        /// <summary>
        /// Route priority. Higher values win before route type is considered.
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// String comparison used for exact text matching.
        /// </summary>
        public StringComparison Comparison { get; set; }

        /// <summary>
        /// Indicates whether this route is a fallback route.
        /// </summary>
        internal bool IsFallback => Text == null;

        /// <summary>
        /// Creates a fallback text route.
        /// </summary>
        public TextQueryAttribute()
        {
            Priority = TextFallbackAttribute.DefaultPriority;
            Comparison = StringComparison.Ordinal;
        }

        /// <summary>
        /// Creates an exact text route.
        /// </summary>
        /// <param name="pattern">Exact text to match.</param>
        public TextQueryAttribute(string pattern)
        {
            if (string.IsNullOrWhiteSpace(pattern))
            {
                throw new ArgumentException("Text query cannot be empty.", nameof(pattern));
            }

            Text = pattern;
            Priority = TextAttribute.DefaultPriority;
            Comparison = StringComparison.Ordinal;
        }

        internal bool IsMatch(string text)
        {
            return IsFallback || string.Equals(text, Text, Comparison);
        }
    }
}
