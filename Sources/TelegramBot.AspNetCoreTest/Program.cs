using TelegramBot.Builders;
using TelegramBot.Extensions;

namespace TelegramBot.AspNetCoreTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddControllers();
            builder.Services.AddBotControllers();

            new BotBuilder(builder)
                .UseApiKey(x => x.FromConfiguration())
                .Build();

            var app = builder.Build();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}
