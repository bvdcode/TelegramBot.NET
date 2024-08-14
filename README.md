[![GitHub](https://img.shields.io/github/license/bvdcode/TelegramBot.NET)](https://github.com/bvdcode/TelegramBot.NET/blob/main/LICENSE.md)
[![Nuget](https://img.shields.io/nuget/dt/TelegramBot.NET?color=%239100ff)](https://www.nuget.org/packages/TelegramBot.NET/)
[![Static Badge](https://img.shields.io/badge/fuget-f88445?logo=readme&logoColor=white)](https://www.fuget.org/packages/TelegramBot.NET)
[![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/bvdcode/TelegramBot.NET/.github%2Fworkflows%2Fpublish-release.yml)](https://github.com/bvdcode/TelegramBot.NET/actions)
[![NuGet version (TelegramBot.NET)](https://img.shields.io/nuget/v/TelegramBot.NET.svg?label=stable)](https://www.nuget.org/packages/TelegramBot.NET/)
[![CodeFactor](https://www.codefactor.io/repository/github/bvdcode/TelegramBot.NET/badge)](https://www.codefactor.io/repository/github/bvdcode/TelegramBot.NET)
![GitHub repo size](https://img.shields.io/github/repo-size/bvdcode/TelegramBot.NET)

<a id="readme-top"></a>

# TelegramBot.NET

Ready-to-use **.NET Standard** library for convenient development of Telegram bots.

# Purposes

Many people know the ASP.NET Core platform and its convenience for developing web API applications.

Now you can see the same pattern in Telegram Bot API.

You can find usage examples in the [TelegramBot.ConsoleTest](https://github.com/bvdcode/TelegramBot.NET/tree/main/Sources/TelegramBot.ConsoleTest) project.

# Getting Started

Start by importing the library into your project

`dotnet add package TelegramBot.NET`

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

or use command line arguments:

```Bash
./TelegramBot.Console TelegramBotToken=YOUR_API_TOKEN
```

3. Implement controller, in this sample - for handling `/start` command:

```CSharp
public class CommandController(ILogger<CommandController> _logger) : BotControllerBase
{
    [TextCommand("/start")]
    public IActionResult HandleStartAsync()
    {
        _logger.LogInformation("Start command received.");
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

<p align="right"><a href="#readme-top">back to top</a></p>

## Features

- Dependency Injection - use `Services` property of `BotBuilder` to add services:

```CSharp
builder.Services.AddDbContext<AppDbContext>(x => x.UseNpgsql(connectionString));
builder.Services.AddScoped<ISomeHandler, Handler>();
```

- Send response to client - use `BotControllerBase` methods - `Inline`, `Text`, `MarkDown`:

```CSharp
[BotCommand("/start")]
public async Task<IActionResult> HandleStartAsync()
{
    string prompt = await _dbContext.Translations.GetTranslationAsync("WelcomePrompt", Language.English);
    InlineKeyboardMarkup keyboard = new KeyboardBuilder()
        .WithColumns(2)
        .AddButton("🇺🇸 English", "/language/en")
        .AddButton("🇷🇺 Русский", "/language/ru")
        .AddButton("🇪🇸 Español", "/language/es")
        .AddButton("🇺🇦 Українська", "/language/uk")
        .Build();
    return Inline(prompt, keyboard);
    // or
    return Text("Hello, it's me!");
    // or
    return Markdown("Okay, click [this](https://example.com) link");
}
```

<p align="right"><a href="#readme-top">back to top</a></p>

## Roadmap

- [x] Add command handlers
- [ ] Add response types:
  - [x] Text
  - [x] Inline
  - [x] Markdown
  - [ ] Image
  - [ ] Video
  - [ ] Delete
  - [ ] Redirect
- [ ] Implement language dictionary service
- [x] Implement router for inline query
- [ ] Inject user model in base controller
- [ ] Add user state service

See the [open issues](https://github.com/BigMakCode/TelegramBot.NET/issues) for a full list of proposed features (and known issues).

<p align="right"><a href="#readme-top">back to top</a></p>

## Contributing

Contributions are what make the open source community such an amazing place to learn, inspire, and create. Any contributions you make are **greatly appreciated**.

If you have a suggestion that would make this better, please fork the repo and create a pull request. You can also simply open an issue with the tag "enhancement".
Don't forget to give the project a star! Thanks again!

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

<p align="right"><a href="#readme-top">back to top</a></p>

# License

Distributed under the MIT License. See LICENSE.md for more information.

# Contact

[E-Mail](mailto:github-telegram-bot-net@belov.us)
