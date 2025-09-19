using ReleaseTrackerWpf.Models;
using System.Threading.Tasks;

namespace ReleaseTrackerWpf.Services
{
    public interface IDirectoryService
    {
        Task<DirectorySnapshot> ScanDirectoryAsync(string path);
        Task SaveSnapshotAsync(DirectorySnapshot snapshot, string filePath);
        Task<DirectorySnapshot> LoadSnapshotAsync(string filePath);
    }
}