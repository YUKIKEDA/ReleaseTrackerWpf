using System.IO;
using ReleaseTrackerWpf.Models;

namespace ReleaseTrackerWpf.Services
{
    public class DirectoryScanService
    {
        public async Task<DirectorySnapshot> ScanDirectoryAsync(string path)
        {
            if (!Directory.Exists(path))
                throw new DirectoryNotFoundException($"Directory not found: {path}");

            var snapshot = new DirectorySnapshot
            {
                RootPath = path,
                CreatedAt = DateTime.Now,
                Items = new List<FileSystemEntry>()
            };

            await Task.Run(() =>
            {
                snapshot.Items = ScanDirectoryRecursive(path, path).ToList();
            });

            return snapshot;
        }

        private static List<FileSystemEntry> ScanDirectoryRecursive(string currentPath, string rootPath)
        {
            var items = new List<FileSystemEntry>();

            try
            {
                // Get directories
                var directories = Directory.GetDirectories(currentPath);
                foreach (var dir in directories)
                {
                    var dirInfo = new DirectoryInfo(dir);
                    var item = new FileSystemEntry
                    {
                        Name = dirInfo.Name,
                        FullPath = dirInfo.FullName,
                        RelativePath = Path.GetRelativePath(rootPath, dirInfo.FullName),
                        IsDirectory = true,
                        Size = 0,
                        LastWriteTime = dirInfo.LastWriteTime,
                        Children = ScanDirectoryRecursive(dir, rootPath).ToList()
                    };
                    items.Add(item);
                }

                // Get files
                var files = Directory.GetFiles(currentPath);
                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    var item = new FileSystemEntry
                    {
                        Name = fileInfo.Name,
                        FullPath = fileInfo.FullName,
                        RelativePath = Path.GetRelativePath(rootPath, fileInfo.FullName),
                        IsDirectory = false,
                        Size = fileInfo.Length,
                        LastWriteTime = fileInfo.LastWriteTime
                    };
                    items.Add(item);
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Skip directories we can't access
            }
            catch (DirectoryNotFoundException)
            {
                // Skip directories that no longer exist
            }

            return items;
        }
    }
}