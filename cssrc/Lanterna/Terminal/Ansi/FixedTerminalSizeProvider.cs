using Lanterna.Terminal;

namespace Lanterna.Terminal.Ansi
{
    /// <summary>
    /// Simple implementation of <see cref="UnixTerminalSizeQuerier"/> that returns a fixed size.
    /// </summary>
    public class FixedTerminalSizeProvider : UnixTerminalSizeQuerier
    {
        private readonly TerminalSize _size;

        public FixedTerminalSizeProvider(TerminalSize size)
        {
            _size = size;
        }

        public TerminalSize QueryTerminalSize() => _size;
    }
}
