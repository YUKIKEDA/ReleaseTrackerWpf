namespace ReleaseTrackerWpf.Models
{
    public record SettingsData(string SnapshotsDirectory = "", bool AutoScanEnabled = true);
}
