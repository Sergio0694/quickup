using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;

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

        // The total number of processed files
        private int _Files;

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
        public void AddFile([NotNull] string path)
        {
            Interlocked.Increment(ref _Files);
            long size = new FileInfo(path).Length;
            Interlocked.Add(ref _Bytes, size);
            SizeMap.AddOrUpdate(Path.GetExtension(path), (1, size), (_, pair) => (pair.Count + 1, pair.Bytes + size));
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
            yield return $"Copied files:\t\t{_Files}";
            if (verbose) yield return $"Bytes copied:\t\t{_Bytes}";
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
