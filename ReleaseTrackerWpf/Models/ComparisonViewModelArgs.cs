using ReleaseTrackerWpf.Services;

namespace ReleaseTrackerWpf.Models
{
    /// <summary>
    /// ComparisonViewModelのコンストラクタ引数をまとめるDTOクラス
    /// </summary>
    public record ComparisonViewModelArgs(
        DirectoryService DirectoryService,
        ComparisonService ComparisonService,
        ExportService ExportService,
        INotificationService NotificationService,
        ISettingsService SettingsService);
}
