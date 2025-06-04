using Lanterna.Graphics;
using Lanterna.Terminal;

namespace Lanterna.Screen
{
    public abstract class AbstractScreen : Screen
    {
        private TerminalPosition? _cursorPosition;
        private ScreenBuffer _backBuffer;
        private ScreenBuffer _frontBuffer;
        private readonly TextCharacter _defaultCharacter;
        private TabBehaviour _tabBehaviour;
        private TerminalSize _terminalSize;
        private TerminalSize? _latestResizeRequest;

        protected AbstractScreen(TerminalSize initialSize)
            : this(initialSize, ScreenConstants.DEFAULT_CHARACTER)
        {
        }

        protected AbstractScreen(TerminalSize initialSize, TextCharacter defaultCharacter)
        {
            _frontBuffer = new ScreenBuffer(initialSize, defaultCharacter);
            _backBuffer = new ScreenBuffer(initialSize, defaultCharacter);
            _defaultCharacter = defaultCharacter;
            _cursorPosition = new TerminalPosition(0, 0);
            _tabBehaviour = TabBehaviour.AlignToColumn4;
            _terminalSize = initialSize;
            _latestResizeRequest = null;
        }

        public TerminalPosition? GetCursorPosition() => _cursorPosition;

        public void SetCursorPosition(TerminalPosition? position)
        {
            if (position == null)
            {
                _cursorPosition = null;
                return;
            }
            var pos = position.Value;
            if (pos.Column < 0) pos = pos.WithColumn(0);
            if (pos.Row < 0) pos = pos.WithRow(0);
            if (pos.Column >= _terminalSize.Columns) pos = pos.WithColumn(_terminalSize.Columns - 1);
            if (pos.Row >= _terminalSize.Rows) pos = pos.WithRow(_terminalSize.Rows - 1);
            _cursorPosition = pos;
        }

        public void SetTabBehaviour(TabBehaviour tabBehaviour)
        {
            _tabBehaviour = tabBehaviour;
        }

        public TabBehaviour GetTabBehaviour() => _tabBehaviour;

        public void SetCharacter(TerminalPosition position, TextCharacter screenCharacter)
        {
            SetCharacter(position.Column, position.Row, screenCharacter);
        }

        private class ImageCopyTextGraphics : ScreenTextGraphics
        {
            private readonly ScreenBuffer _buffer;
            public ImageCopyTextGraphics(AbstractScreen owner) : base(owner)
            {
                _buffer = owner._backBuffer;
            }

            public override TextGraphics DrawImage(TerminalPosition topLeft, TextImage image, TerminalPosition sourceImageTopLeft, TerminalSize sourceImageSize)
            {
                _buffer.CopyFrom(image, sourceImageTopLeft.Row, sourceImageSize.Rows, sourceImageTopLeft.Column, sourceImageSize.Columns, topLeft.Row, topLeft.Column);
                return this;
            }
        }

        public TextGraphics NewTextGraphics()
        {
            return new ImageCopyTextGraphics(this);
        }

        public void SetCharacter(int column, int row, TextCharacter screenCharacter)
        {
            if (screenCharacter.Is('\t'))
            {
                screenCharacter = screenCharacter.WithCharacter(' ');
                for (int i = 0; i < _tabBehaviour.ReplaceTabs("\t", column).Length; i++)
                {
                    _backBuffer.SetCharacterAt(column + i, row, screenCharacter);
                }
            }
            else
            {
                _backBuffer.SetCharacterAt(column, row, screenCharacter);
            }
        }

        public TextCharacter GetFrontCharacter(TerminalPosition position) => GetFrontCharacter(position.Column, position.Row);
        public TextCharacter GetFrontCharacter(int column, int row) => GetCharacterFromBuffer(_frontBuffer, column, row);
        public TextCharacter GetBackCharacter(TerminalPosition position) => GetBackCharacter(position.Column, position.Row);
        public TextCharacter GetBackCharacter(int column, int row) => GetCharacterFromBuffer(_backBuffer, column, row);

        public void Refresh() => Refresh(Screen.RefreshType.Automatic);
        public abstract void Refresh(Screen.RefreshType refreshType);
        public abstract void StartScreen();
        public abstract void StopScreen();
        public virtual void Dispose() => StopScreen();

        public void Clear()
        {
            _backBuffer.SetAll(_defaultCharacter);
        }

        public TerminalSize? DoResizeIfNecessary()
        {
            var pendingResize = GetAndClearPendingResize();
            if (pendingResize == null)
                return null;
            _backBuffer = _backBuffer.Resize(pendingResize.Value, _defaultCharacter);
            _frontBuffer = _frontBuffer.Resize(pendingResize.Value, _defaultCharacter);
            return pendingResize;
        }

        public TerminalSize GetTerminalSize() => _terminalSize;

        protected ScreenBuffer GetFrontBuffer() => _frontBuffer;
        protected ScreenBuffer GetBackBuffer() => _backBuffer;

        private TerminalSize? GetAndClearPendingResize()
        {
            if (_latestResizeRequest != null)
            {
                _terminalSize = _latestResizeRequest.Value;
                _latestResizeRequest = null;
                return _terminalSize;
            }
            return null;
        }

        protected void AddResizeRequest(TerminalSize newSize)
        {
            _latestResizeRequest = newSize;
        }

        private TextCharacter GetCharacterFromBuffer(ScreenBuffer buffer, int column, int row)
        {
            return buffer.GetCharacterAt(column, row);
        }

        public override string ToString() => GetBackBuffer().ToString();

        public void ScrollLines(int firstLine, int lastLine, int distance)
        {
            GetBackBuffer().ScrollLines(firstLine, lastLine, distance);
        }
    }
}
