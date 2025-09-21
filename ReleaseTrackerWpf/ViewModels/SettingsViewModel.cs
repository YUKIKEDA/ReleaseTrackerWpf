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

        [ObservableProperty]
        private ExportedCsvPathFormat csvPathDisplayFormat = ExportedCsvPathFormat.Normal;

        #endregion

        public SettingsViewModel(SettingsViewModelArgs args)
        {
            _settingsRepository = args.SettingsRepository;
            _notificationService = args.NotificationService;
        }

        async partial void OnAutoScanEnabledChanged(bool value)
        {
            // 設定を自動保存
            var settings = new SettingsData(SnapshotsDirectory, value, CsvPathDisplayFormat);
            await _settingsRepository.SaveAsync(settings);
            
            // 設定保存完了を通知
            var message = value ? "自動スキャンが有効になりました" : "自動スキャンが無効になりました";
            _notificationService.ShowInfoBar("設定保存完了", message, InfoBarSeverity.Success, 3);
        }

        async partial void OnCsvPathDisplayFormatChanged(ExportedCsvPathFormat value)
        {
            // 設定を自動保存
            var settings = new SettingsData(SnapshotsDirectory, AutoScanEnabled, value);
            await _settingsRepository.SaveAsync(settings);
            
            // 設定保存完了を通知
            var message = value == ExportedCsvPathFormat.Tree ? "CSVパス表示形式をツリー形式に変更しました" : "CSVパス表示形式を通常形式に変更しました";
            _notificationService.ShowInfoBar("設定保存完了", message, InfoBarSeverity.Success, 3);
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
                    var settings = new SettingsData(SnapshotsDirectory, AutoScanEnabled, CsvPathDisplayFormat);
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
            CsvPathDisplayFormat = settings.CsvPathDisplayFormat;
        }

        #endregion
    }
}
