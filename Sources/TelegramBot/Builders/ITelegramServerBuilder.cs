namespace TelegramBot.Builders
{
    /// <summary>
    /// The builder for the Telegram server API.
    /// </summary>
    public interface ITelegramServerBuilder
    {
        /// <summary>
        /// Set the base URL for the Telegram API.
        /// </summary>
        string BaseUrl { get; set; }

        /// <summary>
        /// Use the configuration value CustomTelegramApiUrl as the base URL.
        /// </summary>
        void FromConfiguration();
    }
}