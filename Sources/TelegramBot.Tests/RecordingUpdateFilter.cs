using Telegram.Bot.Types;
using TelegramBot.Abstractions;

namespace TelegramBot.Tests
{
    public class RecordingUpdateFilter(Func<Update, CancellationToken, Task<IActionResult?>> handler)
        : IBotUpdateFilter, IAsyncDisposable
    {
        public int Calls { get; private set; }

        public bool Disposed { get; private set; }

        public Task<IActionResult?> FilterAsync(Update update, CancellationToken cancellationToken)
        {
            Calls++;
            return handler(update, cancellationToken);
        }

        public ValueTask DisposeAsync()
        {
            Disposed = true;
            return ValueTask.CompletedTask;
        }
    }
}
