namespace TelegramBot.Builders
{
    /// <summary>
    /// The builder for the Telegram API key.
    /// </summary>
    public class TelegramApiKeyBuilder
    {
        /// <summary>
        /// API key for the Telegram bot.
        /// </summary>
        public string? ApiKey { get; set; }

        internal bool UseConfiguration { get; private set; }

        /// <summary>
        /// Use the configuration value TelegramApiKey as the API key.
        /// </summary>
        public void FromConfiguration()
        {
            UseConfiguration = true;
        }
    }
}