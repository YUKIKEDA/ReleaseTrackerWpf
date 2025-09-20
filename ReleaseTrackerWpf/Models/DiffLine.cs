using System.ComponentModel;

namespace ReleaseTrackerWpf.Models
{
    public class DiffLine : INotifyPropertyChanged
    {
        private string _path = string.Empty;
        private DifferenceType _differenceType;
        private bool _isDirectory;
        private string _sizeInfo = string.Empty;
        private bool _hasSizeInfo;

        public string Path
        {
            get => _path;
            set
            {
                _path = value;
                OnPropertyChanged(nameof(Path));
            }
        }

        public DifferenceType DifferenceType
        {
            get => _differenceType;
            set
            {
                _differenceType = value;
                OnPropertyChanged(nameof(DifferenceType));
                OnPropertyChanged(nameof(Prefix));
            }
        }

        public bool IsDirectory
        {
            get => _isDirectory;
            set
            {
                _isDirectory = value;
                OnPropertyChanged(nameof(IsDirectory));
            }
        }

        public string SizeInfo
        {
            get => _sizeInfo;
            set
            {
                _sizeInfo = value;
                OnPropertyChanged(nameof(SizeInfo));
                HasSizeInfo = !string.IsNullOrEmpty(value);
            }
        }

        public bool HasSizeInfo
        {
            get => _hasSizeInfo;
            private set
            {
                _hasSizeInfo = value;
                OnPropertyChanged(nameof(HasSizeInfo));
            }
        }

        public string Prefix => DifferenceType switch
        {
            DifferenceType.Added => "+",
            DifferenceType.Deleted => "-",
            DifferenceType.Modified => "~",
            _ => " "
        };

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}