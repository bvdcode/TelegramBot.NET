using System.Reflection;
using Telegram.Bot.Types;
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
            if (update.Type != Telegram.Bot.Types.Enums.UpdateType.Message
                || update.Message == null
                || string.IsNullOrWhiteSpace(update.Message.Text))
            {
                return null;
            }
            string text = update.Message.Text;

            List<MethodInfo> methods = new List<MethodInfo>();
            foreach (var method in controllerMethods)
            {
                var attributes = method.GetCustomAttributes(typeof(TextQueryAttribute), false);
                foreach (var attribute in attributes)
                {
                    if (attribute is TextQueryAttribute botTextAttribute)
                    {
                        if (method.GetParameters().Length > 1)
                        {
                            continue;
                        }
                        if (method.GetParameters().Length == 1)
                        {
                            if (method.GetParameters()[0].ParameterType != typeof(string))
                            {
                                continue;
                            }
                        }
                        if (botTextAttribute.Regex == null)
                        {
                            methods.Add(method);
                        }
                        else if (botTextAttribute.Regex.IsMatch(text))
                        {
                            methods.Add(method);
                        }
                        else
                        {
                            continue;
                        }
                    }
                }
            }
            if (methods.Count == 1)
            {
                var method = methods[0];
                if (method.GetParameters().Length > 0)
                {
                    _args.Add(text);
                }
                return method;
            }
            else if (methods.Count > 1)
            {
                throw new AmbiguousMatchException("Multiple methods found for text: " + text);
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
