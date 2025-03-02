using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace TelegramBot.Builders
{
    internal class TelegramLoggerBuilder : ILoggingBuilder
    {
        public IServiceCollection Services { get; }

        internal TelegramLoggerBuilder(IServiceCollection _services)
        {
            Services = _services ?? throw new ArgumentNullException(nameof(_services));
        }
    }
}