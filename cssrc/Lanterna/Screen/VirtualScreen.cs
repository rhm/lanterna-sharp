using System;
using Lanterna.Input;
using Lanterna.Terminal;
using Lanterna.Graphics;

namespace Lanterna.Screen
{
    public class VirtualScreen : AbstractScreen
    {
        private readonly Screen _realScreen;
        private readonly FrameRenderer _frameRenderer;
        private TerminalSize _minimumSize;
        private TerminalPosition _viewportTopLeft;
        private TerminalSize _viewportSize;
        private bool _scrollWithCtrl;

        public VirtualScreen(Screen screen) : base(screen.GetTerminalSize())
        {
            _frameRenderer = new DefaultFrameRenderer();
            _realScreen = screen;
            _minimumSize = screen.GetTerminalSize();
            _viewportTopLeft = TerminalPosition.TopLeftCorner;
            _viewportSize = _minimumSize;
            _scrollWithCtrl = false;
        }

        public void SetMinimumSize(TerminalSize minimumSize)
        {
            _minimumSize = minimumSize;
            TerminalSize virtualSize = minimumSize.Max(_realScreen.GetTerminalSize());
            if (!minimumSize.Equals(virtualSize))
            {
                AddResizeRequest(virtualSize);
                base.DoResizeIfNecessary();
            }
            CalculateViewport(_realScreen.GetTerminalSize());
        }

        public TerminalSize GetMinimumSize() => _minimumSize;

        public TerminalSize GetViewportSize() => _viewportSize;

        public void SetScrollOnCTRL(bool scrollOnCTRL) => _scrollWithCtrl = scrollOnCTRL;

        public void SetViewportTopLeft(TerminalPosition position)
        {
            _viewportTopLeft = position;
            while (_viewportTopLeft.Column > 0 && _viewportTopLeft.Column + _viewportSize.Columns > _minimumSize.Columns)
                _viewportTopLeft = _viewportTopLeft.WithColumn(_viewportTopLeft.Column - 1);
            while (_viewportTopLeft.Row > 0 && _viewportTopLeft.Row + _viewportSize.Rows > _minimumSize.Rows)
                _viewportTopLeft = _viewportTopLeft.WithRow(_viewportTopLeft.Row - 1);
        }

        public override void StartScreen() => _realScreen.StartScreen();
        public override void StopScreen() => _realScreen.StopScreen();

        public override TextCharacter GetFrontCharacter(TerminalPosition position) => null;

        public override void SetCursorPosition(TerminalPosition? position)
        {
            base.SetCursorPosition(position);
            if (position == null)
            {
                _realScreen.SetCursorPosition(null);
                return;
            }
            var pos = position.Value.WithColumn(position.Value.Column - _viewportTopLeft.Column).WithRow(position.Value.Row - _viewportTopLeft.Row);
            if (pos.Column >= 0 && pos.Column < _viewportSize.Columns && pos.Row >= 0 && pos.Row < _viewportSize.Rows)
                _realScreen.SetCursorPosition(pos);
            else
                _realScreen.SetCursorPosition(null);
        }

        public override TerminalSize? DoResizeIfNecessary()
        {
            var underlyingSize = _realScreen.DoResizeIfNecessary();
            if (underlyingSize == null)
                return null;
            var newVirtualSize = CalculateViewport(underlyingSize.Value);
            if (!GetTerminalSize().Equals(newVirtualSize))
            {
                AddResizeRequest(newVirtualSize);
                return base.DoResizeIfNecessary();
            }
            return newVirtualSize;
        }

        private TerminalSize CalculateViewport(TerminalSize realTerminalSize)
        {
            TerminalSize newVirtualSize = _minimumSize.Max(realTerminalSize);
            if (newVirtualSize.Equals(realTerminalSize))
            {
                _viewportSize = realTerminalSize;
                _viewportTopLeft = TerminalPosition.TopLeftCorner;
            }
            else
            {
                TerminalSize newViewportSize = _frameRenderer.GetViewportSize(realTerminalSize, newVirtualSize);
                if (newViewportSize.Rows > _viewportSize.Rows)
                    _viewportTopLeft = _viewportTopLeft.WithRow(Math.Max(0, _viewportTopLeft.Row - (newViewportSize.Rows - _viewportSize.Rows)));
                if (newViewportSize.Columns > _viewportSize.Columns)
                    _viewportTopLeft = _viewportTopLeft.WithColumn(Math.Max(0, _viewportTopLeft.Column - (newViewportSize.Columns - _viewportSize.Columns)));
                _viewportSize = newViewportSize;
            }
            return newVirtualSize;
        }

