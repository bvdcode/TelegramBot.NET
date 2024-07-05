using System;
using System.Collections.Generic;
using System.Text;
using TelegramBot.Abstractions;

namespace TelegramBot.Controllers
{
    /// <summary>
    /// Base class for bot controllers.
    /// </summary>
    public abstract class BotControllerBase
    {
        public IActionResult Text(string text)
        {
            throw new NotImplementedException();
        }
    }
}
