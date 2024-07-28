using System;

namespace TelegramBot.Abstractions
{
    /// <summary>
    /// Key-value provider.
    /// </summary>
    public interface IKeyValueProvider
    {
        /// <summary>
        /// Gets the value of the key.
        /// </summary>
        /// <param name="key">Key to get the value for.</param>
        /// <returns>Value of the key.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> is null or empty.</exception>
        string GetValue(string key);

        /// <summary>
        /// Sets the value of the key.
        /// </summary>
        /// <param name="key">Key to set the value for.</param>
        /// <param name="value">Value to set.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> is null or empty.</exception>
        void SetValue(string key, string? value);
    }
}
