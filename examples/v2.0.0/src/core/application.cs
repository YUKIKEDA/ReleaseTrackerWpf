using System;
using SampleApp.Services;

namespace SampleApp.Core
{
    // New architecture in v2.0.0
    public class Application
    {
        private readonly ILoggerService _logger;
        private readonly IConfigService _config;
        
        public Application()
        {
            _logger = new LoggerService();
            _config = new ConfigService();
        }
        
        public void Run(string[] args)
        {
            _logger.Info("Application starting...");
            
            if (args.Length > 0)
            {
                _logger.Info($"Processing arguments: {string.Join(", ", args)}");
            }
            
            _logger.Info("Application completed successfully");
        }
    }
}
