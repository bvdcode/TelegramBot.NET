using System;
using System.IO;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBot.Extensions;
using TelegramBot.Abstractions;
using TelegramBot.ActionResults;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot.Controllers
{
    /// <summary>
    /// Base class for bot controllers.
    /// </summary>
    public abstract class BotControllerBase
    {
        internal IKeyValueProvider? KeyValueProvider { get; set; }

        /// <summary>
        /// User who sent the update.
        /// </summary>
        public User User { get; internal set; } = null!;

        /// <summary>
        /// Update received from the user.
        /// </summary>
        public Update Update { get; internal set; } = null!;

        /// <summary>
        /// <see cref="ITelegramBotClient"/> instance.
        /// </summary>
        public ITelegramBotClient Client { get; internal set; } = null!;

        /// <summary>
        /// Sends a text message to the sender.
        /// </summary>
        /// <param name="text">Text message.</param>
        /// <returns>Result of the <see cref="IActionResult"/> action.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="text"/> is null or empty.</exception>
        public IActionResult Text(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentNullException(nameof(text));
            }
            return new TextResult(text);
        }

        /// <summary>
        /// Sends a markdown message to the sender.
        /// </summary>
        /// <param name="text">Markdown message.</param>
        /// <returns>Result of the <see cref="IActionResult"/> action.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="text"/> is null or empty.</exception>
        public IActionResult Markdown(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentNullException(nameof(text));
            }
            return new MarkdownResult(text);
        }

        /// <summary>
        /// Sends an inline keyboard to the sender.
        /// </summary>
        /// <param name="prompt">Text prompt.</param>
        /// <param name="keyboard">Keyboard markup.</param>
        /// <param name="useMarkdown">Whether to use markdown or not.</param>
        /// <returns>Result of the <see cref="IActionResult"/> action.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="prompt"/> or <paramref name="keyboard"/> is null or empty.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="keyboard"/> does not contain any buttons.</exception>
        public IActionResult Inline(string prompt, InlineKeyboardMarkup keyboard, bool useMarkdown = false)
        {
            if (string.IsNullOrWhiteSpace(prompt))
            {
                throw new ArgumentNullException(nameof(prompt));
            }
            if (keyboard == null)
            {
                throw new ArgumentNullException(nameof(keyboard));
            }
            if (keyboard.InlineKeyboard == null || !keyboard.InlineKeyboard.Any())
            {
                throw new ArgumentException("Keyboard must contain at least one button.", nameof(keyboard));
            }
            return new InlineResult(prompt, keyboard, useMarkdown);
        }

        /// <summary>
        /// Do nothing.
        /// </summary>
        /// <returns>Result of the <see cref="IActionResult"/> action.</returns>
        public IActionResult Void()
        {
            return new EmptyResult();
        }

        /// <summary>
        /// Sends a file to the sender.
        /// </summary>
        /// <param name="filePath">Path to the file.</param>
        /// <returns>Result of the <see cref="IActionResult"/> action.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="filePath"/> is null or empty.</exception>
        public IActionResult File(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }
            return new FileResult(filePath);
        }

        /// <summary>
        /// Sends a file to the sender.
        /// </summary>
        /// <param name="stream">Stream with file content.</param>
        /// <param name="fileName">File name.</param>
        /// <param name="disposeStream">Dispose the stream after sending the file.</param>
        public IActionResult File(Stream stream, string fileName, bool disposeStream = true)
        {
            return new FileResult(stream, fileName, disposeStream);
        }

        /// <summary>
        /// Deletes message from the current update, or does nothing if the message identifier is not found.
        /// <br/>
        /// Note: this method does not throw exceptions when the message ID is not found, access is denied, message is already deleted etc.
        /// </summary>
        public void Delete()
        {
            bool hasMessageId = Update.TryGetMessageId(out int messageId);
            if (!hasMessageId)
            {
                return;
            }
            try
            {
                Client.DeleteMessageAsync(User.Id, messageId);
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Sets the value of the key in the key-value provider.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <exception cref="InvalidOperationException">Thrown when the key-value provider is not set.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> is null or empty.</exception>
        public void SetValue(string key, string? value)
        {
            if (KeyValueProvider == null)
            {
                throw new InvalidOperationException("Key-value provider is not set. You can inject it in the app service provider.");
            }
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }
            KeyValueProvider.SetValue(key, value);
        }

        /// <summary>
        /// Serializes the value to JSON and sets it in the key-value provider.
        /// </summary>
        /// <typeparam name="TValue">Type of the value.</typeparam>
        /// <param name="key">Key to set the value for.</param>
        /// <param name="value">Value to set.</param>
        /// <exception cref="InvalidOperationException">Thrown when the key-value provider is not set.</exception>
        public void SetValue<TValue>(string key, TValue value)
        {
            string? json = value == null ? null : System.Text.Json.JsonSerializer.Serialize(value);
            SetValue(key, json);
        }

        /// <summary>
        /// Gets the value of the key from the key-value provider.
        /// </summary>
        /// <param name="key">Key to get the value for.</param>
        /// <returns>Value of the key.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the key-value provider is not set.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> is null or empty.</exception>
        public string GetValue(string key)
        {
            if (KeyValueProvider == null)
            {
                throw new InvalidOperationException("Key-value provider is not set. You can inject it in the app service provider.");
            }
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }
            return KeyValueProvider.GetValue(key);
        }

        /// <summary>
        /// Deserializes the value from JSON and gets it from the key-value provider.
        /// </summary>
        /// <typeparam name="TValue">Type of the value.</typeparam>
        /// <param name="key">Key to get the value for.</param>
        /// <returns>Value of the key.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the key-value provider is not set.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> is null or empty.</exception>
        public TValue GetValue<TValue>(string key)
        {
            string json = GetValue(key);
            return string.IsNullOrWhiteSpace(json) ? default! : System.Text.Json.JsonSerializer.Deserialize<TValue>(json)!;
        }
    }
}