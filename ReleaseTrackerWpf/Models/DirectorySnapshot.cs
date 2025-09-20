using CommunityToolkit.Mvvm.ComponentModel;

namespace ReleaseTrackerWpf.Models
{
    public partial class DirectorySnapshot : ObservableObject
    {
        #region Constants

        /// <summary>
        /// スナップショットファイルの拡張子
        /// </summary>
        public const string SnapshotFileExtension = ".json";

        /// <summary>
        /// スナップショットファイルの接頭辞
        /// </summary>
        public const string SnapshotFilePrefix = "snapshot_";

        /// <summary>
        /// スナップショットファイルの検索パターン
        /// </summary>
        public const string SnapshotFilePattern = $"{SnapshotFilePrefix}*{SnapshotFileExtension}";

        /// <summary>
        /// スナップショットファイル名のフォーマット
        /// </summary>
        public const string SnapshotFileNameFormat = SnapshotFilePrefix + "{0:yyyyMMdd_HHmmss}" + SnapshotFileExtension;

        #endregion

        #region Properties

        [ObservableProperty]
        private string _rootPath = string.Empty;

        [ObservableProperty]
        private DateTime _createdAt;

        [ObservableProperty]
        private List<FileItem> _items = [];

        #endregion
    }
}