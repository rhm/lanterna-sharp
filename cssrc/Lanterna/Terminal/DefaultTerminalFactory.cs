namespace Lanterna.Terminal
{
    /// <summary>
    /// Simplified default implementation returning a basic terminal.
    /// The real Java version performs extensive detection; this stub just throws.
    /// </summary>
    public class DefaultTerminalFactory : TerminalFactory
    {
        public Terminal CreateTerminal()
        {
            throw new System.NotImplementedException("Terminal creation not implemented");
        }
    }
}
