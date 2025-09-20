using System.Diagnostics;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using ReleaseTrackerWpf.Models;
using ReleaseTrackerWpf.Services;

namespace ReleaseTrackerWpf.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        private readonly ISettingsService _settingsService;

        #region Observable Properties

        [ObservableProperty]
        private string snapshotsDirectory = string.Empty;

        [ObservableProperty]
        private bool autoScanEnabled = false;

        #endregion

        public SettingsViewModel(SettingsViewModelArgs args)
        {
            _settingsService = args.SettingsService;
            _ = LoadSettingsAsync();
        }

        #region Commands

        [RelayCommand]
        private void ChangeSnapshotsFolder()
        {
            var dialog = new OpenFileDialog
            {
                Title = "スナップショット保存先フォルダを選択",
                FileName = "フォルダを選択",
                Filter = "Folder|*.folder",
                CheckFileExists = false,
                CheckPathExists = true
            };

            var result = dialog.ShowDialog();
            if (result == true)
            {
                var selectedPath = Path.GetDirectoryName(dialog.FileName);
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    SnapshotsDirectory = selectedPath;
                    _ = SaveSettingsAsync();
                }
            }
        }

        [RelayCommand]
        private void OpenSnapshotsFolder()
        {
            if (Directory.Exists(SnapshotsDirectory))
            {
                Process.Start("explorer.exe", SnapshotsDirectory);
            }
        }

        #endregion

        #region Private Methods

        private async Task LoadSettingsAsync()
        {
            await _settingsService.LoadSettingsAsync();
            SnapshotsDirectory = _settingsService.SnapshotsDirectory;
            AutoScanEnabled = _settingsService.AutoScanEnabled;
        }

        private async Task SaveSettingsAsync()
        {
            _settingsService.SnapshotsDirectory = SnapshotsDirectory;
            _settingsService.AutoScanEnabled = AutoScanEnabled;
            await _settingsService.SaveSettingsAsync();
        }

        #endregion

        #region Property Changed Handlers

        partial void OnSnapshotsDirectoryChanged(string value)
        {
            _ = SaveSettingsAsync();
        }

        partial void OnAutoScanEnabledChanged(bool value)
        {
            _ = SaveSettingsAsync();
        }

        #endregion
    }
}
