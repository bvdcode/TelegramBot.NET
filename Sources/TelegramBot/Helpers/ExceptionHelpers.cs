using System;
using System.Collections.Generic;
using System.Text;

namespace TelegramBot.Helpers
{
    internal class ExceptionHelpers
    {
        internal static void ThrowIfNegativeOrZero(int value, string paramName)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(paramName, "Value must be greater than zero.");
            }
        }
    }
}
