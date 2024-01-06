using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TelegramBot
{
    public interface IBot
    {
        Task StartAsync(CancellationToken token);
    }
}