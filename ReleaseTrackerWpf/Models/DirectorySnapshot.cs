using System.ComponentModel;

namespace ReleaseTrackerWpf.Models
{
    public class DirectorySnapshot : INotifyPropertyChanged
    {
        private string _rootPath = string.Empty;
        private DateTime _createdAt;
        private List<FileItem> _items = new();

        public string RootPath
        {
            get => _rootPath;
            set
            {
                _rootPath = value;
                OnPropertyChanged(nameof(RootPath));
            }
        }

        public DateTime CreatedAt
        {
            get => _createdAt;
            set
            {
                _createdAt = value;
                OnPropertyChanged(nameof(CreatedAt));
            }
        }

        public List<FileItem> Items
        {
            get => _items ?? new List<FileItem>();
            set
            {
                _items = value;
                OnPropertyChanged(nameof(Items));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}