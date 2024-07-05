using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Telegram.Bot;
using TelegramBot.Controllers;

namespace TelegramBot
{
    /// <summary>
    /// Telegram bot application.
    /// </summary>
    public class BotApp : IBot
    {
        private TelegramBotClient client;
        private ServiceProvider serviceProvider;
        private IReadOnlyCollection<Type> controllers;

        public BotApp(TelegramBotClient client, ServiceProvider serviceProvider)
        {
            this.client = client;
            this.controllers = new List<Type>();
            this.serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Maps controllers inherited from <see cref="BotControllerBase"/>.
        /// </summary>
        public IBot MapControllers()
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
            controllers = result;
            return this;
        }

        [Obsolete]
        /// <summary>
        /// Runs the bot.
        /// </summary>
        /// <param name="token">Cancellation token (optional).</param>
        public void Run(CancellationToken token = default)
        {

        }
    }
}