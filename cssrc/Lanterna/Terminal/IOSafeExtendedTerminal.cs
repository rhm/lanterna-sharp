namespace Lanterna.Terminal
{
    /// <summary>
    /// Combination of IOSafeTerminal and ExtendedTerminal.
    /// </summary>
    public interface IOSafeExtendedTerminal : IOSafeTerminal, ExtendedTerminal
    {
        new void SetTerminalSize(int columns, int rows);
        new void SetTitle(string title);
        new void PushTitle();
        new void PopTitle();
        new void Iconify();
        new void Deiconify();
        new void Maximize();
        new void Unmaximize();
        new void SetMouseCaptureMode(MouseCaptureMode mouseCaptureMode);
        new void ScrollLines(int firstLine, int lastLine, int distance);
    }
}
