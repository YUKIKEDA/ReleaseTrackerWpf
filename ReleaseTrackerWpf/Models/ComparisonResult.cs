using System.Collections.ObjectModel;

namespace ReleaseTrackerWpf.Models
{
    public class ComparisonResult
    {
        public DirectorySnapshot? OldSnapshot { get; set; }
        public DirectorySnapshot? NewSnapshot { get; set; }
        public DateTime ComparisonTime { get; set; }
        public ObservableCollection<FileSystemEntry> LeftTreeItems { get; set; } = new();
        public ObservableCollection<FileSystemEntry> RightTreeItems { get; set; } = new();
        public ComparisonStatistics Statistics { get; set; } = new();
    }

    public class ComparisonStatistics
    {
        public int AddedFiles { get; set; }
        public int DeletedFiles { get; set; }
        public int ModifiedFiles { get; set; }
        public int UnchangedFiles { get; set; }
        public int AddedDirectories { get; set; }
        public int DeletedDirectories { get; set; }
        public int TotalItems => AddedFiles + DeletedFiles + ModifiedFiles + UnchangedFiles + AddedDirectories + DeletedDirectories;
    }
}