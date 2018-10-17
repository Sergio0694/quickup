using CommandLine;

namespace quickup.Options
{
    /// <summary>
    /// A <see langword="class"/> that represents the command to execute a backup with the given parameters
    /// </summary>
    [Verb("run", HelpText = "Executes or updates a backup with the given parameters")]
    internal sealed class RunOption : QuickupOptionsBase { }
}
