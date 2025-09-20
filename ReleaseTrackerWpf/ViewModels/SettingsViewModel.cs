using System.Diagnostics;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using ReleaseTrackerWpf.Models;
using ReleaseTrackerWpf.Repositories;
using ReleaseTrackerWpf.Services;
using Wpf.Ui.Controls;

namespace ReleaseTrackerWpf.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        private readonly ISettingsRepository _settingsRepository;
        private readonly INotificationService _notificationService;

        #region Observable Properties

        [ObservableProperty]
        private string snapshotsDirectory = string.Empty;

        [ObservableProperty]
        private bool autoScanEnabled = false;

        #endregion

        public SettingsViewModel(SettingsViewModelArgs args)
        {
            _settingsRepository = args.SettingsRepository;
            _notificationService = args.NotificationService;
        }

        #region Commands

        [RelayCommand]
        private async Task ChangeSnapshotsFolder()
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
                    var settings = new SettingsData(SnapshotsDirectory, AutoScanEnabled);
                    await _settingsRepository.SaveAsync(settings);
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
            else
            {
                // スナップショット保存先フォルダが存在しない場合はInfoBarを表示
                _notificationService.ShowInfoBar("警告", "スナップショット保存先フォルダが存在しません。", InfoBarSeverity.Warning, 5);
            }
        }

        #endregion


        #region Public Methods

        public async Task LoadSettingsAsync()
        {
            var settings = await _settingsRepository.GetAsync();
            SnapshotsDirectory = settings.SnapshotsDirectory;
            AutoScanEnabled = settings.AutoScanEnabled;
        }

        #endregion
    }
}
