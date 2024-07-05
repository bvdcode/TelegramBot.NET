namespace TelegramBot.Builders
{
    public interface ITelegramServerBuilder
    {
        string BaseUrl { get; set; }

        void FromConfiguration();
    }
}