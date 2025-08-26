using DetailViewer.Core.Interfaces;
using System.IO;

namespace DetailViewer.Infrastructure.Services
{
    /// <summary>
    /// Реализация сервиса логирования, которая записывает сообщения в файл.
    /// </summary>
    public class FileLogger : ILogger
    {
        private readonly string _logFilePath;
        private static readonly object _logLock = new object();

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="FileLogger"/>.
        /// </summary>
        /// <param name="logFilePath">Полный путь к файлу лога.</param>
        public FileLogger(string logFilePath)
        {
            _logFilePath = logFilePath;
            var logDirectory = Path.GetDirectoryName(_logFilePath);
            if (!string.IsNullOrEmpty(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }
        }

        /// <inheritdoc/>
        public void LogError(string message, System.Exception ex = null)
        {
            Log("ERROR", message, ex);
        }

        /// <inheritdoc/>
        public void LogInfo(string message)
        {
            Log("INFO", message);
        }

        /// <inheritdoc/>
        public void LogWarning(string message)
        {
            Log("WARNING", message);
        }

        /// <inheritdoc/>
        public void LogDebug(string message)
        {
            Log("DEBUG", message);
        }

        /// <inheritdoc/>
        public void Log(string message)
        {
            Log("INFO", message);
        }

        /// <summary>
        /// Основной метод для записи лога в файл.
        /// </summary>
        /// <param name="level">Уровень логирования (например, INFO, ERROR).</param>
        /// <param name="message">Сообщение для записи.</param>
        /// <param name="ex">Исключение (опционально).</param>
        private void Log(string level, string message, System.Exception ex = null)
        {
            lock (_logLock)
            {
                using (var streamWriter = new StreamWriter(_logFilePath, true))
                {
                    streamWriter.WriteLine($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}");
                    if (ex != null)
                    {
                        streamWriter.WriteLine(ex.ToString());
                    }
                }
            }
        }
    }
}
