using Telegram.Bot;

namespace TelegramBot.Abstractions
{
    public class ActionContext
    {
        public ITelegramBotClient BotClient { get; }

        public ActionContext(ITelegramBotClient botClient)
        {
            BotClient = botClient;
        }
    }
}