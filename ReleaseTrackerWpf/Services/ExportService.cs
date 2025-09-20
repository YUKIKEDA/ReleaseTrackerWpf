using System.Globalization;
using System.IO;
using System.Text;
using ClosedXML.Excel;
using CsvHelper;
using CsvHelper.Configuration;
using ReleaseTrackerWpf.Models;

namespace ReleaseTrackerWpf.Services
{
    public class ExportService : IExportService
    {
        public async Task ExportToExcelAsync(List<FileItem> items, string filePath)
        {
            await Task.Run(() =>
            {
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Comparison Results");

                // Headers
                worksheet.Cell(1, 1).Value = "File Path";
                worksheet.Cell(1, 2).Value = "Difference Type";
                worksheet.Cell(1, 3).Value = "Size";
                worksheet.Cell(1, 4).Value = "Last Modified";
                worksheet.Cell(1, 5).Value = "Description";

                // Data
                for (int i = 0; i < items.Count; i++)
                {
                    var item = items[i];
                    var row = i + 2;

                    worksheet.Cell(row, 1).Value = item.RelativePath;
                    worksheet.Cell(row, 2).Value = item.DifferenceType.ToString();
                    worksheet.Cell(row, 3).Value = item.IsDirectory ? "Directory" : $"{item.Size} bytes";
                    worksheet.Cell(row, 4).Value = item.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss");
                    worksheet.Cell(row, 5).Value = item.Description ?? "";
                }

                // Format headers
                var headerRange = worksheet.Range(1, 1, 1, 5);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

                // Auto-fit columns
                worksheet.Columns().AdjustToContents();

                workbook.SaveAs(filePath);
            });
        }

        public async Task ExportToCsvAsync(List<FileItem> items, string filePath)
        {
            using var writer = new StringWriter();
            using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture));

            // Write headers
            csv.WriteField("File Path");
            csv.WriteField("Difference Type");
            csv.WriteField("Size");
            csv.WriteField("Last Modified");
            csv.WriteField("Description");
            csv.NextRecord();

            // Write data
            foreach (var item in items)
            {
                csv.WriteField(item.RelativePath);
                csv.WriteField(item.DifferenceType.ToString());
                csv.WriteField(item.IsDirectory ? "Directory" : $"{item.Size} bytes");
                csv.WriteField(item.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss"));
                csv.WriteField(item.Description ?? "");
                csv.NextRecord();
            }

            await File.WriteAllTextAsync(filePath, writer.ToString(), Encoding.UTF8);
        }

        public async Task ExportToTextAsync(List<FileItem> items, string filePath)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Comparison Results");
            sb.AppendLine("================");
            sb.AppendLine();

            var groupedItems = items.GroupBy(x => x.DifferenceType);

            foreach (var group in groupedItems)
            {
                sb.AppendLine($"{group.Key} Items ({group.Count()}):");
                sb.AppendLine(new string('-', 40));

                foreach (var item in group)
                {
                    sb.AppendLine($"  {item.RelativePath}");
                    if (!string.IsNullOrEmpty(item.Description))
                    {
                        sb.AppendLine($"    Description: {item.Description}");
                    }
                    sb.AppendLine($"    Size: {(item.IsDirectory ? "Directory" : $"{item.Size} bytes")}");
                    sb.AppendLine($"    Last Modified: {item.LastWriteTime:yyyy-MM-dd HH:mm:ss}");
                    sb.AppendLine();
                }
                sb.AppendLine();
            }

            await File.WriteAllTextAsync(filePath, sb.ToString(), Encoding.UTF8);
        }

        public async Task<Dictionary<string, string>> ImportDescriptionsFromExcelAsync(string filePath)
        {
            return await Task.Run(() =>
            {
                var descriptions = new Dictionary<string, string>();

                using var workbook = new XLWorkbook(filePath);
                var worksheet = workbook.Worksheet(1);

                var rows = worksheet.RowsUsed().Skip(1); // Skip header row

                foreach (var row in rows)
                {
                    var filePath = row.Cell(1).GetString();
                    var description = row.Cell(5).GetString();

                    if (!string.IsNullOrEmpty(filePath) && !string.IsNullOrEmpty(description))
                    {
                        descriptions[filePath] = description;
                    }
                }

                return descriptions;
            });
        }

        public async Task<Dictionary<string, string>> ImportDescriptionsFromCsvAsync(string filePath)
        {
            var descriptions = new Dictionary<string, string>();

            using var reader = new StringReader(await File.ReadAllTextAsync(filePath, Encoding.UTF8));
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture));

            var records = csv.GetRecords<dynamic>().ToList();

            foreach (var record in records)
            {
                var dict = (IDictionary<string, object>)record;

                if (dict.TryGetValue("File Path", out var filePathObj) &&
                    dict.TryGetValue("Description", out var descriptionObj))
                {
                    var filePathStr = filePathObj?.ToString();
                    var descriptionStr = descriptionObj?.ToString();

                    if (!string.IsNullOrEmpty(filePathStr) && !string.IsNullOrEmpty(descriptionStr))
                    {
                        descriptions[filePathStr] = descriptionStr;
                    }
                }
            }

            return descriptions;
        }
    }
}