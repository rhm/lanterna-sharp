using System;
using System.IO;
using System.Text;

namespace Lanterna.Terminal.Win
{
    /// <summary>
    /// Windows console output stream that writes to the console using Win32 API
    /// </summary>
    public class WindowsConsoleOutputStream : Stream
    {
        private readonly IntPtr hConsoleOutput;
        private readonly Encoding decoderCharset;
        private readonly MemoryStream buffer = new MemoryStream();

        public WindowsConsoleOutputStream(Encoding decoderCharset)
            : this(Wincon.GetStdHandle(Wincon.STD_OUTPUT_HANDLE), decoderCharset)
        {
        }

        public WindowsConsoleOutputStream(IntPtr hConsoleOutput, Encoding decoderCharset)
        {
            this.hConsoleOutput = hConsoleOutput;
            this.decoderCharset = decoderCharset ?? Encoding.UTF8;
        }

        public IntPtr Handle => hConsoleOutput;
        public Encoding DecoderCharset => decoderCharset;

        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => buffer.Length;
        public override long Position
        {
            get => buffer.Position;
            set => buffer.Position = value;
        }

        public override void WriteByte(byte value)
        {
            lock (buffer)
            {
                buffer.WriteByte(value);
            }
        }

        public override void Write(byte[] bytes, int offset, int count)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));
            if (offset < 0 || count < 0 || offset + count > bytes.Length)
                throw new ArgumentOutOfRangeException();

            lock (buffer)
            {
                buffer.Write(bytes, offset, count);
            }
        }

        public override void Flush()
        {
            lock (buffer)
            {
                if (buffer.Length == 0)
                    return;

                // Convert buffer to string using the specified charset
                byte[] bufferBytes = buffer.ToArray();
                string characters = decoderCharset.GetString(bufferBytes);
                buffer.SetLength(0);
                buffer.Position = 0;

                // Write to console in chunks if necessary
                int offset = 0;
                while (offset < characters.Length)
                {
                    string chunk = characters.Substring(offset);
                    uint charsToWrite = (uint)chunk.Length;
                    
                    if (!Wincon.WriteConsole(hConsoleOutput, chunk, charsToWrite, out uint charsWritten, IntPtr.Zero))
                    {
                        throw new EndOfStreamException("Failed to write to console");
                    }

                    offset += (int)charsWritten;
                }
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return buffer.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            buffer.SetLength(value);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    Flush();
                }
                catch
                {
                    // Ignore flush errors during disposal
                }
                buffer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}