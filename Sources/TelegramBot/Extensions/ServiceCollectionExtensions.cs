using System;
using System.Linq;
using System.Reflection;
using TelegramBot.Builders;
using TelegramBot.Attributes;
using TelegramBot.Containers;
using TelegramBot.Controllers;
using TelegramBot.Abstractions;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace TelegramBot.Extensions
{
    /// <summary>
    /// Provides extension methods for the IServiceCollection interface.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the bot controllers in the service collection. <br/>
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The modified service collection with bot controllers registered.</returns>
        public static IServiceCollection AddBotControllers(this IServiceCollection services)
        {
            var types = Assembly.GetCallingAssembly().GetTypes();
            List<Type> result = new List<Type>();
            foreach (var type in types)
            {
                if (type.IsSubclassOf(typeof(BotControllerBase)))
                {
                    result.Add(type);
                }
            }
            var controllerMethods = result
                .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                .ToList();
            BotControllerMethodsContainer container = new BotControllerMethodsContainer();
            foreach (var method in controllerMethods)
            {
                container.AddMethod(method);
            }
            return services.AddSingleton(container);
        }

        /// <summary>
        /// Register commands for the bot. This method only registers the commands in Telegram UI. <br/>
        /// Controllers should be registered separately with <see cref="AddBotControllers(IServiceCollection)"/> <br/>
        /// You can use commands even without this method.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="setup">The setup for the command registration.</param>
        /// <param name="language">The language for the commands, default is English.</param>
        /// <returns>This instance of <see cref="BotBuilder"/>.</returns>
        public static IServiceCollection RegisterCommands(this IServiceCollection services, Action<CommandRegistrationBuilder> setup, string language = "en")
        {
            CommandRegistrationBuilder builder = new CommandRegistrationBuilder(language);
            setup(builder);
            return services.AddSingleton(builder);
        }

        /// <summary>
        /// Use the authorization predicate for methods and controllers with <see cref="AuthorizeAttribute"/>
        /// </summary>
        /// <typeparam name="THandler">The type of the authorization handler.</typeparam>
        /// <param name="services">The service collection.</param>
        /// <returns>The modified service collection.</returns>
        public static IServiceCollection UseAuthorizationHandler<THandler>(this IServiceCollection services) where THandler : class, IBotAuthorizationHandler
        {
            return services.AddSingleton<IBotAuthorizationHandler, THandler>();
        }

        /// <summary>
        /// Registers the specified <see cref="IKeyValueProvider"/> instance as a singleton service in the service collection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="provider">The <see cref="IKeyValueProvider"/> instance to register.</param>
        /// <returns>The modified service collection.</returns>
        public static IServiceCollection UseKeyValueProvider(this IServiceCollection services, IKeyValueProvider provider)
        {
            return services.AddSingleton(provider);
        }

        /// <summary>
        /// Registers a factory function that creates an <see cref="IKeyValueProvider"/> instance as a singleton service in the service collection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="providerFactory">The factory function that creates an <see cref="IKeyValueProvider"/> instance.</param>
        /// <returns>The modified service collection.</returns>
        public static IServiceCollection UseKeyValueProvider(this IServiceCollection services, Func<IServiceProvider, IKeyValueProvider> providerFactory)
        {
            services.AddSingleton(providerFactory);
            return services;
        }

        /// <summary>
        /// Registers the specified <typeparamref name="TImplementation"/> type as a singleton service that implements the <see cref="IKeyValueProvider"/> interface in the service collection.
        /// </summary>
        /// <typeparam name="TImplementation">The type of the implementation.</typeparam>
        /// <param name="services">The service collection.</param>
        /// <returns>The modified service collection.</returns>
        public static IServiceCollection UseKeyValueProvider<TImplementation>(this IServiceCollection services) where TImplementation : class, IKeyValueProvider
        {
            return services.AddSingleton<IKeyValueProvider, TImplementation>();
        }
    }
}
