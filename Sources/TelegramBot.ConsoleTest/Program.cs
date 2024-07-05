using TelegramBot.Builders;

namespace TelegramBot.ConsoleTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            BotBuilder builder = new BotBuilder(args)
                .UseApiKey(x => x.FromConfiguration());

            var app = builder.Build();
            app.MapControllers();
            app.Run();
        }
    }
}