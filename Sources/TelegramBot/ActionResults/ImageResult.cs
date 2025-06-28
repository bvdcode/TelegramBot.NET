using System;
using System.IO;
using Telegram.Bot;
using Telegram.Bot.Types;
using System.Threading.Tasks;
using TelegramBot.Abstractions;

namespace TelegramBot.ActionResults
{
    /// <summary>
    /// Image result with disposing.
    /// </summary>
    public class ImageResult : IActionResult, IDisposable
    {
        private bool _disposed;
        private readonly string _fileName;
        private readonly Stream _imageStream;
        private readonly bool _disposeStream = true;
        private readonly string _caption = string.Empty;

        /// <summary>
        /// Creates a new instance of <see cref="ImageResult"/>.
        /// </summary>
        /// <param name="filePath">Full path to the image.</param>
        /// <param name="caption">Caption for the image.</param>
        public ImageResult(string filePath, string caption = "")
        {
            _caption = caption;
            _fileName = Path.GetFileName(filePath);
            _imageStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        }

        /// <summary>
        /// Creates a new instance of <see cref="ImageResult"/>.
        /// </summary>
        /// <param name="stream">Stream with image content.</param>
        /// <param name="fileName">File name.</param>
        /// <param name="caption">Caption for the image.</param>
        /// <param name="disposeStream">Dispose the stream after sending the file.</param>
        public ImageResult(Stream stream, string fileName, string caption = "", bool disposeStream = true)
        {
            _caption = caption;
            _fileName = fileName;
            _imageStream = stream;
            _disposeStream = disposeStream;
        }

        /// <summary>
        /// Disposes the file stream if it is applicable.
        /// </summary>
        public void Dispose()
        {
            if (!_disposed && _disposeStream)
            {
                _imageStream.Dispose();
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
            InputFile file = InputFile.FromStream(_imageStream, _fileName);
            return context.Bot.SendPhoto(context.ChatId, photo: file, caption: _caption);
        }
    }
}
