namespace SampleApp.Services
{
    // New interface in v2.0.0
    public interface IConfigService
    {
        string GetValue(string key);
        void SetValue(string key, string value);
    }
}
