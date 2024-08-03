using System.Threading.Tasks;
using TelegramBot.Abstractions;

namespace TelegramBot.ActionResults
{
    /// <summary>
    /// Defines an empty result.
    /// </summary>
    public class EmptyResult : IActionResult
    {
        /// <summary>
        /// Does nothing.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>Task representing the result of the action.</returns>
        public Task ExecuteResultAsync(ActionContext context)
        {
            return Task.CompletedTask;
        }
    }
}
