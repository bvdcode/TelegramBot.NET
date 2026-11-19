using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBot.Abstractions;
using TelegramBot.ActionResults;
using TelegramBot.Builders;
using TelegramBot.Services;

namespace TelegramBot.Tests
{
    public class UpdateFilterTests
    {
        [Theory]
        [InlineData("/start")]
        [InlineData("/start invitation")]
        [InlineData("/vpn")]
        [InlineData("/unknown")]
        [InlineData("hello")]
        [InlineData(null)]
        public async Task FilterStopsUpdatesBeforeRouteLookup(string? text)
        {
            using RecordingBotHttpHandler httpHandler = new();
            using HttpClient http = new(httpHandler);
            TelegramBotClient client = new("123456:example-token", http);
            RecordingUpdateFilter filter = new((_, _) =>
                Task.FromResult<IActionResult?>(new TextResult("Access blocked")));
            await using ServiceProvider services = new ServiceCollection()
                .AddScoped<IBotUpdateFilter>(_ => filter)
                .BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
            TelegramBotHostedService service = CreateService(client, services);

            await service.UpdateHandler(client, Message(text), CancellationToken.None);

            Assert.Equal(1, filter.Calls);
            Assert.True(filter.Disposed);
            Assert.Equal(["sendMessage"], httpHandler.Methods);
            Assert.Contains("Access blocked", httpHandler.Payloads.Single());
        }

        [Fact]
        public async Task FilterStopsCallbacksAndAcknowledgesTheButton()
        {
            using RecordingBotHttpHandler httpHandler = new();
            using HttpClient http = new(httpHandler);
            TelegramBotClient client = new("123456:example-token", http);
            await using ServiceProvider services = new ServiceCollection()
                .AddScoped<IBotUpdateFilter>(_ => new RecordingUpdateFilter((_, _) =>
                    Task.FromResult<IActionResult?>(new TextResult("Access blocked"))))
                .BuildServiceProvider();
            TelegramBotHostedService service = CreateService(client, services);
            Update update = new()
            {
                CallbackQuery = new CallbackQuery
                {
                    Id = "callback",
                    From = new User { Id = 1001, FirstName = "Example" },
                    Data = "/telegram/page/2",
                },
            };

            await service.UpdateHandler(client, update, CancellationToken.None);

            Assert.Equal(["answerCallbackQuery", "sendMessage"], httpHandler.Methods);
        }

        [Fact]
        public async Task FiltersRunInOrderAndStopAfterFirstResponse()
        {
            using RecordingBotHttpHandler httpHandler = new();
            using HttpClient http = new(httpHandler);
            TelegramBotClient client = new("123456:example-token", http);
            RecordingUpdateFilter first = new((_, _) => Task.FromResult<IActionResult?>(null));
            RecordingUpdateFilter second = new((_, _) => Task.FromResult<IActionResult?>(new TextResult("Blocked")));
            RecordingUpdateFilter third = new((_, _) => Task.FromResult<IActionResult?>(null));
            await using ServiceProvider services = new ServiceCollection()
                .AddScoped<IBotUpdateFilter>(_ => first)
                .AddScoped<IBotUpdateFilter>(_ => second)
                .AddScoped<IBotUpdateFilter>(_ => third)
                .BuildServiceProvider();
            TelegramBotHostedService service = CreateService(client, services);

            await service.UpdateHandler(client, Message("/unknown"), CancellationToken.None);

            Assert.Equal(1, first.Calls);
            Assert.Equal(1, second.Calls);
            Assert.Equal(0, third.Calls);
            Assert.True(first.Disposed && second.Disposed && third.Disposed);
            Assert.Single(httpHandler.Methods);
        }

        [Fact]
        public async Task PassingFilterDoesNotGenerateAResponse()
        {
            using RecordingBotHttpHandler httpHandler = new();
            using HttpClient http = new(httpHandler);
            TelegramBotClient client = new("123456:example-token", http);
            RecordingUpdateFilter filter = new((_, _) => Task.FromResult<IActionResult?>(null));
            await using ServiceProvider services = new ServiceCollection()
                .AddScoped<IBotUpdateFilter>(_ => filter)
                .BuildServiceProvider();
            TelegramBotHostedService service = CreateService(client, services);

            await service.UpdateHandler(client, Message("/unknown"), CancellationToken.None);

            Assert.Equal(1, filter.Calls);
            Assert.True(filter.Disposed);
            Assert.Empty(httpHandler.Methods);
        }

        [Fact]
        public async Task FilterFailurePropagatesAndDisposesTheScope()
        {
            using RecordingBotHttpHandler httpHandler = new();
            using HttpClient http = new(httpHandler);
            TelegramBotClient client = new("123456:example-token", http);
            RecordingUpdateFilter filter = new((_, _) => throw new InvalidOperationException("Storage unavailable"));
            await using ServiceProvider services = new ServiceCollection()
                .AddScoped<IBotUpdateFilter>(_ => filter)
                .BuildServiceProvider();
            TelegramBotHostedService service = CreateService(client, services);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.UpdateHandler(client, Message("/start"), CancellationToken.None));

            Assert.True(filter.Disposed);
            Assert.Empty(httpHandler.Methods);
        }

        [Fact]
        public async Task FilterReceivesCancellationToken()
        {
            using RecordingBotHttpHandler httpHandler = new();
            using HttpClient http = new(httpHandler);
            TelegramBotClient client = new("123456:example-token", http);
            using CancellationTokenSource cancellation = new();
            CancellationToken observed = default;
            await using ServiceProvider services = new ServiceCollection()
                .AddScoped<IBotUpdateFilter>(_ => new RecordingUpdateFilter((_, token) =>
                {
                    observed = token;
                    return Task.FromResult<IActionResult?>(null);
                }))
                .BuildServiceProvider();
            TelegramBotHostedService service = CreateService(client, services);

            await service.UpdateHandler(client, Message("/start"), cancellation.Token);

            Assert.Equal(cancellation.Token, observed);
        }

        private static TelegramBotHostedService CreateService(TelegramBotClient client, IServiceProvider services)
        {
            return new TelegramBotHostedService(new BotConfiguration(), client,
                NullLogger<TelegramBotHostedService>.Instance, services);
        }

        private static Update Message(string? text)
        {
            return new Update
            {
                Message = new Message
                {
                    Text = text,
                    From = new User { Id = 1001, FirstName = "Example" },
                    Chat = new Chat { Id = 1001 },
                },
            };
        }
    }
}
