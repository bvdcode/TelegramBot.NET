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
        /// A collection of logging providers for the application to compose. This is useful for adding new logging providers.
        /// </summary>
        public ILoggingBuilder Logging { get; }

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
            configuration.AddEnvironmentVariables(prefix: "ASPNETCORE_");

        }

        private IConfigurationRoot CreateConfigurationRoot()
        {
            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
        }

        public BotBuilder UseTelegramServer(Action<ITelegramServerBuilder> configure)
        {
            throw new NotImplementedException();
        }

        public IBot Build()
        {
            throw new NotImplementedException();
        }
    }
}