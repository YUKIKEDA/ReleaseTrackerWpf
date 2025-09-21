namespace ReleaseTrackerWpf.Models
{
    public enum ExportedCsvPathFormat
    {
        Normal,    // 通常形式（バックスラッシュ/スラッシュ）
        Tree       // ツリー形式（│、└など）
    }

    public record SettingsData(
        string SnapshotsDirectory = "", 
        bool AutoScanEnabled = true,
        ExportedCsvPathFormat CsvPathDisplayFormat = ExportedCsvPathFormat.Normal);
}
