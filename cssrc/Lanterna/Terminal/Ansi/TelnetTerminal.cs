using System.IO;
using System.Net.Sockets;
using System.Text;

namespace Lanterna.Terminal.Ansi
{
    /// <summary>
    /// Simplified Telnet terminal implementation over a Socket. Only minimal
    /// telnet negotiation is performed and the socket streams are used directly.
    /// </summary>
    public class TelnetTerminal : ANSITerminal
    {
        private readonly Socket _socket;

        public TelnetTerminal(Socket socket, Encoding encoding)
            : base(new NetworkStream(socket, true), new NetworkStream(socket, true), encoding)
        {
            _socket = socket;
        }

        public Socket RemoteSocket => _socket;

        public override void Close()
        {
            base.Close();
            _socket?.Close();
        }
    }
}
