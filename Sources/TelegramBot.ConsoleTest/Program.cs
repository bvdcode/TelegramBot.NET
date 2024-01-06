using System.Globalization;
using TelegramBot;

namespace TelegramBot.ConsoleTest
{
    internal class Program
    {
        static async void Main(string[] args)
        {
            string apiKey = "ABCD";
            string customTelegramApiUrl = "";

            IBot bot = new BotBuilder()
                .UseApiKey(apiKey)
                .UseCustomTelegramApiUrl(customTelegramApiUrl)
                //.AddScoped<IEnumerable<int>, List<int>>()
                //.AddSingleton<IEnumerable<string>, List<string>>()
                //.AddCultures()
                //.UseRedis(x => x.Host = "localhost")
                //.AddCommands()
                //.AddPages()
                //.AddSchedulerTasks()
                .Build();

            //bot.UpdateReceived += async (IBot sender, UpdateEventArgs e) =>
            //{
            //    await bot.Client.SendTextMessageAsync(e.Message.Chat.Id, "Hello World!");
            //};

            CancellationTokenSource cancellationTokenSource = new();
            await bot.StartAsync(cancellationTokenSource.Token);
        }
    }
}