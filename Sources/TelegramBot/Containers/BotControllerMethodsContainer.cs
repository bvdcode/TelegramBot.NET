using System;
using System.Reflection;
using System.Collections.Generic;

namespace TelegramBot.Containers
{
    internal class BotControllerMethodsContainer
    {
        internal IReadOnlyCollection<MethodInfo> Methods => _methods.AsReadOnly();
        private readonly List<MethodInfo> _methods = new List<MethodInfo>();

        internal void AddMethod(MethodInfo method)
        {
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method), "Method cannot be null.");
            }
            if (!_methods.Contains(method))
            {
                _methods.Add(method);
            }
        }
    }
}
