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

namespace Lanterna.Gui2;

/// <summary>
/// Manages a collection of windows in Z-order and handles focus management and window cycling
/// </summary>
public class WindowList
{
    private readonly List<IWindow> _windows = new();
    private readonly List<IWindow> _stableOrderingOfWindows = new();
    
    private IWindow? _activeWindow;
    private bool _hadWindowAtSomePoint;

    /// <summary>
    /// Gets an unmodifiable list of windows in Z-order (bottom to top)
    /// </summary>
    /// <returns>List of windows in Z-order</returns>
    public IReadOnlyList<IWindow> GetWindowsInZOrder()
    {
        return _windows.AsReadOnly();
    }

    /// <summary>
    /// Gets an unmodifiable list of windows in stable ordering (order they were added)
    /// </summary>
    /// <returns>List of windows in stable order</returns>
    public IReadOnlyList<IWindow> GetWindowsInStableOrder()
    {
        return _stableOrderingOfWindows.AsReadOnly();
    }

    /// <summary>
    /// Sets the active window and moves it to the top of the Z-order
    /// </summary>
    /// <param name="activeWindow">Window to make active, or null to clear active window</param>
    public void SetActiveWindow(IWindow? activeWindow)
    {
        _activeWindow = activeWindow;
        if (activeWindow != null)
        {
            MoveToTop(activeWindow);
        }
    }

    /// <summary>
    /// Gets the currently active window
    /// </summary>
    /// <returns>The active window, or null if no window is active</returns>
    public IWindow? GetActiveWindow()
    {
        return _activeWindow;
    }

    /// <summary>
    /// Adds a window to the window list. If the window doesn't have NO_FOCUS hint, it becomes the active window.
    /// </summary>
    /// <param name="window">Window to add</param>
    public void AddWindow(IWindow window)
    {
        if (!_stableOrderingOfWindows.Contains(window))
        {
            _stableOrderingOfWindows.Add(window);
        }
        if (!_windows.Contains(window))
        {
            _windows.Add(window);
        }
        if (!window.Hints.Contains(IWindow.Hint.NoFocus))
        {
            SetActiveWindow(window);
        }
        _hadWindowAtSomePoint = true;
    }

    /// <summary>
    /// Removes a window from the window list
    /// </summary>
    /// <param name="window">Window to remove</param>
    /// <returns>true if the window was contained in this list</returns>
    public bool RemoveWindow(IWindow window)
    {
        bool contained = _windows.Remove(window);
        _stableOrderingOfWindows.Remove(window);

        if (_activeWindow == window)
        {
            // In case no suitable window is found, pass control back to the background pane
            SetActiveWindow(null);

            // Go backward in reverse and find the first suitable window
            for (int index = _windows.Count - 1; index >= 0; index--)
            {
                var candidate = _windows[index];
                if (!candidate.Hints.Contains(IWindow.Hint.NoFocus))
                {
                    SetActiveWindow(candidate);
                    break;
                }
            }
        }

        return contained;
    }

    /// <summary>
    /// Returns true if this window list has had at least one window added to it at some point
    /// </summary>
    /// <returns>true if a window has been added at some point</returns>
    public bool IsHadWindowAtSomePoint()
    {
        return _hadWindowAtSomePoint;
    }

    /// <summary>
    /// Moves a window to the top of the Z-order
    /// </summary>
    /// <param name="window">Window to move to top</param>
    /// <exception cref="ArgumentException">If the window is not in this list</exception>
    public void MoveToTop(IWindow window)
    {
        if (!_windows.Contains(window))
        {
            throw new ArgumentException($"Window {window} isn't in this WindowList");
        }
        _windows.Remove(window);
        _windows.Add(window);
    }

    /// <summary>
    /// Moves a window to the bottom of the Z-order
    /// </summary>
    /// <param name="window">Window to move to bottom</param>
    /// <exception cref="ArgumentException">If the window is not in this list</exception>
    public void MoveToBottom(IWindow window)
    {
        if (!_windows.Contains(window))
        {
            throw new ArgumentException($"Window {window} isn't in this WindowList");
        }
        _windows.Remove(window);
        _windows.Insert(0, window);
    }

    /// <summary>
    /// Switches the active window by cyclically shuffling the window list. If reverse parameter is false
    /// then the current top window is placed at the bottom of the stack and the window immediately behind it is the new
    /// top. If reverse is set to true then the window at the bottom of the stack is moved up to the
    /// front and the previous top window will be immediately below it
    /// </summary>
    /// <param name="reverse">Direction to cycle through the windows</param>
    /// <returns>This WindowList for method chaining</returns>
    public WindowList CycleActiveWindow(bool reverse)
    {
        if (_windows.Count == 0 || _windows.Count == 1 || 
            (_activeWindow != null && _activeWindow.Hints.Contains(IWindow.Hint.Modal)))
        {
            return this;
        }
        
        var originalActiveWindow = _activeWindow;
        IWindow nextWindow;
        
        if (_activeWindow == null)
        {
            // Cycling out of active background pane
            nextWindow = reverse ? _windows[_windows.Count - 1] : _windows[0];
        }
        else
        {
            // Switch to the next window
            nextWindow = GetNextWindow(reverse, _activeWindow);
        }

        int noFocusWindows = 0;
        while (nextWindow.Hints.Contains(IWindow.Hint.NoFocus))
        {
            noFocusWindows++;
            if (noFocusWindows == _windows.Count)
            {
                // All windows are NO_FOCUS, so give up
                return this;
            }
            nextWindow = GetNextWindow(reverse, nextWindow);
            if (nextWindow == originalActiveWindow)
            {
                return this;
            }
        }

        if (reverse)
        {
            MoveToTop(nextWindow);
        }
        else if (originalActiveWindow != null)
        {
            MoveToBottom(originalActiveWindow);
        }
        SetActiveWindow(nextWindow);
        return this;
    }

    private IWindow GetNextWindow(bool reverse, IWindow window)
    {
        int index = _windows.IndexOf(window);
        if (reverse)
        {
            if (++index >= _windows.Count)
            {
                index = 0;
            }
        }
        else
        {
            if (--index < 0)
            {
                index = _windows.Count - 1;
            }
        }
        return _windows[index];
    }
}