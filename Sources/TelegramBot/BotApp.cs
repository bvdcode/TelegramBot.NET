using System;
using Telegram.Bot;
using System.Threading;
using System.Reflection;
using Telegram.Bot.Types;
using TelegramBot.Extensions;
using TelegramBot.Attributes;
using System.Threading.Tasks;
using TelegramBot.Controllers;
using TelegramBot.Abstractions;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

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

        [Obsolete("Not implemented yet.")]
        private async Task UpdateHandler(ITelegramBotClient client, Update update, CancellationToken token)
        {
            if (update.Message != null && !string.IsNullOrWhiteSpace(update.Message.Text) && update.Message.Text.StartsWith('/'))
            {
                _logger.LogInformation("Received text message: {Text}.", update.Message.Text);
                await HandleTextMessageAsync(update, update.Message);
            }
            else if (update.CallbackQuery != null && update.CallbackQuery.Data != null)
            {
                _logger.LogInformation("Received inline query: {Data}.", update.CallbackQuery.Data);
                await HandleInlineQueryAsync(update, update.CallbackQuery);
            }
            else
            {
                _logger.LogWarning("Unsupported update type: {UpdateType}.", update.Type);
            }
        }

        private async Task HandleInlineQueryAsync(Update update, CallbackQuery inlineQuery)
        {
            // /language/{language}
            // /language/{language}/framework/{framework}

            string command = inlineQuery.Data!;
            foreach (var controllerType in _controllers)
            {
                var controller = (BotControllerBase)ActivatorUtilities.CreateInstance(_serviceProvider, controllerType);
                var methods = controllerType.GetMethods();
                foreach (var method in methods)
                {
                    var attributes = method.GetCustomAttributes(typeof(InlineCommandAttribute), false);
                    foreach (var attribute in attributes)
                    {
                        if (attribute is InlineCommandAttribute botCommandAttribute)
                        {
                            // controller command: /language/{language}
                            // incoming command: /language/en

                            List<object> args = new List<object>();
                            string[] controllerCommandParts = botCommandAttribute.Command.Split('/');
                            string[] incomingCommandParts = command.Split('/');
                            if (controllerCommandParts.Length != incomingCommandParts.Length)
                            {
                                continue;
                            }
                            bool match = true;
                            for (int i = 0; i < controllerCommandParts.Length; i++)
                            {
                                if (controllerCommandParts[i] != incomingCommandParts[i] && !controllerCommandParts[i].StartsWith("{") && !controllerCommandParts[i].EndsWith("}"))
                                {
                                    match = false;
                                    break;
                                }
                                if (controllerCommandParts[i].StartsWith("{") && controllerCommandParts[i].EndsWith("}"))
                                {
                                    args.Add(incomingCommandParts[i]);
                                }
                            }
                            if (match)
                            {
                                controller.Update = update;
                                bool hasUser = update.TryGetUser(out User user);
                                if (!hasUser)
                                {
                                    _logger.LogWarning("User not found in the update {id}.", update.Id);
                                    return;
                                }
                                controller.User = user;
                                var result = method.Invoke(controller, args.ToArray());
                                if (result is Task<IActionResult> taskResult)
                                {
                                    await (await taskResult).ExecuteResultAsync(new ActionContext(_client, user.Id));
                                    return;
                                }
                                else if (result is IActionResult actionResult)
                                {
                                    await actionResult.ExecuteResultAsync(new ActionContext(_client, user.Id));
                                    return;
                                }
                                else
                                {
                                    throw new InvalidOperationException("Invalid result type: " + result.GetType().Name);
                                }
                            }
                        }
                    }
                }
            }

        }

        private async Task HandleTextMessageAsync(Update update, Message message)
        {
            string command = message.Text!.Split(' ')[0];
            foreach (var controllerType in _controllers)
            {
                var controller = (BotControllerBase)ActivatorUtilities.CreateInstance(_serviceProvider, controllerType);
                var methods = controllerType.GetMethods();
                foreach (var method in methods)
                {
                    var attributes = method.GetCustomAttributes(typeof(TextCommandAttribute), false);
                    foreach (var attribute in attributes)
                    {
                        if (attribute is TextCommandAttribute botCommandAttribute)
                        {
                            if (botCommandAttribute.Command == command)
                            {
                                controller.Update = update;
                                bool hasUser = update.TryGetUser(out User user);
                                if (!hasUser)
                                {
                                    _logger.LogWarning("User not found in the update {id}.", update.Id);
                                    return;
                                }
                                controller.User = user;
                                var result = method.Invoke(controller, new object[] { });
                                if (result is Task<IActionResult> taskResult)
                                {
                                    await (await taskResult).ExecuteResultAsync(new ActionContext(_client, user.Id));
                                    return;
                                }
                                else if (result is IActionResult actionResult)
                                {
                                    await actionResult.ExecuteResultAsync(new ActionContext(_client, user.Id));
                                    return;
                                }
                                else
                                {
                                    throw new InvalidOperationException("Invalid result type: " + result.GetType().Name);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}