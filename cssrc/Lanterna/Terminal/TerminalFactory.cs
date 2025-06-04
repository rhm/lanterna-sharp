namespace Lanterna.Terminal
{
    /// <summary>
    /// Factory interface for creating Terminal instances.
    /// </summary>
    public interface TerminalFactory
    {
        Terminal CreateTerminal();
    }
}
