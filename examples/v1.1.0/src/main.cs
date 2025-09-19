using System;

namespace SampleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            Console.WriteLine("Version 1.1.0");
            
            // New feature in v1.1.0
            if (args.Length > 0)
            {
                Console.WriteLine($"Arguments: {string.Join(", ", args)}");
            }
        }
    }
}
