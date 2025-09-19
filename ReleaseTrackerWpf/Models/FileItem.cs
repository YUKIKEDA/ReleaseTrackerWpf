using System;
using System.Text.Json.Serialization;

namespace ReleaseTrackerWpf.Models
{
    public class FileItem
    {
        public string Name { get; set; } = string.Empty;
        public string FullPath { get; set; } = string.Empty;
        public string RelativePath { get; set; } = string.Empty;
        public bool IsDirectory { get; set; }
        public long Size { get; set; }
        public DateTime LastWriteTime { get; set; }
        public List<FileItem> Children { get; set; } = new List<FileItem>();

        [JsonIgnore]
        public DifferenceType DifferenceType { get; set; } = DifferenceType.None;

        [JsonIgnore]
        public string? Description { get; set; }
    }
}