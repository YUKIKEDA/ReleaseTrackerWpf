using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using ReleaseTrackerWpf.Models;
using ReleaseTrackerWpf.Services;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using Wpf.Ui.Controls;

namespace ReleaseTrackerWpf.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IDirectoryService _directoryService;
        private readonly IComparisonService _comparisonService;
        private readonly IExportService _exportService;

        [ObservableProperty]
        private string newDirectoryPath = string.Empty;

        [ObservableProperty]
        private DirectorySnapshot? selectedOldSnapshot;

        [ObservableProperty]
        private DirectorySnapshot? selectedNewSnapshot;

        [ObservableProperty]
        private string statusMessage = "準備完了";

        [ObservableProperty]
        private bool isProcessing = false;

        partial void OnIsProcessingChanged(bool value)
        {
            // デバッグ用：IsProcessingの変更をログ出力
            System.Diagnostics.Debug.WriteLine($"IsProcessing changed: {value}, StatusMessage: '{StatusMessage}'");
        }

        [ObservableProperty]
        private bool hasStatusMessage = false;

        [ObservableProperty]
        private DirectorySnapshot? newSnapshot;

        [ObservableProperty]
        private string snapshotsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ReleaseTracker", "Snapshots");

        [ObservableProperty]
        private bool autoScanEnabled = false;

        // InfoBar関連のプロパティ
        [ObservableProperty]
        private bool isInfoBarOpen = false;

        [ObservableProperty]
        private string infoBarTitle = string.Empty;

        [ObservableProperty]
        private string infoBarMessage = string.Empty;

        [ObservableProperty]
        private InfoBarSeverity infoBarSeverity = InfoBarSeverity.Informational;

        public ObservableCollection<FileItemViewModel> ComparisonResults { get; } = new();
        public ObservableCollection<DirectorySnapshot> AvailableSnapshots { get; } = new();
        public DiffViewModel DiffViewModel { get; } = new();

        public bool HasSnapshots => AvailableSnapshots.Count > 0;
        public bool HasNoSnapshots => AvailableSnapshots.Count == 0;

        private ComparisonResult? _lastComparisonResult;
        private System.Timers.Timer? _autoScanTimer;
        private System.Timers.Timer? _infoBarTimer;

        public MainViewModel(IDirectoryService directoryService, IComparisonService comparisonService, IExportService exportService)
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


        partial void OnNewDirectoryPathChanged(string value)
        {
            if (AutoScanEnabled && !string.IsNullOrEmpty(value) && Directory.Exists(value))
            {
                RestartAutoScanTimer();
            }
        }

        partial void OnStatusMessageChanged(string value)
        {
            HasStatusMessage = !string.IsNullOrEmpty(value);
            
            // デバッグ用：StatusMessageの変更をログ出力
            System.Diagnostics.Debug.WriteLine($"StatusMessage changed: '{value}', IsProcessing: {IsProcessing}");
        }


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

        private void RestartAutoScanTimer()
        {
            if (_autoScanTimer == null) return;

            _autoScanTimer.Stop();
            _autoScanTimer.Start();
        }

        [RelayCommand]
        private void BrowseNewDirectory()
        {
            var dialog = new OpenFolderDialog
            {
                Title = "新構造のフォルダを選択"
            };

            if (dialog.ShowDialog() == true)
            {
                NewDirectoryPath = dialog.FolderName;
            }
        }

        [RelayCommand]
        private async Task ScanAndAddNewStructure()
        {
            var dialog = new OpenFolderDialog
            {
                Title = "新構造のフォルダを選択してスナップショットに追加"
            };

            if (dialog.ShowDialog() == true)
            {
                await ScanAndSaveSnapshotAsync(dialog.FolderName);
            }
        }

        [RelayCommand]
        private async Task CreateNewSnapshot()
        {
            var dialog = new OpenFolderDialog
            {
                Title = "スナップショットを作成するフォルダを選択"
            };

            if (dialog.ShowDialog() == true)
            {
                await ScanAndSaveSnapshotAsync(dialog.FolderName);
            }
        }

        private async Task ScanAndSaveSnapshotAsync(string directoryPath)
        {
            try
            {
                IsProcessing = true;
                StatusMessage = "スナップショットを作成中...";
                
                // プログレス付きInfoBarを表示
                System.Diagnostics.Debug.WriteLine("Showing progress InfoBar: スナップショットを作成中...");
                ShowProgressInfoBar("処理中", "スナップショットを作成中...", 0);

                var snapshot = await _directoryService.ScanDirectoryAsync(directoryPath);
                var fileName = $"snapshot_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                var filePath = Path.Combine(SnapshotsDirectory, fileName);

                await _directoryService.SaveSnapshotAsync(snapshot, filePath);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    _ = LoadAvailableSnapshotsAsync();
                    StatusMessage = "スナップショットを作成しました";
                    
                    // 完了InfoBarを表示（24時間表示）
                    System.Diagnostics.Debug.WriteLine("Showing completion InfoBar: スナップショットを作成しました");
                    ShowInfoBar("通知", "スナップショットを作成しました", 86400); // 24時間 = 86400秒
                });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    System.Windows.MessageBox.Show($"スナップショット作成中にエラーが発生しました: {ex.Message}", "エラー", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    StatusMessage = "エラーが発生しました";
                    
                    // エラーInfoBarを表示
                    ShowInfoBar("エラー", "エラーが発生しました", 0);
                });
            }
            finally
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    System.Diagnostics.Debug.WriteLine("Setting IsProcessing = false in ScanAndSaveSnapshotAsync finally block");
                    IsProcessing = false;
                });
            }
        }

        [RelayCommand]
        private void BrowseFirstTimeDirectory()
        {
            var dialog = new OpenFolderDialog
            {
                Title = "初回スキャン用フォルダを選択"
            };

            if (dialog.ShowDialog() == true)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        IsProcessing = true;
                        StatusMessage = "初回スキャン中...";
                        
                        // プログレス付きInfoBarを表示
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            ShowProgressInfoBar("処理中", "初回スキャン中...", 0);
                        });

                        var snapshot = await _directoryService.ScanDirectoryAsync(dialog.FolderName);
                        var fileName = $"snapshot_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                        var filePath = Path.Combine(SnapshotsDirectory, fileName);

                        await _directoryService.SaveSnapshotAsync(snapshot, filePath);

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            _ = LoadAvailableSnapshotsAsync();
                            StatusMessage = "初回スナップショットを作成しました";
                            
                            // 完了InfoBarを表示（24時間表示）
                            ShowInfoBar("通知", "初回スナップショットを作成しました", 86400); // 24時間 = 86400秒
                        });
                    }
                    catch (Exception ex)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            System.Windows.MessageBox.Show($"初回スキャン中にエラーが発生しました: {ex.Message}", "エラー", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                            StatusMessage = "エラーが発生しました";
                            
                            // エラーInfoBarを表示
                            ShowInfoBar("エラー", "エラーが発生しました", 0);
                        });
                    }
                    finally
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            IsProcessing = false;
                        });
                    }
                });
            }
        }

        private async Task ScanNewDirectoryAsync()
        {
            if (string.IsNullOrEmpty(NewDirectoryPath) || !Directory.Exists(NewDirectoryPath))
                return;

            try
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    IsProcessing = true;
                    StatusMessage = "新構造をスキャン中...";
                    
                    // プログレス付きInfoBarを表示
                    ShowProgressInfoBar("処理中", "新構造をスキャン中...", 0);
                });

                NewSnapshot = await _directoryService.ScanDirectoryAsync(NewDirectoryPath);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    StatusMessage = "新構造のスキャンが完了しました";
                    
                    // 完了InfoBarを表示（24時間表示）
                    ShowInfoBar("通知", "新構造のスキャンが完了しました", 86400); // 24時間 = 86400秒
                });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    System.Windows.MessageBox.Show($"新構造のスキャン中にエラーが発生しました: {ex.Message}", "エラー", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    StatusMessage = "エラーが発生しました";
                    
                    // エラーInfoBarを表示
                    ShowInfoBar("エラー", "エラーが発生しました", 0);
                });
            }
            finally
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    IsProcessing = false;
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
                    IsProcessing = true;
                    StatusMessage = "比較処理中...";
                    
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

                        StatusMessage = $"比較完了: 追加 {_lastComparisonResult.AddedItems.Count}, 削除 {_lastComparisonResult.DeletedItems.Count}, 変更 {_lastComparisonResult.ModifiedItems.Count}";

                        // 完了InfoBarを表示（24時間表示）
                        ShowInfoBar("通知", StatusMessage, 86400); // 24時間 = 86400秒
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
                            AddNewSnapshotToListAsync(NewSnapshot);
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
                    System.Windows.MessageBox.Show($"比較処理中にエラーが発生しました: {ex.Message}", "エラー", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    StatusMessage = "エラーが発生しました";
                    
                    // エラーInfoBarを表示
                    ShowInfoBar("エラー", "エラーが発生しました", 0);
                });
            }
            finally
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    IsProcessing = false;
                });
            }
        }


        [RelayCommand]
        private async Task ExportResultsAsync()
        {
            if (_lastComparisonResult == null || !_lastComparisonResult.AllDifferences.Any())
            {
                System.Windows.MessageBox.Show("エクスポートする比較結果がありません。", "エラー", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
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
                    IsProcessing = true;
                    StatusMessage = "エクスポート中...";

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

                    StatusMessage = "エクスポートが完了しました";
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"エクスポート中にエラーが発生しました: {ex.Message}", "エラー", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    StatusMessage = "エラーが発生しました";
                }
                finally
                {
                    IsProcessing = false;
                }
            }
        }

        [RelayCommand]
        private async Task ImportDescriptionsAsync()
        {
            if (_lastComparisonResult == null || !_lastComparisonResult.AllDifferences.Any())
            {
                System.Windows.MessageBox.Show("インポートする比較結果がありません。", "エラー", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            var dialog = new OpenFileDialog
            {
                Title = "説明ファイルをインポート",
                Filter = "Excelファイル (*.xlsx)|*.xlsx|CSVファイル (*.csv)|*.csv",
                CheckFileExists = true
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    IsProcessing = true;
                    StatusMessage = "説明をインポート中...";

                    Dictionary<string, string> descriptions;
                    var extension = Path.GetExtension(dialog.FileName).ToLower();

                    if (extension == ".xlsx")
                    {
                        descriptions = await _exportService.ImportDescriptionsFromExcelAsync(dialog.FileName);
                    }
                    else
                    {
                        descriptions = await _exportService.ImportDescriptionsFromCsvAsync(dialog.FileName);
                    }

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

                    StatusMessage = $"説明のインポートが完了しました ({descriptions.Count} 件)";
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"説明のインポート中にエラーが発生しました: {ex.Message}", "エラー", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    StatusMessage = "エラーが発生しました";
                }
                finally
                {
                    IsProcessing = false;
                }
            }
        }

        private async Task AddNewSnapshotToListAsync(DirectorySnapshot snapshot)
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
        }

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
                    // Skip corrupted files and log the error
                    System.Diagnostics.Debug.WriteLine($"Failed to load snapshot {file}: {ex.Message}");
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

        partial void OnSelectedOldSnapshotChanged(DirectorySnapshot? value)
        {
            if (value != null && SelectedNewSnapshot != null)
            {
                _ = Task.Run(CompareDirectoriesAsync);
            }
        }

        partial void OnSelectedNewSnapshotChanged(DirectorySnapshot? value)
        {
            if (value != null && SelectedOldSnapshot != null)
            {
                _ = Task.Run(CompareDirectoriesAsync);
            }
        }

        partial void OnSnapshotsDirectoryChanged(string value)
        {
            // Create new directory if it doesn't exist
            Directory.CreateDirectory(value);

            // Reload snapshots from new directory
            _ = LoadAvailableSnapshotsAsync();
        }

        /// <summary>
        /// InfoBarを表示します
        /// </summary>
        /// <param name="title">タイトル</param>
        /// <param name="message">メッセージ</param>
        /// <param name="timeoutSeconds">表示時間（秒、0で無制限）</param>
        public void ShowInfoBar(string title, string message, int timeoutSeconds = 0)
        {
            System.Diagnostics.Debug.WriteLine($"ShowInfoBar called: Title='{title}', Message='{message}', Timeout={timeoutSeconds}");

            Application.Current.Dispatcher.Invoke(() =>
            {
                // 既存のタイマーがあればクリア
                _infoBarTimer?.Dispose();

                // InfoBarの設定
                InfoBarTitle = title;
                InfoBarMessage = message;

                // メッセージ内容に応じてSeverityを設定
                if (message.Contains("エラー") || title.Contains("エラー"))
                {
                    InfoBarSeverity = InfoBarSeverity.Error;
                }
                else if (message.Contains("完了") || title.Contains("完了") || title.Contains("通知"))
                {
                    InfoBarSeverity = InfoBarSeverity.Success;
                }
                else if (message.Contains("処理中") || title.Contains("処理中"))
                {
                    InfoBarSeverity = InfoBarSeverity.Informational;
                }
                else
                {
                    InfoBarSeverity = InfoBarSeverity.Informational;
                }

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
        /// プログレス付きInfoBarを表示します
        /// </summary>
        /// <param name="title">タイトル</param>
        /// <param name="message">メッセージ</param>
        /// <param name="timeoutSeconds">表示時間（秒、0で無制限）</param>
        public void ShowProgressInfoBar(string title, string message, int timeoutSeconds = 0)
        {
            System.Diagnostics.Debug.WriteLine($"ShowProgressInfoBar called: Title='{title}', Message='{message}', Timeout={timeoutSeconds}");

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
    }
}