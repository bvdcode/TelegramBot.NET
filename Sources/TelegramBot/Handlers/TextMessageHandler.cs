using System.Reflection;
using Telegram.Bot.Types;
using TelegramBot.Helpers;
using TelegramBot.Attributes;
using System.Collections.Generic;

namespace TelegramBot.Handlers
{
    internal class TextMessageHandler : ITelegramUpdateHandler
    {
        private readonly List<object> _args;
        private readonly MethodInfo? _methodInfo;

        public TextMessageHandler(IReadOnlyCollection<MethodInfo> controllerMethods, Update update)
        {
            _args = new List<object>();
            _methodInfo = GetMethodInfo(controllerMethods, update);
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
            if (!command.StartsWith("/"))
            {
                return null;
            }
            if (parts.Length > 1)
            {
                _args.AddRange(parts[1..]);
            }

            List<MethodInfo> methods = new List<MethodInfo>();
            foreach (var method in controllerMethods)
            {
                var attributes = method.GetCustomAttributes(typeof(TextCommandAttribute), false);
                foreach (var attribute in attributes)
                {
                    if (attribute is TextCommandAttribute botCommandAttribute)
                    {
                        if (botCommandAttribute.Command == command)
                        {
                            if (_args.Count == method.GetParameters().Length)
                            {
                                bool converted = ObjectHelpers.TryConvertParameters(method, _args.ToArray());
                                if (converted)
                                {
                                    methods.Add(method);
                                }
                            }
                        }
                    }
                }
            }
            if (methods.Count == 1)
            {
                var args = _args.ToArray();
                ObjectHelpers.TryConvertParameters(methods[0], args);
                _args.Clear();
                _args.AddRange(args);
                return methods[0];
            }
            else if (methods.Count > 1)
            {
                throw new AmbiguousMatchException("Multiple methods found with the same command and arguments: " + command);
            }
            return null;
        }


        public object[]? GetArguments()
        {
            return _args.Count > 0 ? _args.ToArray() : null;
        }

        public MethodInfo? GetMethodInfo()
        {
            return _methodInfo;
        }
    }
}