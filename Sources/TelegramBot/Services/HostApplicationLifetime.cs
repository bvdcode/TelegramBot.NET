using System;
using System.Threading;
using Microsoft.Extensions.Hosting;

namespace TelegramBot.Services
{
    internal class HostApplicationLifetime : IHostApplicationLifetime
    {
        public CancellationToken ApplicationStarted => _cancellationTokenSource.Token;

        public CancellationToken ApplicationStopping => _cancellationTokenSource.Token;

        public CancellationToken ApplicationStopped => _cancellationTokenSource.Token;


        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public void StopApplication()
        {
            _cancellationTokenSource.Cancel();
        }
    }
}