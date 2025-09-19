using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ReleaseTrackerWpf.Models
{
    public class ComparisonTreeItem
    {
        public string Name { get; set; } = string.Empty;
        public string FullPath { get; set; } = string.Empty;
        public bool IsDirectory { get; set; }
        public long Size { get; set; }
        public DifferenceType DifferenceType { get; set; } = DifferenceType.None;
        public ObservableCollection<ComparisonTreeItem> Children { get; set; } = new ObservableCollection<ComparisonTreeItem>();

        public bool IsAdded => DifferenceType == DifferenceType.Added;
        public bool IsDeleted => DifferenceType == DifferenceType.Deleted;
        public bool IsModified => DifferenceType == DifferenceType.Modified;

        public string DisplaySize
        {
            get
            {
                if (IsDirectory) return "";

                if (Size < 1024) return $"{Size} B";
                if (Size < 1024 * 1024) return $"{Size / 1024:F1} KB";
                if (Size < 1024 * 1024 * 1024) return $"{Size / (1024 * 1024):F1} MB";
                return $"{Size / (1024 * 1024 * 1024):F1} GB";
            }
        }

        public static ComparisonTreeItem CreateFromFileItem(FileItem fileItem)
        {
            var treeItem = new ComparisonTreeItem
            {
                Name = fileItem.Name,
                FullPath = fileItem.FullPath,
                IsDirectory = fileItem.IsDirectory,
                Size = fileItem.Size,
                DifferenceType = fileItem.DifferenceType
            };

            foreach (var child in fileItem.Children)
            {
                treeItem.Children.Add(CreateFromFileItem(child));
            }

            return treeItem;
        }
    }
}