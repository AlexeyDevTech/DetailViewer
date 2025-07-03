using DetailViewer.Core.Interfaces;
using System;
using System.IO;

namespace DetailViewer.Core.Services
{
    public class FileLogger : ILogger
    {
        private readonly string _logFilePath;
        private static readonly object _lock = new object();

        public FileLogger(string logFilePath)
        {
            _logFilePath = logFilePath;
            // Ensure the directory exists
            var logDirectory = Path.GetDirectoryName(_logFilePath);
            if (!string.IsNullOrEmpty(logDirectory) && !Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }
        }

        public void LogInformation(string message)
        {
            WriteLog("INFO", message);
        }

        public void LogWarning(string message)
        {
            WriteLog("WARN", message);
        }

        public void LogError(string message, Exception exception = null)
        {
            string logMessage = message;
            if (exception != null)
            {
                logMessage += $"\nException: {exception.GetType().Name}\nMessage: {exception.Message}\nStackTrace: {exception.StackTrace}";
                if (exception.InnerException != null)
                {
                    logMessage += $"\nInner Exception: {exception.InnerException.GetType().Name}\nInner Message: {exception.InnerException.Message}\nInner StackTrace: {exception.InnerException.StackTrace}";
                }
            }
            WriteLog("ERROR", logMessage);
        }

        private void WriteLog(string level, string message)
        {
            lock (_lock)
            {
                try
                {
                    using (StreamWriter sw = new StreamWriter(_logFilePath, true))
                    {
                        sw.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{level}] {message}");
                    }
                }
                catch (Exception ex)
                {
                    // Fallback: write to debug console if logging to file fails
                    System.Diagnostics.Debug.WriteLine($"Failed to write to log file: {ex.Message}");
                }
            }
        }
    }
}
