using System.ComponentModel;
using ReleaseTrackerWpf.Models;

namespace ReleaseTrackerWpf.ViewModels
{
    public class ComparisonItemViewModel : INotifyPropertyChanged
    {
        private bool _isExpanded = true;
        private bool _isVisible = true;

        public TreeNodeViewModel? LeftNode { get; set; }
        public TreeNodeViewModel? RightNode { get; set; }

        public int Level { get; set; }
        public bool HasChildren { get; set; }

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                _isExpanded = value;
                OnPropertyChanged(nameof(IsExpanded));
            }
        }

        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                _isVisible = value;
                OnPropertyChanged(nameof(IsVisible));
            }
        }

        public bool HasLeftNode => LeftNode != null;
        public bool HasRightNode => RightNode != null;

        public string Name => LeftNode?.Name ?? RightNode?.Name ?? "";
        public bool IsDirectory => (LeftNode?.IsDirectory ?? false) || (RightNode?.IsDirectory ?? false);

        public DifferenceType DifferenceType
        {
            get
            {
                if (LeftNode == null && RightNode != null)
                    return DifferenceType.Added;
                else if (LeftNode != null && RightNode == null)
                    return DifferenceType.Deleted;
                else if (LeftNode != null && RightNode != null)
                    return LeftNode.DifferenceType != DifferenceType.None
                        ? LeftNode.DifferenceType
                        : RightNode.DifferenceType;
                else
                    return DifferenceType.None;
            }
        }

        public double IndentMargin => Level * 20.0;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}