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
        ISettingsService SettingsService,
        INotificationService NotificationService);
}