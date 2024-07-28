using TelegramBot.Builders;
using Microsoft.EntityFrameworkCore;
using TelegramBot.ConsoleTest.Database;
using Microsoft.Extensions.DependencyInjection;

namespace TelegramBot.ConsoleTest
{
    public class Program
    {
        public static void Main(string[] args)
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