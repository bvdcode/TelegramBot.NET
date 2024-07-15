using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TelegramBot.Builders;
using TelegramBot.ConsoleTest.Database;

namespace TelegramBot.ConsoleTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            BotBuilder builder = new BotBuilder(args)
                .UseApiKey(x => x.FromConfiguration());

            builder.Services.AddDbContext<AppDbContext>(x => x.UseSqlite("Data Source=app.db"));
            var app = builder.Build();
            app.MapControllers();
            app.Run();
        }
    }
}