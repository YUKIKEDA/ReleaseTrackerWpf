using System.Collections.Generic;

namespace SampleApp.Services
{
    // New service implementation in v2.0.0
    public class ConfigService : IConfigService
    {
        private readonly Dictionary<string, string> _config = new Dictionary<string, string>
        {
            { "Version", "2.0.0" },
            { "DebugMode", "false" },
            { "LogLevel", "Info" }
        };
        
        public string GetValue(string key)
        {
            return _config.TryGetValue(key, out var value) ? value : string.Empty;
        }
        
        public void SetValue(string key, string value)
        {
            _config[key] = value;
        }
    }
}
