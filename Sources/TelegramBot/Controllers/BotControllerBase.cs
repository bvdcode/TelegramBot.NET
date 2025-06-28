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
        /// <param name="removeReplyKeyboard">Whether to clear the reply keyboard after sending the message.</param>
        /// <returns>Result of the <see cref="IActionResult"/> action.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="text"/> is null or empty.</exception>
        public IActionResult Text(string text, bool removeReplyKeyboard = false)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentNullException(nameof(text));
            }
            return new TextResult(text, removeReplyKeyboard);
        }

        /// <summary>
        /// Edits a text message in the current update.
        /// </summary>
        /// <param name="text">Text message.</param>
        /// <param name="keyboard">Keyboard markup.</param>
        /// <returns>Result of the <see cref="IActionResult"/> action.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="text"/> is null or empty.</exception>
        public IActionResult TextEdit(string text, InlineKeyboardMarkup? keyboard = null)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentNullException(nameof(text));
            }
            bool hasMessageId = Update.TryGetMessageId(out int messageId);
            if (!hasMessageId)
            {
                throw new InvalidOperationException("Cannot edit message text without a message ID.");
            }
            return new TextEditResult(text, messageId, keyboard);
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
        /// Edits an inline keyboard in the current update.
        /// </summary>
        /// <param name="prompt">Text prompt.</param>
        /// <param name="keyboard">Keyboard markup.</param>
        /// <returns>Result of the <see cref="IActionResult"/> action.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="prompt"/> or <paramref name="keyboard"/> is null or empty.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="keyboard"/> does not contain any buttons.</exception>
        public IActionResult InlineEdit(string prompt, InlineKeyboardMarkup keyboard)
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
            bool hasMessageId = Update.TryGetMessageId(out int messageId);
            if (!hasMessageId)
            {
                throw new InvalidOperationException("Cannot edit inline message without a message ID.");
            }
            return new InlineEditResult(prompt, keyboard, messageId);
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
        /// Sends an image to the sender.
        /// </summary>
        /// <param name="stream">Stream with image content.</param>
        /// <param name="fileName">File name of the image.</param>
        /// <param name="caption">Caption under the image.</param>
        /// <param name="disposeStream">Dispose the stream after sending the image.</param>
        public IActionResult Image(Stream stream, string fileName, string caption = "", bool disposeStream = true)
        {
            return new ImageResult(stream, fileName, caption, disposeStream);
        }

        /// <summary>
        /// Deletes message from the current update, or does nothing if the message identifier is not found.
        /// <br/>
        /// Note: this method does not throw exceptions when the message ID is not found, access is denied, message is already deleted etc.
        /// </summary>
        public IActionResult DeleteMessage()
        {
            bool hasMessageId = Update.TryGetMessageId(out int messageId);
            if (!hasMessageId)
            {
                return Void();
            }
            return new DeleteMessageResult(messageId);
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

        /// <summary>
        /// Combines multiple actions into a single action result.
        /// </summary>
        /// <param name="actions">Array of actions to combine.</param>
        /// <returns>Combined action result.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="actions"/> is null or empty.</exception>
        public IActionResult MultiAction(params IActionResult[] actions)
        {
            if (actions == null || actions.Length == 0)
            {
                throw new ArgumentNullException(nameof(actions), "At least one action must be provided.");
            }
            return new MultiActionResult(actions);
        }
    }
}