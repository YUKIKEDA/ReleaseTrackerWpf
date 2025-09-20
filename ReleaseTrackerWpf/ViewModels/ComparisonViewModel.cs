using System.Collections.ObjectModel;
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
        [NotifyPropertyChangedFor(nameof(BothSnapshotsSelected))]
        private DirectorySnapshot? selectedOldSnapshot;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(BothSnapshotsSelected))]
        private DirectorySnapshot? selectedNewSnapshot;

        #endregion

        #region Properties

        public bool HasSnapshots => AvailableSnapshots.Count > 0;
        public bool HasNoSnapshots => AvailableSnapshots.Count == 0;
        public bool BothSnapshotsSelected => SelectedOldSnapshot != null && SelectedNewSnapshot != null;

        #endregion


        public ComparisonViewModel(ComparisonViewModelArgs args)
        {
            _directoryScanService = args.DirectoryScanService;
            _comparisonService = args.ComparisonService;
            _exportService = args.ExportService;
            _notificationService = args.NotificationService;
            _settingsRepository = args.SettingsRepository;
            _snapshotRepository = args.SnapshotRepository;
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
        private async Task CompareDirectoryAsync()
        {
            // 選択されたスナップショットの検証
            if (SelectedOldSnapshot == null || SelectedNewSnapshot == null)
            {
                _notificationService.ShowInfoBar("警告", "比較するスナップショットを両方選択してください", InfoBarSeverity.Warning, 5);
                return;
            }

            try
            {
                // プログレス付きInfoBarを表示
                _notificationService.ShowProgressInfoBar("処理中", "ディレクトリ構造を比較中...", 0);

                // ディレクトリ構造の比較を実行
                var comparisonResult = await _comparisonService.CompareAsync(SelectedOldSnapshot, SelectedNewSnapshot);

                // 比較結果を表示用データに変換
                await UpdateDisplayedDirectoryStructuresAsync(comparisonResult);

                // 完了InfoBarを表示
                var changeCount = comparisonResult.TotalAddedCount + comparisonResult.TotalDeletedCount + comparisonResult.TotalModifiedCount;
                var message = changeCount > 0 
                    ? $"比較が完了しました（{changeCount}個の変更を検出）" 
                    : "比較が完了しました（変更はありませんでした）";
                
                _notificationService.ShowInfoBar("通知", message, InfoBarSeverity.Success, 5);
            }
            catch (Exception ex)
            {
                // エラーInfoBarを表示
                _notificationService.ShowInfoBar("エラー", $"比較中にエラーが発生しました: {ex.Message}", InfoBarSeverity.Error, 0);
            }
        }

        [RelayCommand]
        private Task ExportResultsAsync()
        {
            // TODO: Implement export functionality
            // エクスポート成功のInfoBarを表示
            _notificationService.ShowInfoBar("通知", "エクスポートが完了しました", InfoBarSeverity.Success, 5);
            return Task.CompletedTask;
        }

        [RelayCommand]
        private Task ImportDescriptionsAsync()
        {
            // TODO : Implement import functionality
            // インポート成功のInfoBarを表示
            _notificationService.ShowInfoBar("通知", "説明のインポートが完了しました", InfoBarSeverity.Success, 5);
            return Task.CompletedTask;
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

        #region Private Methods

        /// <summary>
        /// 比較結果を表示用のディレクトリ構造に変換します
        /// </summary>
        private async Task UpdateDisplayedDirectoryStructuresAsync(ComparisonResult comparisonResult)
        {
            
        }

        #endregion
    }
}
