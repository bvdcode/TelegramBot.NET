using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBot.Abstractions;
using TelegramBot.Attributes;
using TelegramBot.Controllers;
using TelegramBot.Extensions;

namespace TelegramBot.Handlers
{
    internal class TextMessageHandler : ITelegramUpdateHandler
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly IReadOnlyCollection<Type> _controllers;

        public TextMessageHandler(IReadOnlyCollection<Type> controllers, ServiceProvider serviceProvider)
        {
            _controllers = controllers;
            _serviceProvider = serviceProvider;
        }

        public async Task HandleAsync(Update update)
        {
            if (update.Type != Telegram.Bot.Types.Enums.UpdateType.Message || update.Message == null)
            {
                return;
            }
            var message = update.Message;
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
                                    return;
                                }
                                controller.User = user;
                                var result = method.Invoke(controller, new object[] { });
                                ITelegramBotClient client = _serviceProvider.GetRequiredService<ITelegramBotClient>();
                                if (result is Task<IActionResult> taskResult)
                                {
                                    await(await taskResult).ExecuteResultAsync(new ActionContext(client, user.Id));
                                    return;
                                }
                                else if (result is IActionResult actionResult)
                                {
                                    await actionResult.ExecuteResultAsync(new ActionContext(client, user.Id));
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