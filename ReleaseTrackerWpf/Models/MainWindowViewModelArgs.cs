using ReleaseTrackerWpf.Repositories;
using ReleaseTrackerWpf.Services;

namespace ReleaseTrackerWpf.Models
{
    /// <summary>
    /// MainWindowViewModelのコンストラクタ引数をまとめるDTOクラス
    /// </summary>
    public record MainWindowViewModelArgs(
        DirectoryScanService DirectoryScanService,
        ComparisonService ComparisonService,
        ExportService ExportService,
        ImportDescriptionService ImportDescriptionService,
        ISettingsRepository SettingsRepository,
        INotificationService NotificationService,
        ISnapshotRepository SnapshotRepository);
}