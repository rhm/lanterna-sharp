using System;
using System.IO;
using System.Text;

namespace Lanterna.Terminal.Ansi
{
    /// <summary>
    /// Partial port of the Java ANSITerminal class. Implements common ANSI
    /// escape sequence handling for basic terminals.
    /// </summary>
    public abstract class ANSITerminal : StreamBasedTerminal, ExtendedTerminal
    {
        private MouseCaptureMode? _requestedMouseCaptureMode;
        private MouseCaptureMode? _mouseCaptureMode;
        private bool _inPrivateMode;

        protected ANSITerminal(Stream input, Stream output, Encoding encoding)
            : base(input, output, encoding)
        {
        }

        private void WriteCSI(params byte[] tail) =>
            WriteToTerminal(Prepend(new byte[] { 0x1b, (byte)'[' }, tail));

        private void WriteOSC(params byte[] tail) =>
            WriteToTerminal(Prepend(new byte[] { 0x1b, (byte)']' }, tail));

        private static byte[] Prepend(byte[] head, byte[] tail)
        {
            var result = new byte[head.Length + tail.Length];
            Buffer.BlockCopy(head, 0, result, 0, head.Length);
            Buffer.BlockCopy(tail, 0, result, head.Length, tail.Length);
            return result;
        }

        protected virtual TerminalSize FindTerminalSize()
        {
            SaveCursorPosition();
            SetCursorPosition(5000, 5000);
            ResetMemorizedCursorPosition();
            ReportPosition();
            RestoreCursorPosition();
            var pos = WaitForCursorPositionReport();
            if (pos.Column == 0 && pos.Row == 0)
                return new TerminalSize(80, 24);
            return new TerminalSize(pos.Column, pos.Row);
        }

        public override TerminalSize GetTerminalSize() => FindTerminalSize();

        public virtual void SetTerminalSize(int columns, int rows)
        {
            WriteCSI(Encoding.GetBytes($"8;{rows};{columns}t"));
        }

        public virtual void SetTitle(string title)
        {
            if (title == null) return;
            title = title.Replace("\u0007", string.Empty);
            WriteOSC(Encoding.GetBytes($"2;{title}\u0007"));
        }

        public virtual void PushTitle() { }
        public virtual void PopTitle() { }

        public virtual void Iconify() => WriteCSI((byte)'2', (byte)'t');
        public virtual void Deiconify() => WriteCSI((byte)'1', (byte)'t');
        public virtual void Maximize() => WriteCSI((byte)'9', (byte)';', (byte)'1', (byte)'t');
        public virtual void Unmaximize() => WriteCSI((byte)'9', (byte)';', (byte)'0', (byte)'t');

        public virtual void SetMouseCaptureMode(MouseCaptureMode mouseCaptureMode)
        {
            _requestedMouseCaptureMode = mouseCaptureMode;
        }

        public virtual void ScrollLines(int firstLine, int lastLine, int distance)
        {
            if (distance == 0 || lastLine < firstLine)
                return;

            if (firstLine < 0) firstLine = 0;

            var sb = new StringBuilder();
            sb.Append("\x1b[").Append(firstLine + 1)
              .Append(';').Append(lastLine + 1).Append('r');

            int target = distance > 0 ? lastLine : firstLine;
            sb.Append("\x1b[").Append(target + 1).Append(";1H");

            if (distance > 0)
            {
                int num = Math.Min(distance, lastLine - firstLine + 1);
                for (int i = 0; i < num; i++) sb.Append('\n');
            }
            else
            {
                int num = Math.Min(-distance, lastLine - firstLine + 1);
                for (int i = 0; i < num; i++) sb.Append("\x1bM");
            }

            sb.Append("\x1b[r");
            WriteToTerminal(Encoding.GetBytes(sb.ToString()));
        }

        public override void SetForegroundColor(ITextColor color) =>
            WriteCSI(color.GetForegroundSGRSequence());

        public override void SetBackgroundColor(ITextColor color) =>
            WriteCSI(color.GetBackgroundSGRSequence());

