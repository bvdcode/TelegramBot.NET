using System;
using System.Reflection;
using Telegram.Bot.Types;
using TelegramBot.Attributes;
using System.Collections.Generic;
using TelegramBot.Helpers;

namespace TelegramBot.Handlers
{
    internal class InlineQueryHandler : ITelegramUpdateHandler
    {
        private readonly List<object> _args;
        private readonly MethodInfo _methodInfo;

        public InlineQueryHandler(IReadOnlyCollection<MethodInfo> controllerMethods, Update update)
        {
            _args = new List<object>();
            _methodInfo = GetMethodInfo(controllerMethods, update) ?? throw new InvalidOperationException("Method not found for query " + update.CallbackQuery?.Data);
        }

        private MethodInfo? GetMethodInfo(IReadOnlyCollection<MethodInfo> controllerMethods, Update update)
        {
            if (update.Type != Telegram.Bot.Types.Enums.UpdateType.CallbackQuery || update.CallbackQuery == null)
            {
                return null;
            }
            var inlineQuery = update.CallbackQuery;
            string command = inlineQuery.Data!;
            List<MethodInfo> methods = new List<MethodInfo>();
            foreach (var method in controllerMethods)
            {
                var attributes = method.GetCustomAttributes(typeof(InlineCommandAttribute), false);
                foreach (var attribute in attributes)
                {
                    if (attribute is InlineCommandAttribute botCommandAttribute)
                    {
                        string[] controllerCommandParts = botCommandAttribute.Command.Split('/');
                        string[] incomingCommandParts = command.Split('/');
                        if (controllerCommandParts.Length != incomingCommandParts.Length)
                        {
                            continue;
                        }
                        bool match = true;
                        for (int i = 0; i < controllerCommandParts.Length; i++)
                        {
                            if (controllerCommandParts[i] != incomingCommandParts[i] 
                                && !controllerCommandParts[i].StartsWith('{') 
                                && !controllerCommandParts[i].EndsWith('}'))
                            {
                                match = false;
                                break;
                            }
                            if (controllerCommandParts[i].StartsWith('{') 
                                && controllerCommandParts[i].EndsWith('}'))
                            {
                                _args.Add(incomingCommandParts[i]);
                            }
                        }
                        if (!match)
                        {
                            continue;
                        }
                        methods.Add(method);
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

        public MethodInfo GetMethodInfo()
        {
            return _methodInfo;
        }
    }
}
