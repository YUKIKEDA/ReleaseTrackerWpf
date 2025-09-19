using System;

namespace SampleApp
{
    public static class Utils
    {
        public static void LogMessage(string message)
        {
            Console.WriteLine($"[LOG] {message}");
        }
        
        // New method in v1.1.0
        public static void LogError(string error)
        {
            Console.WriteLine($"[ERROR] {error}");
        }
    }
}
