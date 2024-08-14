using System;
using System.IO;
using Telegram.Bot;
using Telegram.Bot.Types;
using System.Threading.Tasks;
using TelegramBot.Abstractions;

namespace TelegramBot.ActionResults
{
    /// <summary>
    /// File result with disposing.
    /// </summary>
    public class FileResult : IActionResult, IDisposable
    {
        private bool _disposed;
        private readonly string _fileName;
        private readonly Stream _fileStream;
        private readonly bool _disposeStream = true;

        /// <summary>
        /// Creates a new instance of <see cref="FileResult"/>.
        /// </summary>
        /// <param name="filePath">Full path to the file.</param>
        public FileResult(string filePath)
        {
            _fileName = Path.GetFileName(filePath);
            _fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        }

        /// <summary>
        /// Creates a new instance of <see cref="FileResult"/>.
        /// </summary>
        /// <param name="stream">Stream with file content.</param>
        /// <param name="fileName">File name.</param>
        /// <param name="disposeStream">Dispose the stream after sending the file.</param>
        public FileResult(Stream stream, string fileName, bool disposeStream = true)
        {
            _fileName = fileName;
            _fileStream = stream;
            _disposeStream = disposeStream;
        }

        /// <summary>
        /// Disposes the file stream if it is applicable.
        /// </summary>
        public void Dispose()
        {
            if (!_disposed && _disposeStream)
            {
                _fileStream.Dispose();
                _disposed = true;
            }
        }

        /// <summary>
        /// Executes the result asynchronously.
        /// </summary>
        /// <param name="context">Action context.</param>
        /// <returns>The task representing the result of the action.</returns>
        public Task ExecuteResultAsync(ActionContext context)
        {
            InputFile file = InputFile.FromStream(_fileStream, _fileName);
            return context.Bot.SendDocumentAsync(context.ChatId, document: file);
        }
    }
}
