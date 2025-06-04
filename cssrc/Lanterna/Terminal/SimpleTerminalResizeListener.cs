namespace Lanterna.Terminal
{
    /// <summary>
    /// Helper listener to keep track of terminal size changes.
    /// </summary>
    public class SimpleTerminalResizeListener : TerminalResizeListener
    {
        private bool _wasResized;
        private TerminalSize _lastKnownSize;

        public SimpleTerminalResizeListener(TerminalSize initialSize)
        {
            _lastKnownSize = initialSize;
        }

        /// <summary>
        /// Returns true if a resize event was received since last call.
        /// </summary>
        public bool IsTerminalResized()
        {
            if (_wasResized)
            {
                _wasResized = false;
                return true;
            }
            return false;
        }

        public TerminalSize LastKnownSize => _lastKnownSize;

        public void OnResized(Terminal terminal, TerminalSize newSize)
        {
            _wasResized = true;
            _lastKnownSize = newSize;
        }
    }
}
