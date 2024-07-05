# TelegramBot.NET
Ready-to-use library for convenient development of Telegram bots.

# Purposes
Many people know the ASP.NET Core platform and its convenience for developing web API applications.

I came up with the idea to implement a similar message processing pattern for telegram bots.

# How to use
Start by importing the library into your project

`dotnet add package TelegramBot.NET --version 1.0.1`

1. Implement simple handler in your `Program.cs`

```CSharp
static void Main(string[] args)
{
    BotBuilder builder = new BotBuilder(args)
        .UseApiKey(x => x.FromConfiguration());

    var app = builder.Build();
    app.MapControllers();
    app.Run();
}
```

2. Add your API key from [BotFather](https://t.me/BotFather) to `appsettings.json` file, key is `TelegramBotToken`:

```JSON
{
  "TelegramBotToken": "YOUR_API_TOKEN"
}
```

3. Implement controller, in this sample - for handling `/start` command:

```CSharp
public class CommandController(ILogger<CommandController> _logger) : BotControllerBase
{
    [BotCommand("/start")]
    public async Task<IActionResult> HandleStartAsync()
    {
        _logger.LogInformation("Start command received.");
        await Task.Delay(1000);
        return Text("Hello!");
    }
}
```

4. Run application - and see result:

```Bash
info: TelegramBot.BotApp[0]
      Bot started - receiving updates.
info: TelegramBot.ConsoleTest.Controllers.HomeController[0]
      Start command received.
```