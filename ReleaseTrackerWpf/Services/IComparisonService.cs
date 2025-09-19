using ReleaseTrackerWpf.Models;

namespace ReleaseTrackerWpf.Services
{
    public interface IComparisonService
    {
        ComparisonResult Compare(DirectorySnapshot oldSnapshot, DirectorySnapshot newSnapshot);
    }
}