using System;
using System.Linq;
using Telegram.Bot.Types;
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
        /// <summary>
        /// User who sent the update.
        /// </summary>
        public User User { get; set; } = null!;

        /// <summary>
        /// Update received from the user.
        /// </summary>
        public Update Update { get; set; } = null!;

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
        /// <returns>Result of the <see cref="IActionResult"/> action.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="prompt"/> or <paramref name="keyboard"/> is null or empty.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="keyboard"/> does not contain any buttons.</exception>
        public IActionResult Inline(string prompt, InlineKeyboardMarkup keyboard)
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
            return new InlineResult(prompt, keyboard);
        }
    }
}