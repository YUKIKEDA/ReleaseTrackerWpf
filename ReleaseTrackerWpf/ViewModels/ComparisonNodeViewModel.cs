using System.Collections.ObjectModel;
using System.ComponentModel;
using ReleaseTrackerWpf.Models;

namespace ReleaseTrackerWpf.ViewModels
{
    public class ComparisonNodeViewModel : INotifyPropertyChanged
    {
        private TreeNodeViewModel? _leftNode;
        private TreeNodeViewModel? _rightNode;
        private string _name = string.Empty;
        private bool _isDirectory;
        private bool _isExpanded = true;
        private DifferenceType _differenceType;

        public TreeNodeViewModel? LeftNode
        {
            get => _leftNode;
            set
            {
                _leftNode = value;
                OnPropertyChanged(nameof(LeftNode));
                OnPropertyChanged(nameof(HasLeftNode));
                UpdateCommonProperties();
            }
        }

        public TreeNodeViewModel? RightNode
        {
            get => _rightNode;
            set
            {
                _rightNode = value;
                OnPropertyChanged(nameof(RightNode));
                OnPropertyChanged(nameof(HasRightNode));
                UpdateCommonProperties();
            }
        }

        public bool HasLeftNode => LeftNode != null;
        public bool HasRightNode => RightNode != null;

        public string Name
        {
            get => _name;
            private set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public bool IsDirectory
        {
            get => _isDirectory;
            private set
            {
                _isDirectory = value;
                OnPropertyChanged(nameof(IsDirectory));
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

        public DifferenceType DifferenceType
        {
            get => _differenceType;
            private set
            {
                _differenceType = value;
                OnPropertyChanged(nameof(DifferenceType));
            }
        }

        public ObservableCollection<ComparisonNodeViewModel> Children { get; } = new();

        private void UpdateCommonProperties()
        {
            // 名前は左側を優先、なければ右側
            Name = LeftNode?.Name ?? RightNode?.Name ?? "";

            // ディレクトリかどうかは左右のどちらかがディレクトリなら true
            IsDirectory = (LeftNode?.IsDirectory ?? false) || (RightNode?.IsDirectory ?? false);

            // 差分タイプを決定
            if (LeftNode == null && RightNode != null)
            {
                DifferenceType = DifferenceType.Added;
            }
            else if (LeftNode != null && RightNode == null)
            {
                DifferenceType = DifferenceType.Deleted;
            }
            else if (LeftNode != null && RightNode != null)
            {
                // 両方存在する場合、どちらかの差分タイプを使用
                DifferenceType = LeftNode.DifferenceType != DifferenceType.None
                    ? LeftNode.DifferenceType
                    : RightNode.DifferenceType;
            }
            else
            {
                DifferenceType = DifferenceType.None;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}