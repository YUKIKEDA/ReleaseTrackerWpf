using ReleaseTrackerWpf.Repositories;

namespace ReleaseTrackerWpf.Models
{
    /// <summary>
    /// SettingsViewModelのコンストラクタ引数をまとめるDTOクラス
    /// </summary>
    public record SettingsViewModelArgs(ISettingsRepository SettingsRepository);
}
