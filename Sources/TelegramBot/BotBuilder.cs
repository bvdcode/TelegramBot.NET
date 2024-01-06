using Autofac;
using System;
using System.Collections.Generic;
using System.Text;

namespace TelegramBot
{
    /// <summary>
    /// The <see cref="BotBuilder"/> class is used to build a Telegram Bot instance.
    /// </summary>
    public class BotBuilder : IBotBuilder
    {
        private readonly string _apiKey;
        private string? _customTelegramApiUrl;
        private readonly ContainerBuilder _containerBuilder;

        /// <summary>
        /// Creates a new instance of the <see cref="BotBuilder"/> class.
        /// </summary>
        /// <param name="apiKey">The API key of the Telegram Bot.</param>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="apiKey"/> is null or empty.</exception>
        public BotBuilder(string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new ArgumentException("API key cannot be null or empty.", nameof(apiKey));
            }
            _apiKey = apiKey;
            _containerBuilder = new ContainerBuilder();
        }

        /// <summary>
        /// Use a custom Telegram API URL.
        /// </summary>
        /// <param name="customTelegramApiUrl">The custom Telegram API URL.</param>
        /// <returns>The <see cref="IBotBuilder"/> instance.</returns>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="customTelegramApiUrl"/> is null or empty.</exception>
        public IBotBuilder UseCustomTelegramApiUrl(string customTelegramApiUrl)
        {
            if (string.IsNullOrWhiteSpace(customTelegramApiUrl))
            {
                throw new ArgumentException("Custom Telegram API URL cannot be null or empty.", nameof(customTelegramApiUrl));
            }
            Uri uri = new Uri(customTelegramApiUrl);
            _customTelegramApiUrl = uri.ToString();
            return this;
        }

        /// <summary>
        /// Add a scoped service to the IoC container.
        /// </summary>
        /// <typeparam name="TService">Service type.</typeparam>
        /// <typeparam name="TImplementation">Implementation type.</typeparam>
        /// <returns>The <see cref="IBotBuilder"/> instance.</returns>
        public IBotBuilder AddScoped<TService, TImplementation>()
            where TImplementation : TService
            where TService : class
        {
            _containerBuilder
                .RegisterType<TImplementation>()
                .As<TService>()
                .InstancePerLifetimeScope();
            return this;
        }

        /// <summary>
        /// Add a singleton service to the IoC container.
        /// </summary>
        /// <typeparam name="TService">Service type.</typeparam>
        /// <typeparam name="TImplementation">Implementation type.</typeparam>
        /// <returns>The <see cref="IBotBuilder"/> instance.</returns>
        public IBotBuilder AddSingleton<TService, TImplementation>()
            where TImplementation : TService
            where TService : class
        {
            _containerBuilder
                .RegisterType<TImplementation>()
                .As<TService>()
                .SingleInstance();
            return this;
        }

        /// <summary>
        /// Build the Telegram Bot instance.
        /// </summary>
        /// <returns>The Telegram Bot instance.</returns>
        public IBot Build()
        {
            var container = _containerBuilder.Build();
            return new Bot(_apiKey, _customTelegramApiUrl, container);
        }
    }
}