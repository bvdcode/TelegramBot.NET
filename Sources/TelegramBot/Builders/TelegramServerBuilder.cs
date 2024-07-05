namespace TelegramBot.Builders
{
    /// <summary>
    /// The builder for the Telegram server API.
    /// </summary>
    public class TelegramServerBuilder
    {
        /// <summary>
        /// Set the base URL for the Telegram API.
        /// </summary>
        public string? BaseUrl { get; set; }

        internal bool UseConfiguration { get; private set; }

        /// <summary>
        /// Use the configuration value CustomTelegramApiUrl as the base URL.
        /// </summary>
        public void FromConfiguration()
        {
            UseConfiguration = true;
        }
    }
}