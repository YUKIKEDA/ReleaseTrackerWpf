using ReleaseTrackerWpf.Models;

namespace ReleaseTrackerWpf.Services
{
    public class ComparisonService
    {
        public async Task<ComparisonResult> CompareAsync(DirectorySnapshot oldSnapshot, DirectorySnapshot newSnapshot)
        {
            return await Task.Run(() =>
            {
                var result = new ComparisonResult
                {
                    OldSnapshotPath = oldSnapshot.RootPath,
                    NewSnapshotPath = newSnapshot.RootPath
                };

                // 古いスナップショットのアイテムを辞書に変換（相対パスをキーとして使用）
                var oldItemsDict = oldSnapshot.Items.ToDictionary(item => item.RelativePath, item => item);

                // 新しいスナップショットのアイテムを処理
                foreach (var newItem in newSnapshot.Items)
                {
                    if (oldItemsDict.TryGetValue(newItem.RelativePath, out var oldItem))
                    {
                        // アイテムが存在する場合、変更をチェック
                        if (IsItemModified(oldItem, newItem))
                        {
                            newItem.DifferenceType = DifferenceType.Modified;
                            newItem.Description = GetModificationDescription(oldItem, newItem);
                        }
                        else
                        {
                            newItem.DifferenceType = DifferenceType.Unchanged;
                        }
                        
                        // 和集合のItemsに追加
                        result.Items.Add(newItem);
                        
                        // 処理済みのアイテムを辞書から削除
                        oldItemsDict.Remove(newItem.RelativePath);
                    }
                    else
                    {
                        // 新しいアイテム
                        newItem.DifferenceType = DifferenceType.Added;
                        newItem.Description = "新しく追加されたアイテム";
                        result.Items.Add(newItem);
                    }
                }

                // 古いスナップショットにのみ存在するアイテム（削除されたアイテム）
                foreach (var deletedItem in oldItemsDict.Values)
                {
                    deletedItem.DifferenceType = DifferenceType.Deleted;
                    deletedItem.Description = "削除されたアイテム";
                    result.Items.Add(deletedItem);
                }

                return result;
            });
        }

        private static bool IsItemModified(FileSystemEntry oldItem, FileSystemEntry newItem)
        {
            // 基本的なプロパティの変更をチェック
            if (oldItem.Size != newItem.Size || 
                oldItem.LastWriteTime != newItem.LastWriteTime ||
                oldItem.IsDirectory != newItem.IsDirectory)
            {
                return true;
            }

            // ディレクトリの場合は、子要素の変更もチェック
            if (oldItem.IsDirectory && newItem.IsDirectory)
            {
                return HasChildrenChanged(oldItem.Children, newItem.Children);
            }

            // ファイルの場合は、より詳細な比較を行う
            if (!oldItem.IsDirectory && !newItem.IsDirectory)
            {
                // ファイル名の変更（相対パスが異なる場合）
                if (oldItem.RelativePath != newItem.RelativePath)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// ディレクトリの子要素に変更があるかチェックします
        /// </summary>
        private static bool HasChildrenChanged(List<FileSystemEntry> oldChildren, List<FileSystemEntry> newChildren)
        {
            // 子要素の数が異なる場合は変更あり
            if (oldChildren.Count != newChildren.Count)
            {
                return true;
            }

            // 子要素を辞書に変換して比較
            var oldChildrenDict = oldChildren.ToDictionary(child => child.RelativePath, child => child);
            var newChildrenDict = newChildren.ToDictionary(child => child.RelativePath, child => child);

            // 新しい子要素をチェック
            foreach (var newChild in newChildren)
            {
                if (!oldChildrenDict.TryGetValue(newChild.RelativePath, out var oldChild))
                {
                    // 新しい子要素が追加された
                    return true;
                }

                // 子要素の変更を再帰的にチェック
                if (IsItemModified(oldChild, newChild))
                {
                    return true;
                }
            }

            // 古い子要素が削除されたかチェック
            foreach (var oldChild in oldChildren)
            {
                if (!newChildrenDict.ContainsKey(oldChild.RelativePath))
                {
                    // 子要素が削除された
                    return true;
                }
            }

            return false;
        }

        private static string GetModificationDescription(FileSystemEntry oldItem, FileSystemEntry newItem)
        {
            var changes = new List<string>();

            // 基本的なプロパティの変更
            if (oldItem.Size != newItem.Size)
            {
                changes.Add($"サイズ: {oldItem.Size:N0} → {newItem.Size:N0} bytes");
            }

            if (oldItem.LastWriteTime != newItem.LastWriteTime)
            {
                changes.Add($"更新日時: {oldItem.LastWriteTime:yyyy/MM/dd HH:mm:ss} → {newItem.LastWriteTime:yyyy/MM/dd HH:mm:ss}");
            }

            if (oldItem.IsDirectory != newItem.IsDirectory)
            {
                changes.Add($"タイプ: {(oldItem.IsDirectory ? "ディレクトリ" : "ファイル")} → {(newItem.IsDirectory ? "ディレクトリ" : "ファイル")}");
            }

            // ディレクトリの場合は子要素の変更も説明に含める
            if (oldItem.IsDirectory && newItem.IsDirectory)
            {
                var childChanges = GetChildChangesDescription(oldItem.Children, newItem.Children);
                if (!string.IsNullOrEmpty(childChanges))
                {
                    changes.Add($"子要素: {childChanges}");
                }
            }

            return changes.Count > 0 ? string.Join(", ", changes) : "変更されたアイテム";
        }

        /// <summary>
        /// ディレクトリの子要素の変更を説明文字列として取得します
        /// </summary>
        private static string GetChildChangesDescription(List<FileSystemEntry> oldChildren, List<FileSystemEntry> newChildren)
        {
            var oldChildrenDict = oldChildren.ToDictionary(child => child.RelativePath, child => child);
            var newChildrenDict = newChildren.ToDictionary(child => child.RelativePath, child => child);

            var addedCount = 0;
            var deletedCount = 0;
            var modifiedCount = 0;

            // 新しい子要素をチェック
            foreach (var newChild in newChildren)
            {
                if (!oldChildrenDict.ContainsKey(newChild.RelativePath))
                {
                    addedCount++;
                }
                else if (oldChildrenDict.TryGetValue(newChild.RelativePath, out var oldChild) && IsItemModified(oldChild, newChild))
                {
                    modifiedCount++;
                }
            }

            // 削除された子要素をチェック
            foreach (var oldChild in oldChildren)
            {
                if (!newChildrenDict.ContainsKey(oldChild.RelativePath))
                {
                    deletedCount++;
                }
            }

            var changes = new List<string>();
            if (addedCount > 0) changes.Add($"{addedCount}個追加");
            if (deletedCount > 0) changes.Add($"{deletedCount}個削除");
            if (modifiedCount > 0) changes.Add($"{modifiedCount}個変更");

            return changes.Count > 0 ? string.Join(", ", changes) : string.Empty;
        }
    }
}