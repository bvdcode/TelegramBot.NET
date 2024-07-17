using System;
using Telegram.Bot;
using System.Reflection;
using Telegram.Bot.Types;
using TelegramBot.Attributes;
using TelegramBot.Extensions;
using System.Threading.Tasks;
using TelegramBot.Controllers;
using TelegramBot.Abstractions;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace TelegramBot.Handlers
{
    internal class TextMessageHandler : ITelegramUpdateHandler
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly IReadOnlyCollection<MethodInfo> _controllerMethods;

        public TextMessageHandler(IReadOnlyCollection<MethodInfo> controllerMethods, ServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _controllerMethods = controllerMethods;
        }

        public async Task HandleAsync(Update update)
        {
            if (update.Type != Telegram.Bot.Types.Enums.UpdateType.Message || update.Message == null)
            {
                return;
            }
            bool hasUser = update.TryGetUser(out User user);
            var message = update.Message;
            string command = message.Text!.Split(' ')[0];

            foreach (var method in _controllerMethods)
            {
                var foundAttribute = GetAttribute(method, command);
                if (foundAttribute == null)
                {
                    continue;
                }
                if (!hasUser)
                {
                    return;
                }
                var controller = (BotControllerBase)ActivatorUtilities.CreateInstance(_serviceProvider, method.DeclaringType);
                controller.Update = update;
                controller.User = user;
                var result = method.Invoke(controller, new object[] { });
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

        private TextCommandAttribute? GetAttribute(MethodInfo method, string command)
        {
            var attributes = method.GetCustomAttributes(typeof(TextCommandAttribute), false);
            foreach (var attribute in attributes)
            {
                if (attribute is TextCommandAttribute botCommandAttribute)
                {
                    if (botCommandAttribute.Command == command)
                    {
                        return botCommandAttribute;
                    }
                }
            }
            return null;
        }
    }
}