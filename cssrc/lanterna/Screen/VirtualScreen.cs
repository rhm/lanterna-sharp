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
 * Copyright (C) 2010-2024 Martin Berglund
 */

using Lanterna.Core;
using Lanterna.Graphics;
using Lanterna.Input;
using Lanterna.Gui2;

namespace Lanterna.Screen;

/// <summary>
/// VirtualScreen wraps a normal screen and presents it as a screen that has a configurable minimum size; if the real
/// screen is smaller than this size, the presented screen will add scrolling to get around it. To anyone using this
/// class, it will appear and behave just as a normal screen. Scrolling is done by using CTRL + arrow keys.
/// <p>
/// The use case for this class is to allow you to set a minimum size that you can count on be honored, no matter how
/// small the user makes the terminal. This should make programming GUIs easier.
/// </summary>
public class VirtualScreen : AbstractScreen
{
    private readonly IScreen _realScreen;
    private readonly IFrameRenderer _frameRenderer;
    private TerminalSize _minimumSize;
    private TerminalPosition _viewportTopLeft;
    private TerminalSize _viewportSize;
    private bool _scrollWithCtrl;

    /// <summary>
    /// Creates a new VirtualScreen that wraps a supplied Screen. The screen passed in here should be the real screen
    /// that is created on top of the real Terminal, it will have the correct size and content for what's
    /// actually displayed to the user, but this class will present everything as one view with a fixed minimum size,
    /// no matter what size the real terminal has.
    /// <p>
    /// The initial minimum size will be the current size of the screen.
    /// </summary>
    /// <param name="screen">Real screen that will be used when drawing the whole or partial virtual screen</param>
    public VirtualScreen(IScreen screen) : base(screen.GetTerminalSize())
    {
        _frameRenderer = new DefaultFrameRenderer();
        _realScreen = screen;
        _minimumSize = screen.GetTerminalSize();
        _viewportTopLeft = TerminalPosition.TopLeftCorner;
        _viewportSize = _minimumSize;
        _scrollWithCtrl = false;
    }

    /// <summary>
    /// Sets the minimum size we want the virtual screen to have. If the user resizes the real terminal to something
    /// smaller than this, the virtual screen will refuse to make it smaller and add scrollbars to the view.
    /// </summary>
    /// <param name="minimumSize">Minimum size we want the screen to have</param>
    public void SetMinimumSize(TerminalSize minimumSize)
    {
        _minimumSize = minimumSize;
        var virtualSize = _minimumSize.Max(_realScreen.GetTerminalSize());
        if (!_minimumSize.Equals(virtualSize))
        {
            SetLatestResizeRequest(virtualSize);
            DoResizeIfNecessary();
        }
        CalculateViewport(_realScreen.GetTerminalSize());
    }

    /// <summary>
    /// Returns the minimum size this virtual screen can have. If the real terminal is made smaller than this, the
    /// virtual screen will draw scrollbars and implement scrolling
    /// </summary>
    /// <returns>Minimum size configured for this virtual screen</returns>
    public TerminalSize GetMinimumSize()
    {
        return _minimumSize;
    }

    /// <summary>
    /// Returns the current size of the viewport. This will generally match the dimensions of the underlying terminal.
    /// </summary>
    /// <returns>Viewport size for this VirtualScreen</returns>
    public TerminalSize GetViewportSize()
    {
        return _viewportSize;
    }

    /// <summary>
    /// When the viewport is too small, user can scroll using ALT + arrow keys, but ALT can be replaced by CTRL by
    /// calling this method.
    /// </summary>
    /// <param name="scrollOnCtrl">Scroll using CTRL instead of ALT if set to true, ALT if false</param>
    public void SetScrollOnCtrl(bool scrollOnCtrl)
    {
        _scrollWithCtrl = scrollOnCtrl;
    }

