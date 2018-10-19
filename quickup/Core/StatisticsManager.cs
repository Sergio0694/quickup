using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;
using quickup.Enums;

namespace quickup.Core
{
    /// <summary>
    /// A <see langword="class"/> that calculates and prepares data statistics on a requested operation
    /// </summary>
    internal sealed class StatisticsManager
    {
        // The timer to keep track of the elapsed time
        [NotNull]
        private readonly Stopwatch Stopwatch = new Stopwatch();

        // A map between each available file extensions and its relative data
        [NotNull]
        private readonly ConcurrentDictionary<string, (int Count, long Bytes)> SizeMap = new ConcurrentDictionary<string, (int, long)>();

        // The map for the number of different file operations performed
        [NotNull]
        private readonly ConcurrentDictionary<FileUpdateType, int> FileOperationsMap = new ConcurrentDictionary<FileUpdateType, int>();

        // The total number of duplicate bytes identified
        private long _Bytes;

        /// <summary>
        /// Creates a new instance and automatically starts the internal timer
        /// </summary>
        public StatisticsManager() => Stopwatch.Start();

        /// <summary>
        /// Adds a new processed file to the statistics
        /// </summary>
        /// <param name="path">The path of the processed file</param>
        /// <param name="type">The operation performed on the current file</param>
        public void AddOperation([NotNull] string path, FileUpdateType type)
        {
            if (type != FileUpdateType.Remove)
            {
                long size = new FileInfo(path).Length;
                Interlocked.Add(ref _Bytes, size);
                SizeMap.AddOrUpdate(Path.GetExtension(path), (1, size), (_, pair) => (pair.Count + 1, pair.Bytes + size));
            }
            FileOperationsMap.AddOrUpdate(type, _ => 1, (_, i) => i + 1);
        }

        /// <summary>
        /// Stops the internal timer
        /// </summary>
        public void StopTracking() => Stopwatch.Stop();

        /// <summary>
        /// Prepares the statistics to display to the user
        /// </summary>
        /// <param name="verbose">Indicates whether or not to also include additional info</param>
        [Pure, NotNull, ItemNotNull]
        public IEnumerable<string> ExtractStatistics(bool verbose)
        {
            yield return $"Elapsed time:\t\t{Stopwatch.Elapsed:g}";
            if (verbose)
            {
                if (FileOperationsMap.TryGetValue(FileUpdateType.Add, out int i) && i > 0) yield return $"Added:\t\t\t{i}";
                if (FileOperationsMap.TryGetValue(FileUpdateType.Update, out i) && i > 0) yield return $"Updated:\t\t\t{i}";
                if (FileOperationsMap.TryGetValue(FileUpdateType.Remove, out i) && i > 0) yield return $"Removed:\t\t\t{i}";
                yield return $"Bytes copied:\t\t{_Bytes}";
            }
            yield return $"Approximate size:\t{_Bytes.ToFileSizeString()}";
            if (verbose)
            {
                // Frequent file extensions
                var frequent = (
                    from pair in SizeMap
                    orderby pair.Value.Count descending
                    let extension = string.IsNullOrEmpty(pair.Key) ? "{none}" : pair.Key
                    select (Key: extension, pair.Value.Count)).Take(5).ToArray();
                if (frequent.Length == 0) yield break;
                yield return "Frequent extensions:\t" + frequent.Skip(1).Aggregate(
                    $"{frequent[0].Key}: {frequent[0].Count}",
                    (seed, value) => $"{seed}, {value.Key}: {value.Count}");

                // Heaviest file extensions
                var heaviest = (
                    from pair in SizeMap
                    orderby pair.Value.Bytes descending
                    let extension = string.IsNullOrEmpty(pair.Key) ? "{none}" : pair.Key
                    select (Key: extension, pair.Value.Bytes)).Take(5).ToArray();
                yield return "Heaviest extensions:\t" + heaviest.Skip(1).Aggregate(
                    $"{heaviest[0].Key}: {heaviest[0].Bytes.ToFileSizeString()}",
                    (seed, value) => $"{seed}, {value.Key}: {value.Bytes.ToFileSizeString()}");
            }

        }
    }
}
