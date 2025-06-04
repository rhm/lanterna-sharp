namespace Lanterna.Terminal
{
    /// <summary>
    /// Modes for capturing mouse input. Behavior depends on the terminal emulator.
    /// </summary>
    public enum MouseCaptureMode
    {
        Click,
        ClickRelease,
        ClickReleaseDrag,
        ClickReleaseDragMove,
        ClickAutodetect
    }
}
