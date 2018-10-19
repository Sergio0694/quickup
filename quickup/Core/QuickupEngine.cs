using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using quickup.Enums;
using quickup.Options;

namespace quickup.Core
{
    /// <summary>
    /// The core <see langword="class"/> that contains the actual logic of the quickup executable
    /// </summary>
    internal static class QuickupEngine
    {
        #region APIs

        /// <summary>
        /// Executes the run command
        /// </summary>
        /// <param name="options">The command options</param>
        [NotNull]
        public static StatisticsManager Run([NotNull] QuickupOptions options)
        {
            // Track the current operation
            StatisticsManager statistics = new StatisticsManager();

            // Load the source files to sync
            ConsoleHelper.WriteTaggedMessage(MessageType.Info, "Querying files...");
            IReadOnlyCollection<string>
                extensions = options.Preset == ExtensionsPreset.None
                    ? options.FileInclusions.Select(ext => ext.ToLowerInvariant()).ToArray()
                    : options.Preset.Convert(),
                exclusions = new HashSet<string>(options.FileExclusions.Select(entry => $".{entry.ToLowerInvariant()}"));
            IReadOnlyDictionary<string, IReadOnlyCollection<string>> map = LoadFiles(options.SourceDirectory, extensions, exclusions, options.Verbose);

            // Process the loaded files from the source directory
            ConsoleHelper.WriteTaggedMessage(MessageType.Info, "Syncing files...");
            int threads = options.Multithread
                ? options.Threads == -1
                    ? Environment.ProcessorCount
                    : Environment.ProcessorCount >= options.Threads ? options.Threads : Environment.ProcessorCount
                : 1;
            SyncFiles(map, options.SourceDirectory, options.TargetDirectory, statistics, threads);

            // Display the statistics
            statistics.StopTracking();
            return statistics;
        }

        #endregion

        #region Implementation

        /// <summary>
        /// Loads the files to sync from a source directory
        /// </summary>
        /// <param name="path">The path of the source directory to load</param>
        /// <param name="extensions">The list of file extensions to exclusively include</param>
        /// <param name="exclusions">The list of file extensions to exclude</param>
        /// <param name="verbose">Indicates whether or not to display info for blocked directories</param>
        private static IReadOnlyDictionary<string, IReadOnlyCollection<string>> LoadFiles(
            [NotNull] string path,
            [NotNull, ItemNotNull] IReadOnlyCollection<string> extensions,
            [NotNull, ItemNotNull] IReadOnlyCollection<string> exclusions,
            bool verbose)
        {
            Dictionary<string, IReadOnlyCollection<string>> map = new Dictionary<string, IReadOnlyCollection<string>>();

            void Explore(string directory)
            {
                try
                {
                    // Load the files with the specified extensions, or all of them if none is provided
                    IEnumerable<string> query = extensions.Count == 0
                        ? Directory.EnumerateFiles(directory, "*")
                        : extensions.SelectMany(extension => Directory.EnumerateFiles(directory, $"*.{extension}"));
                    IReadOnlyCollection<string> files = query.Where(file => !exclusions.Contains(Path.GetExtension(file))).ToArray();
                    if (files.Count > 0) map.Add(directory, files);

                    // Drill down
                    foreach (string subdirectory in Directory.EnumerateDirectories(directory))
                        Explore(subdirectory);
                }
                catch (Exception e) when (e is UnauthorizedAccessException || e is PathTooLongException || e is DirectoryNotFoundException)
                {
                    // Just ignore and carry on
                    if (verbose) ConsoleHelper.WriteTaggedMessage(MessageType.Error, path);
                }
            }
            Explore(path);

            return map;
        }

        /// <summary>
        /// Syncs the loaded files to the specified target directory
        /// </summary>
        /// <param name="map">The map of files to sync</param>
        /// <param name="source">The original source directory</param>
        /// <param name="target">The root target directory</param>
        /// <param name="statistics">The statistics instance to track the performed operations</param>
        /// <param name="threads">The maximum number of threads to use to perform the copy operations</param>
        [SuppressMessage("ReSharper", "AccessToDisposedClosure")] // Progress bar inside parallel code
        private static void SyncFiles(
            [NotNull] IReadOnlyDictionary<string, IReadOnlyCollection<string>> map,
            [NotNull] string source, [NotNull] string target,
            [NotNull] StatisticsManager statistics,
            int threads)
        {
            using (AsciiProgressBar bar = new AsciiProgressBar())
            {
                int progress = 0, total = map.Values.Sum(l => l.Count);
                string name = Path.GetFileName(source); // Get the name of the source folder

                // Copy the files in parallel, one task for each subdirectory in the source tree
                IReadOnlyList<KeyValuePair<string, IReadOnlyCollection<string>>> files = map.ToArray();
                Parallel.For(0, files.Count, new ParallelOptions { MaxDegreeOfParallelism = threads }, i =>
                {
                    try
                    {
                        // Create the target directory
                        KeyValuePair<string, IReadOnlyCollection<string>> pair = files[i];
                        string
                            relative = pair.Key.Substring(source.Length),
                            folder = string.IsNullOrEmpty(relative)
                                ? Path.Join(target, name)
                                : Path.Join(target, name, relative);
                        Directory.CreateDirectory(folder);

                        // Copy the original files, when needed
                        foreach (string file in pair.Value)
                        {
                            string copy = Path.Join(folder, Path.GetFileName(file));
                            if (!File.Exists(copy))
                            {
                                File.Copy(file, copy);
                                statistics.AddOperation(copy, FileUpdateType.Add);
                            }
                            else if (File.GetLastWriteTimeUtc(file).CompareTo(File.GetLastWriteTimeUtc(copy)) > 0)
                            {
                                File.Copy(file, copy, true);
                                statistics.AddOperation(copy, FileUpdateType.Update);
                            }
                            bar.Report((double)Interlocked.Increment(ref progress) / total);
                        }
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // Carry on
                    }
                });
            }
        }

        #endregion
    }
}
