using System.Text.Json.Serialization;

namespace ReleaseTrackerWpf.Models
{
    public class FileSystemEntry
    {
        public string Name { get; set; } = string.Empty;
        public string FullPath { get; set; } = string.Empty;
        public string RelativePath { get; set; } = string.Empty;
        public bool IsDirectory { get; set; }
        public long Size { get; set; }
        public DateTime LastWriteTime { get; set; }
        public List<FileSystemEntry> Children { get; set; } = [];

        [JsonIgnore]
        public DifferenceType DifferenceType { get; set; } = DifferenceType.None;

        public string? Description { get; set; }
    }
}