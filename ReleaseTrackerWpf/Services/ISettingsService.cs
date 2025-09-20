using System.Threading.Tasks;
using ReleaseTrackerWpf.Models;

namespace ReleaseTrackerWpf.Services
{
    public interface ISettingsService
    {
        Task<SettingsData> LoadSettingsAsync();
        Task SaveSettingsAsync(SettingsData settings);
    }
}
