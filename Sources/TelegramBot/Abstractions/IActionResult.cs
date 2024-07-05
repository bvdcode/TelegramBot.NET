using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot;

namespace TelegramBot.Abstractions
{
    public interface IActionResult
    {
        Action<ITelegramBotClient> Handle { get; }
    }
}
