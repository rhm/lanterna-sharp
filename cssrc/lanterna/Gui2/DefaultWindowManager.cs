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
using Lanterna.Core;

namespace Lanterna.Gui2;

/// <summary>
/// The default window manager implementation used by Lanterna. New windows will be generally added in a tiled manner,
/// starting in the top-left corner and moving down-right as new windows are added. By using the various window hints
/// that are available you have some control over how the window manager will place and size the windows.
/// </summary>
public class DefaultWindowManager : IWindowManager
{
    private readonly IWindowDecorationRenderer? _windowDecorationRendererOverride;
    private TerminalSize _lastKnownScreenSize;

    /// <summary>
    /// Default constructor, will create a window manager that uses DefaultWindowDecorationRenderer for drawing
    /// window decorations, unless the current theme has an override. Any size calculations done before the text GUI has
    /// actually been started and displayed on the terminal will assume the terminal size is 80x24.
    /// </summary>
    public DefaultWindowManager() : this(null)
    {
    }

    /// <summary>
    /// Creates a new DefaultWindowManager using a DefaultWindowDecorationRenderer for drawing window
    /// decorations, unless the current theme has an override. Any size calculations done before the text GUI has
    /// actually been started and displayed on the terminal will use the size passed in with the
    /// initialScreenSize parameter (if null then size will be assumed to be 80x24)
    /// </summary>
    /// <param name="initialScreenSize">Size to assume the terminal has until the text GUI is started and can be notified of the correct size</param>
    public DefaultWindowManager(TerminalSize? initialScreenSize) : this(null, initialScreenSize)
    {
    }

    /// <summary>
    /// Creates a new DefaultWindowManager using a specified windowDecorationRendererOverride for drawing window
    /// decorations. Any size calculations done before the text GUI has actually been started and displayed on the
    /// terminal will use the size passed in with the initialScreenSize parameter
    /// </summary>
    /// <param name="windowDecorationRenderer">Window decoration renderer to use when drawing windows</param>
    /// <param name="initialScreenSize">Size to assume the terminal has until the text GUI is started and can be notified of the correct size</param>
    public DefaultWindowManager(IWindowDecorationRenderer? windowDecorationRenderer, TerminalSize? initialScreenSize)
    {
        _windowDecorationRendererOverride = windowDecorationRenderer;
        _lastKnownScreenSize = initialScreenSize ?? new TerminalSize(80, 24);
    }

    public bool IsInvalid => false;

    public IWindowDecorationRenderer GetWindowDecorationRenderer(IWindow window)
    {
        if (window.Hints.Contains(IWindow.Hint.NoDecorations))
        {
            return new EmptyWindowDecorationRenderer();
        }
        else if (_windowDecorationRendererOverride != null)
        {
            return _windowDecorationRendererOverride;
        }
        else if (window.Theme?.WindowDecorationRenderer != null)
        {
            return window.Theme.WindowDecorationRenderer;
        }
        else
        {
            return new DefaultWindowDecorationRenderer();
        }
    }

    public void OnAdded(IWindowBasedTextGUI textGUI, IWindow window, IReadOnlyList<IWindow> allWindows)
    {
        var decorationRenderer = GetWindowDecorationRenderer(window);
        var expectedDecoratedSize = decorationRenderer.GetDecoratedSize(window, window.PreferredSize);
        window.DecoratedSize = expectedDecoratedSize;

        if (window.Hints.Contains(IWindow.Hint.FixedPosition))
        {
            // Don't place the window, assume the position is already set
        }
        else if (allWindows.Count == 0)
        {
            window.Position = TerminalPosition.Offset1X1;
        }
        else if (window.Hints.Contains(IWindow.Hint.Centered))
        {
            int left = (_lastKnownScreenSize.Columns - expectedDecoratedSize.Columns) / 2;
            int top = (_lastKnownScreenSize.Rows - expectedDecoratedSize.Rows) / 2;
            window.Position = new TerminalPosition(left, top);
        }
        else
        {
            var nextPosition = allWindows[allWindows.Count - 1].Position.WithRelative(2, 1);
            if (nextPosition.Column + expectedDecoratedSize.Columns > _lastKnownScreenSize.Columns ||
                nextPosition.Row + expectedDecoratedSize.Rows > _lastKnownScreenSize.Rows)
            {
                nextPosition = TerminalPosition.Offset1X1;
            }
            window.Position = nextPosition;
        }

        // Finally, run through the usual calculations so the window manager's usual prepare method can have its say
        PrepareWindow(_lastKnownScreenSize, window);
    }

