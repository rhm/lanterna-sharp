using System;
using System.IO;
using System.Text;
using System.Net.Sockets;
using Lanterna.Input;

namespace Lanterna.Terminal.Ansi
{
    /// <summary>
    /// Base class for terminals that operate on input and output streams.
    /// This is only a lightweight placeholder of the Java implementation.
    /// </summary>
    public abstract class StreamBasedTerminal : AbstractTerminal
    {
        private readonly Stream _input;
        private readonly Stream _output;
        protected readonly Encoding Encoding;

        private readonly byte[] _singleChar = new byte[1];

        protected StreamBasedTerminal(Stream input, Stream output, Encoding encoding)
        {
            _input = input;
            _output = output;
            Encoding = encoding ?? Encoding.UTF8;
        }

        protected void WriteToTerminal(byte[] bytes)
        {
            lock (_output)
            {
                _output.Write(bytes, 0, bytes.Length);
            }
        }

        private KeyStroke ByteToKeyStroke(int value)
        {
            if (value == -1)
                return null;

            char c = (char)value;
            if (c == 0x1b)
                return new KeyStroke(KeyType.Escape);
            return new KeyStroke(c);
        }

        private int ReadByteNonBlocking()
        {
            switch (_input)
            {
                case NetworkStream ns:
                    return ns.DataAvailable ? ns.ReadByte() : -1;
                case MemoryStream ms:
                    if (ms.Position < ms.Length)
                        return ms.ReadByte();
                    return -1;
                case FileStream fs:
                    if (fs.Length > fs.Position)
                        return fs.ReadByte();
                    return -1;
                default:
                    return -1;
            }
        }

        private int ReadByteBlocking()
        {
            int b = _input.ReadByte();
            return b;
        }

        protected virtual byte[] TranslateCharacter(char input)
        {
            return Encoding.GetBytes(new[] { input });
        }

        public override void PutCharacter(char c)
        {
            WriteToTerminal(TranslateCharacter(c));
        }

        public override void PutString(string text)
        {
            if (text != null)
            {
                foreach (var ch in text)
                    PutCharacter(ch);
            }
        }

        public override void Flush()
        {
            _output.Flush();
        }

        public override void Bell()
        {
            WriteToTerminal(new byte[] { 7 });
            Flush();
        }

        public override byte[] EnquireTerminal(int timeoutMilliseconds)
        {
            lock (_output)
            {
                _output.WriteByte(5); // ENQ
                _output.Flush();
            }

            var buffer = new MemoryStream();
            var start = DateTime.UtcNow;
            while ((DateTime.UtcNow - start).TotalMilliseconds < timeoutMilliseconds && ReadByteNonBlocking() is int b && b >= 0)
            {
                buffer.WriteByte((byte)b);
            }
            return buffer.ToArray();
        }

        public override KeyStroke PollInput()
        {
            int b = ReadByteNonBlocking();
            return ByteToKeyStroke(b);
        }

        public override KeyStroke ReadInput()
        {
            int b = ReadByteBlocking();
            return ByteToKeyStroke(b);
        }

        public override void Dispose()
        {
            _input?.Dispose();
            _output?.Dispose();
        }
    }
}
