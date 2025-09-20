using System.IO;
using ReleaseTrackerWpf.Models;

namespace ReleaseTrackerWpf.Services
{
    public class ComparisonService
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

        public (List<FileItem> oldUnionItems, List<FileItem> newUnionItems) CreateUnionStructure(DirectorySnapshot oldSnapshot, DirectorySnapshot newSnapshot)
        {
            var oldItems = FlattenItems(oldSnapshot.Items).ToDictionary(x => x.RelativePath, x => x);
            var newItems = FlattenItems(newSnapshot.Items).ToDictionary(x => x.RelativePath, x => x);

            // Get all unique paths from both snapshots
            var allPaths = oldItems.Keys.Union(newItems.Keys).OrderBy(x => x).ToList();

            var oldUnionItems = new List<FileItem>();
            var newUnionItems = new List<FileItem>();

            foreach (var path in allPaths)
            {
                var oldItem = oldItems.TryGetValue(path, out var old) ? old : null;
                var newItem = newItems.TryGetValue(path, out var new_) ? new_ : null;

                // Create items for both sides
                var oldUnionItem = oldItem != null ? CloneFileItem(oldItem) : CreatePlaceholderItem(path, newItem?.IsDirectory ?? false);
                var newUnionItem = newItem != null ? CloneFileItem(newItem) : CreatePlaceholderItem(path, oldItem?.IsDirectory ?? false);

                // Set difference types for visual indication
                if (oldItem == null && newItem != null)
                {
                    // Added item - gray out on left, green on right
                    oldUnionItem.DifferenceType = DifferenceType.Deleted; // Use deleted type for gray out
                    newUnionItem.DifferenceType = DifferenceType.Added;
                }
                else if (oldItem != null && newItem == null)
                {
                    // Deleted item - red on left, gray out on right
                    oldUnionItem.DifferenceType = DifferenceType.Deleted;
                    newUnionItem.DifferenceType = DifferenceType.Added; // Use added type for gray out
                }
                else if (oldItem != null && newItem != null)
                {
                    // Both exist - check if modified
                    if (IsItemModified(oldItem, newItem))
                    {
                        oldUnionItem.DifferenceType = DifferenceType.Modified;
                        newUnionItem.DifferenceType = DifferenceType.Modified;
                    }
                    else
                    {
                        oldUnionItem.DifferenceType = DifferenceType.Unchanged;
                        newUnionItem.DifferenceType = DifferenceType.Unchanged;
                    }
                }

                oldUnionItems.Add(oldUnionItem);
                newUnionItems.Add(newUnionItem);
            }

            // Build hierarchical structure
            return (BuildHierarchicalStructure(oldUnionItems), BuildHierarchicalStructure(newUnionItems));
        }

        private FileItem CreatePlaceholderItem(string relativePath, bool isDirectory)
        {
            var name = Path.GetFileName(relativePath);
            if (string.IsNullOrEmpty(name))
                name = "Root";

            return new FileItem
            {
                Name = name,
                FullPath = string.Empty,
                RelativePath = relativePath,
                IsDirectory = isDirectory,
                Size = 0,
                LastWriteTime = DateTime.MinValue,
                Children = new List<FileItem>(),
                DifferenceType = DifferenceType.None,
                Description = null
            };
        }

        private List<FileItem> BuildHierarchicalStructure(List<FileItem> flatItems)
        {
            var rootItems = new List<FileItem>();
            var itemMap = flatItems.ToDictionary(x => x.RelativePath, x => x);

            foreach (var item in flatItems)
            {
                var parentPath = Path.GetDirectoryName(item.RelativePath)?.Replace('\\', '/');
                if (string.IsNullOrEmpty(parentPath))
                {
                    // Root level item
                    rootItems.Add(item);
                }
                else
                {
                    // Find parent and add as child
                    if (itemMap.TryGetValue(parentPath, out var parent))
                    {
                        parent.Children.Add(item);
                    }
                    else
                    {
                        // Parent not found, add to root
                        rootItems.Add(item);
                    }
                }
            }

            return rootItems;
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