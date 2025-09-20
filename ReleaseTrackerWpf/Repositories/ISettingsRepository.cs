using ReleaseTrackerWpf.Models;

namespace ReleaseTrackerWpf.Repositories
{
    public interface ISettingsRepository
    {
        Task<SettingsData> GetAsync();
        Task SaveAsync(SettingsData settings);
    }
}
