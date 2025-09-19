using System;
using SampleApp.Core;
using SampleApp.Services;

namespace SampleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to Sample Application!");
            Console.WriteLine("Version 2.0.0");
            
            var app = new Application();
            app.Run(args);
        }
    }
}
