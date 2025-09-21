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

        [JsonIgnore]
        public bool IsAdded => DifferenceType == DifferenceType.Added;

        [JsonIgnore]
        public bool IsDeleted => DifferenceType == DifferenceType.Deleted;

        [JsonIgnore]
        public bool IsModified => DifferenceType == DifferenceType.Modified;

        [JsonIgnore]
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
    }
}