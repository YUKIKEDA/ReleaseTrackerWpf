namespace ReleaseTrackerWpf.Models
{
    public class ComparisonResult
    {
        /// <summary>
        /// 旧ディレクトリ構造と新ディレクトリ構造の和集合のディレクトリ構造
        /// 各アイテムには変更情報（DifferenceType）が設定されている
        /// </summary>
        public List<FileSystemEntry> Items { get; set; } = [];

        /// <summary>
        /// 比較実行日時
        /// </summary>
        public DateTime ComparisonDateTime { get; set; }

        /// <summary>
        /// 旧スナップショットのパス
        /// </summary>
        public string OldSnapshotPath { get; set; } = string.Empty;

        /// <summary>
        /// 新スナップショットのパス
        /// </summary>
        public string NewSnapshotPath { get; set; } = string.Empty;

        public ComparisonResult()
        {
            ComparisonDateTime = DateTime.Now;
        }

        /// <summary>
        /// 追加されたアイテムを取得します
        /// </summary>
        public List<FileSystemEntry> AddedItems => 
            Items.Where(item => item.DifferenceType == DifferenceType.Added).ToList();

        /// <summary>
        /// 削除されたアイテムを取得します
        /// </summary>
        public List<FileSystemEntry> DeletedItems => 
            Items.Where(item => item.DifferenceType == DifferenceType.Deleted).ToList();

        /// <summary>
        /// 変更されたアイテムを取得します
        /// </summary>
        public List<FileSystemEntry> ModifiedItems => 
            Items.Where(item => item.DifferenceType == DifferenceType.Modified).ToList();

        /// <summary>
        /// 変更されていないアイテムを取得します
        /// </summary>
        public List<FileSystemEntry> UnchangedItems => 
            Items.Where(item => item.DifferenceType == DifferenceType.Unchanged).ToList();

        /// <summary>
        /// 追加されたアイテムの数
        /// </summary>
        public int TotalAddedCount => AddedItems.Count;

        /// <summary>
        /// 削除されたアイテムの数
        /// </summary>
        public int TotalDeletedCount => DeletedItems.Count;

        /// <summary>
        /// 変更されたアイテムの数
        /// </summary>
        public int TotalModifiedCount => ModifiedItems.Count;

        /// <summary>
        /// 変更されていないアイテムの数
        /// </summary>
        public int TotalUnchangedCount => UnchangedItems.Count;

        /// <summary>
        /// すべての変更されたアイテム（追加、削除、変更）を取得します
        /// </summary>
        public List<FileSystemEntry> AllChangedItems => 
            Items.Where(item => item.DifferenceType != DifferenceType.Unchanged).ToList();

        /// <summary>
        /// 変更があるかどうかを示します
        /// </summary>
        public bool HasChanges => AllChangedItems.Any();
    }
}