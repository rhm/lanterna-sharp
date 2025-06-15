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
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using Lanterna.Core;
using static Lanterna.Terminal.Ansi.TelnetProtocol;

namespace Lanterna.Terminal.Ansi
{
    /// <summary>
    /// This class is used by the TelnetTerminalServer class when a client has connected in; this class will be the
    /// interaction point for that client. All operations are sent to the client over the network socket and some of the
    /// meta-operations (like echo mode) are communicated using Telnet negotiation language. You can't create objects of this
    /// class directly; they are created for you when you are listening for incoming connections using a
    /// TelnetTerminalServer and a client connects.
    /// </summary>
    public class TelnetTerminal : ANSITerminal
    {
        private readonly Socket _socket;
        private readonly NegotiationState _negotiationState;

        internal TelnetTerminal(Socket socket, Encoding terminalCharset) 
            : this(socket, new TelnetClientIACFilterer(socket), new NetworkStream(socket), terminalCharset)
        {
        }

        private TelnetTerminal(Socket socket, TelnetClientIACFilterer inputStream, Stream outputStream, Encoding terminalCharset) 
            : base(inputStream, outputStream, terminalCharset)
        {
            _socket = socket;
            _negotiationState = inputStream.NegotiationState;
            inputStream.SetEventListener(new TelnetClientEventListener(this));
            SetLineMode0();
            SetEchoOff();
            SetResizeNotificationOn();
        }

        /// <summary>
        /// Returns the socket address for the remote endpoint of the telnet connection
        /// </summary>
        /// <returns>EndPoint representing the remote client</returns>
        public System.Net.EndPoint? GetRemoteSocketAddress()
        {
            return _socket.RemoteEndPoint;
        }

        private void SetEchoOff()
        {
            WriteToTerminal(new byte[] { COMMAND_IAC, COMMAND_WILL, OPTION_ECHO });
            Flush();
        }

        private void SetLineMode0()
        {
            WriteToTerminal(new byte[] 
            {
                COMMAND_IAC, COMMAND_DO, OPTION_LINEMODE,
                COMMAND_IAC, COMMAND_SUBNEGOTIATION, OPTION_LINEMODE, 1, 0, COMMAND_IAC, COMMAND_SUBNEGOTIATION_END
            });
            Flush();
        }

        private void SetResizeNotificationOn()
        {
            WriteToTerminal(new byte[] { COMMAND_IAC, COMMAND_DO, OPTION_NAWS });
            Flush();
        }

        /// <summary>
        /// Retrieves the current negotiation state with the client, containing details on what options have been enabled
        /// and what the client has said it supports.
        /// </summary>
        /// <returns>The current negotiation state for this client</returns>
        public NegotiationState GetNegotiationState()
        {
            return _negotiationState;
        }

        public override TerminalSize GetTerminalSize()
        {
            // For telnet terminal, we should track the size from NAWS negotiation
            // For now, return a default size
            return new TerminalSize(80, 24);
        }