        public override void EnableSGR(SGR sgr)
        {
            switch (sgr)
            {
                case SGR.Bold: WriteCSI((byte)'1', (byte)'m'); break;
                case SGR.Reverse: WriteCSI((byte)'7', (byte)'m'); break;
                case SGR.Underline: WriteCSI((byte)'4', (byte)'m'); break;
                case SGR.Blink: WriteCSI((byte)'5', (byte)'m'); break;
                case SGR.Italic: WriteCSI((byte)'3', (byte)'m'); break;
                case SGR.Bordered: WriteCSI((byte)'5', (byte)'1', (byte)'m'); break;
                case SGR.Fraktur: WriteCSI((byte)'2', (byte)'0', (byte)'m'); break;
                case SGR.CrossedOut: WriteCSI((byte)'9', (byte)'m'); break;
                case SGR.Circled: WriteCSI((byte)'5', (byte)'2', (byte)'m'); break;
            }
        }

        public override void DisableSGR(SGR sgr)
        {
            switch (sgr)
            {
                case SGR.Bold: WriteCSI((byte)'2', (byte)'2', (byte)'m'); break;
                case SGR.Reverse: WriteCSI((byte)'2', (byte)'7', (byte)'m'); break;
                case SGR.Underline: WriteCSI((byte)'2', (byte)'4', (byte)'m'); break;
                case SGR.Blink: WriteCSI((byte)'2', (byte)'5', (byte)'m'); break;
                case SGR.Italic: WriteCSI((byte)'2', (byte)'3', (byte)'m'); break;
                case SGR.Bordered: WriteCSI((byte)'5', (byte)'4', (byte)'m'); break;
                case SGR.Fraktur: WriteCSI((byte)'2', (byte)'3', (byte)'m'); break;
                case SGR.CrossedOut: WriteCSI((byte)'2', (byte)'9', (byte)'m'); break;
                case SGR.Circled: WriteCSI((byte)'5', (byte)'4', (byte)'m'); break;
            }
        }

        public override void ResetColorAndSGR() => WriteCSI((byte)'0', (byte)'m');
        public override void ClearScreen() => WriteCSI((byte)'2', (byte)'J');

        public override void EnterPrivateMode()
        {
            if (_inPrivateMode)
                throw new InvalidOperationException("Already in private mode");
            WriteCSI((byte)'?', (byte)'1', (byte)'0', (byte)'4', (byte)'9', (byte)'h');
            _inPrivateMode = true;
        }

        public override void ExitPrivateMode()
        {
            if (!_inPrivateMode)
                throw new InvalidOperationException("Not in private mode");
            ResetColorAndSGR();
            SetCursorVisible(true);
            WriteCSI((byte)'?', (byte)'1', (byte)'0', (byte)'4', (byte)'9', (byte)'l');
            _inPrivateMode = false;
        }

        public override void Close()
        {
            if (_inPrivateMode)
                ExitPrivateMode();
            base.Dispose();
        }

        public override void SetCursorPosition(int x, int y) =>
            WriteCSI(Encoding.GetBytes($"{y + 1};{x + 1}H"));

        public override void SetCursorPosition(TerminalPosition position) =>
            SetCursorPosition(position.Column, position.Row);

        private TerminalPosition WaitForCursorPositionReport()
        {
            var buffer = new StringBuilder();
            while (true)
            {
                var ks = ReadInput();
                if (ks == null) continue;
                buffer.Append(ks.Character);
                if (ks.Character == 'R') break;
            }
            string result = buffer.ToString();
            // format ESC [ row ; column R
            int start = result.IndexOf('[');
            int sep = result.IndexOf(';');
            int end = result.IndexOf('R');
            if (start >= 0 && sep > start && end > sep)
            {
                if (int.TryParse(result.Substring(start + 1, sep - start - 1), out int row) &&
                    int.TryParse(result.Substring(sep + 1, end - sep - 1), out int col))
                {
                    return new TerminalPosition(col, row);
                }
            }
            return new TerminalPosition(0, 0);
        }

        public override TerminalPosition GetCursorPosition()
        {
            ResetMemorizedCursorPosition();
            ReportPosition();
            var pos = WaitForCursorPositionReport();
            return new TerminalPosition(pos.Column - 1, pos.Row - 1);
        }

        public override void SetCursorVisible(bool visible) =>
            WriteCSI(Encoding.GetBytes($"?25{(visible ? 'h' : 'l')}"));

        protected void ReportPosition() => WriteCSI(Encoding.GetBytes("6n"));
        protected void RestoreCursorPosition() => WriteCSI(Encoding.GetBytes("u"));
        protected void SaveCursorPosition() => WriteCSI(Encoding.GetBytes("s"));
    }
}
