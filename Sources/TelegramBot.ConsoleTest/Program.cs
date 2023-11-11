namespace TelegramBot.ConsoleTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string apiKey = "";
            string customTelegramApiUrl = "";

            IBot bot = new Bot(apiKey, customTelegramApiUrl);
        }
    }
}