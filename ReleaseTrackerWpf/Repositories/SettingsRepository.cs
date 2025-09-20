using System.IO;
using System.Text.Json;
using ReleaseTrackerWpf.Models;

namespace ReleaseTrackerWpf.Repositories
{
    public class SettingsRepository : ISettingsRepository
    {
        public const string SettingsDirectoryName = "ReleaseTracker";
        public const string SettingsFileName = "settings.json";

        private static readonly string _appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private static readonly string _settingsDirectoryPath = Path.Combine(_appDataPath, SettingsDirectoryName);
        private static readonly string _settingsFilePath = Path.Combine(_settingsDirectoryPath, SettingsFileName);

        public SettingsRepository()
        {
            Directory.CreateDirectory(_settingsDirectoryPath);
        }

        public async Task<SettingsData> GetAsync()
        {
            try
            {
                if (File.Exists(_settingsFilePath))
                {
                    var json = await File.ReadAllTextAsync(_settingsFilePath);
                    return JsonSerializer.Deserialize<SettingsData>(json) ?? GetDefaultSettings();
                }
                else
                {
                    return GetDefaultSettings();
                }
            }
            catch (Exception)
            {
                // 設定ファイルの読み込みに失敗した場合はデフォルト設定を使用
                return GetDefaultSettings();
            }
        }

        public async Task SaveAsync(SettingsData settings)
        {
            try
            {
                var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(_settingsFilePath, json);
            }
            catch (Exception)
            {
                throw new Exception("Failed to save settings");
            }
        }

        private static SettingsData GetDefaultSettings()
        {
            return new SettingsData(
                SnapshotsDirectory: Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ReleaseTracker", "Snapshots"),
                AutoScanEnabled: true
            );
        }
    }
}
