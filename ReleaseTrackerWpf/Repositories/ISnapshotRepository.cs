using ReleaseTrackerWpf.Models;

namespace ReleaseTrackerWpf.Repositories
{
    public interface ISnapshotRepository
    {
        Task SaveSnapshotAsync(DirectorySnapshot snapshot, string filePath);
        Task<DirectorySnapshot> LoadSnapshotAsync(string filePath);
    }
}
