using System;
using Telegram.Bot;
using System.Threading;
using System.Reflection;
using Telegram.Bot.Types;
using System.Threading.Tasks;
using TelegramBot.Controllers;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using TelegramBot.Attributes;
using TelegramBot.Abstractions;

namespace TelegramBot
{
    /// <summary>
    /// Telegram bot application.
    /// </summary>
    public class BotApp : IBot
    {
        private readonly ILogger<BotApp> _logger;
        private readonly TelegramBotClient _client;
        private IReadOnlyCollection<Type> _controllers;
        private readonly ServiceProvider _serviceProvider;

        /// <summary>
        /// Creates a new instance of <see cref="BotApp"/>.
        /// </summary>
        /// <param name="client">Telegram bot client.</param>
        /// <param name="serviceProvider">Service provider.</param>
        public BotApp(TelegramBotClient client, ServiceProvider serviceProvider)
        {
            _client = client;
            _controllers = new List<Type>();
            _serviceProvider = serviceProvider;
            _logger = serviceProvider.GetRequiredService<ILogger<BotApp>>();
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
            _controllers = result;
            return this;
        }

        /// <summary>
        /// Runs the bot.
        /// </summary>
        /// <param name="token">Cancellation token (optional).</param>
        public void Run(CancellationToken token = default)
        {
            var botUser = _client.GetMeAsync().Result;
            _logger.BeginScope("Bot user: {BotUser}", botUser.Username);            
            _client.StartReceiving(UpdateHandler, ErrorHandler, cancellationToken: token);
            _logger.LogInformation("Bot started - receiving updates.");
            Task.Delay(-1, token).Wait(token);
        }

        private Task ErrorHandler(ITelegramBotClient client, Exception exception, CancellationToken token)
        {
            _logger.LogError(exception, "Error occurred while receiving updates.");
            return Task.CompletedTask;
        }

        private async Task UpdateHandler(ITelegramBotClient client, Update update, CancellationToken token)
        {
            if (update.Message != null && !string.IsNullOrWhiteSpace(update.Message.Text) && update.Message.Text.StartsWith('/'))
            {
                string command = update.Message.Text.Split(' ')[0];
                foreach (var controllerType in _controllers)
                {
                    var controller = (BotControllerBase)ActivatorUtilities.CreateInstance(_serviceProvider, controllerType);
                    var methods = controllerType.GetMethods();
                    foreach (var method in methods)
                    {
                        var attributes = method.GetCustomAttributes(typeof(BotCommandAttribute), false);
                        foreach (var attribute in attributes)
                        {
                            if (attribute is BotCommandAttribute botCommandAttribute)
                            {
                                if (botCommandAttribute.Command == command)
                                {
                                    var result = (IActionResult)method.Invoke(controller, new object[] { update });
                                    await result.ExecuteResultAsync(new ActionContext(_client, update.Message.Chat.Id));
                                    return;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}