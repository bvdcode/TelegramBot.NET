﻿using System;
using System.Linq;
using Telegram.Bot;
using System.Threading;
using System.Reflection;
using Telegram.Bot.Types;
using TelegramBot.Handlers;
using TelegramBot.Services;
using TelegramBot.Builders;
using TelegramBot.Attributes;
using System.Threading.Tasks;
using TelegramBot.Extensions;
using TelegramBot.Controllers;
using TelegramBot.Abstractions;
using System.Collections.Generic;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace TelegramBot
{
    /// <summary>
    /// Telegram bot application.
    /// </summary>
    public class BotApp : IBot
    {
        private bool _disposed = false;
        private readonly ILogger<BotApp> _logger;
        private readonly TelegramBotClient _client;
        private readonly ServiceProvider _serviceProvider;
        private readonly BotConfiguration _botConfiguration;
        private IReadOnlyCollection<MethodInfo> _controllerMethods;
        private readonly CancellationTokenSource _cancellationTokenSource;

        /// <summary>
        /// Gets the services configured for the program (for example, using <see cref="M:HostBuilder.ConfigureServices(Action&lt;HostBuilderContext,IServiceCollection&gt;)" />).
        /// </summary>
        public IServiceProvider Services => _serviceProvider;

        /// <summary>
        /// Creates a new instance of <see cref="BotApp"/>.
        /// </summary>
        /// <param name="client">Telegram bot client.</param>
        /// <param name="serviceProvider">Service provider.</param>
        /// <param name="botConfiguration">Bot configuration.</param>
        public BotApp(TelegramBotClient client, ServiceProvider serviceProvider, BotConfiguration botConfiguration)
        {
            _client = client;
            _serviceProvider = serviceProvider;
            _botConfiguration = botConfiguration;
            _controllerMethods = new List<MethodInfo>();
            _cancellationTokenSource = new CancellationTokenSource();
            _logger = serviceProvider.GetRequiredService<ILogger<BotApp>>();
        }

        /// <summary>
        /// Maps controllers inherited from <see cref="BotControllerBase"/>.
        /// </summary>
        public IBot MapControllers()
        {
            CheckDisposed();
            var types = Assembly.GetCallingAssembly().GetTypes();
            List<Type> result = new List<Type>();
            foreach (var type in types)
            {
                if (type.IsSubclassOf(typeof(BotControllerBase)))
                {
                    result.Add(type);
                }
            }
            _controllerMethods = result
                .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                .ToList();
            return this;
        }

        /// <summary>
        /// Runs the bot.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token (optional).</param>
        public void Run(CancellationToken cancellationToken = default)
        {
            var mergedToken = MergeTokens(cancellationToken);
            CheckDisposed();
            StartAsync(mergedToken).Wait();
            try
            {
                Task.Delay(-1, mergedToken).Wait();
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Bot stopped - no longer receiving updates.");
            }
        }

        /// <summary>
        /// Starts the <see cref="IHostedService" /> objects configured for the program.
        /// The application will run until interrupted or until <see cref="M:IHostApplicationLifetime.StopApplication()" /> is called.
        /// </summary>
        /// <param name="cancellationToken">Used to abort program start.</param>
        /// <returns>A <see cref="Task"/> that will be completed when the <see cref="IHost"/> starts.</returns>
        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            var mergedToken = MergeTokens(cancellationToken);
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);
            CheckDisposed();
            try
            {
                var botUser = _client.GetMe().Result;
                if (_botConfiguration.ReceiveUpdates)
                {
                    _client.StartReceiving(UpdateHandler, ErrorHandler, cancellationToken: mergedToken);
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

            var hostApplicationLifetime = _serviceProvider.GetService<IHostApplicationLifetime>() as HostApplicationLifetime
                ?? throw new InvalidOperationException("Host application lifetime is not registered.");
            hostApplicationLifetime.NotifyStarted();

            var hostedServices = _serviceProvider.GetServices<IHostedService>();
            foreach (var hostedService in hostedServices)
            {
                await hostedService.StartAsync(mergedToken);
                _logger.LogInformation("Started '{hostedService}'.", hostedService.GetType().Name);
            }

            var commandRegistrationBuilders = _serviceProvider.GetServices<CommandRegistrationBuilder>();
            if (commandRegistrationBuilders != null && commandRegistrationBuilders.Any())
            {
                foreach (var builder in commandRegistrationBuilders)
                {
                    var commands = builder.Build();
                    await _client.SetMyCommands(commands,
                            languageCode: builder.Language,
                            cancellationToken: mergedToken);
                    _logger.LogInformation("Registered {count} commands for language '{language}'.",
                        commands.Count(), builder.Language);
                }
            }
        }

        private void OnProcessExit(object sender, EventArgs e)
        {
            _logger.LogInformation("Process exit event received.");
            _cancellationTokenSource.Cancel();
        }

        /// <summary>
        /// Attempts to gracefully stop the program.
        /// </summary>
        /// <param name="cancellationToken">Used to indicate when stop should no longer be graceful.</param>
        /// <returns>A <see cref="Task"/> that will be completed when the <see cref="IHost"/> stops.</returns>
        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            _logger.LogInformation("Stopping hosted services...");
            var hostApplicationLifetime = _serviceProvider.GetRequiredService<IHostApplicationLifetime>()
                as HostApplicationLifetime ?? throw new InvalidOperationException("Host application lifetime is not registered.");
            hostApplicationLifetime.NotifyStopping();
            var hostedServices = _serviceProvider.GetServices<IHostedService>();
            List<Task> tasks = new List<Task>();
            foreach (var hostedService in hostedServices)
            {
                try
                {
                    var task = hostedService.StopAsync(cancellationToken);
                    tasks.Add(task);
                    _logger.LogInformation("Stopping '{hostedService}'...", hostedService.GetType().Name);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while stopping hosted service '{hostedService}'.", hostedService.GetType().Name);
                }
            }
            _logger.LogInformation("Stopping bot updates...");
            _cancellationTokenSource.Cancel();
            await Task.WhenAll(tasks);
            hostApplicationLifetime.NotifyStopped();
            hostApplicationLifetime.StopApplication();
        }

        /// <summary>
        /// Disposes the bot.
        /// </summary>
        public void Dispose()
        {
            CheckDisposed();
            GC.SuppressFinalize(this);
            _disposed = true;
        }

        private CancellationToken MergeTokens(CancellationToken token)
        {
            return CancellationTokenSource.CreateLinkedTokenSource(_cancellationTokenSource.Token, token).Token;
        }

        private Task ErrorHandler(ITelegramBotClient client, Exception exception, CancellationToken token)
        {
            CheckDisposed();
            _logger.LogError(exception, "Error occurred while receiving updates.");
            return Task.CompletedTask;
        }

        private async Task UpdateHandler(ITelegramBotClient client, Update update, CancellationToken token)
        {
            CheckDisposed();
            if (update.Message != null && !string.IsNullOrWhiteSpace(update.Message.Text) && !update.Message.Text.StartsWith('/'))
            {
                _logger.LogInformation("Received text message: {Text}.", update.Message.Text);
                var handler = new TextMessageHandler(_controllerMethods, update);
                await HandleRequestAsync(handler, update);
            }
            else if (update.Message != null && !string.IsNullOrWhiteSpace(update.Message.Text) && update.Message.Text.StartsWith('/'))
            {
                _logger.LogInformation("Received text command: {Text}.", update.Message.Text);
                var handler = new TextCommandHandler(_controllerMethods, update);
                await HandleRequestAsync(handler, update);
            }
            else if (update.CallbackQuery != null && update.CallbackQuery.Data != null)
            {
                _logger.LogInformation("Received inline query: {Data}.", update.CallbackQuery.Data);
                var handler = new InlineQueryHandler(_controllerMethods, update);
                await HandleRequestAsync(handler, update);
            }
            else
            {
                _logger.LogWarning("Unsupported update type: {UpdateType}.", update.Type);
            }
        }

        private async Task HandleRequestAsync(ITelegramUpdateHandler handler, Update update)
        {
            CheckDisposed();
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

        private void CheckDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(BotApp));
            }
        }
    }
}