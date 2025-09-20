using ReleaseTrackerWpf.Services;

namespace ReleaseTrackerWpf.Models
{
    /// <summary>
    /// SettingsViewModelのコンストラクタ引数をまとめるDTOクラス
    /// </summary>
    public record SettingsViewModelArgs(ISettingsService SettingsService);
}
