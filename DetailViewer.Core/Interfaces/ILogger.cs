using System;

namespace DetailViewer.Core.Interfaces
{
    /// <summary>
    /// Определяет контракт для сервиса логирования сообщений.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Логирует информационное сообщение.
        /// </summary>
        /// <param name="message">Сообщение для логирования.</param>
        void Log(string message);

        /// <summary>
        /// Логирует сообщение об ошибке.
        /// </summary>
        /// <param name="message">Сообщение об ошибке.</param>
        /// <param name="ex">Исключение (опционально).</param>
        void LogError(string message, Exception ex = null);

        /// <summary>
        /// Логирует информационное сообщение.
        /// </summary>
        /// <param name="message">Информационное сообщение.</param>
        void LogInfo(string message);

        /// <summary>
        /// Логирует предупреждение.
        /// </summary>
        /// <param name="message">Сообщение-предупреждение.</param>
        void LogWarning(string message);

        /// <summary>
        /// Логирует отладочное сообщение.
        /// </summary>
        /// <param name="message">Отладочное сообщение.</param>
        void LogDebug(string message);
    }
}
