/*
 * This file is part of lanterna (https://github.com/mabe02/lanterna).
 *
 * lanterna is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *
 * Copyright (C) 2010-2020 Martin Berglund
 */

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Lanterna.Terminal.Ansi
{
    /// <summary>
    /// This class implements a Telnet server, capable of accepting multiple clients and presenting each one as their own
    /// Terminal. You need to tell it at least what port to listen on and then it create a Server socket listening for
    /// incoming connections. Use AcceptConnection() to wait for the next incoming connection, it will be returned as
    /// a TelnetTerminal object that represents the client and which will be the way for the server to send content
    /// to this client. Next connecting client (through AcceptConnection() will get a different
    /// TelnetTerminal, i.e. their content will not be in sync automatically but considered as two different
    /// terminals.
    /// </summary>
    public class TelnetTerminalServer : IDisposable
    {
        private readonly Encoding _charset;
        private readonly TcpListener _serverSocket;
        private bool _disposed;

        /// <summary>
        /// Creates a new TelnetTerminalServer on a specific port
        /// </summary>
        /// <param name="port">Port to listen for incoming telnet connections</param>
        public TelnetTerminalServer(int port) : this(port, Encoding.Default)
        {
        }

        /// <summary>
        /// Creates a new TelnetTerminalServer on a specific port, using a certain character set
        /// </summary>
        /// <param name="port">Port to listen for incoming telnet connections</param>
        /// <param name="charset">Character set to use</param>
        public TelnetTerminalServer(int port, Encoding charset)
        {
            _serverSocket = new TcpListener(IPAddress.Any, port);
            _charset = charset ?? throw new ArgumentNullException(nameof(charset));
        }

        /// <summary>
        /// Creates a new TelnetTerminalServer on a specific endpoint
        /// </summary>
        /// <param name="localEndpoint">Local endpoint to bind to</param>
        /// <param name="charset">Character set to use</param>
        public TelnetTerminalServer(IPEndPoint localEndpoint, Encoding charset)
        {
            _serverSocket = new TcpListener(localEndpoint);
            _charset = charset ?? throw new ArgumentNullException(nameof(charset));
        }

        /// <summary>
        /// Returns the actual server socket used by this object. Can be used to tweak settings but be careful!
        /// </summary>
        /// <returns>Underlying TcpListener</returns>
        public TcpListener GetServerSocket()
        {
            return _serverSocket;
        }

        /// <summary>
        /// Starts listening for incoming connections
        /// </summary>
        public void Start()
        {
            _serverSocket.Start();
        }

        /// <summary>
        /// Stops listening for incoming connections
        /// </summary>
        public void Stop()
        {
            _serverSocket.Stop();
        }

        /// <summary>
        /// Waits for the next client to connect in to our server and returns a Terminal implementation, TelnetTerminal, that
        /// represents the remote terminal this client is running. The terminal can be used just like any other Terminal, but
        /// keep in mind that all operations are sent over the network.
        /// </summary>
        /// <returns>TelnetTerminal for the remote client's terminal</returns>
        public TelnetTerminal AcceptConnection()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(TelnetTerminalServer));

            var clientSocket = _serverSocket.AcceptSocket();
            clientSocket.NoDelay = true;
            return new TelnetTerminal(clientSocket, _charset);
        }

        /// <summary>
        /// Asynchronously waits for the next client to connect in to our server and returns a Terminal implementation, 
        /// TelnetTerminal, that represents the remote terminal this client is running.
        /// </summary>
        /// <returns>Task containing TelnetTerminal for the remote client's terminal</returns>
        public async System.Threading.Tasks.Task<TelnetTerminal> AcceptConnectionAsync()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(TelnetTerminalServer));

            var clientSocket = await _serverSocket.AcceptSocketAsync();
            clientSocket.NoDelay = true;
            return new TelnetTerminal(clientSocket, _charset);
        }

        /// <summary>
        /// Closes the server socket, accepting no new connection. Any call to AcceptConnection() after this will fail.
        /// </summary>
        public void Close()
        {
            Dispose();
        }

        /// <summary>
        /// Disposes the server socket and releases all resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _serverSocket?.Stop();
                _disposed = true;
            }
        }
    }
}