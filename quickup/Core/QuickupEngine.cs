using System;
using JetBrains.Annotations;
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
        public static int Run([NotNull] RunOption options)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
