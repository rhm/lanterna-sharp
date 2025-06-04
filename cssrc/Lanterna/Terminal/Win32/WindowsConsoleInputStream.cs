using System;
using System.IO;
using System.Text;

namespace Lanterna.Terminal.Win32
{
    /// <summary>
    /// Minimal stand-in for the Windows console input stream.
    /// </summary>
    public class WindowsConsoleInputStream : Stream
    {
        private readonly Stream _stream;

        public WindowsConsoleInputStream(Encoding encoding)
        {
            _stream = Console.OpenStandardInput();
        }

        public override bool CanRead => _stream.CanRead;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => _stream.Length;
        public override long Position { get => _stream.Position; set => _stream.Position = value; }
        public override void Flush() => _stream.Flush();
        public override int Read(byte[] buffer, int offset, int count) => _stream.Read(buffer, offset, count);
        public override long Seek(long offset, SeekOrigin origin) => _stream.Seek(offset, origin);
        public override void SetLength(long value) => _stream.SetLength(value);
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
