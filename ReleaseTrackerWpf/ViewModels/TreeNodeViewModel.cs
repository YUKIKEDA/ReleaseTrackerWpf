using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using ReleaseTrackerWpf.Models;

namespace ReleaseTrackerWpf.ViewModels
{
    public class TreeNodeViewModel : INotifyPropertyChanged
    {
        private string _name = string.Empty;
        private string _fullPath = string.Empty;
        private bool _isDirectory;
        private long _size;
        private DifferenceType _differenceType;
        private bool _isExpanded = true;
        private bool _showSize;

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public string FullPath
        {
            get => _fullPath;
            set
            {
                _fullPath = value;
                OnPropertyChanged(nameof(FullPath));
            }
        }

        public bool IsDirectory
        {
            get => _isDirectory;
            set
            {
                _isDirectory = value;
                OnPropertyChanged(nameof(IsDirectory));
                UpdateShowSize();
            }
        }

        public long Size
        {
            get => _size;
            set
            {
                _size = value;
                OnPropertyChanged(nameof(Size));
                OnPropertyChanged(nameof(DisplaySize));
                UpdateShowSize();
            }
        }

        public DifferenceType DifferenceType
        {
            get => _differenceType;
            set
            {
                _differenceType = value;
                OnPropertyChanged(nameof(DifferenceType));
            }
        }

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                _isExpanded = value;
                OnPropertyChanged(nameof(IsExpanded));
            }
        }

        public bool ShowSize
        {
            get => _showSize;
            private set
            {
                _showSize = value;
                OnPropertyChanged(nameof(ShowSize));
            }
        }

        public string DisplaySize
        {
            get
            {
                if (IsDirectory) return "";
                if (Size < 1024) return $"{Size} B";
                if (Size < 1024 * 1024) return $"{Size / 1024.0:F1} KB";
                if (Size < 1024 * 1024 * 1024) return $"{Size / (1024.0 * 1024.0):F1} MB";
                return $"{Size / (1024.0 * 1024.0 * 1024.0):F1} GB";
            }
        }

        public ObservableCollection<TreeNodeViewModel> Children { get; } = new();

        private void UpdateShowSize()
        {
            ShowSize = !IsDirectory && Size > 0;
        }

        public static TreeNodeViewModel FromFileItem(FileItem fileItem, DifferenceType? overrideDifferenceType = null)
        {
            var node = new TreeNodeViewModel
            {
                Name = fileItem.Name,
                FullPath = fileItem.FullPath,
                IsDirectory = fileItem.IsDirectory,
                Size = fileItem.Size,
                DifferenceType = overrideDifferenceType ?? fileItem.DifferenceType
            };

            foreach (var child in fileItem.Children)
            {
                node.Children.Add(FromFileItem(child, overrideDifferenceType));
            }

            return node;
        }

        public static ObservableCollection<TreeNodeViewModel> BuildTreeFromSnapshot(DirectorySnapshot? snapshot, DifferenceType? overrideDifferenceType = null)
        {
            var result = new ObservableCollection<TreeNodeViewModel>();

            if (snapshot?.Items == null || !snapshot.Items.Any())
                return result;

            // DirectorySnapshot.Items already contains the root-level items with proper hierarchy
            // We just need to convert FileItem objects to TreeNodeViewModel objects
            foreach (var item in snapshot.Items)
            {
                var treeNode = ConvertFileItemToTreeNode(item, overrideDifferenceType);
                result.Add(treeNode);
            }

            // Sort items: directories first, then files
            SortTreeItems(result);

            return result;
        }

        private static TreeNodeViewModel ConvertFileItemToTreeNode(FileItem fileItem, DifferenceType? overrideDifferenceType = null)
        {
            var node = new TreeNodeViewModel
            {
                Name = fileItem.Name,
                FullPath = fileItem.RelativePath,
                IsDirectory = fileItem.IsDirectory,
                Size = fileItem.Size,
                DifferenceType = overrideDifferenceType ?? fileItem.DifferenceType
            };

            // Recursively convert children
            foreach (var child in fileItem.Children)
            {
                var childNode = ConvertFileItemToTreeNode(child, overrideDifferenceType);
                node.Children.Add(childNode);
            }

            return node;
        }

        private static void SortTreeItems(ObservableCollection<TreeNodeViewModel> items)
        {
            var sortedItems = items.OrderBy(item => item.IsDirectory ? 0 : 1)
                                  .ThenBy(item => item.Name)
                                  .ToList();

            items.Clear();
            foreach (var item in sortedItems)
            {
                items.Add(item);
                SortTreeItems(item.Children);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}