        public override void Refresh(Screen.RefreshType refreshType)
        {
            SetCursorPosition(GetCursorPosition());
            if (!_viewportSize.Equals(_realScreen.GetTerminalSize()))
            {
                _frameRenderer.DrawFrame(
                    _realScreen.NewTextGraphics(),
                    _realScreen.GetTerminalSize(),
                    GetTerminalSize(),
                    _viewportTopLeft);
            }

            TerminalPosition viewportOffset = _frameRenderer.GetViewportOffset();
            if (_realScreen is AbstractScreen asAbstract)
            {
                GetBackBuffer().CopyTo(
                    asAbstract.GetBackBuffer(),
                    _viewportTopLeft.Row,
                    _viewportSize.Rows,
                    _viewportTopLeft.Column,
                    _viewportSize.Columns,
                    viewportOffset.Row,
                    viewportOffset.Column);
            }
            else
            {
                for (int y = 0; y < _viewportSize.Rows; y++)
                {
                    for (int x = 0; x < _viewportSize.Columns; x++)
                    {
                        _realScreen.SetCharacter(
                            x + viewportOffset.Column,
                            y + viewportOffset.Row,
                            GetBackBuffer().GetCharacterAt(x + _viewportTopLeft.Column, y + _viewportTopLeft.Row));
                    }
                }
            }
            _realScreen.Refresh(refreshType);
        }

        public override KeyStroke PollInput() => Filter(_realScreen.PollInput());
        public override KeyStroke ReadInput() => Filter(_realScreen.ReadInput());

        private KeyStroke Filter(KeyStroke keyStroke)
        {
            if (keyStroke == null) return null;
            if (IsScrollTrigger(keyStroke) && keyStroke.KeyType == KeyType.ArrowLeft)
            {
                if (_viewportTopLeft.Column > 0)
                {
                    _viewportTopLeft = _viewportTopLeft.WithColumn(_viewportTopLeft.Column - 1);
                    Refresh();
                    return null;
                }
            }
            else if (IsScrollTrigger(keyStroke) && keyStroke.KeyType == KeyType.ArrowRight)
            {
                if (_viewportTopLeft.Column + _viewportSize.Columns < GetTerminalSize().Columns)
                {
                    _viewportTopLeft = _viewportTopLeft.WithColumn(_viewportTopLeft.Column + 1);
                    Refresh();
                    return null;
                }
            }
            else if (IsScrollTrigger(keyStroke) && keyStroke.KeyType == KeyType.ArrowUp)
            {
                if (_viewportTopLeft.Row > 0)
                {
                    _viewportTopLeft = _viewportTopLeft.WithRow(_viewportTopLeft.Row - 1);
                    _realScreen.ScrollLines(0, _viewportSize.Rows - 1, -1);
                    Refresh();
                    return null;
                }
            }
            else if (IsScrollTrigger(keyStroke) && keyStroke.KeyType == KeyType.ArrowDown)
            {
                if (_viewportTopLeft.Row + _viewportSize.Rows < GetTerminalSize().Rows)
                {
                    _viewportTopLeft = _viewportTopLeft.WithRow(_viewportTopLeft.Row + 1);
                    _realScreen.ScrollLines(0, _viewportSize.Rows - 1, 1);
                    Refresh();
                    return null;
                }
            }
            else if (IsScrollTrigger(keyStroke) && keyStroke.KeyType == KeyType.PageUp)
            {
                if (_viewportTopLeft.Row > 0)
                {
                    int scroll = Math.Min(_viewportSize.Rows, _viewportTopLeft.Row);
                    _viewportTopLeft = _viewportTopLeft.WithRow(_viewportTopLeft.Row - scroll);
                    _realScreen.ScrollLines(0, _viewportSize.Rows - scroll, -scroll);
                    Refresh();
                    return null;
                }
            }
            else if (IsScrollTrigger(keyStroke) && (keyStroke.KeyType == KeyType.PageDown || IsSpaceBarPress(keyStroke)))
            {
                if (_viewportTopLeft.Row + _viewportSize.Rows < GetTerminalSize().Rows)
                {
                    int scroll = _viewportSize.Rows;
                    if (_viewportTopLeft.Row + _viewportSize.Rows + scroll >= GetTerminalSize().Rows)
                        scroll = GetTerminalSize().Rows - _viewportTopLeft.Row - _viewportSize.Rows;
                    _viewportTopLeft = _viewportTopLeft.WithRow(_viewportTopLeft.Row + scroll);
                    _realScreen.ScrollLines(0, _viewportSize.Rows - scroll, scroll);
                    Refresh();
                    return null;
                }
            }
            return keyStroke;
        }

