using ReleaseTrackerWpf.Repositories;
using ReleaseTrackerWpf.Services;

namespace ReleaseTrackerWpf.Models
{
    /// <summary>
    /// ComparisonViewModelのコンストラクタ引数をまとめるDTOクラス
    /// </summary>
    public record ComparisonViewModelArgs(
        DirectoryScanService DirectoryScanService,
        ComparisonService ComparisonService,
        ExportService ExportService,
        ComparisonExportService ComparisonExportService,
        ImportDescriptionService ImportDescriptionService,
        INotificationService NotificationService,
        ISettingsRepository SettingsRepository,
        ISnapshotRepository SnapshotRepository);
}
