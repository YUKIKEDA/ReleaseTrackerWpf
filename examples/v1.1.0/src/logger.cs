using System;

namespace SampleApp
{
    // New file in v1.1.0
    public class Logger
    {
        public void Info(string message)
        {
            Console.WriteLine($"[INFO] {DateTime.Now}: {message}");
        }
        
        public void Error(string message)
        {
            Console.WriteLine($"[ERROR] {DateTime.Now}: {message}");
        }
    }
}
