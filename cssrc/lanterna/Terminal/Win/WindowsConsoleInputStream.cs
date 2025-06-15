using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace Lanterna.Terminal.Win
{
    /// <summary>
    /// Windows console input stream that handles keyboard, mouse, and window buffer size events
    /// </summary>
    public class WindowsConsoleInputStream : Stream
    {
        private readonly IntPtr hConsoleInput;
        private readonly Encoding encoderCharset;
        private byte[] buffer = new byte[0];
        private int bufferPosition = 0;

        // Event handlers
        public event Action<WinDef.KEY_EVENT_RECORD>? KeyEvent;
        public event Action<WinDef.MOUSE_EVENT_RECORD>? MouseEvent;
        public event Action<WinDef.WINDOW_BUFFER_SIZE_RECORD>? WindowBufferSizeEvent;

        public WindowsConsoleInputStream(Encoding encoderCharset)
            : this(Wincon.GetStdHandle(Wincon.STD_INPUT_HANDLE), encoderCharset)
        {
        }

        public WindowsConsoleInputStream(IntPtr hConsoleInput, Encoding encoderCharset)
        {
            this.hConsoleInput = hConsoleInput;
            this.encoderCharset = encoderCharset ?? Encoding.UTF8;
        }

        public IntPtr Handle => hConsoleInput;
        public Encoding EncoderCharset => encoderCharset;

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position 
        { 
            get => throw new NotSupportedException(); 
            set => throw new NotSupportedException(); 
        }

        private WinDef.INPUT_RECORD[] ReadConsoleInput()
        {
            const uint bufferSize = 64;
            var lpBuffer = new WinDef.INPUT_RECORD[bufferSize];
            
            if (Wincon.ReadConsoleInput(hConsoleInput, lpBuffer, bufferSize, out uint numberOfEventsRead))
            {
                var result = new WinDef.INPUT_RECORD[numberOfEventsRead];
                Array.Copy(lpBuffer, result, (int)numberOfEventsRead);
                return result;
            }
            
            throw new EndOfStreamException("Failed to read console input");
        }

        private uint AvailableConsoleInput()
        {
            if (Wincon.GetNumberOfConsoleInputEvents(hConsoleInput, out uint numberOfEvents))
            {
                return numberOfEvents;
            }
            return 0;
        }

        public override int ReadByte()
        {
            while (bufferPosition >= buffer.Length)
            {
                buffer = ReadKeyEvents(true);
                bufferPosition = 0;
            }

            return buffer[bufferPosition++];
        }

        public override int Read(byte[] b, int offset, int count)
        {
            if (b == null) throw new ArgumentNullException(nameof(b));
            if (offset < 0 || count < 0 || offset + count > b.Length)
                throw new ArgumentOutOfRangeException();

            while (count > 0 && bufferPosition >= buffer.Length)
            {
                buffer = ReadKeyEvents(true);
                bufferPosition = 0;
            }

            int bytesToRead = Math.Min(buffer.Length - bufferPosition, count);
            if (bytesToRead > 0)
            {
                Array.Copy(buffer, bufferPosition, b, offset, bytesToRead);
                bufferPosition += bytesToRead;
                return bytesToRead;
            }

            return 0;
        }

        private byte[] ReadKeyEvents(bool blocking)
        {
            var keyEvents = new StringBuilder();

            if (blocking || AvailableConsoleInput() > 0)
            {
                var inputRecords = ReadConsoleInput();
                foreach (var inputRecord in inputRecords)
                {
                    Filter(inputRecord, keyEvents);
                }
            }

            return encoderCharset.GetBytes(keyEvents.ToString());
        }

        private void Filter(WinDef.INPUT_RECORD input, StringBuilder keyEvents)
        {
            switch (input.EventType)
            {
                case WinDef.INPUT_RECORD.KEY_EVENT:
                    if (input.Event.KeyEvent.uChar != 0 && input.Event.KeyEvent.bKeyDown)
                    {
                        keyEvents.Append(input.Event.KeyEvent.uChar);
                    }
                    KeyEvent?.Invoke(input.Event.KeyEvent);
                    break;

                case WinDef.INPUT_RECORD.MOUSE_EVENT:
                    MouseEvent?.Invoke(input.Event.MouseEvent);
                    break;

                case WinDef.INPUT_RECORD.WINDOW_BUFFER_SIZE_EVENT:
                    WindowBufferSizeEvent?.Invoke(input.Event.WindowBufferSizeEvent);
                    break;
            }
        }

        public override void Flush()
        {
            // Input stream doesn't need flushing
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        protected override void Dispose(bool disposing)
        {
            // Console handles are managed by the system
            base.Dispose(disposing);
        }
    }
}