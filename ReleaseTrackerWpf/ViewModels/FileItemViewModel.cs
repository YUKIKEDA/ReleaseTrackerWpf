using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using ReleaseTrackerWpf.Models;

namespace ReleaseTrackerWpf.ViewModels
{
    public partial class FileItemViewModel : ObservableObject
    {
        [ObservableProperty]
        private string name = string.Empty;

        [ObservableProperty]
        private string relativePath = string.Empty;

        [ObservableProperty]
        private bool isDirectory;

        [ObservableProperty]
        private long size;

        [ObservableProperty]
        private DateTime lastWriteTime;

        [ObservableProperty]
        private DifferenceType differenceType;

        [ObservableProperty]
        private string? description;

        [ObservableProperty]
        private bool isExpanded;

        public ObservableCollection<FileItemViewModel> Children { get; } = new();

        public string DisplaySize => IsDirectory ? "Directory" : $"{Size:N0} bytes";

        public string DifferenceTypeText => DifferenceType.ToString();

        public static FileItemViewModel FromModel(FileItem item)
        {
            var viewModel = new FileItemViewModel
            {
                Name = item.Name,
                RelativePath = item.RelativePath,
                IsDirectory = item.IsDirectory,
                Size = item.Size,
                LastWriteTime = item.LastWriteTime,
                DifferenceType = item.DifferenceType,
                Description = item.Description
            };

            foreach (var child in item.Children)
            {
                viewModel.Children.Add(FromModel(child));
            }

            return viewModel;
        }

        public FileItem ToModel()
        {
            return new FileItem
            {
                Name = Name,
                RelativePath = RelativePath,
                IsDirectory = IsDirectory,
                Size = Size,
                LastWriteTime = LastWriteTime,
                DifferenceType = DifferenceType,
                Description = Description,
                Children = Children.Select(c => c.ToModel()).ToList()
            };
        }
    }
}