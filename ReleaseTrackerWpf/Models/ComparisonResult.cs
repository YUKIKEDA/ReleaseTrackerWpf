namespace ReleaseTrackerWpf.Models
{
    public class ComparisonResult
    {
        public List<FileItem> AddedItems { get; set; } = new List<FileItem>();
        public List<FileItem> DeletedItems { get; set; } = new List<FileItem>();
        public List<FileItem> ModifiedItems { get; set; } = new List<FileItem>();
        public List<FileItem> AllDifferences { get; set; } = new List<FileItem>();
    }
}