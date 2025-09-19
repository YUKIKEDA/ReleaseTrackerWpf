using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using ReleaseTrackerWpf.Models;
using ReleaseTrackerWpf.Services;
using ReleaseTrackerWpf.Views;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
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
        private DirectorySnapshot? selectedNewSnapshot;

        [ObservableProperty]
        private string statusMessage = "準備完了";

        [ObservableProperty]
        private bool isProcessing = false;

        [ObservableProperty]
        private DirectorySnapshot? newSnapshot;

        [ObservableProperty]
        private string snapshotsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ReleaseTracker", "Snapshots");

        [ObservableProperty]
        private bool autoScanEnabled = false;

        [ObservableProperty]
        private int addedCount;

        [ObservableProperty]
        private int deletedCount;

        [ObservableProperty]
        private int modifiedCount;

        public ObservableCollection<FileItemViewModel> ComparisonResults { get; } = new();
        public ObservableCollection<DirectorySnapshot> AvailableSnapshots { get; } = new();
        public ObservableCollection<ComparisonTreeItem> OldStructureTree { get; } = new();
        public ObservableCollection<ComparisonTreeItem> NewStructureTree { get; } = new();

        public bool HasSnapshots => AvailableSnapshots.Count > 0;
        public bool HasNoSnapshots => AvailableSnapshots.Count == 0;
        public bool HasComparisonResults => _lastComparisonResult != null;

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

                var snapshot = await _directoryService.ScanDirectoryAsync(directoryPath);
                var fileName = $"snapshot_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                var filePath = Path.Combine(SnapshotsDirectory, fileName);

                await _directoryService.SaveSnapshotAsync(snapshot, filePath);

                Application.Current.Dispatcher.Invoke(async () =>
                {
                    await LoadAvailableSnapshotsAsync();
                    StatusMessage = "スナップショットを作成しました";
                });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"スナップショット作成中にエラーが発生しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
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

                        Application.Current.Dispatcher.Invoke(async () =>
                        {
                            await LoadAvailableSnapshotsAsync();
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
            if (SelectedOldSnapshot == null || SelectedNewSnapshot == null)
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
                    _lastComparisonResult = _comparisonService.Compare(SelectedOldSnapshot, SelectedNewSnapshot);
                });

                Application.Current.Dispatcher.Invoke(() =>
                {
                    ComparisonResults.Clear();
                    OldStructureTree.Clear();
                    NewStructureTree.Clear();

                    if (_lastComparisonResult != null)
                    {
                        foreach (var item in _lastComparisonResult.AllDifferences)
                        {
                            ComparisonResults.Add(FileItemViewModel.FromModel(item));
                        }

                        // 左右のツリー構造を構築
                        BuildOldStructureTree(SelectedOldSnapshot.Items, _lastComparisonResult);
                        BuildNewStructureTree(SelectedNewSnapshot.Items, _lastComparisonResult);

                        // 統計情報を更新
                        AddedCount = _lastComparisonResult.AddedItems.Count;
                        DeletedCount = _lastComparisonResult.DeletedItems.Count;
                        ModifiedCount = _lastComparisonResult.ModifiedItems.Count;

                        StatusMessage = $"比較完了: 追加 {AddedCount}, 削除 {DeletedCount}, 変更 {ModifiedCount}";
                        OnPropertyChanged(nameof(HasComparisonResults));
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

                        Application.Current.Dispatcher.Invoke(async () =>
                        {
                            // 自動保存の場合は新しいスナップショットのみを追加
                            await AddNewSnapshotToListAsync(NewSnapshot);
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

        private void BuildOldStructureTree(List<FileItem> rootItems, ComparisonResult comparisonResult)
        {
            OldStructureTree.Clear();

            var deletedPaths = comparisonResult.DeletedItems.Select(f => f.RelativePath).ToHashSet();
            foreach (var item in rootItems)
            {
                BuildStructureTreeRecursive(item, OldStructureTree, deletedPaths, DifferenceType.Deleted);
            }
        }

        private void BuildNewStructureTree(List<FileItem> rootItems, ComparisonResult comparisonResult)
        {
            NewStructureTree.Clear();

            var addedPaths = comparisonResult.AddedItems.Select(f => f.RelativePath).ToHashSet();
            var modifiedPaths = comparisonResult.ModifiedItems.Select(f => f.RelativePath).ToHashSet();

            foreach (var item in rootItems)
            {
                BuildNewStructureTreeRecursive(item, NewStructureTree, addedPaths, modifiedPaths);
            }
        }

        private void BuildNewStructureTreeRecursive(FileItem item, ObservableCollection<ComparisonTreeItem> collection, HashSet<string> addedPaths, HashSet<string> modifiedPaths)
        {
            var treeItem = new ComparisonTreeItem
            {
                Name = item.Name,
                FullPath = item.FullPath,
                IsDirectory = item.IsDirectory,
                Size = item.Size
            };

            // 追加・変更されたファイルかどうかをチェック
            if (addedPaths.Contains(item.RelativePath))
            {
                treeItem.DifferenceType = DifferenceType.Added;
            }
            else if (modifiedPaths.Contains(item.RelativePath))
            {
                treeItem.DifferenceType = DifferenceType.Modified;
            }

            // 子アイテムを再帰的に処理
            foreach (var child in item.Children)
            {
                BuildNewStructureTreeRecursive(child, treeItem.Children, addedPaths, modifiedPaths);
            }

            // すべてのアイテムを常に表示
            collection.Add(treeItem);
        }

        private void BuildStructureTreeRecursive(FileItem item, ObservableCollection<ComparisonTreeItem> collection, HashSet<string> changedPaths, DifferenceType differenceType)
        {
            var treeItem = new ComparisonTreeItem
            {
                Name = item.Name,
                FullPath = item.FullPath,
                IsDirectory = item.IsDirectory,
                Size = item.Size
            };

            // 差分タイプをチェック
            if (changedPaths.Contains(item.RelativePath))
            {
                treeItem.DifferenceType = differenceType;
            }

            // 子アイテムを再帰的に処理
            foreach (var child in item.Children)
            {
                BuildStructureTreeRecursive(child, treeItem.Children, changedPaths, differenceType);
            }

            // すべてのアイテムを常に表示
            collection.Add(treeItem);
        }

        private void BuildTreeWithFilter(FileItem item, ObservableCollection<ComparisonTreeItem> collection, HashSet<string> targetPaths, bool isOldStructure)
        {
            var treeItem = new ComparisonTreeItem
            {
                Name = item.Name,
                FullPath = item.FullPath,
                IsDirectory = item.IsDirectory,
                Size = item.Size
            };

            // このアイテムまたはその子が対象パスに含まれているかチェック
            bool hasTargetChildren = false;
            bool isTarget = targetPaths.Contains(item.RelativePath);

            // 子アイテムを再帰的に処理
            foreach (var child in item.Children)
            {
                var childHasTarget = HasTargetInSubtree(child, targetPaths);
                if (childHasTarget || targetPaths.Contains(child.RelativePath))
                {
                    BuildTreeWithFilter(child, treeItem.Children, targetPaths, isOldStructure);
                    hasTargetChildren = true;
                }
            }

            // このアイテム自体が対象か、対象となる子を持つ場合のみ表示
            if (isTarget || hasTargetChildren)
            {
                // 差分タイプを設定
                if (isTarget)
                {
                    if (isOldStructure)
                    {
                        treeItem.DifferenceType = DifferenceType.Deleted;
                    }
                    else
                    {
                        // 新構造の場合、追加か変更かを区別
                        if (_lastComparisonResult?.AddedItems.Any(a => a.RelativePath == item.RelativePath) == true)
                        {
                            treeItem.DifferenceType = DifferenceType.Added;
                        }
                        else if (_lastComparisonResult?.ModifiedItems.Any(m => m.RelativePath == item.RelativePath) == true)
                        {
                            treeItem.DifferenceType = DifferenceType.Modified;
                        }
                    }
                }

                collection.Add(treeItem);
            }
        }

        private bool HasTargetInSubtree(FileItem item, HashSet<string> targetPaths)
        {
            if (targetPaths.Contains(item.RelativePath))
                return true;

            return item.Children.Any(child => HasTargetInSubtree(child, targetPaths));
        }
    }
}