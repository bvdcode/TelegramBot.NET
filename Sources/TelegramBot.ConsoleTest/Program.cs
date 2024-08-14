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
                        .RegisterCommand("/burgersdone", "notifies that the order is ready")
                        .RegisterCommand("/receipt", "shows the receipt");
                }, "en")
                .RegisterCommands(x =>
                {
                    x.RegisterCommand("/start", "инициирует бота")
                        .RegisterCommand("/help", "показывает сообщение справки")
                        .RegisterCommand("/burgers", "показывает меню бургеров")
                        .RegisterCommand("/burgersdone", "уведомляет о готовности заказа")
                        .RegisterCommand("/receipt", "показывает чек");
                }, "ru");

            builder.Services
                .AddDbContext<AppDbContext>(x => x.UseSqlite("Data Source=app.db"))
                .UseAuthorizationHandler<AuthorizationHandler>();
            var app = builder.Build();
            app.MapControllers();
            
            app.Run();
        }
    }
}