        /// <summary>
        /// Closes the socket to the client, effectively ending the telnet session and the terminal.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                base.Dispose(disposing);
                _socket?.Close();
            }
        }

        internal new void OnResized(int columns, int rows)
        {
            base.OnResized(columns, rows);
        }

        internal void RequestReply(bool will, byte option)
        {
            WriteToTerminal(new byte[] { COMMAND_IAC, will ? COMMAND_WILL : COMMAND_WONT, option });
        }

        /// <summary>
        /// This class contains some of the various states that the Telnet negotiation protocol defines. Lanterna doesn't
        /// support all of them but the more common ones are represented.
        /// </summary>
        public class NegotiationState
        {
            private bool _clientEcho;
            private bool _clientLineMode0;
            private bool _clientResizeNotification;
            private bool _suppressGoAhead;
            private bool _extendedAscii;

            internal NegotiationState()
            {
                _clientEcho = true;
                _clientLineMode0 = false;
                _clientResizeNotification = false;
                _suppressGoAhead = true;
                _extendedAscii = true;
            }

            /// <summary>
            /// Is the telnet client echo mode turned on (client is echoing characters locally)
            /// </summary>
            /// <returns>true if client echo is enabled</returns>
            public bool IsClientEcho() => _clientEcho;

            /// <summary>
            /// Is the telnet client line mode 0 turned on (client sends character by character instead of line by line)
            /// </summary>
            /// <returns>true if client line mode 0 is enabled</returns>
            public bool IsClientLineMode0() => _clientLineMode0;

            /// <summary>
            /// Is the telnet client resize notification turned on (client notifies server when the terminal window has
            /// changed size)
            /// </summary>
            /// <returns>true if client resize notification is enabled</returns>
            public bool IsClientResizeNotification() => _clientResizeNotification;

            /// <summary>
            /// Is the telnet client suppress go-ahead turned on
            /// </summary>
            /// <returns>true if client suppress go-ahead is enabled</returns>
            public bool IsSuppressGoAhead() => _suppressGoAhead;

            /// <summary>
            /// Is the telnet client extended ascii turned on
            /// </summary>
            /// <returns>true if client extended ascii is enabled</returns>
            public bool IsExtendedAscii() => _extendedAscii;

            internal void SetClientEcho(bool value) => _clientEcho = value;
            internal void SetClientLineMode0(bool value) => _clientLineMode0 = value;
            internal void SetClientResizeNotification(bool value) => _clientResizeNotification = value;
            internal void SetSuppressGoAhead(bool value) => _suppressGoAhead = value;
            internal void SetExtendedAscii(bool value) => _extendedAscii = value;

            internal void OnUnsupportedStateCommand(bool enabling, byte value)
            {
                Console.Error.WriteLine($"Unsupported operation: Client says it {(enabling ? "will" : "won't")} do {CODE_TO_NAME.GetValueOrDefault(value, value.ToString())}");
            }

            internal void OnUnsupportedRequestCommand(bool askedToDo, byte value)
            {
                Console.Error.WriteLine($"Unsupported request: Client asks us, {(askedToDo ? "do" : "don't")} {CODE_TO_NAME.GetValueOrDefault(value, value.ToString())}");
            }

            internal void OnUnsupportedSubnegotiation(byte option, byte[] additionalData)
            {
                var dataStr = string.Join(" ", additionalData.Select(b => $"{b:X2}"));
                Console.Error.WriteLine($"Unsupported subnegotiation: Client send {CODE_TO_NAME.GetValueOrDefault(option, option.ToString())} with extra data [{dataStr}]");
            }
        }

        private class TelnetClientEventListener
        {
            private readonly TelnetTerminal _terminal;

            public TelnetClientEventListener(TelnetTerminal terminal)
            {
                _terminal = terminal;
            }

            public void OnResize(int columns, int rows)
            {
                _terminal.OnResized(columns, rows);
            }

            public void RequestReply(bool will, byte option)
            {
                _terminal.RequestReply(will, option);
            }
        }

        private class TelnetClientIACFilterer : Stream
        {
            private readonly NegotiationState _negotiationState;
            private readonly Stream _inputStream;
            private readonly byte[] _buffer;
            private readonly byte[] _workingBuffer;
            private int _bytesInBuffer;
            private TelnetClientEventListener? _eventListener;
            private readonly Socket _socket;

            public NegotiationState NegotiationState => _negotiationState;

            internal TelnetClientIACFilterer(Socket socket)
            {
                _negotiationState = new NegotiationState();
                _inputStream = new NetworkStream(socket);
                _buffer = new byte[64 * 1024];
                _workingBuffer = new byte[1024];
                _bytesInBuffer = 0;
                _socket = socket;
            }

            internal void SetEventListener(TelnetClientEventListener eventListener)
            {
                _eventListener = eventListener;
            }

            public override bool CanRead => true;
            public override bool CanSeek => false;
            public override bool CanWrite => false;
            public override long Length => throw new NotSupportedException();
            public override long Position 
            { 
                get => throw new NotSupportedException(); 
                set => throw new NotSupportedException(); 
            }

            public override int ReadByte()
            {
                throw new NotSupportedException("TelnetClientIACFilterer doesn't support ReadByte()");
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    _inputStream?.Dispose();
                }
                base.Dispose(disposing);
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                if (_bytesInBuffer == -1)
                    return -1;

                if (Available() == 0)
                {
                    FillBuffer(true);
                }

                if (_bytesInBuffer <= 0)
                    return -1;

                int bytesToCopy = Math.Min(count, _bytesInBuffer);
                Array.Copy(_buffer, 0, buffer, offset, bytesToCopy);
                Array.Copy(_buffer, bytesToCopy, _buffer, 0, _buffer.Length - bytesToCopy);
                _bytesInBuffer -= bytesToCopy;
                return bytesToCopy;
            }

            private int Available()
            {
                if (_bytesInBuffer > 0)
                    return _bytesInBuffer;
                
                FillBuffer(false);
                return Math.Abs(_bytesInBuffer);
            }

            private void FillBuffer(bool block)
            {
                int maxFill = Math.Min(_workingBuffer.Length, _buffer.Length - _bytesInBuffer);

                int oldTimeout = _socket.ReceiveTimeout;
                if (!block) 
                    _socket.ReceiveTimeout = 1;

                int readBytes;
                try
                {
                    readBytes = _inputStream.Read(_workingBuffer, 0, maxFill);
                }
                catch (IOException)
                {
                    readBytes = 0;
                }

                if (!block) 
                    _socket.ReceiveTimeout = oldTimeout;

                if (readBytes == -1)
                {
                    _bytesInBuffer = -1;
                    return;
                }

                for (int i = 0; i < readBytes; i++)
                {
                    if (_workingBuffer[i] == COMMAND_IAC)
                    {
                        i++;
                        var commands = new[] { COMMAND_DO, COMMAND_DONT, COMMAND_WILL, COMMAND_WONT };
                        if (commands.Contains(_workingBuffer[i]))
                        {
                            ParseCommand(_workingBuffer, i, readBytes);
                            i++;
                            continue;
                        }
                        else if (_workingBuffer[i] == COMMAND_SUBNEGOTIATION)
                        {
                            i += ParseSubNegotiation(_workingBuffer, ++i, readBytes);
                            continue;
                        }
                        else if (_workingBuffer[i] != COMMAND_IAC)
                        {
                            Console.Error.WriteLine($"Unknown Telnet command: {_workingBuffer[i]}");
                        }
                    }
                    _buffer[_bytesInBuffer++] = _workingBuffer[i];
                }
            }

            private void ParseCommand(byte[] buffer, int position, int max)
            {
                if (position + 1 >= max)
                {
                    throw new InvalidOperationException("State error, we got a command signal from the remote telnet client but not enough characters available in the stream");
                }

                byte command = buffer[position];
                byte value = buffer[position + 1];

                switch (command)
                {
                    case COMMAND_DO:
                    case COMMAND_DONT:
                        if (value == OPTION_SUPPRESS_GO_AHEAD)
                        {
                            _negotiationState.SetSuppressGoAhead(command == COMMAND_DO);
                            _eventListener?.RequestReply(command == COMMAND_DO, value);
                        }
                        else if (value == OPTION_EXTEND_ASCII)
                        {
                            _negotiationState.SetExtendedAscii(command == COMMAND_DO);
                            _eventListener?.RequestReply(command == COMMAND_DO, value);
                        }
                        else
                        {
                            _negotiationState.OnUnsupportedRequestCommand(command == COMMAND_DO, value);
                        }
                        break;

                    case COMMAND_WILL:
                    case COMMAND_WONT:
                        if (value == OPTION_ECHO)
                        {
                            _negotiationState.SetClientEcho(command == COMMAND_WILL);
                        }
                        else if (value == OPTION_LINEMODE)
                        {
                            _negotiationState.SetClientLineMode0(command == COMMAND_WILL);
                        }
                        else if (value == OPTION_NAWS)
                        {
                            _negotiationState.SetClientResizeNotification(command == COMMAND_WILL);
                        }
                        else
                        {
                            _negotiationState.OnUnsupportedStateCommand(command == COMMAND_WILL, value);
                        }
                        break;

                    default:
                        throw new NotSupportedException($"No command handler implemented for {CODE_TO_NAME.GetValueOrDefault(command, command.ToString())}");
                }
            }

            private int ParseSubNegotiation(byte[] buffer, int position, int max)
            {
                int originalPosition = position;
                byte operation = buffer[position++];

                using var outputBuffer = new MemoryStream();
                while (position < max)
                {
                    byte read = buffer[position];
                    if (read != COMMAND_IAC)
                    {
                        outputBuffer.WriteByte(read);
                    }
                    else
                    {
                        if (position + 1 == max)
                        {
                            throw new InvalidOperationException("State error, unexpected end of buffer when reading subnegotiation");
                        }
                        position++;
                        if (buffer[position] == COMMAND_IAC)
                        {
                            outputBuffer.WriteByte(COMMAND_IAC);
                        }
                        else if (buffer[position] == COMMAND_SUBNEGOTIATION_END)
                        {
                            ParseSubNegotiation(operation, outputBuffer.ToArray());
                            return ++position - originalPosition;
                        }
                    }
                    position++;
                }
                throw new InvalidOperationException("State error, unexpected end of buffer when reading subnegotiation, no IAC SE");
            }

            private void ParseSubNegotiation(byte option, byte[] additionalData)
            {
                switch (option)
                {
                    case OPTION_NAWS:
                        if (additionalData.Length >= 4)
                        {
                            _eventListener?.OnResize(
                                ConvertTwoBytesToInt(additionalData[1], additionalData[0]),
                                ConvertTwoBytesToInt(additionalData[3], additionalData[2]));
                        }
                        break;
                    case OPTION_LINEMODE:
                        // We don't parse this, as this is a very complicated command
                        break;
                    default:
                        _negotiationState.OnUnsupportedSubnegotiation(option, additionalData);
                        break;
                }
            }

            public override void Flush() => _inputStream.Flush();
            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
            public override void SetLength(long value) => throw new NotSupportedException();
            public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        }

        private static int ConvertTwoBytesToInt(byte b1, byte b2)
        {
            return ((b2 & 0xFF) << 8) | (b1 & 0xFF);
        }
    }
}