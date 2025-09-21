using System.Globalization;
using System.IO;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using ReleaseTrackerWpf.Models;

namespace ReleaseTrackerWpf.Services
{
    public class ImportDescriptionService
    {
        public async Task<Dictionary<string, string>> ImportDescriptionsFromCsvAsync(string filePath)
        {
            return await Task.Run(() =>
            {
                var descriptions = new Dictionary<string, string>();
                
                var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    Encoding = Encoding.UTF8
                };

                using var reader = new StreamReader(filePath, Encoding.UTF8);
                using var csv = new CsvReader(reader, csvConfig);

                // ヘッダー行をスキップ
                csv.Read();
                csv.ReadHeader();

                while (csv.Read())
                {
                    var path = csv.GetField("パス");
                    var description = csv.GetField("説明");

                    if (!string.IsNullOrEmpty(path) && !string.IsNullOrEmpty(description) && 
                        description != "TODO ここに説明を追加")
                    {
                        descriptions[path] = description;
                    }
                }

                return descriptions;
            });
        }

        public void UpdateSnapshotDescriptions(DirectorySnapshot snapshot, Dictionary<string, string> descriptions)
        {
            UpdateEntryDescriptions(snapshot.Items, descriptions, "");
        }

        private void UpdateEntryDescriptions(IEnumerable<FileSystemEntry> entries, Dictionary<string, string> descriptions, string currentPath)
        {
            foreach (var entry in entries)
            {
                var fullPath = string.IsNullOrEmpty(currentPath) ? entry.Name : Path.Combine(currentPath, entry.Name);
                
                // 説明を更新
                if (descriptions.TryGetValue(fullPath, out var description))
                {
                    entry.Description = description;
                }

                // フォルダの場合は子要素も再帰的に処理
                if (entry.IsDirectory && entry.Children.Any())
                {
                    UpdateEntryDescriptions(entry.Children, descriptions, fullPath);
                }
            }
        }
    }
}
