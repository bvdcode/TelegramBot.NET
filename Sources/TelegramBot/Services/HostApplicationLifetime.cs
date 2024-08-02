using System.Threading;
using Microsoft.Extensions.Hosting;

namespace TelegramBot.Services
{
    internal class HostApplicationLifetime : IHostApplicationLifetime
    {
        private readonly CancellationTokenSource _cancellationTokenSource;

        public HostApplicationLifetime(CancellationTokenSource cancellationTokenSource)
        {
            _cancellationTokenSource = cancellationTokenSource ?? throw new System.ArgumentNullException(nameof(cancellationTokenSource));
        }

        public CancellationToken ApplicationStarted => _cancellationTokenSource.Token;

        public CancellationToken ApplicationStopping => _cancellationTokenSource.Token;

        public CancellationToken ApplicationStopped => _cancellationTokenSource.Token;

        public void StopApplication()
        {
            _cancellationTokenSource.Cancel();
        }
    }
}