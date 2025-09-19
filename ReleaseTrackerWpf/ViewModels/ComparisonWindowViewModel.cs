using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using ReleaseTrackerWpf.Models;
using ReleaseTrackerWpf.Services;

namespace ReleaseTrackerWpf.ViewModels
{
    public partial class ComparisonWindowViewModel : ObservableObject
    {
        private readonly IExportService _exportService;

        [ObservableProperty]
        private DirectorySnapshot? oldSnapshot;

        [ObservableProperty]
        private DirectorySnapshot? newSnapshot;

        [ObservableProperty]
        private ObservableCollection<ComparisonTreeItem> oldStructureTree = new();

        [ObservableProperty]
        private ObservableCollection<ComparisonTreeItem> newStructureTree = new();

        [ObservableProperty]
        private int addedCount;

        [ObservableProperty]
        private int deletedCount;

        [ObservableProperty]
        private int modifiedCount;

        public ComparisonWindowViewModel(IExportService exportService)
        {
            _exportService = exportService;
        }

        public void LoadComparison(DirectorySnapshot oldSnapshot, DirectorySnapshot newSnapshot, ComparisonResult comparisonResult)
        {
            OldSnapshot = oldSnapshot;
            NewSnapshot = newSnapshot;

            // 統計を計算
            AddedCount = comparisonResult.AddedItems.Count;
            DeletedCount = comparisonResult.DeletedItems.Count;
            ModifiedCount = comparisonResult.ModifiedItems.Count;

            // 旧構造のツリーを構築（削除されたファイルのみ表示）
            BuildOldStructureTree(oldSnapshot.Items, comparisonResult);

            // 新構造のツリーを構築（追加・変更されたファイルのみ表示）
            BuildNewStructureTree(newSnapshot.Items, comparisonResult);
        }

        private void BuildOldStructureTree(List<FileItem> rootItems, ComparisonResult comparisonResult)
        {
            OldStructureTree.Clear();

            var deletedPaths = comparisonResult.DeletedItems.Select(f => f.RelativePath).ToHashSet();
            foreach (var item in rootItems)
            {
                BuildTreeWithFilter(item, OldStructureTree, deletedPaths, true);
            }
        }

        private void BuildNewStructureTree(List<FileItem> rootItems, ComparisonResult comparisonResult)
        {
            NewStructureTree.Clear();

            var addedPaths = comparisonResult.AddedItems.Select(f => f.RelativePath).ToHashSet();
            var modifiedPaths = comparisonResult.ModifiedItems.Select(f => f.RelativePath).ToHashSet();
            var changedPaths = addedPaths.Union(modifiedPaths).ToHashSet();

            foreach (var item in rootItems)
            {
                BuildTreeWithFilter(item, NewStructureTree, changedPaths, false);
            }
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
                    treeItem.DifferenceType = isOldStructure ? DifferenceType.Deleted :
                        (targetPaths.Contains(item.RelativePath) ? DifferenceType.Added : DifferenceType.Modified);
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

        [RelayCommand]
        private async Task Export()
        {
            if (OldSnapshot == null || NewSnapshot == null) return;

            var dialog = new SaveFileDialog
            {
                Title = "比較結果をエクスポート",
                Filter = "Excelファイル (*.xlsx)|*.xlsx|CSVファイル (*.csv)|*.csv|テキストファイル (*.txt)|*.txt",
                DefaultExt = ".xlsx",
                FileName = $"比較結果_{OldSnapshot.CreatedAt:yyyyMMdd}_{NewSnapshot.CreatedAt:yyyyMMdd}.xlsx"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var allItems = GetItemsWithDifferenceType(OldStructureTree, DifferenceType.Deleted)
                        .Concat(GetItemsWithDifferenceType(NewStructureTree, DifferenceType.Added))
                        .Concat(GetItemsWithDifferenceType(NewStructureTree, DifferenceType.Modified))
                        .ToList();

                    var extension = Path.GetExtension(dialog.FileName).ToLower();
                    switch (extension)
                    {
                        case ".xlsx":
                            await _exportService.ExportToExcelAsync(allItems, dialog.FileName);
                            break;
                        case ".csv":
                            await _exportService.ExportToCsvAsync(allItems, dialog.FileName);
                            break;
                        case ".txt":
                            await _exportService.ExportToTextAsync(allItems, dialog.FileName);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"エクスポート中にエラーが発生しました: {ex.Message}", "エラー",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
        }

        private List<FileItem> GetItemsWithDifferenceType(ObservableCollection<ComparisonTreeItem> tree, DifferenceType differenceType)
        {
            var result = new List<FileItem>();
            foreach (var item in tree)
            {
                CollectItemsWithDifferenceType(item, differenceType, result);
            }
            return result;
        }

        private void CollectItemsWithDifferenceType(ComparisonTreeItem item, DifferenceType differenceType, List<FileItem> result)
        {
            if (item.DifferenceType == differenceType)
            {
                result.Add(new FileItem
                {
                    Name = item.Name,
                    FullPath = item.FullPath,
                    IsDirectory = item.IsDirectory,
                    Size = item.Size,
                    DifferenceType = item.DifferenceType
                });
            }

            foreach (var child in item.Children)
            {
                CollectItemsWithDifferenceType(child, differenceType, result);
            }
        }

    }
}