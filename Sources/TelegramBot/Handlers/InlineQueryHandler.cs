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

            var attributes = method.GetCustomAttributes(typeof(InlineCommandAttribute), inherit: false)
                .OfType<InlineCommandAttribute>()
                .ToArray();
            if (attributes.Length == 0)
            {
                return false;
            }

            var incomingCommandParts = command.Split('/');
            var methodParameters = method.GetParameters();

            // Fast path: parameterless method with an exact attribute command
            if (methodParameters.Length == 0)
            {
                if (attributes.Any(a => string.Equals(a.Command, command, StringComparison.Ordinal)))
                {
                    return true;
                }
                return false;
            }

            foreach (var botAttribute in attributes)
            {
                var controllerCommandParts = botAttribute.Command.Split('/');
                if (controllerCommandParts.Length != incomingCommandParts.Length)
                {
                    continue;
                }

                if (TryExtractArgsFromParts(controllerCommandParts, incomingCommandParts, methodParameters, out var tempArgs))
                {
                    args = tempArgs;
                    return true;
                }
            }

            return false;
        }

        private bool TryExtractArgsFromParts(string[] controllerCommandParts, string[] incomingCommandParts, ParameterInfo[] methodParameters, out List<object> tempArgs)
        {
            tempArgs = new List<object>(incomingCommandParts.Length);

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
                            return false;
                        }
                    }
                    tempArgs.Add(incomingPart);
                }
                else if (!string.Equals(controllerPart, incomingPart, StringComparison.Ordinal))
                {
                    return false;
                }
            }

            return true;
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
