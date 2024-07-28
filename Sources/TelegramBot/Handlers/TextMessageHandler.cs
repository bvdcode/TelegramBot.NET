using System;
using System.Reflection;
using Telegram.Bot.Types;
using TelegramBot.Attributes;
using System.Collections.Generic;

namespace TelegramBot.Handlers
{
    internal class TextMessageHandler : ITelegramUpdateHandler
    {
        private readonly List<object> _args;
        private readonly MethodInfo _methodInfo;

        public TextMessageHandler(IReadOnlyCollection<MethodInfo> controllerMethods, Update update)
        {
            _args = new List<object>();
            _methodInfo = GetMethodInfo(controllerMethods, update) ?? throw new InvalidOperationException("Method not found for message " + update.Message?.Text);
        }

        private MethodInfo? GetMethodInfo(IReadOnlyCollection<MethodInfo> controllerMethods, Update update)
        {
            if (update.Type != Telegram.Bot.Types.Enums.UpdateType.Message || update.Message == null)
            {
                return null;
            }
            var message = update.Message;
            var parts = message.Text!.Split(' ');
            string command = parts[0];

            foreach (var method in controllerMethods)
            {
                var attributes = method.GetCustomAttributes(typeof(TextCommandAttribute), false);
                foreach (var attribute in attributes)
                {
                    if (attribute is TextCommandAttribute botCommandAttribute)
                    {
                        if (botCommandAttribute.Command == command)
                        {
                            if (parts.Length == 2 && method.GetParameters().Length == 1)
                            {
                                _args.Add(parts[1]);
                            }
                            return method;
                        }
                    }
                }
            }
            return null;
        }


        public object[]? GetArguments()
        {
            return _args.Count > 0 ? _args.ToArray() : null;
        }

        public MethodInfo GetMethodInfo()
        {
            return _methodInfo;
        }
    }
}