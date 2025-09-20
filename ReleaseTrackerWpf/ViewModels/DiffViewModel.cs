using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using ReleaseTrackerWpf.Models;

namespace ReleaseTrackerWpf.ViewModels
{
    public class DiffViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<ComparisonItemViewModel> _comparisonItems = new();
        private string _oldSnapshotName = string.Empty;
        private string _newSnapshotName = string.Empty;

        public ObservableCollection<ComparisonItemViewModel> ComparisonItems
        {
            get => _comparisonItems;
            set
            {
                _comparisonItems = value;
                OnPropertyChanged(nameof(ComparisonItems));
            }
        }

        public string OldSnapshotName
        {
            get => _oldSnapshotName;
            set
            {
                _oldSnapshotName = value;
                OnPropertyChanged(nameof(OldSnapshotName));
            }
        }

        public string NewSnapshotName
        {
            get => _newSnapshotName;
            set
            {
                _newSnapshotName = value;
                OnPropertyChanged(nameof(NewSnapshotName));
            }
        }


        public void LoadComparison(DirectorySnapshot? oldSnapshot, DirectorySnapshot? newSnapshot)
        {
            // Set snapshot names
            OldSnapshotName = oldSnapshot?.RootPath ?? "なし";
            NewSnapshotName = newSnapshot?.RootPath ?? "なし";

            // Create flat dictionaries for comparison
            var oldItemsFlat = CreateFlatItemDictionary(oldSnapshot?.Items);
            var newItemsFlat = CreateFlatItemDictionary(newSnapshot?.Items);

            // Mark items with their difference types
            MarkDifferences(oldItemsFlat, newItemsFlat);

            // Build old and new trees
            var oldTree = TreeNodeViewModel.BuildTreeFromSnapshot(oldSnapshot);
            var newTree = TreeNodeViewModel.BuildTreeFromSnapshot(newSnapshot);

            // Create flat dictionaries of TreeNodeViewModels for easier lookup
            var oldTreeFlat = CreateFlatTreeDictionary(oldTree);
            var newTreeFlat = CreateFlatTreeDictionary(newTree);

            // Build flat comparison list
            ComparisonItems = BuildFlatComparisonList(oldTreeFlat, newTreeFlat);
        }

        private Dictionary<string, FileItem> CreateFlatItemDictionary(List<FileItem>? items)
        {
            var result = new Dictionary<string, FileItem>();
            if (items == null) return result;

            foreach (var item in items)
            {
                FlattenFileItems(item, result);
            }
            return result;
        }

        private void FlattenFileItems(FileItem item, Dictionary<string, FileItem> dictionary)
        {
            dictionary[item.RelativePath] = item;

            foreach (var child in item.Children)
            {
                FlattenFileItems(child, dictionary);
            }
        }

        private void MarkDifferences(Dictionary<string, FileItem> oldItems, Dictionary<string, FileItem> newItems)
        {
            var allPaths = new HashSet<string>();
            allPaths.UnionWith(oldItems.Keys);
            allPaths.UnionWith(newItems.Keys);

            foreach (var path in allPaths)
            {
                var existsInOld = oldItems.ContainsKey(path);
                var existsInNew = newItems.ContainsKey(path);

                if (!existsInOld && existsInNew)
                {
                    // Added
                    newItems[path].DifferenceType = DifferenceType.Added;
                }
                else if (existsInOld && !existsInNew)
                {
                    // Deleted
                    oldItems[path].DifferenceType = DifferenceType.Deleted;
                }
                else if (existsInOld && existsInNew)
                {
                    var oldItem = oldItems[path];
                    var newItem = newItems[path];

                    // Check if modified
                    if (!oldItem.IsDirectory && !newItem.IsDirectory &&
                        (oldItem.Size != newItem.Size || oldItem.LastWriteTime != newItem.LastWriteTime))
                    {
                        oldItem.DifferenceType = DifferenceType.Modified;
                        newItem.DifferenceType = DifferenceType.Modified;
                    }
                    else
                    {
                        oldItem.DifferenceType = DifferenceType.None;
                        newItem.DifferenceType = DifferenceType.None;
                    }
                }
            }
        }



        private Dictionary<string, TreeNodeViewModel> CreateFlatTreeDictionary(ObservableCollection<TreeNodeViewModel> tree)
        {
            var result = new Dictionary<string, TreeNodeViewModel>();

            foreach (var node in tree)
            {
                FlattenTreeNodes(node, result);
            }

            return result;
        }

        private void FlattenTreeNodes(TreeNodeViewModel node, Dictionary<string, TreeNodeViewModel> dictionary)
        {
            dictionary[node.FullPath] = node;

            foreach (var child in node.Children)
            {
                FlattenTreeNodes(child, dictionary);
            }
        }

        private ObservableCollection<ComparisonItemViewModel> BuildFlatComparisonList(
            Dictionary<string, TreeNodeViewModel> oldTreeFlat,
            Dictionary<string, TreeNodeViewModel> newTreeFlat)
        {
            var result = new ObservableCollection<ComparisonItemViewModel>();

            // 全てのパスを取得してソート
            var allPaths = new HashSet<string>();
            allPaths.UnionWith(oldTreeFlat.Keys);
            allPaths.UnionWith(newTreeFlat.Keys);

            // パスを階層順にソート
            var sortedPaths = allPaths.OrderBy(path => path).ToList();

            foreach (var path in sortedPaths)
            {
                var leftNode = oldTreeFlat.ContainsKey(path) ? oldTreeFlat[path] : null;
                var rightNode = newTreeFlat.ContainsKey(path) ? newTreeFlat[path] : null;

                if (leftNode != null || rightNode != null)
                {
                    var level = path.Count(c => c == '\\');

                    var item = new ComparisonItemViewModel
                    {
                        LeftNode = leftNode,
                        RightNode = rightNode,
                        Level = level,
                        HasChildren = HasChildrenInPaths(path, allPaths)
                    };

                    result.Add(item);
                }
            }

            return result;
        }

        private bool HasChildrenInPaths(string parentPath, HashSet<string> allPaths)
        {
            return allPaths.Any(path => IsDirectChild(parentPath, path));
        }

        private bool IsDirectChild(string parentPath, string childPath)
        {
            if (parentPath == childPath)
                return false;

            // 子パスが親パスで始まっているかチェック
            if (!childPath.StartsWith(parentPath))
                return false;

            // 親パスの後にパス区切り文字が続く場合のみ
            var remainder = childPath.Substring(parentPath.Length);

            // ルートレベルの場合
            if (string.IsNullOrEmpty(parentPath))
            {
                return !remainder.Contains('\\');
            }

            // 親パスの直後がパス区切り文字で、その後にパス区切り文字がない場合
            return remainder.StartsWith("\\") &&
                   remainder.Substring(1).Count(c => c == '\\') == 0;
        }

        private static string FormatFileSize(long size)
        {
            if (size < 1024) return $"{size} B";
            if (size < 1024 * 1024) return $"{size / 1024.0:F1} KB";
            if (size < 1024 * 1024 * 1024) return $"{size / (1024.0 * 1024.0):F1} MB";
            return $"{size / (1024.0 * 1024.0 * 1024.0):F1} GB";
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}