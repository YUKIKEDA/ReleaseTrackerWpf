using System.IO;
using System.Text.Json;
using ReleaseTrackerWpf.Models;

namespace ReleaseTrackerWpf.Repositories
{
    public class SnapshotRepository : ISnapshotRepository
    {
        public async Task SaveSnapshotAsync(DirectorySnapshot snapshot, string filePath)
        {
            var json = JsonSerializer.Serialize(snapshot, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            await File.WriteAllTextAsync(filePath, json);
        }

        public async Task<DirectorySnapshot> LoadSnapshotAsync(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Snapshot file not found: {filePath}");
            }

            var json = await File.ReadAllTextAsync(filePath);
            var snapshot = JsonSerializer.Deserialize<DirectorySnapshot>(json);

            return snapshot ?? throw new InvalidOperationException("Failed to deserialize snapshot");
        }
    }
}
