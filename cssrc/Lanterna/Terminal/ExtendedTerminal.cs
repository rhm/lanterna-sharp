namespace Lanterna.Terminal
{
    /// <summary>
    /// Additional optional features for terminals.
    /// </summary>
    public interface ExtendedTerminal : Terminal
    {
        void SetTerminalSize(int columns, int rows);
        void SetTitle(string title);
        void PushTitle();
        void PopTitle();
        void Iconify();
        void Deiconify();
        void Maximize();
        void Unmaximize();
        void SetMouseCaptureMode(MouseCaptureMode mouseCaptureMode);
        void ScrollLines(int firstLine, int lastLine, int distance);
    }
}
