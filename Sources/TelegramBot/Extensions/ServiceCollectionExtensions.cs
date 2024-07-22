using System;
using TelegramBot.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace TelegramBot.Extensions
{
    /// <summary>
    /// Provides extension methods for the IServiceCollection interface.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the specified <see cref="IKeyValueProvider"/> instance as a singleton service in the service collection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="provider">The <see cref="IKeyValueProvider"/> instance to register.</param>
        /// <returns>The modified service collection.</returns>
        public static IServiceCollection UseKeyValueProvider(this IServiceCollection services, IKeyValueProvider provider)
        {
            services.AddSingleton(provider);
            return services;
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
            services.AddSingleton<IKeyValueProvider, TImplementation>();
            return services;
        }
    }
}
