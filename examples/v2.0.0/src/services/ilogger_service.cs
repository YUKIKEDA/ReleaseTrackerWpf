namespace SampleApp.Services
{
    // New interface in v2.0.0
    public interface ILoggerService
    {
        void Info(string message);
        void Error(string message);
        void Warning(string message);
    }
}