    public void OnRemoved(IWindowBasedTextGUI textGUI, IWindow window, IReadOnlyList<IWindow> allWindows)
    {
        // NOP
    }

    public void PrepareWindows(IWindowBasedTextGUI textGUI, IReadOnlyList<IWindow> allWindows, TerminalSize screenSize)
    {
        _lastKnownScreenSize = screenSize;
        foreach (var window in allWindows)
        {
            PrepareWindow(screenSize, window);
        }
    }

    /// <summary>
    /// Called by DefaultWindowManager when iterating through all windows to decide their size and position. If
    /// you override DefaultWindowManager to add your own logic to how windows are placed on the screen, you can
    /// override this method and selectively choose which window to interfere with. Note that the two key properties that
    /// are read by the GUI system after preparing all windows are the position and decorated size. Your custom
    /// implementation should set these two fields directly on the window. You can infer the decorated size from the
    /// content size by using the window decoration renderer that is attached to the window manager.
    /// </summary>
    /// <param name="screenSize">Size of the terminal that is available to draw on</param>
    /// <param name="window">Window to prepare decorated size and position for</param>
    protected virtual void PrepareWindow(TerminalSize screenSize, IWindow window)
    {
        TerminalSize contentAreaSize;
        if (window.Hints.Contains(IWindow.Hint.FixedSize))
        {
            contentAreaSize = window.Size;
        }
        else
        {
            contentAreaSize = window.PreferredSize;
        }
        
        var size = GetWindowDecorationRenderer(window).GetDecoratedSize(window, contentAreaSize);
        var position = window.Position;

        if (window.Hints.Contains(IWindow.Hint.FullScreen))
        {
            position = TerminalPosition.TopLeftCorner;
            size = screenSize;
        }
        else if (window.Hints.Contains(IWindow.Hint.Expanded))
        {
            position = TerminalPosition.Offset1X1;
            size = screenSize.WithRelative(
                -Math.Min(4, screenSize.Columns),
                -Math.Min(3, screenSize.Rows));
            if (!size.Equals(window.DecoratedSize))
            {
                window.Invalidate();
            }
        }
        else if (window.Hints.Contains(IWindow.Hint.FitTerminalWindow) ||
                 window.Hints.Contains(IWindow.Hint.Centered))
        {
            // If the window is too big for the terminal, move it up towards 0x0 and if that's not enough then shrink
            // it instead
            while (position.Row > 0 && position.Row + size.Rows > screenSize.Rows)
            {
                position = position.WithRelativeRow(-1);
            }
            while (position.Column > 0 && position.Column + size.Columns > screenSize.Columns)
            {
                position = position.WithRelativeColumn(-1);
            }
            if (position.Row + size.Rows > screenSize.Rows)
            {
                size = size.WithRows(screenSize.Rows - position.Row);
            }
            if (position.Column + size.Columns > screenSize.Columns)
            {
                size = size.WithColumns(screenSize.Columns - position.Column);
            }
            if (window.Hints.Contains(IWindow.Hint.Centered))
            {
                int left = (_lastKnownScreenSize.Columns - size.Columns) / 2;
                int top = (_lastKnownScreenSize.Rows - size.Rows) / 2;
                position = new TerminalPosition(left, top);
            }
        }

        window.Position = position;
        window.DecoratedSize = size;
    }
}