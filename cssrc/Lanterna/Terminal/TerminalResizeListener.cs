namespace Lanterna.Terminal
{
    /// <summary>
    /// Listener notified when the terminal changes size.
    /// </summary>
    public interface TerminalResizeListener
    {
        void OnResized(Terminal terminal, TerminalSize newSize);
    }
}
