using System;

namespace TelegramBot.Attributes
{
    /// <summary>
    /// Fallback text route attribute used when no higher priority text route matches.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class TextFallbackAttribute : Attribute
    {
        /// <summary>
        /// Default priority for fallback text routes.
        /// </summary>
        public const int DefaultPriority = -100;

        /// <summary>
        /// Route priority. Higher values win before route type is considered.
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// Creates a new instance of <see cref="TextFallbackAttribute"/>.
        /// </summary>
        public TextFallbackAttribute()
        {
            Priority = DefaultPriority;
        }
    }
}
