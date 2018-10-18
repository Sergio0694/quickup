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
            IReadOnlyCollection<string>
                extensions = options.Preset == ExtensionsPreset.None
                    ? options.FileInclusions.Select(ext => ext.ToLowerInvariant()).ToArray()
                    : options.Preset.Convert(),
                exclusions = new HashSet<string>(options.FileExclusions.Select(entry => $".{entry.ToLowerInvariant()}"));
            IReadOnlyDictionary<string, IReadOnlyCollection<string>> map = LoadFiles(options.SourceDirectory, extensions, exclusions, options.Verbose);

            // Process the loaded files from the source directory
            SyncFiles(map, options.SourceDirectory, options.TargetDirectory, statistics);

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
                    map.Add(directory, query.Where(file => !exclusions.Contains(Path.GetExtension(file))).ToArray());

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
        [SuppressMessage("ReSharper", "AccessToDisposedClosure")] // Progress bar inside parallel code
        private static void SyncFiles(
            [NotNull] IReadOnlyDictionary<string, IReadOnlyCollection<string>> map,
            [NotNull] string source, [NotNull] string target,
            [NotNull] StatisticsManager statistics)
        {
            using (AsciiProgressBar bar = new AsciiProgressBar())
            {
                int i = 0;
                string name = Path.GetFileName(source); // Get the name of the source folder

                // Copy the files in parallel, one task for each subdirectory in the source tree
                Parallel.ForEach(map, pair =>
                {
                    try
                    {
                        // Create the target directory
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
                            if (!File.Exists(copy) || File.GetLastWriteTimeUtc(file).CompareTo(File.GetLastWriteTimeUtc(copy)) > 0)
                            {
                                File.Copy(file, copy);
                                statistics.AddFile(copy);
                            }
                        }
                        bar.Report((double)Interlocked.Increment(ref i) / map.Count);
                    }
                    catch (IOException)
                    {
                        // Carry on
                    }
                });
            }
        }

        #endregion
    }
}
