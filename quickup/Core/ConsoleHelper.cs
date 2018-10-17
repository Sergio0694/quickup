using System;
using JetBrains.Annotations;
using quickup.Enums;

namespace quickup.Core
{
    /// <summary>
    /// A small <see langword="class"/> with some helper methods to print info to the user
    /// </summary>
    internal static class ConsoleHelper
    {
        /// <summary>
        /// Writes a new message to the console, making sure it's displayed at the start of a new line
        /// </summary>
        /// <param name="message">The message to display</param>
        public static void Write([NotNull] string message)
        {
            Console.Write($"{(Console.CursorLeft > 0 ? Environment.NewLine : string.Empty)}{message}");
        }

        /// <summary>
        /// Writes a new message to the console, making sure it's displayed at the start of a new line, and appends a line terminator
        /// </summary>
        /// <param name="message">The message to display</param>
        public static void WriteLine([NotNull] string message) => Write($"{message}{Environment.NewLine}");

        /// <summary>
        /// Shows a message to the user
        /// </summary>
        /// <param name="type">The type of message being displayed</param>
        /// <param name="message">The text of the message</param>
        public static void WriteTaggedMessage(MessageType type, [NotNull] string message)
        {
            switch (type)
            {
                case MessageType.Error:
                    WriteTaggedMessage(ConsoleColor.DarkYellow, "ERROR", message);
                    break;
                case MessageType.Info:
                    WriteTaggedMessage(ConsoleColor.DarkCyan, "INFO", message);
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(type), "Invalid message type");
            }
        }

        // Shows a tagged message to the user
        private static void WriteTaggedMessage(ConsoleColor errorColor, [NotNull] string tag, [NotNull] string message)
        {
            Console.ForegroundColor = errorColor;
            Console.Write($"{(Console.CursorLeft > 0 ? Environment.NewLine : string.Empty)}[{tag}] ");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
