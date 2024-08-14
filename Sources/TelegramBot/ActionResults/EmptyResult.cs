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
        /// Executes the result asynchronously.
        /// </summary>
        /// <param name="context">Action context.</param>
        /// <returns>The task representing the result of the action.</returns>
        public Task ExecuteResultAsync(ActionContext context)
        {
            return Task.CompletedTask;
        }
    }
}
