using System;
using System.Threading.Tasks;
using TelegramBot.Abstractions;

namespace TelegramBot.ActionResults
{
    /// <summary>
    /// Combines multiple action results into one.
    /// </summary>
    public class MultiActionResult : IActionResult
    {
        private readonly IActionResult[] actions;

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiActionResult"/> class.
        /// </summary>
        /// <param name="actions">Array of action results to combine.</param>
        public MultiActionResult(IActionResult[] actions)
        {
            if (actions == null || actions.Length == 0)
            {
                throw new ArgumentNullException(nameof(actions), "At least one action must be provided.");
            }
            this.actions = actions;
        }

        /// <summary>
        /// Executes the combined action results asynchronously.
        /// </summary>
        /// <param name="context">Action context containing the bot and chat information.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task ExecuteResultAsync(ActionContext context)
        {
            foreach (var action in actions)
            {
                await action.ExecuteResultAsync(context);
            }
        }
    }
}
