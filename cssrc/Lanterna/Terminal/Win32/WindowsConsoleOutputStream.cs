using System;
using System.IO;
using System.Text;

namespace Lanterna.Terminal.Win32
{
    /// <summary>
    /// Minimal stand-in for the Windows console output stream.
    /// </summary>
    public class WindowsConsoleOutputStream : Stream
    {
        private readonly Stream _stream;

        public WindowsConsoleOutputStream(Encoding encoding)
        {
            _stream = Console.OpenStandardOutput();
        }

        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => _stream.CanWrite;
        public override long Length => _stream.Length;
        public override long Position { get => _stream.Position; set => _stream.Position = value; }
        public override void Flush() => _stream.Flush();
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => _stream.Seek(offset, origin);
        public override void SetLength(long value) => _stream.SetLength(value);
        public override void Write(byte[] buffer, int offset, int count) => _stream.Write(buffer, offset, count);
    }
}
