using TelegramBot.Builders;
using TelegramBot.Extensions;
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
                .UseApiKey(x => x.FromConfiguration())
                .RegisterCommands(x =>
                {
                    x.RegisterCommand("/start", "initiates the bot")
                        .RegisterCommand("/help", "shows help message")
                        .RegisterCommand("/burgers", "shows burgers menu")
                        .RegisterCommand("/burgersdone", "notifies that the order is ready");
                }, "en");

            builder.Services
                .AddDbContext<AppDbContext>(x => x.UseSqlite("Data Source=app.db"))
                .UseAuthorizationHandler<AuthorizationHandler>();
            var app = builder.Build();
            app.MapControllers();
            
            app.Run();
        }
    }
}