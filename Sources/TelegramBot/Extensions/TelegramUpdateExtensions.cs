using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot.Types;

namespace TelegramBot.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="Update"/>.
    /// </summary>
    public static class TelegramUpdateExtensions
    {
        /// <summary>
        /// Determines whether the update is a text message.
        /// </summary>
        /// <param name="update">Telegram update.</param>
        /// <returns>If the update is a text message, returns true; otherwise, false.</returns>
        public static bool IsTextMessage(this Update update)
        {
            return update.Message?.Text != null;
        }

        /// <summary>
        /// Determines whether the update is an inline query.
        /// </summary>
        /// <param name="update">Telegram update.</param>
        /// <returns>If the update is an inline query, returns true; otherwise, false.</returns>
        public static bool IsInlineQuery(this Update update)
        {
            return update.InlineQuery != null;
        }
    }
}
