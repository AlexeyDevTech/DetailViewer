using DetailViewer.Core.Interfaces;
using System.IO;

namespace DetailViewer.Core.Services
{
    public class FileLogger : ILogger
    {
        private readonly string _logFilePath;
        private static readonly object _logLock = new object();

        public FileLogger(string logFilePath)
        {
            _logFilePath = logFilePath;
            Directory.CreateDirectory(Path.GetDirectoryName(_logFilePath));
        }

        public void LogError(string message, System.Exception ex = null)
        {
            Log("ERROR", message, ex);
        }

        public void LogInfo(string message)
        {
            Log("INFO", message);
        }

        public void LogWarning(string message)
        {
            Log("WARNING", message);
        }

        public void LogDebug(string message)
        {
            Log("DEBUG", message);
        }

        public void Log(string message)
        {
            Log("INFO", message);
        }

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