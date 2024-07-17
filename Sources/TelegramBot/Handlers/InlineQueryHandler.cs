using System;
using Telegram.Bot;
using Telegram.Bot.Types;
using System.Threading.Tasks;
using TelegramBot.Attributes;
using TelegramBot.Extensions;
using TelegramBot.Controllers;
using TelegramBot.Abstractions;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace TelegramBot.Handlers
{
    internal class InlineQueryHandler : ITelegramUpdateHandler
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly IReadOnlyCollection<Type> _controllers;

        public InlineQueryHandler(IReadOnlyCollection<Type> controllers, ServiceProvider serviceProvider)
        {
            _controllers = controllers;
            _serviceProvider = serviceProvider;
        }

        public async Task HandleAsync(Update update)
        {
            if (update.Type != Telegram.Bot.Types.Enums.UpdateType.CallbackQuery || update.CallbackQuery == null)
            {
                return;
            }
            var inlineQuery = update.CallbackQuery;
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
                                    return;
                                }
                                controller.User = user;
                                var result = method.Invoke(controller, args.ToArray());
                                ITelegramBotClient client = _serviceProvider.GetRequiredService<ITelegramBotClient>();
                                if (result is Task<IActionResult> taskResult)
                                {
                                    await (await taskResult).ExecuteResultAsync(new ActionContext(client, user.Id));
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
