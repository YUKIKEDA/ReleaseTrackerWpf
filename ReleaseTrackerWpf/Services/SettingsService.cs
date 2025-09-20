using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace ReleaseTrackerWpf.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly string _settingsFilePath;
        private SettingsData _settings;

        public SettingsService()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var settingsDir = Path.Combine(appDataPath, "ReleaseTracker");
            Directory.CreateDirectory(settingsDir);
            _settingsFilePath = Path.Combine(settingsDir, "settings.json");
            
            _settings = new SettingsData();
        }

        public string SnapshotsDirectory
        {
            get => _settings.SnapshotsDirectory;
            set => _settings.SnapshotsDirectory = value;
        }

        public bool AutoScanEnabled
        {
            get => _settings.AutoScanEnabled;
            set => _settings.AutoScanEnabled = value;
        }

        public async Task LoadSettingsAsync()
        {
            try
            {
                if (File.Exists(_settingsFilePath))
                {
                    var json = await File.ReadAllTextAsync(_settingsFilePath);
                    _settings = JsonSerializer.Deserialize<SettingsData>(json) ?? new SettingsData();
                }
                else
                {
                    // デフォルト設定
                    _settings = new SettingsData
                    {
                        SnapshotsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ReleaseTracker", "Snapshots"),
                        AutoScanEnabled = false
                    };
                }
            }
            catch (Exception)
            {
                // 設定ファイルの読み込みに失敗した場合はデフォルト設定を使用
                _settings = new SettingsData
                {
                    SnapshotsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ReleaseTracker", "Snapshots"),
                    AutoScanEnabled = false
                };
            }
        }

        public async Task SaveSettingsAsync()
        {
            try
            {
                var json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(_settingsFilePath, json);
            }
            catch (Exception)
            {
                // 設定の保存に失敗した場合は無視
            }
        }

        private class SettingsData
        {
            public string SnapshotsDirectory { get; set; } = string.Empty;
            public bool AutoScanEnabled { get; set; } = false;
        }
    }
}
