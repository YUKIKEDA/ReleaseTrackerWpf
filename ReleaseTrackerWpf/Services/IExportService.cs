using ReleaseTrackerWpf.Models;

namespace ReleaseTrackerWpf.Services
{
    public interface IExportService
    {
        Task ExportToExcelAsync(List<FileItem> items, string filePath);
        Task ExportToCsvAsync(List<FileItem> items, string filePath);
        Task ExportToTextAsync(List<FileItem> items, string filePath);
        Task<Dictionary<string, string>> ImportDescriptionsFromExcelAsync(string filePath);
        Task<Dictionary<string, string>> ImportDescriptionsFromCsvAsync(string filePath);
    }
}