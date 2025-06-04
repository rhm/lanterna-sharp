using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Lanterna.Terminal.Ansi
{
    /// <summary>
    /// Very small Telnet server accepting incoming connections and creating
    /// TelnetTerminal instances for them.
    /// </summary>
    public class TelnetTerminalServer
    {
        private readonly TcpListener _listener;
        private readonly Encoding _encoding;

        public TelnetTerminalServer(int port, Encoding encoding)
        {
            _listener = new TcpListener(IPAddress.Any, port);
            _listener.Start();
            _encoding = encoding;
        }

        public TelnetTerminal AcceptConnection()
        {
            var client = _listener.AcceptSocket();
            client.NoDelay = true;
            return new TelnetTerminal(client, _encoding);
        }

        public void Close() => _listener.Stop();
    }
}
