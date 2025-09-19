using System;

namespace SampleApp.Services
{
    // New service implementation in v2.0.0
    public class LoggerService : ILoggerService
    {
        public void Info(string message)
        {
            Console.WriteLine($"[INFO] {DateTime.Now:yyyy-MM-dd HH:mm:ss}: {message}");
        }
        
        public void Error(string message)
        {
            Console.WriteLine($"[ERROR] {DateTime.Now:yyyy-MM-dd HH:mm:ss}: {message}");
        }
        
        public void Warning(string message)
        {
            Console.WriteLine($"[WARNING] {DateTime.Now:yyyy-MM-dd HH:mm:ss}: {message}");
        }
    }
}
