using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Threading;
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
        private readonly ComparisonExportService _comparisonExportService;
        private readonly ImportDescriptionService _importDescriptionService;
        private readonly INotificationService _notificationService;
        private readonly ISettingsRepository _settingsRepository;
        private readonly ISnapshotRepository _snapshotRepository;

        #region Observable Properties

        public ObservableCollection<DirectorySnapshot> AvailableSnapshots { get; } = [];
        public ObservableCollection<FileItemViewModel> OldDisplayedDirectoryStructure { get; } = [];
        public ObservableCollection<FileItemViewModel> NewDisplayedDirectoryStructure { get; } = [];
        public ObservableCollection<FileItemViewModel> OldFlattenedDirectoryStructure { get; } = [];
        public ObservableCollection<FileItemViewModel> NewFlattenedDirectoryStructure { get; } = [];

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(BothSnapshotsSelected))]
        private DirectorySnapshot? selectedOldSnapshot;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(BothSnapshotsSelected))]
        private DirectorySnapshot? selectedNewSnapshot;

        [ObservableProperty]
        private bool isComparison;

        [ObservableProperty]
        private string progressMessage = "処理中...";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasComparisonResult))]
        private ComparisonResult? comparisonResult;

        #endregion

        #region Properties

        public bool HasSnapshots => AvailableSnapshots.Count > 0;
        public bool HasNoSnapshots => AvailableSnapshots.Count == 0;
        public bool BothSnapshotsSelected => SelectedOldSnapshot != null && SelectedNewSnapshot != null;
        public bool HasComparisonResult => ComparisonResult != null;

        #endregion

        #region Property Change Handlers

        async partial void OnSelectedOldSnapshotChanged(DirectorySnapshot? value)
        {
            OnPropertyChanged(nameof(BothSnapshotsSelected));

            // 自動スキャンが有効で、両方のスナップショットが選択された場合、自動で比較を実行
            if (BothSnapshotsSelected && !IsComparison)
            {
                var settings = await _settingsRepository.GetAsync();
                if (settings.AutoScanEnabled)
                {
                    await CompareDirectoryAsync();
                }
            }
        }

        async partial void OnSelectedNewSnapshotChanged(DirectorySnapshot? value)
        {
            OnPropertyChanged(nameof(BothSnapshotsSelected));

            // 自動スキャンが有効で、両方のスナップショットが選択された場合、自動で比較を実行
            if (BothSnapshotsSelected && !IsComparison)
            {
                var settings = await _settingsRepository.GetAsync();
                if (settings.AutoScanEnabled)
                {
                    await CompareDirectoryAsync();
                }
            }
        }

        #endregion

        public ComparisonViewModel(ComparisonViewModelArgs args)
        {
            _directoryScanService = args.DirectoryScanService;
            _comparisonService = args.ComparisonService;
            _exportService = args.ExportService;
            _comparisonExportService = args.ComparisonExportService;
            _importDescriptionService = args.ImportDescriptionService;
            _notificationService = args.NotificationService;
            _settingsRepository = args.SettingsRepository;
            _snapshotRepository = args.SnapshotRepository;

            // ObservableCollectionの変更をHasSnapshots/HasNoSnapshotsプロパティに通知
            AvailableSnapshots.CollectionChanged += (_, _) =>
            {
                OnPropertyChanged(nameof(HasSnapshots));
                OnPropertyChanged(nameof(HasNoSnapshots));
            };
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
                // 進捗状態をオンに設定
                ProgressMessage = "スナップショットを作成中...";
                IsComparison = true;

                try
                {                
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
                finally
                {
                    // 進捗状態をオフに設定
                    IsComparison = false;
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

            // 進捗状態をオンに設定
            ProgressMessage = "ディレクトリ構造を比較中...";
            IsComparison = true;

            try
            {
                // ディレクトリ構造の比較を実行
                var result = await _comparisonService.CompareAsync(SelectedOldSnapshot, SelectedNewSnapshot);
                ComparisonResult = result;

                // UIの更新を確実に完了させる
                await Dispatcher.CurrentDispatcher.InvokeAsync(() => { }, DispatcherPriority.Render);

                // 比較結果を表示用データに変換
                CreateCompareResultForDisplay(result);

                // 完了InfoBarを表示
                var statistics = result.Statistics;
                var changeCount = statistics.AddedFiles + statistics.DeletedFiles + statistics.ModifiedFiles +
                                statistics.AddedDirectories + statistics.DeletedDirectories;
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
            finally
            {
                // 進捗状態をオフに設定
                IsComparison = false;
            }
        }

        [RelayCommand]
        private async Task ExportOldSnapshotAsync()
        {
            if (SelectedOldSnapshot == null)
            {
                _notificationService.ShowInfoBar("警告", "比較元のスナップショットを選択してください", InfoBarSeverity.Warning, 5);
                return;
            }

            await ExportSingleSnapshotAsync(SelectedOldSnapshot);
        }

        [RelayCommand]
        private async Task ExportNewSnapshotAsync()
        {
            if (SelectedNewSnapshot == null)
            {
                _notificationService.ShowInfoBar("警告", "比較先のスナップショットを選択してください", InfoBarSeverity.Warning, 5);
                return;
            }

            await ExportSingleSnapshotAsync(SelectedNewSnapshot);
        }

        private async Task ExportSingleSnapshotAsync(DirectorySnapshot snapshot)
        {
            // ファイル保存ダイアログを表示
            var saveDialog = new SaveFileDialog
            {
                Title = "CSVファイルを保存",
                Filter = "CSVファイル (*.csv)|*.csv|すべてのファイル (*.*)|*.*",
                DefaultExt = "csv",
                FileName = $"スナップショット_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            };

            if (saveDialog.ShowDialog() == true)
            {
                // 進捗状態をオンに設定
                ProgressMessage = "CSVファイルをエクスポート中...";
                IsComparison = true;

                try
                {
                    // 設定を取得してパス表示形式を決定
                    var settings = await _settingsRepository.GetAsync();
                    
                    // スナップショットをCSVにエクスポート
                    await _exportService.ExportToCsvAsync(snapshot, saveDialog.FileName, settings.CsvPathDisplayFormat);

                    // 完了InfoBarを表示
                    _notificationService.ShowInfoBar("通知", "CSVファイルのエクスポートが完了しました", InfoBarSeverity.Success, 5);
                }
                catch (Exception ex)
                {
                    // エラーInfoBarを表示
                    _notificationService.ShowInfoBar("エラー", $"エクスポート中にエラーが発生しました: {ex.Message}", InfoBarSeverity.Error, 0);
                }
                finally
                {
                    // 進捗状態をオフに設定
                    IsComparison = false;
                }
            }
        }

        [RelayCommand]
        private async Task ImportOldDescriptionsAsync()
        {
            if (SelectedOldSnapshot == null)
            {
                _notificationService.ShowInfoBar("警告", "比較元のスナップショットを選択してください", InfoBarSeverity.Warning, 5);
                return;
            }

            await ImportDescriptionsForSnapshotAsync(SelectedOldSnapshot, "比較元");
        }

        [RelayCommand]
        private async Task ImportNewDescriptionsAsync()
        {
            if (SelectedNewSnapshot == null)
            {
                _notificationService.ShowInfoBar("警告", "比較先のスナップショットを選択してください", InfoBarSeverity.Warning, 5);
                return;
            }

            await ImportDescriptionsForSnapshotAsync(SelectedNewSnapshot, "比較先");
        }

        [RelayCommand]
        private async Task ExportComparisonExcelAsync()
        {
            if (ComparisonResult == null)
            {
                _notificationService.ShowInfoBar("警告", "比較結果がありません。先に比較を実行してください", InfoBarSeverity.Warning, 5);
                return;
            }

            // ファイル保存ダイアログを表示
            var saveDialog = new SaveFileDialog
            {
                Title = "Excelファイルを保存",
                Filter = "Excelファイル (*.xlsx)|*.xlsx|すべてのファイル (*.*)|*.*",
                DefaultExt = "xlsx",
                FileName = $"比較結果_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
            };

            if (saveDialog.ShowDialog() == true)
            {
                // 進捗状態をオンに設定
                ProgressMessage = "Excelファイルをエクスポート中...";
                IsComparison = true;

                try
                {
                    // 色付きExcelエクスポートを実行
                    await _comparisonExportService.ExportComparisonToColoredExcelAsync(ComparisonResult, saveDialog.FileName);

                    // 完了InfoBarを表示
                    _notificationService.ShowInfoBar("通知", "Excelファイルのエクスポートが完了しました", InfoBarSeverity.Success, 5);
                }
                catch (Exception ex)
                {
                    // エラーInfoBarを表示
                    _notificationService.ShowInfoBar("エラー", $"エクスポート中にエラーが発生しました: {ex.Message}", InfoBarSeverity.Error, 0);
                }
                finally
                {
                    // 進捗状態をオフに設定
                    IsComparison = false;
                }
            }
        }

        private async Task ImportDescriptionsForSnapshotAsync(DirectorySnapshot snapshot, string snapshotType)
        {
            // ファイル選択ダイアログを表示
            var openDialog = new OpenFileDialog
            {
                Title = $"{snapshotType}用の説明が追加されたCSVファイルを選択",
                Filter = "CSVファイル (*.csv)|*.csv|すべてのファイル (*.*)|*.*",
                DefaultExt = "csv"
            };

            if (openDialog.ShowDialog() == true)
            {
                // 進捗状態をオンに設定
                ProgressMessage = "CSVファイルから説明をインポート中...";
                IsComparison = true;

                try
                {
                    // CSVファイルから説明をインポート
                    var descriptions = await _importDescriptionService.ImportDescriptionsFromCsvAsync(openDialog.FileName);

                    // スナップショットに説明を適用
                    _importDescriptionService.UpdateSnapshotDescriptions(snapshot, descriptions);

                    // 更新されたスナップショットを保存
                    var settings = await _settingsRepository.GetAsync();
                    var fileName = string.Format(DirectorySnapshot.SnapshotFileNameFormat, snapshot.CreatedAt);
                    var filePath = Path.Combine(settings.SnapshotsDirectory, fileName);
                    await _snapshotRepository.SaveSnapshotAsync(snapshot, filePath);

                    // 完了InfoBarを表示
                    var updatedDescriptionsCount = descriptions.Count;
                    var message = updatedDescriptionsCount > 0
                        ? $"{snapshotType}の説明のインポートが完了しました（{updatedDescriptionsCount}個の説明を更新）"
                        : $"{snapshotType}の説明のインポートが完了しました（更新された説明はありませんでした）";

                    _notificationService.ShowInfoBar("通知", message, InfoBarSeverity.Success, 5);
                }
                catch (Exception ex)
                {
                    // エラーInfoBarを表示
                    _notificationService.ShowInfoBar("エラー", $"インポート中にエラーが発生しました: {ex.Message}", InfoBarSeverity.Error, 0);
                }
                finally
                {
                    // 進捗状態をオフに設定
                    IsComparison = false;
                }
            }
        }

        #endregion

        #region Public Methods

        public async Task LoadAvailableSnapshotsAsync(string snapshotsDirectory)
        {
            // 現在の選択状態を保存
            var oldSelectedPath = SelectedOldSnapshot?.RootPath;
            var newSelectedPath = SelectedNewSnapshot?.RootPath;

            // 既存のスナップショットをクリアして重複を防ぐ
            AvailableSnapshots.Clear();

            var settings = await _settingsRepository.GetAsync();
            var snapshotFiles = Directory.GetFiles(settings.SnapshotsDirectory, DirectorySnapshot.SnapshotFilePattern);
            foreach (var snapshotFile in snapshotFiles)
            {
                var snapshot = await _snapshotRepository.LoadSnapshotAsync(snapshotFile);
                AvailableSnapshots.Add(snapshot);
            }

            // 選択状態を復元
            if (oldSelectedPath != null)
            {
                SelectedOldSnapshot = AvailableSnapshots.FirstOrDefault(s => s.RootPath == oldSelectedPath);
            }
            if (newSelectedPath != null)
            {
                SelectedNewSnapshot = AvailableSnapshots.FirstOrDefault(s => s.RootPath == newSelectedPath);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 比較結果を表示用のディレクトリ構造に変換します
        /// </summary>
        private void CreateCompareResultForDisplay(ComparisonResult comparisonResult)
        {
            // 既存の表示データをクリア
            OldDisplayedDirectoryStructure.Clear();
            NewDisplayedDirectoryStructure.Clear();
            OldFlattenedDirectoryStructure.Clear();
            NewFlattenedDirectoryStructure.Clear();

            // 左側（旧ディレクトリ構造）の表示データを作成
            for (int i = 0; i < comparisonResult.LeftTreeItems.Count; i++)
            {
                var entry = comparisonResult.LeftTreeItems[i];
                var viewModel = FileItemViewModel.FromModel(entry);
                viewModel.IsLastChild = (i == comparisonResult.LeftTreeItems.Count - 1);
                OldDisplayedDirectoryStructure.Add(viewModel);
                FlattenTreeStructure(viewModel, OldFlattenedDirectoryStructure, 0, new List<bool>());
            }

            // 右側（新ディレクトリ構造）の表示データを作成
            for (int i = 0; i < comparisonResult.RightTreeItems.Count; i++)
            {
                var entry = comparisonResult.RightTreeItems[i];
                var viewModel = FileItemViewModel.FromModel(entry);
                viewModel.IsLastChild = (i == comparisonResult.RightTreeItems.Count - 1);
                NewDisplayedDirectoryStructure.Add(viewModel);
                FlattenTreeStructure(viewModel, NewFlattenedDirectoryStructure, 0, new List<bool>());
            }
        }

        /// <summary>
        /// ツリー構造をフラットなリストに変換します
        /// </summary>
        /// <param name="item">現在のアイテム</param>
        /// <param name="flatList">フラットリスト</param>
        /// <param name="depth">深さ</param>
        /// <param name="parentIsLastChild">各深さレベルで親が最後の子かどうかのリスト</param>
        private void FlattenTreeStructure(FileItemViewModel item, ObservableCollection<FileItemViewModel> flatList, int depth, List<bool> parentIsLastChild)
        {
            item.Depth = depth;

            // TreePrefixを構築
            var prefix = new System.Text.StringBuilder();

            // 各祖先レベルの線を追加
            for (int i = 0; i < parentIsLastChild.Count; i++)
            {
                if (parentIsLastChild[i])
                {
                    prefix.Append("  "); // 親が最後の子だった場合はスペース
                }
                else
                {
                    prefix.Append("│ "); // 親がまだ続いている場合は縦線
                }
            }

            // 自分の位置を示す線を追加
            if (depth > 0)
            {
                if (item.IsLastChild)
                {
                    prefix.Append("└─");
                }
                else
                {
                    prefix.Append("├─");
                }
            }

            item.TreePrefix = prefix.ToString();
            flatList.Add(item);

            // 子要素を処理
            for (int i = 0; i < item.Children.Count; i++)
            {
                var child = item.Children[i];
                child.IsLastChild = (i == item.Children.Count - 1);

                var newParentIsLastChild = new List<bool>(parentIsLastChild);
                if (depth > 0)
                {
                    newParentIsLastChild.Add(item.IsLastChild);
                }

                FlattenTreeStructure(child, flatList, depth + 1, newParentIsLastChild);
            }
        }

        #endregion
    }
}
