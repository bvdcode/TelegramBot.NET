using System.Linq;
using TelegramBot.Helpers;
using System.Collections.Generic;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot.Builders
{
    /// <summary>
    /// Creates an inline keyboard builder.
    /// </summary>
    public class KeyboardBuilder
    {
        private int rows = -1;
        private int columns = -1;
        private readonly List<InlineKeyboardButton> buttons = [];

        /// <summary>
        /// Adds a button to the keyboard.
        /// </summary>
        /// <param name="text">Button text.</param>
        /// <param name="payload">Button payload.</param>
        /// <returns>Instance of current <see cref="KeyboardBuilder"/>.</returns>
        public KeyboardBuilder AddButton(string text, string payload)
        {
            buttons.Add(InlineKeyboardButton.WithCallbackData(text, payload));
            return this;
        }

        /// <summary>
        /// Sets the number of columns in the keyboard.
        /// </summary>
        /// <param name="columns">Columns count.</param>
        /// <returns>Instance of current <see cref="KeyboardBuilder"/>.</returns>
        public KeyboardBuilder WithColumns(int columns)
        {
            ExceptionHelpers.ThrowIfNegativeOrZero(columns, nameof(columns));
            this.columns = columns;
            return this;
        }

        /// <summary>
        /// Sets the number of rows in the keyboard.
        /// </summary>
        /// <param name="rows">Rows count.</param>
        /// <returns>Instance of current <see cref="KeyboardBuilder"/>.</returns>
        public KeyboardBuilder WithRows(int rows)
        {
            ExceptionHelpers.ThrowIfNegativeOrZero(rows, nameof(rows));
            this.rows = rows;
            return this;
        }

        /// <summary>
        /// Builds the keyboard.
        /// </summary>
        /// <returns>Inlined keyboard markup instance.</returns>
        public InlineKeyboardMarkup Build()
        {
            if (columns == -1)
            {
                columns = 1;
            }
            if (rows == -1)
            {
                rows = buttons.Count;
            }

            List<List<InlineKeyboardButton>> keyboard = new List<List<InlineKeyboardButton>>();
            foreach (var row in Enumerable.Range(0, rows))
            {
                keyboard.Add(buttons.Skip(row * columns).Take(columns).ToList());
            }

            return new InlineKeyboardMarkup(keyboard);
        }
    }
}
