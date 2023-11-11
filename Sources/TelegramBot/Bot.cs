using System;

namespace TelegramBot
{
    public class Bot : IBot
    {
        private string apiKey;
        private string customTelegramApiUrl;

        public Bot(string apiKey, string customTelegramApiUrl)
        {
            this.apiKey = apiKey;
            this.customTelegramApiUrl = customTelegramApiUrl;
        }
    }
}
