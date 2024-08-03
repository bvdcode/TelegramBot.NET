using System;
using System.Reflection;
using System.Collections.Generic;

namespace TelegramBot.Helpers
{
    internal static class ObjectHelpers
    {
        internal static bool TryConvertParameters(MethodInfo method, object[] args)
        {
            List<object> newArgs = new List<object>(args.Length);
            var parameters = method.GetParameters();
            foreach (var parameter in parameters)
            {
                if (parameter.ParameterType.IsPrimitive || parameter.ParameterType == typeof(string))
                {
                    try
                    {
                        var changedType = Convert.ChangeType(args[parameter.Position], parameter.ParameterType);
                        newArgs.Add(changedType);
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            for (int i = 0; i < newArgs.Count; i++)
            {
                args[i] = newArgs[i];
            }
            return true;
        }
    }
}