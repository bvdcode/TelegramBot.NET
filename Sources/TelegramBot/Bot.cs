using System;
using Autofac;
using Telegram.Bot;
using System.Threading;
using System.Threading.Tasks;

namespace TelegramBot
{
    /// <summary>
    /// Bot class for Telegram Bot API access.
    /// </summary>
    public class Bot : IBot
    {
        private readonly IContainer _container;
        private readonly ITelegramBotClient _telegramBotClient;

        internal Bot(string apiKey, string? customTelegramApiUrl, IContainer container)
        {
            _container = container;
            TelegramBotClientOptions options = new TelegramBotClientOptions(apiKey, customTelegramApiUrl);
            _telegramBotClient = new TelegramBotClient(options);
        }

        public async Task StartAsync(CancellationToken token)
        {

        }
    }
}