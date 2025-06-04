using Lanterna.Terminal;

namespace Lanterna.Terminal.Ansi
{
    /// <summary>
    /// Strategy interface for querying terminal size on Unix-like systems.
    /// </summary>
    public interface UnixTerminalSizeQuerier
    {
        TerminalSize QueryTerminalSize();
    }
}
