using System.Reflection;
using Telegram.Bot.Types;
using TelegramBot.Helpers;
using TelegramBot.Attributes;
using System.Collections.Generic;
using System.Linq;

namespace TelegramBot.Handlers
{
    internal class TextCommandHandler : ITelegramUpdateHandler
    {
        private readonly List<object> _args;
        private readonly MethodInfo? _methodInfo;

        public TextCommandHandler(IReadOnlyCollection<MethodInfo> controllerMethods, Update update)
        {
            _args = new List<object>();
            _methodInfo = GetMethodInfo(controllerMethods, update);
        }

        private MethodInfo? GetMethodInfo(IReadOnlyCollection<MethodInfo> controllerMethods, Update update)
        {
            if (!IsValidUpdate(update))
            {
                return null;
            }

            var parts = GetCommandParts(update.Message!.Text!);
            if (parts.Length == 0)
            {
                return null;
            }

            var command = parts[0];
            if (!IsCommandValid(command))
            {
                return null;
            }

            AddArguments(parts);

            var methods = FindMatchingMethods(controllerMethods, command);
            if (methods.Count == 1)
            {
                return methods[0];
            }
            else if (methods.Count > 1)
            {
                throw new AmbiguousMatchException("Multiple methods found with the same command and arguments: " + command);
            }

            return null;
        }

        private bool IsValidUpdate(Update update)
        {
            return update.Type == Telegram.Bot.Types.Enums.UpdateType.Message
                && update.Message != null
                && !string.IsNullOrWhiteSpace(update.Message.Text);
        }

        private string[] GetCommandParts(string text)
        {
            var parts = text.Split(' ');
            var arguments = new List<string>();
            var currentArgument = string.Empty;

            foreach (var part in parts)
            {
                if (part.StartsWith('"') && part.EndsWith('"'))
                {
                    arguments.Add(part);
                }
                else if (part.StartsWith('"'))
                {
                    currentArgument = part;
                }
                else if (part.EndsWith('"'))
                {
                    currentArgument += " " + part;
                    arguments.Add(currentArgument);
                    currentArgument = string.Empty;
                }
                else if (!string.IsNullOrEmpty(currentArgument))
                {
                    currentArgument += " " + part;
                }
                else
                {
                    arguments.Add(part);
                }
            }

            return arguments
                .Select(p => p.Trim('"'))
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .ToArray();
        }

        private bool IsCommandValid(string command)
        {
            return command.StartsWith('/');
        }

        private void AddArguments(string[] parts)
        {
            if (parts.Length > 1)
            {
                _args.AddRange(parts[1..]);
            }
        }

        private List<MethodInfo> FindMatchingMethods(IReadOnlyCollection<MethodInfo> controllerMethods, string command)
        {
            var methods = new List<MethodInfo>();

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
                                var args = _args.ToArray();
                                if (ObjectHelpers.TryConvertParameters(method, args))
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
            }

            return methods;
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