    public void SetViewportTopLeft(TerminalPosition position)
    {
        _viewportTopLeft = position;
        while (_viewportTopLeft.Column > 0 && _viewportTopLeft.Column + _viewportSize.Columns > _minimumSize.Columns)
        {
            _viewportTopLeft = _viewportTopLeft.WithRelativeColumn(-1);
        }
        while (_viewportTopLeft.Row > 0 && _viewportTopLeft.Row + _viewportSize.Rows > _minimumSize.Rows)
        {
            _viewportTopLeft = _viewportTopLeft.WithRelativeRow(-1);
        }
    }

    public override void StartScreen()
    {
        _realScreen.StartScreen();
    }

    public override void StopScreen()
    {
        _realScreen.StopScreen();
    }

    public override TextCharacter GetFrontCharacter(TerminalPosition position)
    {
        return GetFrontCharacter(position.Column, position.Row);
    }

    public override void SetCursorPosition(TerminalPosition? position)
    {
        base.SetCursorPosition(position);
        if (position == null)
        {
            _realScreen.SetCursorPosition(null);
            return;
        }
        
        var adjustedPosition = position.Value.WithRelativeColumn(-_viewportTopLeft.Column).WithRelativeRow(-_viewportTopLeft.Row);
        if (adjustedPosition.Column >= 0 && adjustedPosition.Column < _viewportSize.Columns &&
            adjustedPosition.Row >= 0 && adjustedPosition.Row < _viewportSize.Rows)
        {
            _realScreen.SetCursorPosition(adjustedPosition);
        }
        else
        {
            _realScreen.SetCursorPosition(null);
        }
    }

    public override TerminalSize? DoResizeIfNecessary()
    {
        var underlyingSize = _realScreen.DoResizeIfNecessary();
        if (underlyingSize == null)
        {
            return null;
        }

        var newVirtualSize = CalculateViewport(underlyingSize.Value);
        if (!GetTerminalSize().Equals(newVirtualSize))
        {
            SetLatestResizeRequest(newVirtualSize);
            return ProcessResizeIfNecessary();
        }
        return newVirtualSize;
    }

    private TerminalSize CalculateViewport(TerminalSize realTerminalSize)
    {
        var newVirtualSize = _minimumSize.Max(realTerminalSize);
        if (newVirtualSize.Equals(realTerminalSize))
        {
            _viewportSize = realTerminalSize;
            _viewportTopLeft = TerminalPosition.TopLeftCorner;
        }
        else
        {
            var newViewportSize = _frameRenderer.GetViewportSize(realTerminalSize, newVirtualSize);
            if (newViewportSize.Rows > _viewportSize.Rows)
            {
                _viewportTopLeft = _viewportTopLeft.WithRow(Math.Max(0, _viewportTopLeft.Row - (newViewportSize.Rows - _viewportSize.Rows)));
            }
            if (newViewportSize.Columns > _viewportSize.Columns)
            {
                _viewportTopLeft = _viewportTopLeft.WithColumn(Math.Max(0, _viewportTopLeft.Column - (newViewportSize.Columns - _viewportSize.Columns)));
            }
            _viewportSize = newViewportSize;
        }
        return newVirtualSize;
    }

