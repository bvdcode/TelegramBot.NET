namespace TelegramBot.Builders
{
    /// <summary>
    /// Bot configuration.
    /// </summary>
    public class BotConfiguration
    {
        /// <summary>
        /// Gets or sets a value indicating whether the bot should receive updates. Default is <see langword="true"/>.
        /// </summary>
        public bool ReceiveUpdates { get; set; } = true;
    }
}