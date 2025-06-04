using System.IO;
using System.Text;
using Lanterna.Input;

namespace Lanterna.Terminal.Ansi
{
    /// <summary>
    /// Base for terminals behaving like Unix consoles.
    /// </summary>
    public abstract class UnixLikeTerminal : ANSITerminal
    {
        public enum CtrlCBehaviour
        {
            TRAP,
            CTRL_C_KILLS_APPLICATION
        }

        protected UnixLikeTerminal(Stream input, Stream output, Encoding encoding, CtrlCBehaviour behaviour)
            : base(input, output, encoding)
        {
        }

        protected abstract void RegisterTerminalResizeListener(System.Action onResize);
        protected abstract void SaveTerminalSettings();
        protected abstract void RestoreTerminalSettings();
        protected abstract void KeyEchoEnabled(bool enabled);
        protected abstract void CanonicalMode(bool enabled);
        protected abstract void KeyStrokeSignalsEnabled(bool enabled);
        public virtual KeyStroke IsCtrlC(KeyStroke key) => key;
    }
}
