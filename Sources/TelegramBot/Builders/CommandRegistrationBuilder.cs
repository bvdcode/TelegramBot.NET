using System;
using System.Linq;
using Telegram.Bot.Types;
using System.Collections.Generic;

namespace TelegramBot.Builders
{
    /// <summary>
    /// Command registration builder.
    /// </summary>
    public class CommandRegistrationBuilder
    {
        internal string Language { get; set; } = string.Empty;
        private readonly List<BotCommand> botCommands = new List<BotCommand>();

        /// <summary>
        /// Creates a new instance of the <see cref="CommandRegistrationBuilder"/> class.
        /// </summary>
        /// <param name="language">Language code of the commands.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="language"/> is null or empty.</exception>
        public CommandRegistrationBuilder(string language)
        {
            if (string.IsNullOrWhiteSpace(language))
            {
                throw new ArgumentNullException(nameof(language));
            }
            Language = language;
        }

        /// <summary>
        /// Registers a command with a description.
        /// </summary>
        /// <param name="command">Text of the command, 1-32 characters. Can contain only lowercase English letters, digits and underscores.</param>
        /// <param name="description">Description of the command, 3-256 characters.</param>
        /// <returns>The <see cref="CommandRegistrationBuilder"/>.</returns>
        public CommandRegistrationBuilder RegisterCommand(string command, string description)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                throw new ArgumentNullException(nameof(command));
            }
            if (string.IsNullOrWhiteSpace(description))
            {
                throw new ArgumentNullException(nameof(description));
            }
            while (command.StartsWith('/'))
            {
                command = command[1..];
            }
            if (command.Length < 1 || command.Length > 32)
            {
                throw new ArgumentOutOfRangeException(nameof(command), "Command must be 1-32 characters long.");
            }
            if (!command.All(char.IsLower) && !command.All(char.IsDigit) && !command.All(char.IsLetter))
            {
                throw new ArgumentException("Command can contain only lowercase English letters, digits and underscores.", nameof(command));
            }
            var botCommand = new BotCommand()
            {
                Command = command,
                Description = description
            };
            botCommands.Add(botCommand);
            return this;
        }

        /// <summary>
        /// Builds the collection of bot commands.
        /// </summary>
        /// <returns>The collection of bot commands.</returns>
        public IEnumerable<BotCommand> Build()
        {
            return botCommands;
        }
    }
}