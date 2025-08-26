namespace DetailViewer.Core.Interfaces
{
    public interface ILogger
    {
        void Log(string message);
        void LogError(string message, System.Exception ex = null);
        void LogInfo(string message);
        void LogWarning(string message);
        void LogDebug(string message);
    }
}