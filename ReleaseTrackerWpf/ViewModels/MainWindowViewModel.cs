using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using ReleaseTrackerWpf.Models;
using ReleaseTrackerWpf.Services;
using Wpf.Ui.Controls;

namespace ReleaseTrackerWpf.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly DirectoryService _directoryService;
        private readonly ComparisonService _comparisonService;
        private readonly ExportService _exportService;

        #region Observable Properties

        public ObservableCollection<DirectorySnapshot> AvailableSnapshots { get; } = [];
        
        [ObservableProperty]
        private DirectorySnapshot? selectedOldSnapshot;

        [ObservableProperty]
        private DirectorySnapshot? selectedNewSnapshot;

        [ObservableProperty]
        private bool autoScanEnabled = false;

        [ObservableProperty]
        private string snapshotsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ReleaseTracker", "Snapshots");

        // InfoBar関連のプロパティ
        [ObservableProperty]
        private bool isInfoBarOpen = false;

        [ObservableProperty]
        private string infoBarTitle = string.Empty;

        [ObservableProperty]
        private string infoBarMessage = string.Empty;
        
        [ObservableProperty]
        private InfoBarSeverity infoBarSeverity = InfoBarSeverity.Informational;

        [ObservableProperty]
        private DirectorySnapshot? newSnapshot;

        [ObservableProperty]
        private string newDirectoryPath = string.Empty;

        #endregion


        #region Properties

        public bool HasSnapshots => AvailableSnapshots.Count > 0;
        public bool HasNoSnapshots => AvailableSnapshots.Count == 0;

        #endregion

        public ObservableCollection<FileItemViewModel> ComparisonResults { get; } = [];
        public DiffViewModel DiffViewModel { get; } = new();

        private ComparisonResult? _lastComparisonResult;
        private System.Timers.Timer? _autoScanTimer;
        private System.Timers.Timer? _infoBarTimer;

        public MainWindowViewModel(DirectoryService directoryService, ComparisonService comparisonService, ExportService exportService)
        {
            _directoryService = directoryService;
            _comparisonService = comparisonService;
            _exportService = exportService;

            // Create snapshots directory if it doesn't exist
            Directory.CreateDirectory(SnapshotsDirectory);

            // Load available snapshots
            _ = LoadAvailableSnapshotsAsync();

            // Setup auto-scan timer
            SetupAutoScanTimer();
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
                    ShowProgressInfoBar("処理中", "スナップショットを作成中...", 0);

                    var snapshot = await _directoryService.ScanDirectoryAsync(dialog.FolderName);
                    var fileName = $"snapshot_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                    var filePath = Path.Combine(SnapshotsDirectory, fileName);

                    await _directoryService.SaveSnapshotAsync(snapshot, filePath);

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        _ = LoadAvailableSnapshotsAsync();
                        
                        // 完了InfoBarを表示（24時間表示）
                        ShowInfoBar("通知", "スナップショットを作成しました", InfoBarSeverity.Success, 86400); // 24時間 = 86400秒
                    });
                }
                catch (Exception ex)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {                        
                        // エラーInfoBarを表示
                        ShowInfoBar("エラー", $"スナップショット作成中にエラーが発生しました: {ex.Message}", InfoBarSeverity.Error, 0);
                    });
                }
            }
        }


        [RelayCommand]
        private async Task ExportResultsAsync()
        {
                if (_lastComparisonResult == null || !_lastComparisonResult.AllDifferences.Any())
            {
                ShowInfoBar("警告", "エクスポートする比較結果がありません。", InfoBarSeverity.Warning, 5);
                return;
            }

            var dialog = new SaveFileDialog
            {
                Title = "比較結果をエクスポート",
                Filter = "Excelファイル (*.xlsx)|*.xlsx|CSVファイル (*.csv)|*.csv|テキストファイル (*.txt)|*.txt",
                DefaultExt = ".xlsx"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var extension = Path.GetExtension(dialog.FileName).ToLower();
                    switch (extension)
                    {
                        case ".xlsx":
                            await _exportService.ExportToExcelAsync(_lastComparisonResult.AllDifferences, dialog.FileName);
                            break;
                        case ".csv":
                            await _exportService.ExportToCsvAsync(_lastComparisonResult.AllDifferences, dialog.FileName);
                            break;
                        case ".txt":
                            await _exportService.ExportToTextAsync(_lastComparisonResult.AllDifferences, dialog.FileName);
                            break;
                    }
                    
                    // エクスポート成功のInfoBarを表示
                    ShowInfoBar("通知", "エクスポートが完了しました", InfoBarSeverity.Success, 5);
                }
                catch (Exception ex)
                {
                    ShowInfoBar("エラー", $"エクスポート中にエラーが発生しました: {ex.Message}", InfoBarSeverity.Error, 0);
                }
            }
        }

        [RelayCommand]
        private async Task ImportDescriptionsAsync()
        {
                if (_lastComparisonResult == null || !_lastComparisonResult.AllDifferences.Any())
            {
                ShowInfoBar("警告", "インポートする比較結果がありません。", InfoBarSeverity.Warning, 5);
                return;
            }

            var dialog = new OpenFileDialog
            {
                Title = "説明ファイルをインポート",
                Filter = "CSVファイル (*.csv)|*.csv",
                CheckFileExists = true
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    Dictionary<string, string> descriptions;
                    var extension = Path.GetExtension(dialog.FileName).ToLower();

                    descriptions = await _exportService.ImportDescriptionsFromCsvAsync(dialog.FileName);

                    // Update descriptions in comparison results
                    foreach (var item in _lastComparisonResult.AllDifferences)
                    {
                        if (descriptions.TryGetValue(item.RelativePath, out var description))
                        {
                            item.Description = description;
                        }
                    }

                    // Update ViewModels
                    foreach (var viewModel in ComparisonResults)
                    {
                        if (descriptions.TryGetValue(viewModel.RelativePath, out var description))
                        {
                            viewModel.Description = description;
                        }
                    }
                    
                    // インポート成功のInfoBarを表示
                    ShowInfoBar("通知", "説明のインポートが完了しました", InfoBarSeverity.Success, 5);
                }
                catch (Exception ex)
                {
                    ShowInfoBar("エラー", $"説明のインポート中にエラーが発生しました: {ex.Message}", InfoBarSeverity.Error, 0);
                }
            }
        }

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

        private async Task LoadAvailableSnapshotsAsync()
        {
            // 現在選択されているスナップショットの情報を保存
            var currentSelection = SelectedOldSnapshot;
            var currentSelectionKey = currentSelection != null 
                ? $"{currentSelection.RootPath}_{currentSelection.CreatedAt:yyyyMMdd_HHmmss}" 
                : null;

            AvailableSnapshots.Clear();

            if (!Directory.Exists(SnapshotsDirectory))
            {
                OnPropertyChanged(nameof(HasSnapshots));
                OnPropertyChanged(nameof(HasNoSnapshots));
                return;
            }

            var jsonFiles = Directory.GetFiles(SnapshotsDirectory, "*.json")
                .OrderByDescending(f => File.GetCreationTime(f));

            var loadedSnapshots = new HashSet<string>(); // 重複を防ぐためのセット
            DirectorySnapshot? restoredSelection = null;

            foreach (var file in jsonFiles)
            {
                try
                {
                    var snapshot = await _directoryService.LoadSnapshotAsync(file);
                    
                    // 同じパスと作成日時のスナップショットが既に読み込まれていないかチェック
                    var snapshotKey = $"{snapshot.RootPath}_{snapshot.CreatedAt:yyyyMMdd_HHmmss}";
                    if (!loadedSnapshots.Contains(snapshotKey))
                    {
                        loadedSnapshots.Add(snapshotKey);
                        AvailableSnapshots.Add(snapshot);

                        // 以前選択されていたスナップショットと同じものを復元
                        if (currentSelectionKey != null && snapshotKey == currentSelectionKey)
                        {
                            restoredSelection = snapshot;
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Skip corrupted files and show warning
                    ShowInfoBar("警告", $"スナップショットファイルの読み込みに失敗しました: {Path.GetFileName(file)} ({ex.Message})", InfoBarSeverity.Warning, 5);
                }
            }

            // 選択を復元
            if (restoredSelection != null)
            {
                SelectedOldSnapshot = restoredSelection;
            }

            OnPropertyChanged(nameof(HasSnapshots));
            OnPropertyChanged(nameof(HasNoSnapshots));
        }

        #endregion


        #region Utility Methods

        /// <summary>
        /// プログレス付きInfoBarを表示します
        /// </summary>
        /// <param name="title">タイトル</param>
        /// <param name="message">メッセージ</param>
        /// <param name="timeoutSeconds">表示時間（秒、0で無制限）</param>
        public void ShowProgressInfoBar(string title, string message, int timeoutSeconds = 0)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                // 既存のタイマーがあればクリア
                _infoBarTimer?.Dispose();

                // InfoBarの設定（プログレス表示用のメッセージに変更）
                InfoBarTitle = title;
                InfoBarMessage = $"🔄 {message}";
                InfoBarSeverity = InfoBarSeverity.Informational;

                // InfoBarを表示
                IsInfoBarOpen = true;

                // タイムアウトが設定されている場合
                if (timeoutSeconds > 0)
                {
                    _infoBarTimer = new System.Timers.Timer(timeoutSeconds * 1000);
                    _infoBarTimer.Elapsed += (s, e) =>
                    {
                        _infoBarTimer.Stop();
                        _infoBarTimer.Dispose();
                        _infoBarTimer = null;
                        
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            IsInfoBarOpen = false;
                        });
                    };
                    _infoBarTimer.Start();
                }
            });
        }

        /// <summary>
        /// InfoBarを表示します
        /// </summary>
        /// <param name="title">タイトル</param>
        /// <param name="message">メッセージ</param>
        /// <param name="severity">InfoBarSeverity</param>
        /// <param name="timeoutSeconds">表示時間（秒、0で無制限）</param>
        public void ShowInfoBar(string title, string message, InfoBarSeverity severity = InfoBarSeverity.Informational, int timeoutSeconds = 0)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                // 既存のタイマーがあればクリア
                _infoBarTimer?.Dispose();

                // InfoBarの設定
                InfoBarTitle = title;
                InfoBarMessage = message;
                InfoBarSeverity = severity;

                // InfoBarを表示
                IsInfoBarOpen = true;

                // タイムアウトが設定されている場合
                if (timeoutSeconds > 0)
                {
                    _infoBarTimer = new System.Timers.Timer(timeoutSeconds * 1000);
                    _infoBarTimer.Elapsed += (s, e) =>
                    {
                        _infoBarTimer.Stop();
                        _infoBarTimer.Dispose();
                        _infoBarTimer = null;
                        
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            IsInfoBarOpen = false;
                        });
                    };
                    _infoBarTimer.Start();
                }
            });
        }

        #endregion

        private void SetupAutoScanTimer()
        {
            _autoScanTimer = new System.Timers.Timer(2000); // 2 seconds delay
            _autoScanTimer.Elapsed += async (s, e) =>
            {
                _autoScanTimer.Stop();
                await ScanNewDirectoryAsync();
                await CompareDirectoriesAsync();
            };
        }

        private async Task ScanNewDirectoryAsync()
        {
            if (string.IsNullOrEmpty(NewDirectoryPath) || !Directory.Exists(NewDirectoryPath))
                return;

            try
            {
                Application.Current.Dispatcher.Invoke(() =>
                {                    
                    // プログレス付きInfoBarを表示
                    ShowProgressInfoBar("処理中", "スキャン中...", 0);
                });

                NewSnapshot = await _directoryService.ScanDirectoryAsync(NewDirectoryPath);

                Application.Current.Dispatcher.Invoke(() =>
                {                    
                    // 完了InfoBarを表示（24時間表示）
                    ShowInfoBar("通知", "スキャンが完了しました", InfoBarSeverity.Success, 86400); // 24時間 = 86400秒
                });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {                    
                    // エラーInfoBarを表示
                    ShowInfoBar("エラー", $"スキャン中にエラーが発生しました: {ex.Message}", InfoBarSeverity.Error, 0);
                });
            }
        }

        private async Task CompareDirectoriesAsync()
        {
            if (SelectedOldSnapshot == null || SelectedNewSnapshot == null)
                return;

            try
            {
                Application.Current.Dispatcher.Invoke(() =>
                {                    
                    // プログレス付きInfoBarを表示
                    ShowProgressInfoBar("処理中", "比較処理中...", 0);
                });

                await Task.Run(() =>
                {
                    _lastComparisonResult = _comparisonService.Compare(SelectedOldSnapshot, SelectedNewSnapshot);
                });

                Application.Current.Dispatcher.Invoke(() =>
                {
                    ComparisonResults.Clear();
                    if (_lastComparisonResult != null)
                    {
                        foreach (var item in _lastComparisonResult.AllDifferences)
                        {
                            ComparisonResults.Add(FileItemViewModel.FromModel(item));
                        }

                        // Update DiffViewModel
                        DiffViewModel.LoadComparison(SelectedOldSnapshot, SelectedNewSnapshot);

                        var statusMessage = $"比較完了: 追加 {_lastComparisonResult.AddedItems.Count}, 削除 {_lastComparisonResult.DeletedItems.Count}, 変更 {_lastComparisonResult.ModifiedItems.Count}";

                        // 完了InfoBarを表示（24時間表示）
                        ShowInfoBar("通知", statusMessage, InfoBarSeverity.Success, 86400); // 24時間 = 86400秒
                    }
                });

                // Auto-save new snapshot after successful comparison
                if (NewSnapshot != null)
                {
                    try
                    {
                        var fileName = $"snapshot_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                        var filePath = Path.Combine(SnapshotsDirectory, fileName);
                        await _directoryService.SaveSnapshotAsync(NewSnapshot, filePath);

                        // 自動保存の場合は新しいスナップショットのみを追加（UIスレッドで実行）
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            _ = AddNewSnapshotToListAsync(NewSnapshot);
                        });
                    }
                    catch
                    {
                        // Silent failure for auto-save
                    }
                }
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {                    
                    // エラーInfoBarを表示
                    ShowInfoBar("エラー", $"比較処理中にエラーが発生しました: {ex.Message}", InfoBarSeverity.Error, 0);
                });
            }
        }

        private Task AddNewSnapshotToListAsync(DirectorySnapshot snapshot)
        {
            // 重複チェック
            var snapshotKey = $"{snapshot.RootPath}_{snapshot.CreatedAt:yyyyMMdd_HHmmss}";
            var existingSnapshot = AvailableSnapshots.FirstOrDefault(s => 
                $"{s.RootPath}_{s.CreatedAt:yyyyMMdd_HHmmss}" == snapshotKey);
            
            if (existingSnapshot == null)
            {
                AvailableSnapshots.Insert(0, snapshot); // 最新のものを先頭に追加
                OnPropertyChanged(nameof(HasSnapshots));
                OnPropertyChanged(nameof(HasNoSnapshots));
            }
            
            return Task.CompletedTask;
        }
    }
}