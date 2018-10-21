using System.Collections.Generic;
using JetBrains.Annotations;

namespace quickup.Enums
{
    /// <summary>
    /// An <see langword="enum"/> that represents a preset for a group of file types
    /// </summary>
    internal enum ExtensionsPreset
    {
        None,

        // Classic (inclusions only)
        Documents,
        Images,
        Music,
        Videos,
        Code,

        // Special (exclusions/directoryes)
        VS,
        UWP
    }

    /// <summary>
    /// A simple <see langword="class"/> to extract file extensions from a preset
    /// </summary>
    internal static class ExtensionsPresetConverter
    {
        /// <summary>
        /// Tries to get a list of file inclusions for the given present
        /// </summary>
        /// <param name="preset">The source preset to convert</param>
        /// <param name="inclusions">The resulting inclusions list, if available</param>
        [MustUseReturnValue]
        public static bool TryConvert(this ExtensionsPreset preset, out IReadOnlyList<string> inclusions)
        {
            switch (preset)
            {
                case ExtensionsPreset.Documents:
                    inclusions = new[] { "doc", "docx", "txt", "rtf", "tex", "csv", "pps", "ppsx", "ppt", "pptx", "xls", "xlsx", "xlr", "odt", "pdf" };
                    return true;
                case ExtensionsPreset.Images:
                    inclusions = new[] { "ai", "bmp", "gif", "ico", "jpeg", "jpg", "png", "ps", "psd", "svg", "tif", "tiff", "gif", "tga", "yuv" };
                    return true;
                case ExtensionsPreset.Music:
                    inclusions = new[] { "aif", "cda", "mid", "midi", "mp3", "mpa", "ogg", "wav", "wma", "wpl", "flac", "iff", "m3u", "m4a" };
                    return true;
                case ExtensionsPreset.Videos:
                    inclusions = new[] { "3g2", "3gp", "avi", "flv", "h264", "m4v", "mkv", "mov", "mp4", "mpg", "mpeg", "rm", "swf", "vob", "wmv", "asf", "rm", "srt" };
                    return true;
                case ExtensionsPreset.Code:
                    inclusions = new[] { "c", "class", "cpp", "cc", "cu", "cs", "h", "java", "sh", "swift", "vb", "rb", "asp", "aspx", "css", "htm", "html", "js", "jsp", "php", "xml", "xaml", "lua", "m", "pl", "py", "pyc", "sh" };
                    return true;
                default:
                    inclusions = default;
                    return false;
            }
        }

        /// <summary>
        /// Tries to expand the input preset, if it's not a classic preset
        /// </summary>
        /// <param name="preset">The preset to expand</param>
        /// <param name="expansion">The expansion result, if available</param>
        [MustUseReturnValue]
        public static bool TryExpand(this ExtensionsPreset preset, out (IReadOnlyList<string> Exclusions, IReadOnlyList<string> Directories) expansion)
        {
            switch (preset)
            {
                case ExtensionsPreset.VS:
                    expansion = (new[] { "user", "suo" }, new[] { ".git", ".vs", "bin", "obj" });
                    return true;
                case ExtensionsPreset.UWP:
                    expansion = (new[] { "user", "suo", "pfx" }, new[] { ".git", ".vs", "bin", "obj", "Builds", "BundleArtifacts" });
                    return true;
                default:
                    expansion = default;
                    return false;
            }
        }
    }
}
