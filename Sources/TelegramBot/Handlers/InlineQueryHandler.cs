using System;
using System.Linq;
using System.Reflection;
using Telegram.Bot.Types;
using TelegramBot.Helpers;
using TelegramBot.Attributes;
using System.Collections.Generic;

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

            // Collect all candidate methods with their parsed arguments
            var candidates = new List<(MethodInfo Method, List<object> Args)>();
            foreach (var method in controllerMethods)
            {
                if (TryMatchMethod(method, command, out var args))
                {
                    candidates.Add((method, args));
                }
            }

            if (candidates.Count == 1)
            {
                var (method, rawArgs) = candidates[0];
                var argsArray = rawArgs.ToArray();
                ObjectHelpers.TryConvertParameters(method, argsArray);
                _args.Clear();
                _args.AddRange(argsArray);
                return method;
            }

            if (candidates.Count > 1)
            {
                throw new AmbiguousMatchException("Multiple methods found with the same command and arguments: " + command + "\n" +
                    string.Join("\n", candidates.Select(c => c.Method.Name + " - " + c.Method.GetParameters().Length + " parameters")));
            }

            return null;
        }

        private static bool IsPlaceholder(string segment)
        {
            return segment.Length > 1 && segment[0] == '{' && segment[segment.Length - 1] == '}';
        }

        private static string PlaceholderName(string segment)
        {
            return segment.Trim('{', '}');
        }

        private bool TryMatchMethod(MethodInfo method, string command, out List<object> args)
        {
            args = new List<object>();

            var attributes = method.GetCustomAttributes(typeof(InlineCommandAttribute), inherit: false);
            if (attributes.Length == 0)
            {
                return false;
            }

            var incomingCommandParts = command.Split('/');
            var methodParameters = method.GetParameters();

            foreach (var attribute in attributes)
            {
                if (!(attribute is InlineCommandAttribute botAttribute))
                {
                    continue;
                }

                var controllerCommandParts = botAttribute.Command.Split('/');

                // Exact match shortcut for methods without parameters
                if (methodParameters.Length == 0)
                {
                    if (string.Equals(botAttribute.Command, command, StringComparison.Ordinal))
                    {
                        return true;
                    }
                    continue;
                }

                if (controllerCommandParts.Length != incomingCommandParts.Length)
                {
                    continue;
                }

                var tempArgs = new List<object>(incomingCommandParts.Length);
                bool match = true;

                for (int i = 0; i < controllerCommandParts.Length; i++)
                {
                    var controllerPart = controllerCommandParts[i];
                    var incomingPart = incomingCommandParts[i];

                    if (IsPlaceholder(controllerPart))
                    {
                        var name = PlaceholderName(controllerPart);
                        var parameter = methodParameters.FirstOrDefault(p => p.Name == name);
                        if (parameter != null)
                        {
                            if (!TryConvertToType(incomingPart, parameter.ParameterType))
                            {
                                match = false;
                                break;
                            }
                        }
                        tempArgs.Add(incomingPart);
                    }
                    else if (!string.Equals(controllerPart, incomingPart, StringComparison.Ordinal))
                    {
                        match = false;
                        break;
                    }
                }

                if (match)
                {
                    args = tempArgs;
                    return true;
                }
            }

            return false;
        }

        private bool TryConvertToType(string value, Type targetType)
        {
            try
            {
                if (targetType == typeof(int))
                {
                    return int.TryParse(value, out _);
                }
                if (targetType == typeof(long))
                {
                    return long.TryParse(value, out _);
                }
                if (targetType == typeof(double))
                {
                    return double.TryParse(value, out _);
                }
                if (targetType == typeof(bool))
                {
                    return bool.TryParse(value, out _);
                }
                if (targetType == typeof(Guid))
                {
                    return Guid.TryParse(value, out _);
                }

                Convert.ChangeType(value, targetType);
                return true;
            }
            catch
            {
                return false;
            }
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
