using Telegram.Bot.Types;
using System.Threading.Tasks;

namespace TelegramBot.Handlers
{
    internal interface ITelegramUpdateHandler
    {
        Task HandleAsync(Update update);
    }
}