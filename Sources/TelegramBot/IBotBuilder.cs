using System;
using System.Collections.Generic;
using System.Text;

namespace TelegramBot
{
    public interface IBotBuilder
    {
        IBotBuilder AddScoped<TService, TImplementation>()
            where TService : class
            where TImplementation : TService;
        IBotBuilder AddSingleton<TService, TImplementation>()
            where TService : class
            where TImplementation : TService;
        IBot Build();
        IBotBuilder UseCustomTelegramApiUrl(string customTelegramApiUrl);
    }
}