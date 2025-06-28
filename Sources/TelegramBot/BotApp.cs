using System;
using System.Linq;
using System.Threading;
using System.Reflection;
using TelegramBot.Services;
using System.Threading.Tasks;
using TelegramBot.Containers;
using TelegramBot.Controllers;
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
        private readonly ServiceProvider _serviceProvider;
        private readonly CancellationTokenSource _cancellationTokenSource;

        /// <summary>
        /// Gets the services configured for the program (for example, using <see cref="M:HostBuilder.ConfigureServices(Action&lt;HostBuilderContext,IServiceCollection&gt;)" />).
        /// </summary>
        public IServiceProvider Services => _serviceProvider;

        /// <summary>
        /// Creates a new instance of <see cref="BotApp"/>.
        /// </summary>
        /// <param name="serviceProvider">Service provider.</param>
        public BotApp(ServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
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
            var controllerMethods = result
                .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                .ToList();
            BotControllerMethodsContainer container = _serviceProvider.GetService<BotControllerMethodsContainer>()
                ?? throw new InvalidOperationException("Bot controller methods container is not registered.");
            foreach (var method in controllerMethods)
            {
                container.AddMethod(method);
            }
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

            var hostApplicationLifetime = _serviceProvider.GetService<IHostApplicationLifetime>() as HostApplicationLifetime
                ?? throw new InvalidOperationException("Host application lifetime is not registered.");
            hostApplicationLifetime.NotifyStarted();

            var hostedServices = _serviceProvider.GetServices<IHostedService>();
            foreach (var hostedService in hostedServices)
            {
                await hostedService.StartAsync(mergedToken);
                _logger.LogInformation("Started hosted service: '{hostedService}'.", hostedService.GetType().Name);
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

        private void CheckDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(BotApp));
            }
        }
    }
}