        private static bool IsSpaceBarPress(KeyStroke keyStroke) => keyStroke.KeyType == KeyType.Character && keyStroke.Character == ' ';
        private bool IsScrollTrigger(KeyStroke keyStroke) => _scrollWithCtrl ? keyStroke.CtrlDown : keyStroke.AltDown;

        public override void ScrollLines(int firstLine, int lastLine, int distance)
        {
            base.ScrollLines(firstLine, lastLine, distance);
            int vpFirst = _viewportTopLeft.Row;
            int vpRows = _viewportSize.Rows;
            firstLine = Math.Max(0, firstLine - vpFirst);
            lastLine = Math.Min(vpRows - 1, lastLine - vpFirst);
            if (firstLine <= lastLine)
                _realScreen.ScrollLines(firstLine, lastLine, distance);
        }

        public interface FrameRenderer
        {
            TerminalSize GetViewportSize(TerminalSize realSize, TerminalSize virtualSize);
            TerminalPosition GetViewportOffset();
            void DrawFrame(TextGraphics graphics, TerminalSize realSize, TerminalSize virtualSize, TerminalPosition virtualScrollPosition);
        }

        private class DefaultFrameRenderer : FrameRenderer
        {
            public TerminalSize GetViewportSize(TerminalSize realSize, TerminalSize virtualSize)
            {
                if (realSize.Columns > 1 && realSize.Rows > 2)
                    return realSize.WithColumns(realSize.Columns - 1).WithRows(realSize.Rows - 2);
                return realSize;
            }

            public TerminalPosition GetViewportOffset() => TerminalPosition.TopLeftCorner;

            public void DrawFrame(TextGraphics graphics, TerminalSize realSize, TerminalSize virtualSize, TerminalPosition virtualScrollPosition)
            {
                if (realSize.Columns == 1 || realSize.Rows <= 2) return;
                TerminalSize viewportSize = GetViewportSize(realSize, virtualSize);
                graphics.SetForegroundColor(TextColor.ANSI.White);
                graphics.SetBackgroundColor(TextColor.ANSI.Black);
                graphics.Fill(' ');
                graphics.PutString(0, graphics.GetSize().Rows - 1, "Terminal too small, use ALT+arrows to scroll");
                int horizontalSize = (int)(((double)viewportSize.Columns / virtualSize.Columns) * viewportSize.Columns);
                int scrollable = viewportSize.Columns - horizontalSize - 1;
                int horizontalPosition = (int)(scrollable * ((double)virtualScrollPosition.Column / (virtualSize.Columns - viewportSize.Columns)));
                graphics.DrawLine(
                    new TerminalPosition(horizontalPosition, graphics.GetSize().Rows - 2),
                    new TerminalPosition(horizontalPosition + horizontalSize, graphics.GetSize().Rows - 2),
                    Symbols.BlockMiddle);
                int verticalSize = (int)(((double)viewportSize.Rows / virtualSize.Rows) * viewportSize.Rows);
                scrollable = viewportSize.Rows - verticalSize - 1;
                int verticalPosition = (int)(scrollable * ((double)virtualScrollPosition.Row / (virtualSize.Rows - viewportSize.Rows)));
                graphics.DrawLine(
                    new TerminalPosition(graphics.GetSize().Columns - 1, verticalPosition),
                    new TerminalPosition(graphics.GetSize().Columns - 1, verticalPosition + verticalSize),
                    Symbols.BlockMiddle);
            }
        }
    }
}
