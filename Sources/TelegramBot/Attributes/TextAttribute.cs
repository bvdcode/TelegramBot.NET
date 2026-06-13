using System;

namespace TelegramBot.Attributes
{
    /// <summary>
    /// Exact text route attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class TextAttribute : Attribute
    {
        /// <summary>
        /// Default priority for exact text routes.
        /// </summary>
        public const int DefaultPriority = 100;

        /// <summary>
        /// Exact texts to match.
        /// </summary>
        public string[] Texts { get; }

        /// <summary>
        /// Route priority. Higher values win before route type is considered.
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// String comparison used for exact text matching.
        /// </summary>
        public StringComparison Comparison { get; set; }

        /// <summary>
        /// Creates a new instance of <see cref="TextAttribute"/>.
        /// </summary>
        /// <param name="texts">Exact texts to match.</param>
        public TextAttribute(params string[] texts)
        {
            if (texts == null || texts.Length == 0)
            {
                throw new ArgumentException("At least one text value is required.", nameof(texts));
            }

            foreach (string text in texts)
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    throw new ArgumentException("Text route values cannot be empty.", nameof(texts));
                }
            }

            Texts = texts;
            Priority = DefaultPriority;
            Comparison = StringComparison.Ordinal;
        }

        internal bool IsMatch(string text)
        {
            foreach (string routeText in Texts)
            {
                if (string.Equals(text, routeText, Comparison))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
