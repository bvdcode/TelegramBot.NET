using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace TelegramBot.Builders
{
    public interface IBotBuilder
    {
        IServiceCollection Services { get; }
    }
}