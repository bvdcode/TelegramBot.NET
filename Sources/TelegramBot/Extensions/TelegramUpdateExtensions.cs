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

        /// <summary>
        /// Tries to get the user who sent the update.
        /// </summary>
        /// <param name="update">Telegram update.</param>
        /// <param name="user">The user who sent the update.</param>
        /// <returns>If the user is not null, returns true; otherwise, false.</returns>
        public static bool TryGetUser(this Update update, out User user)
        {
            if (update.Message != null && update.Message.From != null)
            {
                user = update.Message.From;
                return true;
            }
            else if (update.CallbackQuery != null && update.CallbackQuery.From != null)
            {
                user = update.CallbackQuery.From;
                return true;
            }
            else if (update.InlineQuery != null && update.InlineQuery.From != null)
            {
                user = update.InlineQuery.From;
                return true;
            }

            user = null!;
            return false;
        }

        /// <summary>
        /// Tries to get the message ID of the update.
        /// </summary>
        /// <param name="update">Telegram update.</param>
        /// <param name="messageId">Message identifier.</param>
        /// <returns>If the message identifier is not 0, returns true; otherwise, false.</returns>
        public static bool TryGetMessageId(this Update update, out int messageId)
        {
            if (update.Message != null)
            {
                messageId = update.Message.MessageId;
                return true;
            }
            else if (update.CallbackQuery != null && update.CallbackQuery.Message != null)
            {
                messageId = update.CallbackQuery.Message.MessageId;
                return true;
            }

            messageId = 0;
            return false;
        }
    }
}
