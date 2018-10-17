using System;
using System.Text;
using System.Threading;
using JetBrains.Annotations;

namespace quickup.Core
{
    /// <summary>
    /// An ASCII progress bar, see <see href="https://gist.github.com/DanielSWolf/0ab6a96899cc5377bf54"/>
    /// </summary>
    internal sealed class AsciiProgressBar : IDisposable, IProgress<double>
    {
        // Constants
        private const int BlockCount = 10;
        private readonly TimeSpan AnimationInterval = TimeSpan.FromSeconds(1.0 / 8);
        private const string Animation = @"|/-\";

        /// <summary>
        /// The <see cref="System.Threading.Timer"/> instance to use to play the animation
        /// </summary>
        [NotNull]
        private readonly Timer Timer;

        // Private fields
        private double _CurrentProgress;
        private string _CurrentText = string.Empty;
        private bool _Disposed;
        private int _AnimationIndex;

        /// <summary>
        /// Creates a new instance. Make sure to call this inside a <see langword="using"/> block
        /// </summary>
        public AsciiProgressBar()
        {
            Timer = new Timer(TimerHandler, new AutoResetEvent(false), TimeSpan.FromSeconds(1.0 / 8), TimeSpan.FromSeconds(1.0 / 8));

            // A progress bar is only for temporary display in a console window.
            // If the console output is redirected to a file, draw nothing.
            // Otherwise, we'll end up with a lot of garbage in the target file.
            if (!Console.IsOutputRedirected)
            {
                ResetTimer();
            }
        }

        /// <summary>
        /// Updates the current value of the progress bar
        /// </summary>
        /// <param name="value">The new value in [0, 1] to display</param>
        public void Report(double value)
        {
            // Make sure value is in [0, 1] range
            value = Math.Max(0, Math.Min(1, value));
            Interlocked.Exchange(ref _CurrentProgress, value);
        }

        // Refreshes the UI when the timer ticks
        private void TimerHandler(object state)
        {
            lock (Timer)
            {
                if (_Disposed) return;

                int progressBlockCount = (int)(_CurrentProgress * BlockCount);
                int percent = (int)(_CurrentProgress * 100);
                string text = string.Format("[{0}{1}] {2,3}% {3}",
                    new string('#', progressBlockCount), new string('-', BlockCount - progressBlockCount),
                    percent,
                    Animation[_AnimationIndex++ % Animation.Length]);
                UpdateText(text);

                ResetTimer();
            }
        }

        // Updates the displayed text
        private void UpdateText(string text)
        {
            // Get length of common portion
            int commonPrefixLength = 0;
            int commonLength = Math.Min(_CurrentText.Length, text.Length);
            while (commonPrefixLength < commonLength && text[commonPrefixLength] == _CurrentText[commonPrefixLength])
            {
                commonPrefixLength++;
            }

            // Backtrack to the first differing character
            StringBuilder outputBuilder = new StringBuilder();
            outputBuilder.Append('\b', _CurrentText.Length - commonPrefixLength);

            // Output new suffix
            outputBuilder.Append(text.Substring(commonPrefixLength));

            // If the new text is shorter than the old one: delete overlapping characters
            int overlapCount = _CurrentText.Length - text.Length;
            if (overlapCount > 0)
            {
                outputBuilder.Append(' ', overlapCount);
                outputBuilder.Append('\b', overlapCount);
            }

            Console.Write(outputBuilder);
            _CurrentText = text;
        }

        // Resets the timer when it is no longer needed
        private void ResetTimer()
        {
            Timer.Change(AnimationInterval, TimeSpan.FromMilliseconds(-1));
        }

        // Disposes the current instance (and clears the currently displayed text)
        public void Dispose()
        {
            lock (Timer)
            {
                _Disposed = true;
                UpdateText(string.Empty);
            }
        }
    }
}