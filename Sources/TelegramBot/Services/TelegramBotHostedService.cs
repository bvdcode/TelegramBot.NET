using System;
using System.Linq;
using Telegram.Bot;
using System.Threading;
using System.Reflection;
using Telegram.Bot.Types;
using TelegramBot.Builders;
using TelegramBot.Handlers;
using System.Threading.Tasks;
using TelegramBot.Attributes;
using TelegramBot.Extensions;
using TelegramBot.Containers;
using TelegramBot.Controllers;
using TelegramBot.Abstractions;
using System.Collections.Generic;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace TelegramBot.Services
{
    internal class TelegramBotHostedService : IHostedService
    {
        private readonly ITelegramBotClient _client;
        private readonly IServiceProvider _serviceProvider;
        private readonly BotConfiguration _botConfiguration;
        private readonly IReadOnlyCollection<MethodInfo> _methods;
        private readonly ILogger<TelegramBotHostedService> _logger;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public TelegramBotHostedService(BotConfiguration botConfiguration, ITelegramBotClient telegramBotClient,
            ILogger<TelegramBotHostedService> logger, IServiceProvider serviceProvider, 
            BotControllerMethodsContainer botControllerMethodsContainer)
        {
            _logger = logger;
            _client = telegramBotClient;
            _serviceProvider = serviceProvider;
            _botConfiguration = botConfiguration;
            _methods = botControllerMethodsContainer.Methods;
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            cancellationToken = MergeTokens(cancellationToken);
            try
            {
                var botUser = await _client.GetMe(cancellationToken: cancellationToken);
                if (_botConfiguration.ReceiveUpdates)
                {
                    _client.StartReceiving(UpdateHandler, ErrorHandler, cancellationToken: cancellationToken);
                    _logger.LogInformation("Bot '{botUser}' started - receiving updates.", botUser.Username);
                }
                else
                {
                    _logger.LogInformation("Bot '{botUser}' started - not receiving updates.", botUser.Username);
                }
            }
            catch (Exception ex)
            {
                if (ex is AggregateException aggregateException)
                {
                    ex = aggregateException.InnerException;
                }
                _logger.LogError(ex, "Error occurred while starting the bot. Probably the bot token is invalid or the network is not available.");
                throw ex;
            }

            var commandRegistrationBuilders = _serviceProvider.GetServices<CommandRegistrationBuilder>();
            if (commandRegistrationBuilders != null && commandRegistrationBuilders.Any())
            {
                foreach (var builder in commandRegistrationBuilders)
                {
                    var commands = builder.Build();
                    await _client.SetMyCommands(commands,
                            languageCode: builder.Language,
                            cancellationToken: cancellationToken);
                    _logger.LogInformation("Registered {count} commands for language '{language}'.",
                        commands.Count(), builder.Language);
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _cancellationTokenSource.Cancel();
            _logger.LogInformation("Stopping bot - no longer receiving updates.");
            return Task.CompletedTask;
        }

        private Task ErrorHandler(ITelegramBotClient client, Exception exception, CancellationToken token)
        {
            _logger.LogError(exception, "Error occurred while receiving updates.");
            return Task.CompletedTask;
        }

        private async Task UpdateHandler(ITelegramBotClient client, Update update, CancellationToken token)
        {
            if (update.Message != null && !string.IsNullOrWhiteSpace(update.Message.Text) && !update.Message.Text.StartsWith('/'))
            {
                _logger.LogInformation("Received text message: {Text}.", update.Message.Text);
                var handler = new TextMessageHandler(_methods, update);
                await HandleRequestAsync(handler, update);
            }
            else if (update.Message != null && !string.IsNullOrWhiteSpace(update.Message.Text) && update.Message.Text.StartsWith('/'))
            {
                _logger.LogInformation("Received text command: {Text}.", update.Message.Text);
                var handler = new TextCommandHandler(_methods, update);
                await HandleRequestAsync(handler, update);
            }
            else if (update.CallbackQuery != null && update.CallbackQuery.Data != null)
            {
                _logger.LogInformation("Received inline query: {Data}.", update.CallbackQuery.Data);
                var handler = new InlineQueryHandler(_methods, update);
                await HandleRequestAsync(handler, update);
            }
            else
            {
                _logger.LogWarning("Unsupported update type: {UpdateType}.", update.Type);
            }
        }

        private async Task HandleRequestAsync(ITelegramUpdateHandler handler, Update update)
        {
            bool hasUser = update.TryGetUser(out User user);
            if (!hasUser)
            {
                return;
            }
            var args = handler.GetArguments();
            MethodInfo? method = handler.GetMethodInfo();
            if (method == null)
            {
                _logger.LogWarning("Method not found for message: {Text}.", update.Message?.Text);
                return;
            }
            bool isAuthorized = await AuthorizeAsync(method, user);
            if (!isAuthorized)
            {
                return;
            }
            CheckMethodMatching(method, args);
            BotControllerBase controller = (BotControllerBase)ActivatorUtilities.CreateInstance(_serviceProvider, method.DeclaringType!);
            controller.Update = update;
            controller.User = user;
            controller.Client = _client;
            if (_serviceProvider.GetService<IKeyValueProvider>() is IKeyValueProvider keyValueProvider)
            {
                controller.KeyValueProvider = keyValueProvider;
            }
            var result = method.Invoke(controller, args);
            await ExecuteResultAsync(result, user.Id);
            await DisposeAsync(controller);
            await DisposeAsync(result);
        }

        private void CheckMethodMatching(MethodInfo method, object[]? args)
        {
            if (method.ReturnType != typeof(Task<IActionResult>) && method.ReturnType != typeof(IActionResult))
            {
                throw new InvalidOperationException("Invalid return type: " + method.ReturnType.Name);
            }
            if (args != null && method.GetParameters().Length != args?.Length)
            {
                throw new InvalidOperationException("Invalid arguments count: " + args?.Length);
            }
        }

        private async Task<bool> AuthorizeAsync(MethodInfo method, User user)
        {
            if (method.GetCustomAttribute<AuthorizeAttribute>() != null
                || method.DeclaringType?.GetCustomAttribute<AuthorizeAttribute>() != null)
            {
                if (_serviceProvider.GetService<IBotAuthorizationHandler>() is IBotAuthorizationHandler authorizationHandler)
                {
                    if (!authorizationHandler.Authorize(user))
                    {
                        await authorizationHandler
                            .HandleUnauthorized(user)
                            .ExecuteResultAsync(new ActionContext(_client, user.Id));
                        return false;
                    }
                }
            }
            return true;
        }

        private async Task ExecuteResultAsync(object result, long userId)
        {
            if (result is Task<IActionResult> taskResult)
            {
                await (await taskResult).ExecuteResultAsync(new ActionContext(_client, userId));
            }
            else if (result is IActionResult actionResult)
            {
                await actionResult.ExecuteResultAsync(new ActionContext(_client, userId));
            }
            else
            {
                throw new InvalidOperationException("Invalid result type: " + result.GetType().Name);
            }
        }

        private CancellationToken MergeTokens(CancellationToken token)
        {
            return CancellationTokenSource.CreateLinkedTokenSource(_cancellationTokenSource.Token, token).Token;
        }

        private async Task DisposeAsync(object obj)
        {
            if (obj is IAsyncDisposable asyncDisposable)
            {
                await asyncDisposable.DisposeAsync();
            }
            else if (obj is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