    public override void Refresh(RefreshType refreshType)
    {
        SetCursorPosition(GetCursorPosition()); // Make sure the cursor is at the correct position
        if (!_viewportSize.Equals(_realScreen.GetTerminalSize()))
        {
            _frameRenderer.DrawFrame(
                _realScreen.NewTextGraphics(),
                _realScreen.GetTerminalSize(),
                GetTerminalSize(),
                _viewportTopLeft);
        }

        // Copy the rows
        var viewportOffset = _frameRenderer.GetViewportOffset();
        if (_realScreen is AbstractScreen asAbstractScreen)
        {
            BackBuffer.CopyTo(
                asAbstractScreen.GetBackBuffer(),
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
                        BackBuffer.GetCharacterAt(
                            x + _viewportTopLeft.Column,
                            y + _viewportTopLeft.Row));
                }
            }
        }
        _realScreen.Refresh(refreshType);
    }

    public override void Refresh()
    {
        Refresh(RefreshType.Automatic);
    }

    public override KeyStroke? PollInput()
    {
        return Filter(_realScreen.PollInput());
    }

    public override KeyStroke ReadInput()
    {
        return Filter(_realScreen.ReadInput()) ?? ReadInput();
    }

    public override KeyStroke? ReadInput(TimeSpan timeout)
    {
        return Filter(_realScreen.ReadInput(timeout));
    }

    private KeyStroke? Filter(KeyStroke? keyStroke)
    {
        if (keyStroke == null)
        {
            return null;
        }
        else if (IsScrollTrigger(keyStroke) && keyStroke.KeyType == KeyType.ArrowLeft)
        {
            if (_viewportTopLeft.Column > 0)
            {
                _viewportTopLeft = _viewportTopLeft.WithRelativeColumn(-1);
                Refresh();
                return null;
            }
        }
        else if (IsScrollTrigger(keyStroke) && keyStroke.KeyType == KeyType.ArrowRight)
        {
            if (_viewportTopLeft.Column + _viewportSize.Columns < GetTerminalSize().Columns)
            {
                _viewportTopLeft = _viewportTopLeft.WithRelativeColumn(1);
                Refresh();
                return null;
            }
        }
        else if (IsScrollTrigger(keyStroke) && keyStroke.KeyType == KeyType.ArrowUp)
        {
            if (_viewportTopLeft.Row > 0)
            {
                _viewportTopLeft = _viewportTopLeft.WithRelativeRow(-1);
                _realScreen.ScrollLines(0, _viewportSize.Rows - 1, -1);
                Refresh();
                return null;
            }
        }
        else if (IsScrollTrigger(keyStroke) && keyStroke.KeyType == KeyType.ArrowDown)
        {
            if (_viewportTopLeft.Row + _viewportSize.Rows < GetTerminalSize().Rows)
            {
                _viewportTopLeft = _viewportTopLeft.WithRelativeRow(1);
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
                _viewportTopLeft = _viewportTopLeft.WithRelativeRow(-scroll);
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
                {
                    scroll = GetTerminalSize().Rows - _viewportTopLeft.Row - _viewportSize.Rows;
                }
                _viewportTopLeft = _viewportTopLeft.WithRelativeRow(scroll);
                _realScreen.ScrollLines(0, _viewportSize.Rows - scroll, scroll);
                Refresh();
                return null;
            }
        }
        return keyStroke;
    }

    private static bool IsSpaceBarPress(KeyStroke keyStroke)
    {
        return keyStroke.KeyType == KeyType.Character && keyStroke.Character == ' ';
    }

    private bool IsScrollTrigger(KeyStroke keyStroke)
    {
        return _scrollWithCtrl ? keyStroke.IsCtrlDown : keyStroke.IsAltDown;
    }

    public override void ScrollLines(int firstLine, int lastLine, int distance)
    {
        // do base class stuff (scroll own back buffer)
        base.ScrollLines(firstLine, lastLine, distance);
        // vertical range visible in realScreen:
        int vpFirst = _viewportTopLeft.Row,
            vpRows = _viewportSize.Rows;
        // adapt to realScreen range:
        firstLine = Math.Max(0, firstLine - vpFirst);
        lastLine = Math.Min(vpRows - 1, lastLine - vpFirst);
        // if resulting range non-empty: scroll that range in realScreen:
        if (firstLine <= lastLine)
        {
            _realScreen.ScrollLines(firstLine, lastLine, distance);
        }
    }

    /// <summary>
    /// Interface for rendering the virtual screen's frame when the real terminal is too small for the virtual screen
    /// </summary>
    public interface IFrameRenderer
    {
        /// <summary>
        /// Given the size of the real terminal and the current size of the virtual screen, how large should the viewport
        /// where the screen content is drawn be?
        /// </summary>
        /// <param name="realSize">Size of the real terminal</param>
        /// <param name="virtualSize">Size of the virtual screen</param>
        /// <returns>Size of the viewport, according to this FrameRenderer</returns>
        TerminalSize GetViewportSize(TerminalSize realSize, TerminalSize virtualSize);

        /// <summary>
        /// Where in the virtual screen should the top-left position of the viewport be? To draw the viewport from the
        /// top-left position of the screen, return 0x0 (or TerminalPosition.TopLeftCorner) here.
        /// </summary>
        /// <returns>Position of the top-left corner of the viewport inside the screen</returns>
        TerminalPosition GetViewportOffset();

        /// <summary>
        /// Drawn the 'frame', meaning anything that is outside the viewport (title, scrollbar, etc)
        /// </summary>
        /// <param name="graphics">Graphics to use to text drawing operations</param>
        /// <param name="realSize">Size of the real terminal</param>
        /// <param name="virtualSize">Size of the virtual screen</param>
        /// <param name="virtualScrollPosition">If the virtual screen is larger than the real terminal, this is the current
        ///                              scroll offset the VirtualScreen is using</param>
        void DrawFrame(
            ITextGraphics graphics,
            TerminalSize realSize,
            TerminalSize virtualSize,
            TerminalPosition virtualScrollPosition);
    }

    private class DefaultFrameRenderer : IFrameRenderer
    {
        public TerminalSize GetViewportSize(TerminalSize realSize, TerminalSize virtualSize)
        {
            if (realSize.Columns > 1 && realSize.Rows > 2)
            {
                return realSize.WithRelativeColumns(-1).WithRelativeRows(-2);
            }
            else
            {
                return realSize;
            }
        }

        public TerminalPosition GetViewportOffset()
        {
            return TerminalPosition.TopLeftCorner;
        }

        public void DrawFrame(
            ITextGraphics graphics,
            TerminalSize realSize,
            TerminalSize virtualSize,
            TerminalPosition virtualScrollPosition)
        {
            if (realSize.Columns == 1 || realSize.Rows <= 2)
            {
                return;
            }
            var viewportSize = GetViewportSize(realSize, virtualSize);

            graphics.SetForegroundColor(new AnsiTextColor(AnsiColor.White));
            graphics.SetBackgroundColor(new AnsiTextColor(AnsiColor.Black));
            graphics.Fill(' ');
            graphics.PutString(0, graphics.Size.Rows - 1, "Terminal too small, use ALT+arrows to scroll");

            int horizontalSize = (int)(((double)(viewportSize.Columns) / (double)virtualSize.Columns) * (viewportSize.Columns));
            int scrollable = viewportSize.Columns - horizontalSize - 1;
            int horizontalPosition = (int)((double)scrollable * ((double)virtualScrollPosition.Column / (double)(virtualSize.Columns - viewportSize.Columns)));
            graphics.DrawLine(
                new TerminalPosition(horizontalPosition, graphics.Size.Rows - 2),
                new TerminalPosition(horizontalPosition + horizontalSize, graphics.Size.Rows - 2),
                Symbols.BLOCK_MIDDLE);

            int verticalSize = (int)(((double)(viewportSize.Rows) / (double)virtualSize.Rows) * (viewportSize.Rows));
            scrollable = viewportSize.Rows - verticalSize - 1;
            int verticalPosition = (int)((double)scrollable * ((double)virtualScrollPosition.Row / (double)(virtualSize.Rows - viewportSize.Rows)));
            graphics.DrawLine(
                new TerminalPosition(graphics.Size.Columns - 1, verticalPosition),
                new TerminalPosition(graphics.Size.Columns - 1, verticalPosition + verticalSize),
                Symbols.BLOCK_MIDDLE);
        }
    }
}