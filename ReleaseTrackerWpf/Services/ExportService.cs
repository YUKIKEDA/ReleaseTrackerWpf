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
        public async Task ExportToCsvAsync(DirectorySnapshot snapshot, string filePath)
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

                // スナップショットの全エントリを再帰的に書き込み
                WriteSnapshotEntries(csv, snapshot.Items, "");
            });
        }

        private void WriteSnapshotEntries(CsvWriter csv, IEnumerable<FileSystemEntry> entries, string currentPath)
        {
            foreach (var entry in entries)
            {
                var fullPath = string.IsNullOrEmpty(currentPath) ? entry.Name : Path.Combine(currentPath, entry.Name);
                
                csv.WriteField(fullPath);
                csv.WriteField(entry.Description ?? "TODO ここに説明を追加");
                csv.NextRecord();

                // フォルダの場合は子要素も再帰的に処理
                if (entry.IsDirectory && entry.Children.Any())
                {
                    WriteSnapshotEntries(csv, entry.Children, fullPath);
                }
            }
        }

    }
}