using System.Threading.Tasks;

namespace TelegramBot.Abstractions
{
    /// <summary>
    /// Action result interface.
    /// </summary>
    public interface IActionResult
    {
        /// <summary>
        /// Executes the result asynchronously.
        /// </summary>
        /// <param name="context">Action context.</param>
        /// <returns>Task representing the result of the action.</returns>
        Task ExecuteResultAsync(ActionContext context);
    }
}
