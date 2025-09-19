using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using ReleaseTrackerWpf.Models;
using ReleaseTrackerWpf.Services;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

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
        private string statusMessage = "準備完了";

        [ObservableProperty]
        private bool isProcessing = false;

        [ObservableProperty]
        private DirectorySnapshot? newSnapshot;

        [ObservableProperty]
        private string snapshotsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ReleaseTracker", "Snapshots");

        [ObservableProperty]
        private bool autoScanEnabled = true;

        public ObservableCollection<FileItemViewModel> ComparisonResults { get; } = new();
        public ObservableCollection<DirectorySnapshot> AvailableSnapshots { get; } = new();

        public bool HasSnapshots => AvailableSnapshots.Count > 0;
        public bool HasNoSnapshots => AvailableSnapshots.Count == 0;

        private ComparisonResult? _lastComparisonResult;
        private System.Timers.Timer? _autoScanTimer;

        public MainViewModel(IDirectoryService directoryService, IComparisonService comparisonService, IExportService exportService)
        {
            _directoryService = directoryService;
            _comparisonService = comparisonService;
            _exportService = exportService;

            // Create snapshots directory if it doesn't exist
            Directory.CreateDirectory(SnapshotsDirectory);

            // Load available snapshots
            LoadAvailableSnapshots();

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

                        var snapshot = await _directoryService.ScanDirectoryAsync(dialog.FolderName);
                        var fileName = $"snapshot_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                        var filePath = Path.Combine(SnapshotsDirectory, fileName);

                        await _directoryService.SaveSnapshotAsync(snapshot, filePath);

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            LoadAvailableSnapshots();
                            StatusMessage = "初回スナップショットを作成しました";
                        });
                    }
                    catch (Exception ex)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            MessageBox.Show($"初回スキャン中にエラーが発生しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                            StatusMessage = "エラーが発生しました";
                        });
                    }
                    finally
                    {
                        IsProcessing = false;
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
                });

                NewSnapshot = await _directoryService.ScanDirectoryAsync(NewDirectoryPath);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    StatusMessage = "新構造のスキャンが完了しました";
                });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"新構造のスキャン中にエラーが発生しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                    StatusMessage = "エラーが発生しました";
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
            if (SelectedOldSnapshot == null || NewSnapshot == null)
                return;

            try
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    IsProcessing = true;
                    StatusMessage = "比較処理中...";
                });

                await Task.Run(() =>
                {
                    _lastComparisonResult = _comparisonService.Compare(SelectedOldSnapshot, NewSnapshot);
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
                        StatusMessage = $"比較完了: 追加 {_lastComparisonResult.AddedItems.Count}, 削除 {_lastComparisonResult.DeletedItems.Count}, 変更 {_lastComparisonResult.ModifiedItems.Count}";
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

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            LoadAvailableSnapshots();
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
                    MessageBox.Show($"比較処理中にエラーが発生しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                    StatusMessage = "エラーが発生しました";
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
                MessageBox.Show("エクスポートする比較結果がありません。", "エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                    MessageBox.Show($"エクスポート中にエラーが発生しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show("インポートする比較結果がありません。", "エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                    MessageBox.Show($"説明のインポート中にエラーが発生しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                    StatusMessage = "エラーが発生しました";
                }
                finally
                {
                    IsProcessing = false;
                }
            }
        }

        private void LoadAvailableSnapshots()
        {
            AvailableSnapshots.Clear();

            if (!Directory.Exists(SnapshotsDirectory))
            {
                OnPropertyChanged(nameof(HasSnapshots));
                OnPropertyChanged(nameof(HasNoSnapshots));
                return;
            }

            var jsonFiles = Directory.GetFiles(SnapshotsDirectory, "*.json")
                .OrderByDescending(f => File.GetCreationTime(f));

            foreach (var file in jsonFiles)
            {
                try
                {
                    var snapshot = Task.Run(() => _directoryService.LoadSnapshotAsync(file)).Result;
                    AvailableSnapshots.Add(snapshot);
                }
                catch
                {
                    // Skip corrupted files
                }
            }

            OnPropertyChanged(nameof(HasSnapshots));
            OnPropertyChanged(nameof(HasNoSnapshots));
        }

        partial void OnSelectedOldSnapshotChanged(DirectorySnapshot? value)
        {
            if (value != null && NewSnapshot != null)
            {
                _ = Task.Run(CompareDirectoriesAsync);
            }
        }

        partial void OnSnapshotsDirectoryChanged(string value)
        {
            // Create new directory if it doesn't exist
            Directory.CreateDirectory(value);

            // Reload snapshots from new directory
            LoadAvailableSnapshots();
        }
    }
}