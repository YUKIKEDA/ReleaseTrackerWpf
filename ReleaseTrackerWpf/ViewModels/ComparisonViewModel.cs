using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using ReleaseTrackerWpf.Models;
using ReleaseTrackerWpf.Repositories;
using ReleaseTrackerWpf.Services;
using Wpf.Ui.Controls;

namespace ReleaseTrackerWpf.ViewModels
{
    public partial class ComparisonViewModel : ObservableObject
    {
        private readonly DirectoryScanService _directoryScanService;
        private readonly ComparisonService _comparisonService;
        private readonly ExportService _exportService;
        private readonly INotificationService _notificationService;
        private readonly ISettingsRepository _settingsRepository;
        private readonly ISnapshotRepository _snapshotRepository;

        #region Observable Properties

        public ObservableCollection<DirectorySnapshot> AvailableSnapshots { get; } = [];
        public ObservableCollection<FileItemViewModel> OldDisplayedDirectoryStructure { get; } = [];
        public ObservableCollection<FileItemViewModel> NewDisplayedDirectoryStructure { get; } = [];

        [ObservableProperty]
        private DirectorySnapshot? selectedOldSnapshot;

        [ObservableProperty]
        private DirectorySnapshot? selectedNewSnapshot;

        #endregion

        #region Properties

        public bool HasSnapshots => AvailableSnapshots.Count > 0;
        public bool HasNoSnapshots => AvailableSnapshots.Count == 0;

        // InfoBar関連のプロパティ（NotificationServiceから取得）
        public bool IsInfoBarOpen => _notificationService.IsInfoBarOpen;
        public string InfoBarTitle => _notificationService.InfoBarTitle;
        public string InfoBarMessage => _notificationService.InfoBarMessage;
        public InfoBarSeverity InfoBarSeverity => _notificationService.InfoBarSeverity;

        #endregion


        public ComparisonViewModel(ComparisonViewModelArgs args)
        {
            _directoryScanService = args.DirectoryScanService;
            _comparisonService = args.ComparisonService;
            _exportService = args.ExportService;
            _notificationService = args.NotificationService;
            _settingsRepository = args.SettingsRepository;
            _snapshotRepository = args.SnapshotRepository;

            // NotificationServiceの変更を監視
            _notificationService.NotificationChanged += OnNotificationChanged;
        }

        private void OnNotificationChanged(object? sender, NotificationEventArgs e)
        {
            // プロパティ変更を通知
            OnPropertyChanged(nameof(IsInfoBarOpen));
            OnPropertyChanged(nameof(InfoBarTitle));
            OnPropertyChanged(nameof(InfoBarMessage));
            OnPropertyChanged(nameof(InfoBarSeverity));
        }

        #region Commands

        [RelayCommand]
        private async Task AddSnapshotAsync()
        {
            var dialog = new OpenFolderDialog
            {
                Title = "フォルダを選択してスナップショットに追加"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {                
                    // プログレス付きInfoBarを表示
                    _notificationService.ShowProgressInfoBar("処理中", "スナップショットを作成中...", 0);

                    var settings = await _settingsRepository.GetAsync();
                    var snapshot = await _directoryScanService.ScanDirectoryAsync(dialog.FolderName);
                    var fileName = string.Format(DirectorySnapshot.SnapshotFileNameFormat, DateTime.Now);
                    var filePath = Path.Combine(settings.SnapshotsDirectory, fileName);

                    await _snapshotRepository.SaveSnapshotAsync(snapshot, filePath);

                    await LoadAvailableSnapshotsAsync(settings.SnapshotsDirectory);

                    // 完了InfoBarを表示（24時間表示）
                    _notificationService.ShowInfoBar("通知", "スナップショットを作成しました", InfoBarSeverity.Success, 86400); // 24時間 = 86400秒
                }
                catch (Exception ex)
                {
                    // エラーInfoBarを表示
                    _notificationService.ShowInfoBar("エラー", $"スナップショット作成中にエラーが発生しました: {ex.Message}", InfoBarSeverity.Error, 0);
                }
            }
        }

        [RelayCommand]
        private async Task ScanAndCompareDirectoryAsync()
        {
            // TODO : Implement refresh functionality
            // ディレクトリ構造のスキャンを再実行

            // ディレクトリ構造の比較を再実行

            // リフレッシュ成功のInfoBarを表示
            _notificationService.ShowInfoBar("通知", "説明のインポートが完了しました", InfoBarSeverity.Success, 5);
        }

        [RelayCommand]
        private async Task ExportResultsAsync()
        {
            // TODO: Implement export functionality
            // エクスポート成功のInfoBarを表示
            _notificationService.ShowInfoBar("通知", "エクスポートが完了しました", InfoBarSeverity.Success, 5);
        }

        [RelayCommand]
        private async Task ImportDescriptionsAsync()
        {
            // TODO : Implement import functionality
            // インポート成功のInfoBarを表示
            _notificationService.ShowInfoBar("通知", "説明のインポートが完了しました", InfoBarSeverity.Success, 5);
        }

        #endregion

        #region Public Methods

        public async Task LoadAvailableSnapshotsAsync(string snapshotsDirectory)
        {
            var settings = await _settingsRepository.GetAsync();
            var snapshotFiles = Directory.GetFiles(settings.SnapshotsDirectory, DirectorySnapshot.SnapshotFilePattern);
            foreach (var snapshotFile in snapshotFiles)
            {
                var snapshot = await _snapshotRepository.LoadSnapshotAsync(snapshotFile);
                AvailableSnapshots.Add(snapshot);
            }
        }

        #endregion
    }
}
