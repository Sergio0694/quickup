using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace quickup.Enums
{
    /// <summary>
    /// An <see langword="enum"/> that represents a preset for a group of file types
    /// </summary>
    internal enum ExtensionsPreset
    {
        Documents,
        Images,
        Audio,
        Video,
        Code
    }

    /// <summary>
    /// A simple <see langword="class"/> to extract file extensions from a preset
    /// </summary>
    internal static class ExtensionsPresetConverter
    {
        /// <summary>
        /// Returns a list of file extensions for a given preset
        /// </summary>
        /// <param name="preset">The source preset to convert</param>
        [Pure, NotNull, ItemNotNull]
        public static IReadOnlyList<string> Convert(this ExtensionsPreset preset)
        {
            switch (preset)
            {
                case ExtensionsPreset.Documents:
                    return new[] { "doc", "docx", "txt", "rtf", "tex", "csv", "pps", "ppsx", "ppt", "pptx", "xls", "xlsx", "xlr", "odt", "pdf" };
                case ExtensionsPreset.Images:
                    return new[] { "ai", "bmp", "gif", "ico", "jpeg", "jpg", "png", "ps", "psd", "svg", "tif", "tiff", "gif", "tga", "yuv" };
                case ExtensionsPreset.Audio:
                    return new[] { "aif", "cda", "mid", "midi", "mp3", "mpa", "ogg", "wav", "wma", "wpl", "flac", "iff", "m3u", "m4a" };
                case ExtensionsPreset.Video:
                    return new[] { "3g2", "3gp", "avi", "flv", "h264", "m4v", "mkv", "mov", "mp4", "mpg", "mpeg", "rm", "swf", "vob", "wmv", "asf", "rm", "srt" };
                case ExtensionsPreset.Code:
                    return new[] { "c", "class", "cpp", "cc", "cu", "cs", "h", "java", "sh", "swift", "vb", "rb", "asp", "aspx", "css", "htm", "html", "js", "jsp", "php", "xml", "xaml", "lua", "m", "pl", "py", "pyc", "sh" };
                default: throw new ArgumentOutOfRangeException(nameof(preset), preset, "Invalid preset");
            }
        }
    }
}
