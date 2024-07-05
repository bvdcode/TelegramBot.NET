using System;
using Microsoft.Extensions.DependencyInjection;

namespace TelegramBot.Builders
{
    /// <summary>
    /// The <see cref="BotBuilder"/> class is used to build a Telegram Bot instance.
    /// </summary>
    public class BotBuilder : IBotBuilder
    {
        public IServiceCollection Services { get; }

        public BotBuilder() : this(Array.Empty<string>()) { }

        public BotBuilder(params string[] args)
        {
            Services = new ServiceCollection();
            AddDefaultServices();
        }

        public IBot Build()
        {
            throw new NotImplementedException();
        }

        private void AddDefaultServices()
        {
            throw new NotImplementedException();
        }
    }
}