using TelegramBot.Builders;
using Microsoft.Extensions.DependencyInjection;

namespace TelegramBot.ConsoleTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            BotBuilder builder = new(args);
            builder.Services.AddSingleton<IEnumerable<string>, List<string>>();
            var app = builder.Build();
            app.MapControllers();
            app.Run();
        }
    }
}