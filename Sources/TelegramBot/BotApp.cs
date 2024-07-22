using System;
using System.Linq;
using Telegram.Bot;
using System.Threading;
using System.Reflection;
using Telegram.Bot.Types;
using TelegramBot.Handlers;
using System.Threading.Tasks;
using TelegramBot.Extensions;
using TelegramBot.Controllers;
using TelegramBot.Abstractions;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace TelegramBot
{
    /// <summary>
    /// Telegram bot application.
    /// </summary>
    public class BotApp : IBot
    {
        private readonly ILogger<BotApp> _logger;
        private readonly TelegramBotClient _client;
        private readonly ServiceProvider _serviceProvider;
        private IReadOnlyCollection<MethodInfo> _controllerMethods;

        /// <summary>
        /// Creates a new instance of <see cref="BotApp"/>.
        /// </summary>
        /// <param name="client">Telegram bot client.</param>
        /// <param name="serviceProvider">Service provider.</param>
        public BotApp(TelegramBotClient client, ServiceProvider serviceProvider)
        {
            _client = client;
            _serviceProvider = serviceProvider;
            _controllerMethods = new List<MethodInfo>();
            _logger = serviceProvider.GetRequiredService<ILogger<BotApp>>();
        }

        /// <summary>
        /// Maps controllers inherited from <see cref="BotControllerBase"/>.
        /// </summary>
        public IBot MapControllers()
        {
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
        /// <param name="token">Cancellation token (optional).</param>
        public void Run(CancellationToken token = default)
        {
            try
            {
                var botUser = _client.GetMeAsync().Result;
                _client.StartReceiving(UpdateHandler, ErrorHandler, cancellationToken: token);
                _logger.LogInformation("Bot '{botUser}' started - receiving updates.", botUser.Username);
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
            Task.Delay(-1, token).Wait(token);
        }

        private Task ErrorHandler(ITelegramBotClient client, Exception exception, CancellationToken token)
        {
            _logger.LogError(exception, "Error occurred while receiving updates.");
            return Task.CompletedTask;
        }

        private async Task UpdateHandler(ITelegramBotClient client, Update update, CancellationToken token)
        {
            if (update.Message != null && !string.IsNullOrWhiteSpace(update.Message.Text) && update.Message.Text.StartsWith('/'))
            {
                _logger.LogInformation("Received text message: {Text}.", update.Message.Text);
                var handler = new TextMessageHandler(_controllerMethods, update);
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
            bool hasUser = update.TryGetUser(out User user);
            if (!hasUser)
            {
                return;
            }
            var args = handler.GetArguments();
            MethodInfo method = handler.GetMethodInfo();
            if (method.ReturnType != typeof(Task<IActionResult>) && method.ReturnType != typeof(IActionResult))
            {
                throw new InvalidOperationException("Invalid return type: " + method.ReturnType.Name);
            }
            BotControllerBase controller = (BotControllerBase)ActivatorUtilities.CreateInstance(_serviceProvider, method.DeclaringType);
            controller.Update = update;
            controller.User = user;
            if (_serviceProvider.GetService<IKeyValueProvider>() is IKeyValueProvider keyValueProvider)
            {
                controller.KeyValueProvider = keyValueProvider;
            }
            var result = method.Invoke(controller, args);
            if (result is Task<IActionResult> taskResult)
            {
                await (await taskResult).ExecuteResultAsync(new ActionContext(_client, user.Id));
            }
            else if (result is IActionResult actionResult)
            {
                await actionResult.ExecuteResultAsync(new ActionContext(_client, user.Id));
            }
            else
            {
                throw new InvalidOperationException("Invalid result type: " + result.GetType().Name);
            }
        }
    }
}