using TelegramBot.Builders;
using Microsoft.Extensions.DependencyInjection;

namespace TelegramBot.ConsoleTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            BotBuilder builder = new BotBuilder(args)
                .UseTelegramServer(x => x.FromConfiguration())
                .UseTelegramServer(x => x.BaseUrl = "https://api.telegram.org");
            builder.Services.AddSingleton<IEnumerable<string>, List<string>>();
            var app = builder.Build();
            app.MapControllers();
            app.Run();
        }
    }
}