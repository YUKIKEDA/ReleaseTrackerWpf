using System.Globalization;
using System.IO;
using System.Text;
using ClosedXML.Excel;
using CsvHelper;
using CsvHelper.Configuration;
using ReleaseTrackerWpf.Models;

namespace ReleaseTrackerWpf.Services
{
    public class ExportService
    {
        public async Task ExportToCsvAsync(DirectorySnapshot snapshot, string filePath, ExportedCsvPathFormat pathDisplayFormat = ExportedCsvPathFormat.Normal)
        {
            await Task.Run(() =>
            {
                var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    Encoding = Encoding.UTF8
                };

                using var writer = new StreamWriter(filePath, false, Encoding.UTF8);
                using var csv = new CsvWriter(writer, csvConfig);

                // ヘッダーを書き込み
                csv.WriteField("パス");
                csv.WriteField("説明");
                csv.NextRecord();

                // フォーマットに応じて異なるメソッドを呼び出し
                if (pathDisplayFormat == ExportedCsvPathFormat.Tree)
                {
                    WriteSnapshotEntriesAsTree(csv, snapshot.Items, new List<bool>());
                }
                else
                {
                    WriteSnapshotEntriesAsNormal(csv, snapshot.Items, "");
                }
            });
        }

        /// <summary>
        /// 通常形式でスナップショットエントリを書き込みます
        /// </summary>
        private void WriteSnapshotEntriesAsNormal(CsvWriter csv, IEnumerable<FileSystemEntry> entries, string currentPath)
        {
            foreach (var entry in entries)
            {
                var displayPath = string.IsNullOrEmpty(currentPath) ? entry.Name : Path.Combine(currentPath, entry.Name);
                
                csv.WriteField(displayPath);
                csv.WriteField(entry.Description ?? "TODO ここに説明を追加");
                csv.NextRecord();

                // フォルダの場合は子要素も再帰的に処理
                if (entry.IsDirectory && entry.Children.Any())
                {
                    var childPath = string.IsNullOrEmpty(currentPath) ? entry.Name : Path.Combine(currentPath, entry.Name);
                    WriteSnapshotEntriesAsNormal(csv, entry.Children, childPath);
                }
            }
        }

        /// <summary>
        /// ツリー形式でスナップショットエントリを書き込みます
        /// </summary>
        private void WriteSnapshotEntriesAsTree(CsvWriter csv, IEnumerable<FileSystemEntry> entries, List<bool> parentIsLast)
        {
            var entryList = entries.ToList();
            
            foreach (var (entry, index) in entryList.Select((e, i) => (e, i)))
            {
                var isLast = index == entryList.Count - 1;
                var treePrefix = BuildTreePrefix(parentIsLast, isLast);
                
                csv.WriteField($"{treePrefix}{entry.Name}{(entry.IsDirectory ? "/" : "")}");
                csv.WriteField(entry.Description ?? "TODO ここに説明を追加");
                csv.NextRecord();

                // フォルダの場合は子要素も再帰的に処理
                if (entry.IsDirectory && entry.Children.Any())
                {
                    var newParentIsLast = new List<bool>(parentIsLast) { isLast };
                    WriteSnapshotEntriesAsTree(csv, entry.Children, newParentIsLast);
                }
            }
        }

        /// <summary>
        /// ツリー構造のプレフィックスを構築します
        /// </summary>
        /// <param name="parentIsLast">親階層での各要素が最後の要素かどうか</param>
        /// <param name="isLast">現在の要素が最後の要素かどうか</param>
        /// <returns>ツリー構造のプレフィックス文字列</returns>
        private string BuildTreePrefix(List<bool> parentIsLast, bool isLast)
        {
            var prefix = new StringBuilder();
            
            // 親階層のプレフィックスを構築
            foreach (var parentLast in parentIsLast)
            {
                if (parentLast)
                {
                    prefix.Append("    "); // 最後の要素の子は空白4文字
                }
                else
                {
                    prefix.Append("│   "); // 中間要素の子は縦線
                }
            }
            
            // 現在の要素のプレフィックス
            if (parentIsLast.Count > 0) // ルート要素でない場合
            {
                prefix.Append(isLast ? "└─ " : "├─ ");
            }
            
            return prefix.ToString();
        }

    }
}