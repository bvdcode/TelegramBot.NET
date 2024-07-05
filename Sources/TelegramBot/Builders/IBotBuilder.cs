using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace TelegramBot.Builders
{
    /// <summary>
    /// Builder for the Telegram Bot.
    /// </summary>
    public interface IBotBuilder
    {
        /// <summary>
        /// A collection of services for the application to compose. This is useful for adding user provided or framework provided services.
        /// </summary>
        IServiceCollection Services { get; }

        /// <summary>
        /// A collection of configuration providers for the application to compose. This is useful for adding new configuration sources and providers.
        /// </summary>
        ConfigurationManager Configuration { get; }

        /// <summary>
        /// Build the bot.
        /// </summary>
        /// <returns>Built bot.</returns>
        IBot Build();

        /// <summary>
        /// Use the Telegram server with the specified base URL.
        /// </summary>
        /// <param name="configure">The configuration for the Telegram server.</param>
        /// <returns>This instance of <see cref="IBotBuilder"/>.</returns>
        BotBuilder UseTelegramServer(Action<TelegramServerBuilder> configure);
    }
}