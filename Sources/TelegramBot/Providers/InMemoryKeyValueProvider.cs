using System;
using TelegramBot.Abstractions;
using System.Collections.Concurrent;

namespace TelegramBot.Providers
{
    /// <summary>
    /// Default implementation of <see cref="IKeyValueProvider"/> that stores key-value pairs in memory.
    /// </summary>
    public class InMemoryKeyValueProvider : IKeyValueProvider
    {
        private readonly ConcurrentDictionary<string, string> _store = new ConcurrentDictionary<string, string>();

        /// <summary>
        /// Get the value for the specified key.
        /// </summary>
        /// <param name="key">The key to get the value for.</param>
        /// <returns>The value for the specified key.</returns>
        public string GetValue(string key)
        {
            if (_store.TryGetValue(key, out string? value))
            {
                return value;
            }
            return string.Empty;
        }

        /// <summary>
        /// Set the value for the specified key.
        /// </summary>
        /// <param name="key">The key to set the value for.</param>
        /// <param name="value">The value to set.</param>
        public void SetValue(string key, string? value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }
            if (value == null)
            {
                _store.TryRemove(key, out string? _);
                return;
            }
            _store.AddOrUpdate(key, value, (k, v) => value);
        }
    }
}
