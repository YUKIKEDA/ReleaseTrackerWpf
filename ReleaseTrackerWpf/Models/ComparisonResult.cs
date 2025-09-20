namespace ReleaseTrackerWpf.Models
{
    public class ComparisonResult
    {
        public List<FileSystemEntry> AddedItems { get; set; } = [];
        public List<FileSystemEntry> DeletedItems { get; set; } = [];
        public List<FileSystemEntry> ModifiedItems { get; set; } = [];
        public List<FileSystemEntry> AllDifferences { get; set; } = [];
    }
}