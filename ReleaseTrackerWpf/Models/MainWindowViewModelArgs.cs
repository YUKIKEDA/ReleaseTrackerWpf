using ReleaseTrackerWpf.Repositories;
using ReleaseTrackerWpf.Services;

namespace ReleaseTrackerWpf.Models
{
    /// <summary>
    /// MainWindowViewModelのコンストラクタ引数をまとめるDTOクラス
    /// </summary>
    public record MainWindowViewModelArgs(
        DirectoryService DirectoryService,
        ComparisonService ComparisonService,
        ExportService ExportService,
        ISettingsRepository SettingsRepository,
        INotificationService NotificationService,
        ISnapshotRepository SnapshotRepository);
}