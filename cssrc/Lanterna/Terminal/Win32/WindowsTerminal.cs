using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Lanterna.Terminal.Ansi;

namespace Lanterna.Terminal.Win32
{
    /// <summary>
    /// Basic Windows console terminal implementation using Console APIs.
    /// </summary>
    public class WindowsTerminal : UnixLikeTerminal
    {
        public WindowsTerminal()
            : this(Console.OpenStandardInput(), Console.OpenStandardOutput(), Encoding.UTF8, CtrlCBehaviour.CTRL_C_KILLS_APPLICATION)
        {
        }

        public WindowsTerminal(Stream input, Stream output, Encoding encoding, CtrlCBehaviour behaviour)
            : base(input, output, encoding, behaviour)
        {
        }

        protected override void RegisterTerminalResizeListener(Action onResize)
        {
            // .NET does not expose a resize event for the console
        }

        protected override void SaveTerminalSettings() { }
        protected override void RestoreTerminalSettings() { }
        protected override void KeyEchoEnabled(bool enabled) { Console.TreatControlCAsInput = !enabled; }
        protected override void CanonicalMode(bool enabled) { }
        protected override void KeyStrokeSignalsEnabled(bool enabled) { Console.TreatControlCAsInput = !enabled; }

        protected override TerminalSize FindTerminalSize()
        {
            return new TerminalSize(Console.WindowWidth, Console.WindowHeight);
        }
    }
}
