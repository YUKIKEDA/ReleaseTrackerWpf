using System;
using System.Collections.Generic;

namespace ReleaseTrackerWpf.Models
{
    public class DirectorySnapshot
    {
        public string RootPath { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string Version { get; set; } = string.Empty;
        public List<FileItem> Items { get; set; } = new List<FileItem>();
    }
}