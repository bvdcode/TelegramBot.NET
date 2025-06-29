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
            List<MethodInfo> methods = new List<MethodInfo>();
            foreach (var method in controllerMethods)
            {
                bool isValidMethod = IsValidMethod(method, command);
                if (isValidMethod)
                {
                    methods.Add(method);
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
                throw new AmbiguousMatchException("Multiple methods found with the same command and arguments: " + command + "\n" + 
                    string.Join("\n", methods.Select(m => m.Name + " - " + m.GetParameters().Length + " parameters")));
            }
            return null;
        }

        private bool IsValidMethod(MethodInfo method, string command)
        {
            var attributes = method.GetCustomAttributes(typeof(InlineCommandAttribute), false);
            string[] incomingCommandParts = command.Split('/');

            foreach (var attribute in attributes)
            {
                if (!(attribute is InlineCommandAttribute botAttribute))
                {
                    continue; // Skip if the attribute is not of type InlineCommandAttribute
                }

                // Если метод не имеет параметров, проверяем точное совпадение команды
                if (method.GetParameters().Length == 0 && botAttribute.Command != command)
                {
                    continue; // Skip methods without parameters if the command does not match
                }

                string[] controllerCommandParts = botAttribute.Command.Split('/');
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

                    // Если это параметр в маршруте, добавляем его в аргументы
                    if (controllerCommandParts[i].StartsWith("{") && controllerCommandParts[i].EndsWith("}"))
                    {
                        // Проверка типа аргумента
                        var parameter = method.GetParameters().FirstOrDefault(p => p.Name == controllerCommandParts[i].Trim('{', '}'));
                        if (parameter != null)
                        {
                            if (!TryConvertToType(incomingCommandParts[i], parameter.ParameterType))
                            {
                                match = false;  // If type conversion fails, break the match
                                break;
                            }
                        }
                        _args.Add(incomingCommandParts[i]);
                    }
                }

                if (match)
                {
                    return true;
                }
            }
            return false;
        }

        private bool TryConvertToType(string value, Type targetType)
        {
            try
            {
                // Попробуем привести строку к требуемому типу
                if (targetType == typeof(int))
                {
                    return int.TryParse(value, out _);  // Проверяем, может ли строка быть приведена к int
                }
                if (targetType == typeof(long))
                {
                    return long.TryParse(value, out _);  // Проверяем на long
                }
                if (targetType == typeof(double))
                {
                    return double.TryParse(value, out _);  // Проверяем на double
                }
                if (targetType == typeof(bool))
                {
                    return bool.TryParse(value, out _);  // Проверяем на bool
                }
                if (targetType == typeof(Guid))
                {
                    return Guid.TryParse(value, out _);  // Проверяем на Guid
                }

                // Если тип не поддерживается, пробуем использовать конструктор (например, для строк или других типов)
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
