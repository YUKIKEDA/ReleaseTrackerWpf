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
        private ObservableCollection<TreeNodeViewModel> _oldTree = new();
        private ObservableCollection<TreeNodeViewModel> _newTree = new();
        private int _addedCount;
        private int _deletedCount;
        private int _modifiedCount;
        private string _oldSnapshotName = string.Empty;
        private string _newSnapshotName = string.Empty;

        public ObservableCollection<TreeNodeViewModel> OldTree
        {
            get => _oldTree;
            set
            {
                _oldTree = value;
                OnPropertyChanged(nameof(OldTree));
            }
        }

        public ObservableCollection<TreeNodeViewModel> NewTree
        {
            get => _newTree;
            set
            {
                _newTree = value;
                OnPropertyChanged(nameof(NewTree));
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

        public int AddedCount
        {
            get => _addedCount;
            private set
            {
                _addedCount = value;
                OnPropertyChanged(nameof(AddedCount));
            }
        }

        public int DeletedCount
        {
            get => _deletedCount;
            private set
            {
                _deletedCount = value;
                OnPropertyChanged(nameof(DeletedCount));
            }
        }

        public int ModifiedCount
        {
            get => _modifiedCount;
            private set
            {
                _modifiedCount = value;
                OnPropertyChanged(nameof(ModifiedCount));
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

            // Build trees (the difference highlighting is already applied during MarkDifferences)
            OldTree = TreeNodeViewModel.BuildTreeFromSnapshot(oldSnapshot);
            NewTree = TreeNodeViewModel.BuildTreeFromSnapshot(newSnapshot);

            // Update counts
            UpdateCounts();
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


        private void UpdateCounts()
        {
            AddedCount = CountNodesByType(NewTree, DifferenceType.Added);
            DeletedCount = CountNodesByType(OldTree, DifferenceType.Deleted);
            ModifiedCount = CountNodesByType(NewTree, DifferenceType.Modified);
        }

        private int CountNodesByType(ObservableCollection<TreeNodeViewModel> tree, DifferenceType type)
        {
            int count = 0;
            foreach (var node in tree)
            {
                count += CountNodesByTypeRecursive(node, type);
            }
            return count;
        }

        private int CountNodesByTypeRecursive(TreeNodeViewModel node, DifferenceType type)
        {
            int count = node.DifferenceType == type ? 1 : 0;
            foreach (var child in node.Children)
            {
                count += CountNodesByTypeRecursive(child, type);
            }
            return count;
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