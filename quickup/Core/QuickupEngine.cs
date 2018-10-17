using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public static void Run([NotNull] RunOption options)
        {
            IReadOnlyCollection<string> extensions = options.Preset?.Convert()
                                                     ?? options.FileInclusions.Select(ext => ext.ToLowerInvariant()).ToArray();
            IReadOnlyDictionary<string, IEnumerable<string>> map = LoadFiles(options.SourceDirectory, extensions, options.Verbose);

            string name = Path.GetFileName(options.SourceDirectory);
            IReadOnlyCollection<string> exclusions = options.FileExclusions.Select(entry => $".{entry.ToLowerInvariant()}").ToArray(); // TODO: use this

            Parallel.ForEach(map, pair =>
            {
                // Create the target directory
                string
                    relative = pair.Key.Substring(options.SourceDirectory.Length),
                    folder = string.IsNullOrEmpty(relative)
                        ? Path.Join(options.TargetDirectory, name)
                        : Path.Join(options.TargetDirectory, name, relative);
                Directory.CreateDirectory(folder);

                foreach (string file in pair.Value)
                {
                    string copy = Path.Join(folder, Path.GetFileName(file));
                    if (!File.Exists(copy)) File.Copy(file, copy);
                }
            });
        }

        private static IReadOnlyDictionary<string, IEnumerable<string>> LoadFiles(
            [NotNull] string path,
            [NotNull, ItemNotNull] IReadOnlyCollection<string> extensions,
            bool verbose)
        {
            Dictionary<string, IEnumerable<string>> map = new Dictionary<string, IEnumerable<string>>();

            void Explore(string directory)
            {
                try
                {
                    // Load the files with the specified extensions, or all of them if none is provided
                    IEnumerable<string> query = extensions.Count == 0
                        ? Directory.EnumerateFiles(directory, "*")
                        : extensions.SelectMany(extension => Directory.EnumerateFiles(directory, $"*.{extension}"));
                    map.Add(directory, query);

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

        #endregion
    }
}
