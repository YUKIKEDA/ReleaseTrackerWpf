using System.Collections.ObjectModel;
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
                    OldSnapshot = oldSnapshot,
                    NewSnapshot = newSnapshot,
                    ComparisonTime = DateTime.Now
                };

                var leftTree = new ObservableCollection<FileSystemEntry>();
                var rightTree = new ObservableCollection<FileSystemEntry>();
                var statistics = new ComparisonStatistics();

                // Items contains the direct children of the scanned directory
                var oldItems = oldSnapshot.Items;
                var newItems = newSnapshot.Items;

                // Compare all items at each level
                CompareItemLists(oldItems, newItems, leftTree, rightTree, statistics);

                result.LeftTreeItems = leftTree;
                result.RightTreeItems = rightTree;
                result.Statistics = statistics;

                return result;
            });
        }

        private void CompareItemLists(List<FileSystemEntry> oldItems, List<FileSystemEntry> newItems,
            ObservableCollection<FileSystemEntry> leftTree, ObservableCollection<FileSystemEntry> rightTree,
            ComparisonStatistics statistics)
        {
            // Get all unique item names from both lists
            var allItemNames = oldItems.Select(i => i.Name)
                .Union(newItems.Select(i => i.Name))
                .OrderBy(name => name)
                .ToList();

            foreach (var itemName in allItemNames)
            {
                var oldItem = oldItems.FirstOrDefault(i => i.Name == itemName);
                var newItem = newItems.FirstOrDefault(i => i.Name == itemName);

                var (leftItem, rightItem) = CompareItem(oldItem, newItem, statistics);

                if (leftItem != null)
                    leftTree.Add(leftItem);

                if (rightItem != null)
                    rightTree.Add(rightItem);
            }
        }

        private (FileSystemEntry? leftEntry, FileSystemEntry? rightEntry) CompareItem(
            FileSystemEntry? oldItem, FileSystemEntry? newItem, ComparisonStatistics statistics)
        {
            // Determine the difference type
            var differenceType = DetermineDifferenceType(oldItem, newItem);

            // Create display entries for left and right sides
            var leftEntry = CreateDisplayEntry(oldItem, newItem, differenceType, isLeftSide: true);
            var rightEntry = CreateDisplayEntry(oldItem, newItem, differenceType, isLeftSide: false);

            // Update statistics
            UpdateStatistics(differenceType, oldItem, newItem, statistics);

            // Process children if this is a directory
            if (leftEntry?.IsDirectory == true || rightEntry?.IsDirectory == true)
            {
                var oldChildren = oldItem?.Children ?? new List<FileSystemEntry>();
                var newChildren = newItem?.Children ?? new List<FileSystemEntry>();

                var leftChildTree = new ObservableCollection<FileSystemEntry>();
                var rightChildTree = new ObservableCollection<FileSystemEntry>();

                CompareItemLists(oldChildren, newChildren, leftChildTree, rightChildTree, statistics);

                // Convert ObservableCollection to List for FileSystemEntry.Children
                if (leftEntry != null)
                    leftEntry.Children = leftChildTree.ToList();

                if (rightEntry != null)
                    rightEntry.Children = rightChildTree.ToList();
            }

            return (leftEntry, rightEntry);
        }

        private DifferenceType DetermineDifferenceType(FileSystemEntry? oldEntry, FileSystemEntry? newEntry)
        {
            if (oldEntry == null && newEntry != null)
                return DifferenceType.Added;

            if (oldEntry != null && newEntry == null)
                return DifferenceType.Deleted;

            if (oldEntry != null && newEntry != null)
            {
                // Check if file was modified (size, last write time, etc.)
                if (oldEntry.IsDirectory == newEntry.IsDirectory)
                {
                    if (!oldEntry.IsDirectory)
                    {
                        if (oldEntry.Size != newEntry.Size ||
                            oldEntry.LastWriteTime != newEntry.LastWriteTime)
                        {
                            return DifferenceType.Modified;
                        }
                    }
                    return DifferenceType.Unchanged;
                }
                else
                {
                    return DifferenceType.Modified; // Type changed (file to dir or vice versa)
                }
            }

            return DifferenceType.None;
        }

        private FileSystemEntry? CreateDisplayEntry(FileSystemEntry? oldEntry, FileSystemEntry? newEntry,
            DifferenceType differenceType, bool isLeftSide)
        {
            FileSystemEntry? sourceEntry = null;

            if (isLeftSide)
            {
                if (differenceType == DifferenceType.Added)
                {
                    // Left side: show placeholder (grayed out) for added items
                    sourceEntry = newEntry;
                    if (sourceEntry != null)
                    {
                        sourceEntry = CloneEntry(sourceEntry);
                        sourceEntry.DifferenceType = DifferenceType.None; // Grayed out
                    }
                }
                else
                {
                    sourceEntry = oldEntry;
                    if (sourceEntry != null)
                    {
                        sourceEntry = CloneEntry(sourceEntry);
                        sourceEntry.DifferenceType = differenceType;
                    }
                }
            }
            else
            {
                if (differenceType == DifferenceType.Deleted)
                {
                    // Right side: show placeholder (grayed out) for deleted items
                    sourceEntry = oldEntry;
                    if (sourceEntry != null)
                    {
                        sourceEntry = CloneEntry(sourceEntry);
                        sourceEntry.DifferenceType = DifferenceType.None; // Grayed out
                    }
                }
                else
                {
                    sourceEntry = newEntry;
                    if (sourceEntry != null)
                    {
                        sourceEntry = CloneEntry(sourceEntry);
                        sourceEntry.DifferenceType = differenceType;
                    }
                }
            }

            return sourceEntry;
        }

        private FileSystemEntry CloneEntry(FileSystemEntry source)
        {
            return new FileSystemEntry
            {
                Name = source.Name,
                FullPath = source.FullPath,
                RelativePath = source.RelativePath,
                IsDirectory = source.IsDirectory,
                Size = source.Size,
                LastWriteTime = source.LastWriteTime,
                DifferenceType = source.DifferenceType,
                Description = source.Description,
                Children = new List<FileSystemEntry>()
            };
        }

        private void UpdateStatistics(DifferenceType differenceType, FileSystemEntry? oldEntry, FileSystemEntry? newEntry, ComparisonStatistics statistics)
        {
            var isDirectory = oldEntry?.IsDirectory ?? newEntry?.IsDirectory ?? false;

            switch (differenceType)
            {
                case DifferenceType.Added:
                    if (isDirectory)
                        statistics.AddedDirectories++;
                    else
                        statistics.AddedFiles++;
                    break;
                case DifferenceType.Deleted:
                    if (isDirectory)
                        statistics.DeletedDirectories++;
                    else
                        statistics.DeletedFiles++;
                    break;
                case DifferenceType.Modified:
                    if (!isDirectory)
                        statistics.ModifiedFiles++;
                    break;
                case DifferenceType.Unchanged:
                    if (!isDirectory)
                        statistics.UnchangedFiles++;
                    break;
            }
        }
    }
}