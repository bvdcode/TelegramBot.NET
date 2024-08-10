using System;
using System.Threading;
using Microsoft.Extensions.Hosting;

namespace TelegramBot.Services
{
    internal class HostApplicationLifetime : IHostApplicationLifetime, IDisposable
    {
        private readonly CancellationTokenSource stopTokenSource = new CancellationTokenSource();
        private readonly CancellationTokenSource stoppingTokenSource = new CancellationTokenSource();
        private readonly CancellationTokenSource startTokenSource = new CancellationTokenSource();

        public CancellationToken ApplicationStarted => startTokenSource.Token;

        public CancellationToken ApplicationStopping => stoppingTokenSource.Token;

        public CancellationToken ApplicationStopped => stopTokenSource.Token;

        public void StopApplication()
        {
            stopTokenSource.Cancel();
            stoppingTokenSource.Cancel();
            startTokenSource.Cancel();
        }

        public void NotifyStarted()
        {
            startTokenSource.Cancel();
        }

        public void NotifyStopped()
        {
            stopTokenSource.Cancel();
        }

        public void NotifyStopping()
        {
            stoppingTokenSource.Cancel();
        }

        public void Dispose()
        {
            stopTokenSource.Dispose();
            stoppingTokenSource.Dispose();
            startTokenSource.Dispose();
        }
    }
}