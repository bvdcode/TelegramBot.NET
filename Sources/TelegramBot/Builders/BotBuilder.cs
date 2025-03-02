﻿using System;
using System.Linq;
using Telegram.Bot;
using TelegramBot.Services;
using TelegramBot.Providers;
using TelegramBot.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TelegramBot.Builders
{
    /// <summary>
    /// A builder for bot application and services.
    /// </summary>
    public class BotBuilder
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
            configuration.AddCommandLine(args);
            configuration.AddJsonFile("appsettings.json", optional: true);
            configuration.AddEnvironmentVariables();
            Configuration = configuration;
            Services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.AddConfiguration(Configuration.GetSection("Logging"));
            });
            Logging = new TelegramLoggerBuilder(Services);
            Services.AddSingleton(Configuration);
            Services.AddSingleton<IConfiguration>(Configuration);
            Services.AddSingleton<IConfigurationRoot>(Configuration);
        }

        private readonly BotConfiguration _botConfiguration = new BotConfiguration();
        private string _baseApiUrl = Constants.DefaultBaseApiUrl;
        private string? _token;

        /// <summary>
        /// Use the Telegram server with the specified base URL.
        /// </summary>
        /// <param name="configure">The configuration for the Telegram server.</param>
        /// <returns>This instance of <see cref="BotBuilder"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when the base URL is not a valid URI.</exception>
        public BotBuilder UseTelegramServer(Action<TelegramServerBuilder> configure)
        {
            TelegramServerBuilder builder = new TelegramServerBuilder();
            configure(builder);
            if (!string.IsNullOrWhiteSpace(builder.BaseUrl))
            {
                bool parsed = Uri.TryCreate(builder.BaseUrl, UriKind.Absolute, out Uri? _);
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

        /// <summary>
        /// Use the API key for the Telegram bot.
        /// </summary>
        /// <param name="setupApiKey">The configuration for the Telegram API key.</param>
        /// <returns>This instance of <see cref="BotBuilder"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the Telegram bot token is not set in the configuration.</exception>
        public BotBuilder UseApiKey(Action<TelegramApiKeyBuilder> setupApiKey)
        {
            TelegramApiKeyBuilder builder = new TelegramApiKeyBuilder();
            setupApiKey(builder);
            if (!string.IsNullOrWhiteSpace(builder.ApiKey))
            {
                _token = builder.ApiKey;
            }
            if (builder.UseConfiguration)
            {
                _token = Configuration["TelegramBotToken"]
                    ?? throw new ArgumentNullException("TelegramBotToken", "The Telegram bot token is not set in the configuration.");
            }
            return this;
        }

        /// <summary>
        /// Use custom services for the bot additionally to the built-in services.
        /// </summary>
        /// <param name="services">The services to use.</param>
        /// <returns>This instance of <see cref="BotBuilder"/>.</returns>
        public BotBuilder UseServices(IServiceCollection services)
        {
            foreach (var service in services)
            {
                Services.Add(service);
            }
            return this;
        }

        /// <summary>
        /// Setup the bot parameters.
        /// </summary>
        /// <param name="setup">The configuration for the bot.</param>
        /// <returns>This instance of <see cref="BotBuilder"/>.</returns>
        public BotBuilder Setup(Action<BotConfiguration> setup)
        {
            setup(_botConfiguration);
            return this;
        }

        /// <summary>
        /// Build the bot.
        /// </summary>
        /// <returns>Built bot.</returns>
        public IBot Build()
        {
            if (string.IsNullOrWhiteSpace(_token))
            {
                throw new ArgumentNullException("TelegramBotToken", "The Telegram bot token is not set.");
            }
            TelegramBotClientOptions options = new TelegramBotClientOptions(_token, _baseApiUrl);
            TelegramBotClient client = new TelegramBotClient(options);
            Services.AddSingleton<ITelegramBotClient>(client);
            bool hasKeyValueProvider = Services.Any(service => service.ServiceType == typeof(IKeyValueProvider));
            if (!hasKeyValueProvider)
            {
                Services.AddSingleton<IKeyValueProvider, InMemoryKeyValueProvider>();
            }
            Services.AddSingleton<IHostApplicationLifetime, HostApplicationLifetime>();
            return new BotApp(client, Services.BuildServiceProvider(), _botConfiguration);
        }
    }
}