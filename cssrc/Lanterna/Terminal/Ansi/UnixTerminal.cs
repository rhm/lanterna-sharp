using System.IO;
using System.Text;

namespace Lanterna.Terminal.Ansi
{
    /// <summary>
    /// Simplified Unix terminal implementation.
    /// </summary>
    public class UnixTerminal : UnixLikeTTYTerminal
    {
        public UnixTerminal() : this(Stream.Null, Stream.Null, Encoding.UTF8) { }

        public UnixTerminal(Stream input, Stream output, Encoding encoding)
            : base("/dev/tty", input, output, encoding, CtrlCBehaviour.CTRL_C_KILLS_APPLICATION)
        {
        }
    }
}
