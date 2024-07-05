using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TelegramBot.Builders
{
    /// <summary>
    /// A builder for bot application and services.
    /// </summary>
    public class BotBuilder : IBotBuilder
    {
        /// <summary>
        /// A collection of services for the application to compose. This is useful for adding user provided or framework provided services.
        /// </summary>
        public IServiceCollection Services { get; }

        /// <summary>
        /// A collection of configuration providers for the application to compose. This is useful for adding new configuration sources and providers.
        /// </summary>
        public ConfigurationManager Configuration { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BotBuilder"/> class with preconfigured defaults.
        /// </summary>
        /// <returns>The <see cref="BotBuilder"/>.</returns>
        public BotBuilder() : this(Array.Empty<string>()) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BotBuilder"/> class with preconfigured defaults.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <returns>The <see cref="BotBuilder"/>.</returns>
        public BotBuilder(params string[] args)
        {
            Services = new ServiceCollection();
            var configuration = new ConfigurationManager();
            configuration.AddCommandLine(args);
            configuration.AddJsonFile("appsettings.json", optional: true);
            configuration.AddEnvironmentVariables();
            Configuration = configuration;
            Services.AddLogging(builder => builder.AddConsole());
        }

        private string _baseApiUrl = Constants.DefaultBaseApiUrl;

        /// <summary>
        /// Use the Telegram server with the specified base URL.
        /// </summary>
        /// <param name="configure">The configuration for the Telegram server.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Thrown when the base URL is not a valid URI.</exception>
        public BotBuilder UseTelegramServer(Action<TelegramServerBuilder> configure)
        {
            TelegramServerBuilder builder = new TelegramServerBuilder();
            configure(builder);
            if (!string.IsNullOrWhiteSpace(builder.BaseUrl))
            {
                bool parsed = Uri.TryCreate(builder.BaseUrl, UriKind.Absolute, out Uri? uri);
                if (!parsed)
                {
                    throw new ArgumentException("The base URL is not a valid URI.", nameof(builder.BaseUrl));
                }
                _baseApiUrl = builder.BaseUrl;
            }
            if (builder.UseConfiguration)
            {
                _baseApiUrl = Configuration["CustomTelegramApiUrl"]
                    ?? throw new ArgumentNullException("CustomTelegramApiUrl", "The custom Telegram API URL is not set in the configuration.");
            }
            return this;
        }

        public IBot Build()
        {
            throw new NotImplementedException();
        }
    }
}