using ReleaseTrackerWpf.Models;
using System.Collections.Generic;
using System.Linq;

namespace ReleaseTrackerWpf.Services
{
    public class ComparisonService : IComparisonService
    {
        public ComparisonResult Compare(DirectorySnapshot oldSnapshot, DirectorySnapshot newSnapshot)
        {
            var result = new ComparisonResult();

            var oldItems = FlattenItems(oldSnapshot.Items).ToDictionary(x => x.RelativePath, x => x);
            var newItems = FlattenItems(newSnapshot.Items).ToDictionary(x => x.RelativePath, x => x);

            // Find added items (in new but not in old)
            foreach (var newItem in newItems.Values)
            {
                if (!oldItems.ContainsKey(newItem.RelativePath))
                {
                    var addedItem = CloneFileItem(newItem);
                    addedItem.DifferenceType = DifferenceType.Added;
                    result.AddedItems.Add(addedItem);
                    result.AllDifferences.Add(addedItem);
                }
            }

            // Find deleted items (in old but not in new)
            foreach (var oldItem in oldItems.Values)
            {
                if (!newItems.ContainsKey(oldItem.RelativePath))
                {
                    var deletedItem = CloneFileItem(oldItem);
                    deletedItem.DifferenceType = DifferenceType.Deleted;
                    result.DeletedItems.Add(deletedItem);
                    result.AllDifferences.Add(deletedItem);
                }
            }

            // Find modified items (exist in both but different)
            foreach (var newItem in newItems.Values)
            {
                if (oldItems.TryGetValue(newItem.RelativePath, out var oldItem))
                {
                    if (IsItemModified(oldItem, newItem))
                    {
                        var modifiedItem = CloneFileItem(newItem);
                        modifiedItem.DifferenceType = DifferenceType.Modified;
                        result.ModifiedItems.Add(modifiedItem);
                        result.AllDifferences.Add(modifiedItem);
                    }
                }
            }

            return result;
        }

        private IEnumerable<FileItem> FlattenItems(IEnumerable<FileItem> items)
        {
            foreach (var item in items)
            {
                yield return item;
                foreach (var child in FlattenItems(item.Children))
                {
                    yield return child;
                }
            }
        }

        private bool IsItemModified(FileItem oldItem, FileItem newItem)
        {
            if (oldItem.IsDirectory != newItem.IsDirectory)
                return true;

            if (!oldItem.IsDirectory)
            {
                // For files, compare size and last write time
                return oldItem.Size != newItem.Size ||
                       oldItem.LastWriteTime != newItem.LastWriteTime;
            }

            return false;
        }

        private FileItem CloneFileItem(FileItem original)
        {
            return new FileItem
            {
                Name = original.Name,
                FullPath = original.FullPath,
                RelativePath = original.RelativePath,
                IsDirectory = original.IsDirectory,
                Size = original.Size,
                LastWriteTime = original.LastWriteTime,
                Children = new List<FileItem>(), // Don't clone children for difference items
                DifferenceType = original.DifferenceType,
                Description = original.Description
            };
        }
    }
}