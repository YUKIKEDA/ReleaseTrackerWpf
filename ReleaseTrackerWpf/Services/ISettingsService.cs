using System.Threading.Tasks;

namespace ReleaseTrackerWpf.Services
{
    public interface ISettingsService
    {
        string SnapshotsDirectory { get; set; }
        bool AutoScanEnabled { get; set; }
        
        Task LoadSettingsAsync();
        Task SaveSettingsAsync();
    }
}
